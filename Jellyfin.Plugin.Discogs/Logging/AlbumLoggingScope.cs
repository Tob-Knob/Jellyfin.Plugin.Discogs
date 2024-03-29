using System.Collections.Generic;

namespace Jellyfin.Plugin.Discogs.Logging
{
    internal class AlbumLoggingScope
    {
        public string? AlbumName { get; set; }

        public int? AlbumYear { get; set; }

        public string? AlbumReleaseId { get; set; }

        public string? AlbumReleaseMasterId { get; set; }

        public string? AlbumArtistId { get; set; }

        public string MethodName { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"[MethodName={MethodName}] [AlbumName={AlbumName}] [AlbumYear={AlbumYear}] [AlbumReleaseId={AlbumReleaseId}] [AlbumReleaseMasterId={AlbumReleaseMasterId}] [AlbumArtist={AlbumArtistId}]";
        }
    }
}
