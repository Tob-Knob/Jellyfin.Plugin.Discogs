namespace Jellyfin.Plugin.Discogs.Logging
{
    internal class ArtistLoggingScope
    {
        public string? ArtistName { get; set; }

        public string? ArtistId { get; set; }

        public override string ToString()
        {
            return $"[ArtistName={ArtistName}] [ArtistId={ArtistId}]";
        }
    }
}
