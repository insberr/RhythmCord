using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using Singularity.Features.Booru.Models;
using Singularity.Features.Booru.Services;
using Singularity.Infrastructure;
using Singularity.Observability;

namespace Singularity.Features.Booru;

/// <summary>
/// Runs an e621 search and replies with the result. Shared by /e621 and its Another button.
/// </summary>
public sealed class BooruResponder(
    BooruService booruService,
    CommandCooldown cooldown,
    BooruOptions options,
    BotMetrics metrics,
    ErrorReporter errors,
    ILogger<BooruResponder> logger)
{
    internal async Task RespondAsync(Interaction interaction, BooruSearch search)
    {
        using var scope = logger.BeginScope("guild={GuildId} user={UserId}", interaction.GuildId, interaction.User.Id);

        if (!cooldown.TryAcquire("e621", interaction.User.Id, options.Cooldown, out var remaining))
        {
            metrics.CooldownHit();
            await interaction.SendResponseAsync(InteractionCallback.Message(new InteractionMessageProperties
            {
                Content = $"Slow down! Try again in {Math.Ceiling(remaining.TotalSeconds)}s.",
                Flags = MessageFlags.Ephemeral
            }));
            return;
        }

        await interaction.SendResponseAsync(InteractionCallback.DeferredMessage());

        // Forces the safe rating in SFW chats. Checked on every run, since a button can be clicked anywhere it is seen.
        var isNsfwChannel = interaction.Channel is TextGuildChannel { Nsfw: true };
        var rating = isNsfwChannel ? search.Rating : BooruRating.Safe;

        ComponentContainerProperties? result = null;
        string? failure = null;
        try
        {
            // The button repeats the search as asked, not as enforced here, so the channel decides again on click.
            result = await booruService.GetRandomPostAsync(search.Tags, rating, search.Type, search.ToCustomId());
            metrics.BooruRequest(result is null ? "empty" : "ok");
        }
        catch (Exception ex)
        {
            metrics.BooruRequest("error");
            failure = errors.Report(ex, "e621");
        }

        try
        {
            await interaction.ModifyResponseAsync(msg =>
            {
                if (result is null)
                {
                    msg.Content = failure ?? "No posts were found.";
                    return;
                }

                // Components V2 messages cannot carry content or embeds; everything lives in the container.
                msg.Flags = MessageFlags.IsComponentsV2;
                msg.Components = [result];
            });
        }
        catch (Exception ex)
        {
            // The user is left looking at "thinking…"; nothing more can be done for them, but the operator should know.
            errors.Record(ex, "e621.respond", "Could not deliver the e621 result to Discord.");
        }
    }
}
