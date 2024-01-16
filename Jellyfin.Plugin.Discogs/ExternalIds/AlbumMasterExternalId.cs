using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace Jellyfin.Plugin.Discogs.ExternalIds
{
    /// <summary>
    /// Discogs album master external id.
    /// </summary>
    public class AlbumMasterExternalId : IExternalId
    {
        /// <inheritdoc />
        public string ProviderName => Constants.Name;

        /// <inheritdoc />
        public string Key => Constants.ProviderIds.AlbumMaster;

        /// <inheritdoc />
        public ExternalIdMediaType? Type => ExternalIdMediaType.ReleaseGroup;

        /// <inheritdoc />
        public string UrlFormatString => Plugin.Instance?.Configuration.Url + "/master/{0}";

        /// <inheritdoc />
        public bool Supports(IHasProviderIds item) => item is Audio || item is MusicAlbum;
    }
}
