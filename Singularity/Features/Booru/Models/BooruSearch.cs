namespace Singularity.Features.Booru.Models;

/// <summary>
/// What an /e621 search asked for. It travels inside the Another button's custom ID so the search can be
/// run again later, even after a restart, without the bot having to remember anything.
/// </summary>
internal sealed record BooruSearch(string Tags, BooruRating? Rating, BooruMediaType? Type)
{
    internal const string AnotherButtonId = "e621-another";

    // Discord's limit for a component custom ID.
    private const int MaxCustomIdLength = 100;

    // Stands in for "not set", since an empty segment is easy to lose.
    private const string None = "-";

    /// <summary>
    /// "e621-another:{rating}:{type}:{tags}", or null when the search is too long to fit in a button.
    /// NetCord splits custom IDs on ':' into the handler's parameters, but e621 tags use ':' all the time
    /// (score:>10), so colons in the tags are percent-encoded, along with '%' itself so decoding is exact.
    /// </summary>
    internal string? ToCustomId()
    {
        var tags = string.Join(' ', Tags.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Replace("%", "%25")
            .Replace(":", "%3A");

        var id = $"{AnotherButtonId}:{Rating?.ToString() ?? None}:{Type?.ToString() ?? None}:{(tags.Length > 0 ? tags : None)}";
        return id.Length <= MaxCustomIdLength ? id : null;
    }

    /// <summary>
    /// The reverse of <see cref="ToCustomId"/>, from the three segments after the button's name.
    /// </summary>
    internal static BooruSearch FromCustomId(string rating, string type, string tags) => new(
        tags == None ? "" : Uri.UnescapeDataString(tags),
        Enum.TryParse<BooruRating>(rating, out var r) ? r : null,
        Enum.TryParse<BooruMediaType>(type, out var t) ? t : null);
}
