using System.Linq;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.Discogs
{
    /// <summary>
    /// Artist info extensions.
    /// </summary>
    public static class ArtistInfoExtensions
    {
        /// <summary>
        /// Get the Discogs artist id.
        /// </summary>
        /// <param name="info">The artist info.</param>
        /// <returns>The artist id.</returns>
        public static string? GetArtistId(this ArtistInfo info)
        {
            info.ProviderIds.TryGetValue(Constants.ProviderIds.Artist, out var id);

            if (string.IsNullOrEmpty(id))
            {
                return info.SongInfos.Select(i => i.GetProviderId(Constants.ProviderIds.AlbumArtist))
                    .FirstOrDefault(i => !string.IsNullOrEmpty(i));
            }

            return id;
        }
    }
}
