using NetCord.Services.ComponentInteractions;
using Singularity.Features.Booru.Models;
using Singularity.Observability;

namespace Singularity.Features.Booru;

public class BooruButtons(BooruResponder responder, BotMetrics metrics) : ComponentInteractionModule<ButtonInteractionContext>
{
    /// <summary>
    /// Another: runs the search behind the clicked post again and posts a new result.
    /// The parameters are the segments of the custom ID built by <see cref="BooruSearch.ToCustomId"/>.
    /// </summary>
    [ComponentInteraction(BooruSearch.AnotherButtonId)]
    public Task AnotherAsync(string rating, string type, string tags)
    {
        metrics.Command("e621.another");
        return responder.RespondAsync(Context.Interaction, BooruSearch.FromCustomId(rating, type, tags));
    }
}
