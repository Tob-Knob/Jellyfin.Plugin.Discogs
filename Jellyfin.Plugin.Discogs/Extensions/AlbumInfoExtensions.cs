using System.Linq;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.Discogs
{
    /// <summary>
    /// Album info extensions.
    /// </summary>
    public static class AlbumInfoExtensions
    {
        /// <summary>
        /// Gets the album artist.
        /// </summary>
        /// <param name="info">The album info.</param>
        /// <returns>The album artist.</returns>
        public static string GetAlbumArtist(this AlbumInfo info)
        {
            var id = info.SongInfos.SelectMany(i => i.AlbumArtists)
                    .FirstOrDefault(i => !string.IsNullOrEmpty(i));

            if (!string.IsNullOrEmpty(id))
            {
                return id;
            }

            return info.AlbumArtists.Count > 0 ? info.AlbumArtists[0] : string.Empty;
        }

        /// <summary>
        /// Gets the release id.
        /// </summary>
        /// <param name="info">The album info.</param>
        /// <returns>The release id.</returns>
        public static string? GetReleaseId(this AlbumInfo info)
        {
            var id = info.GetProviderId(Constants.ProviderIds.Album);

            if (string.IsNullOrEmpty(id))
            {
                return info.SongInfos.Select(i => i.GetProviderId(Constants.ProviderIds.Album))
                    .FirstOrDefault(i => !string.IsNullOrEmpty(i));
            }

            return id;
        }

        /// <summary>
        /// Gets the release master id.
        /// </summary>
        /// <param name="info">THe album info.</param>
        /// <returns>The release master id.</returns>
        public static string? GetReleaseMasterId(this AlbumInfo info)
        {
            return info.GetProviderId(Constants.ProviderIds.AlbumMaster);
        }

        /// <summary>
        /// Get the Discogs artist id.
        /// </summary>
        /// <param name="info">The album info.</param>
        /// <returns>The artist id.</returns>
        public static string? GetDiscogsArtistId(this AlbumInfo info)
        {
            info.ProviderIds.TryGetValue(Constants.ProviderIds.AlbumArtist, out var id);

            if (string.IsNullOrEmpty(id))
            {
                info.ArtistProviderIds.TryGetValue(Constants.ProviderIds.Artist, out id);
            }

            if (string.IsNullOrEmpty(id))
            {
                return info.SongInfos.Select(i => i.GetProviderId(Constants.ProviderIds.AlbumArtist))
                    .FirstOrDefault(i => !string.IsNullOrEmpty(i));
            }

            return id;
        }
    }
}
