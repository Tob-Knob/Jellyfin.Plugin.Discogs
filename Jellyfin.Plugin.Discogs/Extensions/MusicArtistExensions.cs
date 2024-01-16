using MediaBrowser.Controller.Entities.Audio;

namespace Jellyfin.Plugin.Discogs
{
    /// <summary>
    /// Artist info extensions.
    /// </summary>
    public static class MusicArtistExensions
    {
        /// <summary>
        /// Get the Discogs artist id.
        /// </summary>
        /// <param name="artist">The artist info.</param>
        /// <returns>The artist id.</returns>
        public static string? GetArtistId(this MusicArtist artist)
        {
            artist.ProviderIds.TryGetValue(Constants.ProviderIds.Artist, out var id);

            return id;
        }
    }
}
