using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace Jellyfin.Plugin.Discogs.ExternalIds
{
    /// <summary>
    /// Discogs album external id.
    /// </summary>
    public class AlbumExternalId : IExternalId
    {
        /// <inheritdoc />
        public string ProviderName => Constants.Name;

        /// <inheritdoc />
        public string Key => Constants.ProviderIds.Album;

        /// <inheritdoc />
        public ExternalIdMediaType? Type => ExternalIdMediaType.Album;

        /// <inheritdoc />
        public string UrlFormatString => Plugin.Instance?.Configuration.Url + "/release/{0}";

        /// <inheritdoc />
        public bool Supports(IHasProviderIds item) => item is Audio || item is MusicAlbum;
    }
}
