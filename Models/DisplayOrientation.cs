namespace MonitorPresetManager.Models
{
    /// <summary>
    /// Enumeration for display orientation
    /// </summary>
    public enum DisplayOrientation
    {
        /// <summary>
        /// Standard landscape orientation (0 degrees)
        /// </summary>
        Landscape = 0,

        /// <summary>
        /// Portrait orientation (90 degrees clockwise)
        /// </summary>
        Portrait = 1,

        /// <summary>
        /// Flipped landscape orientation (180 degrees)
        /// </summary>
        LandscapeFlipped = 2,

        /// <summary>
        /// Flipped portrait orientation (270 degrees clockwise)
        /// </summary>
        PortraitFlipped = 3
    }
}