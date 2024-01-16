using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.Discogs
{
    /// <summary>
    /// Artist info extensions.
    /// </summary>
    public static class MusicAlbumExensions
    {
        /// <summary>
        /// Gets the release id.
        /// </summary>
        /// <param name="album">The album info.</param>
        /// <returns>The release id.</returns>
        public static string? GetReleaseId(this MusicAlbum album)
        {
            var id = album.GetProviderId(Constants.ProviderIds.Album);

            return id;
        }
    }
}
