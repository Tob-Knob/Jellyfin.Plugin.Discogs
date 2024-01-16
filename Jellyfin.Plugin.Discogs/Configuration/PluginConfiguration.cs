using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Discogs.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
    }

    /// <summary>
    /// Gets or sets the Pesonal access token.
    /// </summary>
    public string AuthToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets the Discogs url.
    /// </summary>
    public string Url => "https://www.discogs.com";

    /// <summary>
    /// Gets or sets a value indicating whether to replace the artist name.
    /// </summary>
    public bool ReplaceArtistName { get; set; }
}
