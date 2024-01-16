namespace Jellyfin.Plugin.Discogs
{
    internal class Constants
    {
        public const string Name = "Discogs";
        public const string UserAgent = "Jellyfin.Plugin.Discogs https://github.com/Tob-Knob/Jellyfin.Plugin.Discogs";

        public class ProviderIds
        {
            private const string BaseProviderId = "Discogs-";

            public const string Album = BaseProviderId + "Album";
            public const string AlbumMaster = BaseProviderId + "AlbumMaster";
            public const string Artist = BaseProviderId + "Artist";
            public const string AlbumArtist = BaseProviderId + "AlbumArtist";
        }
    }
}
