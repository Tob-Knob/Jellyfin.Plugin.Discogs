using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace Jellyfin.Plugin.Discogs.ExternalIds
{
    /// <summary>
    /// Discogs artist external id.
    /// </summary>
    public class ArtistExternalId : IExternalId
    {
        /// <inheritdoc />
        public string ProviderName => Constants.Name;

        /// <inheritdoc />
        public string Key => Constants.ProviderIds.Artist;

        /// <inheritdoc />
        public ExternalIdMediaType? Type => ExternalIdMediaType.Artist;

        /// <inheritdoc />
        public string UrlFormatString => Plugin.Instance?.Configuration.Url + "/artist/{0}";

        /// <inheritdoc />
        public bool Supports(IHasProviderIds item) => item is MusicArtist;
    }
}
