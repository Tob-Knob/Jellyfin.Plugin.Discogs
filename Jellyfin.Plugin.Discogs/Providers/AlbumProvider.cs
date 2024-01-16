using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Discogs.Configuration;
using Jellyfin.Plugin.Discogs.Exceptions;
using Jellyfin.Plugin.Discogs.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Discogs.Providers
{
    /// <summary>
    /// Discogs album provider.
    /// </summary>
    public class AlbumProvider : IRemoteMetadataProvider<MusicAlbum, AlbumInfo>, IRemoteImageProvider
    {
        private readonly ILogger<AlbumProvider> _logger;
        private readonly DiscogsClient.DiscogsClient _discogsClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlbumProvider"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="Logger{AlbumProvider}"/> interface.</param>
        public AlbumProvider(ILogger<AlbumProvider> logger)
        {
            _logger = logger;

            if (string.IsNullOrEmpty(Plugin.Instance!.Configuration.AuthToken))
            {
                _logger.LogError("No auth token configured");
                throw new InvalidConfigurationException(nameof(PluginConfiguration.AuthToken), "No auth token configured");
            }
            else
            {
                _discogsClient = new DiscogsClient.DiscogsClient(
                    new DiscogsClient.Internal.TokenAuthenticationInformation(Plugin.Instance!.Configuration.AuthToken),
                    Constants.UserAgent);
            }
        }

        /// <inheritdoc />
        public string Name => Constants.Name;

        /// <inheritdoc />
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(AlbumInfo searchInfo, CancellationToken cancellationToken)
        {
            var loggingScope = new AlbumLoggingScope
            {
                MethodName = nameof(GetSearchResults),
                AlbumName = searchInfo.Name,
                AlbumYear = searchInfo.Year,
            };
            _logger.LogInformation("{AlbumScope} Performing search for Album", loggingScope);
            if (searchInfo == null || _discogsClient == null)
            {
                return Enumerable.Empty<RemoteSearchResult>();
            }

            var releaseId = searchInfo.GetReleaseId();
            var searchResults = new List<RemoteSearchResult>();

            if (!string.IsNullOrEmpty(releaseId))
            {
                loggingScope.AlbumReleaseId = releaseId;
                _logger.LogInformation("{AlbumScope} Searching for album by ID", loggingScope);
                var release = await _discogsClient.GetReleaseAsync(int.Parse(releaseId, CultureInfo.InvariantCulture), cancellationToken).ConfigureAwait(false);
                if (release != null)
                {
                    loggingScope.AlbumReleaseId = release.id.ToString(CultureInfo.InvariantCulture);
                    loggingScope.AlbumReleaseMasterId = release.master_id.ToString(CultureInfo.InvariantCulture);

                    _logger.LogInformation("{AlbumScope} Found album", loggingScope);
                    var result = new RemoteSearchResult
                    {
                        Name = release.title,
                        ProductionYear = release.year,
                        ImageUrl = release.thumb
                    };

                    if (release.artists.Length > 0)
                    {
                        result.AlbumArtist = new RemoteSearchResult
                        {
                            SearchProviderName = Name,
                            Name = release.artists[0].name
                        };

                        result.AlbumArtist.SetProviderId(Constants.ProviderIds.Artist, release.artists[0].id.ToString(CultureInfo.InvariantCulture));
                    }

                    result.SetProviderId(Constants.ProviderIds.Album, release.id.ToString(CultureInfo.InvariantCulture));
                    result.SetProviderId(Constants.ProviderIds.AlbumMaster, release.master_id.ToString(CultureInfo.InvariantCulture));

                    searchResults.Add(result);
                }
                else
                {
                    _logger.LogWarning("{AlbumScope} Failed to get response when fetching Release by ID", loggingScope);
                }
            }
            else
            {
                _logger.LogInformation("{AlbumScope} Searching for album by query", loggingScope);
                var artist = searchInfo.GetAlbumArtist();
                var search = new DiscogsClient.Data.Query.DiscogsSearch
                {
                    type = DiscogsClient.Data.Query.DiscogsEntityType.release,
                    release_title = searchInfo.Name
                };

                if (!string.IsNullOrEmpty(artist))
                {
                    search.artist = artist;
                }

                if (searchInfo.Year != null)
                {
                    search.year = searchInfo.Year.Value;
                }

                var discogSearchResults = await _discogsClient.SearchAsync(
                    search,
                    null,
                    cancellationToken).ConfigureAwait(false);

                if (discogSearchResults != null)
                {
                    foreach (var searchResult in discogSearchResults.results)
                    {
                        loggingScope.AlbumReleaseId = searchResult.id.ToString(CultureInfo.InvariantCulture);
                        _logger.LogInformation("{AlbumScope} Found matching release {ReleaseUrl}", loggingScope, searchResult.uri);

                        var release = await _discogsClient.GetReleaseAsync(searchResult.id, cancellationToken).ConfigureAwait(false);
                        if (release != null)
                        {
                            if (release.master_id != 0)
                            {
                                loggingScope.AlbumReleaseMasterId = release.master_id.ToString(CultureInfo.InvariantCulture);
                            }

                            _logger.LogInformation("{AlbumScope} Creating search result for release", loggingScope);
                            var result = new RemoteSearchResult
                            {
                                Name = release.title,
                                ProductionYear = release.year,
                                ImageUrl = release.thumb
                            };

                            if (release.artists.Length > 0)
                            {
                                result.AlbumArtist = new RemoteSearchResult
                                {
                                    SearchProviderName = Name,
                                    Name = release.artists[0].name
                                };

                                result.AlbumArtist.SetProviderId(Constants.ProviderIds.Artist, release.artists[0].id.ToString(CultureInfo.InvariantCulture));
                            }

                            result.SetProviderId(Constants.ProviderIds.Album, release.id.ToString(CultureInfo.InvariantCulture));
                            if (release.master_id != 0)
                            {
                                result.SetProviderId(Constants.ProviderIds.AlbumMaster, release.master_id.ToString(CultureInfo.InvariantCulture));
                            }

                            searchResults.Add(result);
                        }
                        else
                        {
                            _logger.LogWarning("{AlbumScope} Failed to get response when fetching Release by ID", loggingScope);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("{AlbumScope} Failed to get response when searching for Release", loggingScope);
                }
            }

            return searchResults;
        }

        /// <inheritdoc />
        public async Task<MetadataResult<MusicAlbum>> GetMetadata(AlbumInfo info, CancellationToken cancellationToken)
        {
            var loggingScope = new AlbumLoggingScope
            {
                MethodName = nameof(GetMetadata),
                AlbumName = info.Name,
                AlbumYear = info.Year
            };

            var result = new MetadataResult<MusicAlbum>
            {
                Item = new MusicAlbum()
            };

            var releaseId = info.GetReleaseId();
            var releaseMasterId = info.GetReleaseMasterId();
            if (releaseMasterId == "0")
            {
                releaseMasterId = null;
            }

            var artistId = info.GetDiscogsArtistId();

            loggingScope.AlbumReleaseId = releaseId;
            loggingScope.AlbumReleaseMasterId = releaseMasterId;
            loggingScope.AlbumArtistId = artistId;

            _logger.LogInformation("{AlbumScope} Getting metadata for Album", loggingScope);

            DiscogsClient.Data.Result.DiscogsRelease? release = null;

            if (!string.IsNullOrEmpty(releaseMasterId))
            {
                _logger.LogInformation("{AlbumScope} Getting master release", loggingScope);
                var master = await _discogsClient.GetMasterAsync(int.Parse(releaseMasterId, CultureInfo.InvariantCulture), cancellationToken).ConfigureAwait(false);
                if (master != null)
                {
                    release = await _discogsClient.GetReleaseAsync(master.main_release, cancellationToken).ConfigureAwait(false);
                }
            }

            if (release == null)
            {
                if (!string.IsNullOrEmpty(releaseId))
                {
                    _logger.LogInformation("{AlbumScope} Getting release", loggingScope);
                    release = await _discogsClient.GetReleaseAsync(int.Parse(releaseId, CultureInfo.InvariantCulture), cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    _logger.LogInformation("{AlbumScope} Album does not have a Release ID or Master ID", loggingScope);

                    if (string.IsNullOrEmpty(artistId))
                    {
                        _logger.LogInformation("{AlbumScope} Album does not have an Artist ID", loggingScope);
                        return result;
                    }

                    _logger.LogInformation("{AlbumScope} Searching for release", loggingScope);

                    var artistReleases = await _discogsClient.GetArtistReleaseAsync(
                        int.Parse(artistId, CultureInfo.InvariantCulture),
                        null,
                        null,
                        cancellationToken).ConfigureAwait(false);
                    if (artistReleases == null)
                    {
                        _logger.LogInformation("{AlbumScope} Failed to get Artist releases", loggingScope);
                        return result;
                    }

                    if (artistReleases.releases == null)
                    {
                        _logger.LogInformation("{AlbumScope} Failed to get Artist releases. No releases.", loggingScope);
                        return result;
                    }

                    var artistRelease = artistReleases.releases.FirstOrDefault(x => x.title.Equals(info.Name, StringComparison.OrdinalIgnoreCase));
                    if (artistRelease == null)
                    {
                        _logger.LogInformation("{AlbumScope} Failed to find matching Artist release", loggingScope);
                        return result;
                    }

                    release = await _discogsClient.GetReleaseAsync(artistRelease.main_release, cancellationToken).ConfigureAwait(false);
                }
            }

            if (release != null)
            {
                result.Item.Name = release.title;
                result.Item.ProductionYear = release.year;
                result.Item.ExternalId = release.uri;
                result.HasMetadata = true;
                if (release.genres != null)
                {
                    foreach (var genre in release.genres)
                    {
                        if (!string.IsNullOrEmpty(genre))
                        {
                            result.Item.AddGenre(genre);
                        }
                    }
                }

                if (release.images != null)
                {
                    foreach (var image in release.images)
                    {
                        result.RemoteImages.Add((image.uri, ImageType.Primary));
                    }
                }

                result.Item.SetProviderId(Constants.ProviderIds.Album, release.id.ToString(CultureInfo.InvariantCulture));

                if (release.master_id != 0)
                {
                    result.Item.SetProviderId(Constants.ProviderIds.AlbumMaster, release.master_id.ToString(CultureInfo.InvariantCulture));
                }

                result.Item.SetProviderId(Constants.ProviderIds.Artist, artistId);

                return result;
            }
            else
            {
                _logger.LogWarning("{AlbumScope} Failed to fetch release for Album", loggingScope);
                return result;
            }
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[ImageUrl={Url}] Getting Album Image Response", url);

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("User-Agent", Constants.UserAgent);

            using var client = Plugin.Instance!.HttpClientFactory.CreateClient();

            var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

            return response;
        }

        /// <inheritdoc />
        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
            => new List<ImageType>
            {
                ImageType.Primary,
                ImageType.Backdrop,
            };

        /// <inheritdoc />
        public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
        {
            var images = new List<RemoteImageInfo>();

            var loggingScope = new AlbumLoggingScope
            {
                MethodName = nameof(GetImages),
                AlbumName = item.Name
            };

            _logger.LogWarning("{AlbumScope} Getting Album images", loggingScope);

            if (item is not MusicAlbum album)
            {
                _logger.LogWarning("Invalid type for Album Get Images");
                return images;
            }

            var releaseId = album.GetReleaseId();
            loggingScope.AlbumReleaseId = releaseId;
            if (string.IsNullOrEmpty(releaseId))
            {
                _logger.LogWarning("{AlbumScope} Album does not have an ID", loggingScope);
                return images;
            }

            _logger.LogInformation("{AlbumScope} Getting artist", loggingScope);
            var discogsRelease = await _discogsClient.GetReleaseAsync(int.Parse(releaseId, CultureInfo.InvariantCulture), cancellationToken).ConfigureAwait(false);
            if (discogsRelease == null)
            {
                _logger.LogWarning("{AlbumScope} Failed to get Album", loggingScope);
                return images;
            }

            if (discogsRelease.images == null)
            {
                _logger.LogWarning("{AlbumScope} Failed to get Album images. No images found", loggingScope);
                return images;
            }

            var primaryImage = discogsRelease.images[0];
            images.Add(new RemoteImageInfo
            {
                ProviderName = Constants.Name,
                Url = primaryImage.uri,
                Type = ImageType.Primary
            });

            if (discogsRelease.images.Length > 1)
            {
                foreach (var image in discogsRelease.images.Skip(1))
                {
                    images.Add(new RemoteImageInfo
                    {
                        ProviderName = Constants.Name,
                        Url = image.uri,
                        Type = ImageType.Backdrop
                    });
                }
            }

            return images;
        }

        /// <inheritdoc />
        public bool Supports(BaseItem item) => item is MusicAlbum;
    }
}
