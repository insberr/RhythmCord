using NetCord.Services.ApplicationCommands;
using Singularity.Features.Booru.Models;
using Singularity.Observability;

namespace Singularity.Features.Booru;

public class BooruCommands(BooruResponder responder, BotMetrics metrics) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("e621", "Search for posts on e621 with optional filters.")]
    public Task SearchE621Async(
        [SlashCommandParameter(Name = "tags", Description = "Space-separated search tags")]
        string tags = "",

        [SlashCommandParameter(Name = "rating", Description = "Override content rating filter")]
        BooruRating? rating = null,

        [SlashCommandParameter(Name = "type", Description = "Filter by media file format")]
        BooruMediaType? type = null)
    {
        metrics.Command("e621");
        return responder.RespondAsync(Context.Interaction, new BooruSearch(tags, rating, type));
    }
}
