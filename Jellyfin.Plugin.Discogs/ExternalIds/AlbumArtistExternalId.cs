using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace Jellyfin.Plugin.Discogs.ExternalIds
{
    /// <summary>
    /// Discogs album artist external id.
    /// </summary>
    public class AlbumArtistExternalId : IExternalId
    {
        /// <inheritdoc />
        public string ProviderName => Constants.Name;

        /// <inheritdoc />
        public string Key => Constants.ProviderIds.AlbumArtist;

        /// <inheritdoc />
        public ExternalIdMediaType? Type => ExternalIdMediaType.AlbumArtist;

        /// <inheritdoc />
        public string UrlFormatString => Plugin.Instance?.Configuration.Url + "/artist/{0}";

        /// <inheritdoc />
        public bool Supports(IHasProviderIds item) => item is Audio;
    }
}
