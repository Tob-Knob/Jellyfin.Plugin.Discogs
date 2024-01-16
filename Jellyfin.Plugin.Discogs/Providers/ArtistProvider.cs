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
    /// Discogs Artist provider.
    /// </summary>
    public class ArtistProvider : IRemoteMetadataProvider<MusicArtist, ArtistInfo>, IRemoteImageProvider
    {
        private readonly ILogger<ArtistProvider> _logger;
        private readonly DiscogsClient.DiscogsClient _discogsClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtistProvider"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="Logger{AlbumProvider}"/> interface.</param>
        public ArtistProvider(ILogger<ArtistProvider> logger)
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
        public string Name => "Discogs";

        /// <inheritdoc />
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(ArtistInfo searchInfo, CancellationToken cancellationToken)
        {
            if (searchInfo == null || _discogsClient == null)
            {
                return Enumerable.Empty<RemoteSearchResult>();
            }

            var loggingScope = new ArtistLoggingScope
            {
                ArtistName = searchInfo.Name
            };

            _logger.LogInformation("{ArtistScope} Performing search for Artist", loggingScope);

            var artistId = searchInfo.GetArtistId();
            var searchResults = new List<RemoteSearchResult>();

            if (!string.IsNullOrEmpty(artistId))
            {
                loggingScope.ArtistId = artistId;
                _logger.LogInformation("{ArtistScope} Searching for Artist by ID", loggingScope);

                var artist = await _discogsClient.GetArtistAsync(int.Parse(artistId, CultureInfo.InvariantCulture), cancellationToken).ConfigureAwait(false);
                if (artist != null)
                {
                    var result = new RemoteSearchResult
                    {
                        Name = artist.name,
                    };

                    if (artist.images.Length > 0)
                    {
                        result.ImageUrl = artist.images[0].uri;
                    }

                    result.SetProviderId(Constants.ProviderIds.Artist, artistId.ToString(CultureInfo.InvariantCulture));

                    searchResults.Add(result);
                }
                else
                {
                    _logger.LogWarning("{ArtistScope} Failed to get response when fetching Artist by ID", loggingScope);
                }
            }
            else
            {
                _logger.LogInformation("{ArtistScope} Searching for Artist by query", loggingScope);

                var search = new DiscogsClient.Data.Query.DiscogsSearch
                {
                    type = DiscogsClient.Data.Query.DiscogsEntityType.artist,
                    query = searchInfo.Name
                };

                var discogSearchResults = await _discogsClient.SearchAsync(
                    search,
                    null,
                    cancellationToken).ConfigureAwait(false);

                if (discogSearchResults != null)
                {
                    foreach (var searchResult in discogSearchResults.results)
                    {
                        loggingScope.ArtistId = searchResult.id.ToString(CultureInfo.InvariantCulture);
                        _logger.LogInformation("{ArtistScope} Found matching Artist {ArtistUrl}", loggingScope, searchResult.uri);
                        var result = new RemoteSearchResult
                        {
                            Name = searchResult.title,
                            ImageUrl = searchResult.thumb
                        };

                        result.SetProviderId(Constants.ProviderIds.Artist, searchResult.id.ToString(CultureInfo.InvariantCulture));

                        searchResults.Add(result);
                    }
                }
                else
                {
                    _logger.LogWarning("{ArtistScope} Failed to get response when querying for Artist", loggingScope);
                }
            }

            return searchResults;
        }

        /// <inheritdoc />
        public async Task<MetadataResult<MusicArtist>> GetMetadata(ArtistInfo info, CancellationToken cancellationToken)
        {
            var loggingScope = new ArtistLoggingScope
            {
                ArtistName = info.Name
            };
            var result = new MetadataResult<MusicArtist>
            {
                Item = new MusicArtist()
            };

            var artistId = info.GetArtistId();
            loggingScope.ArtistId = artistId;

            _logger.LogInformation("{ArtistScope} Getting metadata for Artist", loggingScope);

            DiscogsClient.Data.Result.DiscogsArtist? artist = null;

            if (string.IsNullOrEmpty(artistId))
            {
                _logger.LogWarning("{ArtistScope} Artist does not have an ID", loggingScope);

                _logger.LogInformation("{ArtistScope} Searching for Artist: {ArtistNameQuery}", loggingScope, info.Name);

                var search = new DiscogsClient.Data.Query.DiscogsSearch
                {
                    type = DiscogsClient.Data.Query.DiscogsEntityType.artist,
                    query = info.Name
                };

                var discogSearchResults = await _discogsClient.SearchAsync(
                    search,
                    null,
                    cancellationToken).ConfigureAwait(false);

                if (discogSearchResults != null)
                {
                    if (discogSearchResults.results != null)
                    {
                        var artistResult = discogSearchResults.results.FirstOrDefault();
                        if (artistResult != null)
                        {
                            loggingScope.ArtistId = artistResult.id.ToString(CultureInfo.InvariantCulture);
                            _logger.LogInformation("{ArtistScope} Getting Artist", loggingScope);
                            artist = await _discogsClient.GetArtistAsync(artistResult.id, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("{ArtistScope} Failed to find Artist", loggingScope);
                        return result;
                    }
                }
                else
                {
                    _logger.LogWarning("{ArtistSCope} Failed to get response when querying for Artist", loggingScope);
                    return result;
                }
            }
            else
            {
                _logger.LogInformation("{ArtistScope} Getting Artist", loggingScope);
                artist = await _discogsClient.GetArtistAsync(int.Parse(artistId, CultureInfo.InvariantCulture), cancellationToken).ConfigureAwait(false);
            }

            if (artist == null)
            {
                _logger.LogWarning("{ArtistScope} Failed to fetch Artist", loggingScope);
                return result;
            }
            else
            {
                _logger.LogInformation("{ArtistScope} Updating metadata", loggingScope);
                if (Plugin.Instance?.Configuration.ReplaceArtistName ?? false)
                {
                    result.Item.Name = artist.name;
                }

                // var genres = await GetArtistGenres(artist.id, loggingScope, cancellationToken).ConfigureAwait(false);
                // _logger.LogInformation("{ArtistScope} Retrieved genres: {ArtistGenres}", loggingScope, string.Join(",", genres));
                // foreach (var genre in genres)
                // {
                //    result.Item.AddGenre(genre);
                // }

                result.Item.ExternalId = artist.uri;
                result.Item.Overview = artist.profile;
                result.HasMetadata = true;

                result.Item.SetProviderId(Constants.ProviderIds.Artist, artist.id.ToString(CultureInfo.InvariantCulture));
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[ImageUrl={Url}] Getting Artist Image Response", url);

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("User-Agent", Constants.UserAgent);

            using var client = Plugin.Instance!.HttpClientFactory.CreateClient();

            var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

            return response;
        }

        private async Task<string[]> GetArtistGenres(int artistId, ArtistLoggingScope loggingScope, CancellationToken cancellationToken)
        {
            var genres = new HashSet<string>();
            _logger.LogInformation("{ArtistScope} Getting Artist genres", loggingScope);

            var artistReleases = await _discogsClient.GetArtistReleaseAsync(artistId, null, null, cancellationToken).ConfigureAwait(false);
            if (artistReleases == null)
            {
                _logger.LogWarning("{ArtistScope} Failed to get Artist genres", loggingScope);
                return genres.ToArray();
            }

            var releaseIds = artistReleases.GetResults().Where(x => x.role.Equals("Main", System.StringComparison.OrdinalIgnoreCase)).Select(x => x.id).Distinct();
            foreach (var releaseId in releaseIds)
            {
                _logger.LogWarning("{ArtistScope} Getting Artist Release {ReleaseId}", loggingScope, releaseId);
                var release = await _discogsClient.GetReleaseAsync(releaseId, cancellationToken).ConfigureAwait(false);
                if (release == null)
                {
                    _logger.LogWarning("{ArtistScope} Failed to get Artist Release {ReleaseId}", loggingScope, releaseId);
                }
                else
                {
                    _logger.LogInformation("{ArtistScope} Retrieved Artist Release {ReleaseId}", loggingScope, releaseId);
                    if (release.genres != null)
                    {
                        foreach (var genre in release.genres)
                        {
                            if (!genres.Contains(genre))
                            {
                                genres.Add(genre);
                            }
                        }
                    }
                }
            }

            return genres.ToArray();
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

            var loggingScope = new ArtistLoggingScope
            {
                ArtistName = item.Name
            };

            _logger.LogWarning("{ArtistScope} Getting Artist images", loggingScope);

            if (item is not MusicArtist artist)
            {
                _logger.LogWarning("Invalid type for Artist Get Images");
                return images;
            }

            var artistId = artist.GetArtistId();
            loggingScope.ArtistId = artistId;
            if (string.IsNullOrEmpty(artistId))
            {
                _logger.LogWarning("{ArtistScope} Artist does not have an ID", loggingScope);
                return images;
            }

            _logger.LogInformation("{ArtistScope} Getting artist", loggingScope);
            var discogsArtist = await _discogsClient.GetArtistAsync(int.Parse(artistId, CultureInfo.InvariantCulture), cancellationToken).ConfigureAwait(false);
            if (discogsArtist == null)
            {
                _logger.LogWarning("{ArtistScope} Failed to get Artist", loggingScope);
                return images;
            }

            if (discogsArtist.images == null)
            {
                _logger.LogWarning("{ArtistScope} Failed to get Artist images. No images found", loggingScope);
                return images;
            }

            var primaryImage = discogsArtist.images[0];
            images.Add(new RemoteImageInfo
            {
                ProviderName = Constants.Name,
                Url = primaryImage.uri,
                Type = ImageType.Primary
            });

            if (discogsArtist.images.Length > 1)
            {
                foreach (var image in discogsArtist.images.Skip(1))
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
        public bool Supports(BaseItem item) => item is MusicArtist;
    }
}
