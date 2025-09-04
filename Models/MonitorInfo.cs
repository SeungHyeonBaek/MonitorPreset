namespace MonitorPresetManager.Models
{
    /// <summary>
    /// Class representing configuration information for an individual monitor
    /// </summary>
    public class MonitorInfo
    {
        /// <summary>Device name (e.g., \\.\DISPLAY1)</summary>
        public string DeviceName { get; set; } = string.Empty;
        
        /// <summary>Resolution width</summary>
        public int Width { get; set; }
        
        /// <summary>Resolution height</summary>
        public int Height { get; set; }
        
        /// <summary>X coordinate position</summary>
        public int PositionX { get; set; }
        
        /// <summary>Y coordinate position</summary>
        public int PositionY { get; set; }
        
        /// <summary>Whether this is the primary monitor</summary>
        public bool IsPrimary { get; set; }
        
        /// <summary>Display orientation</summary>
        public DisplayOrientation Orientation { get; set; } = DisplayOrientation.Landscape;
        
        /// <summary>
        /// String representation of monitor information
        /// </summary>
        /// <returns>Monitor information summary</returns>
        public override string ToString()
        {
            return $"{DeviceName}: {Width}x{Height} at ({PositionX},{PositionY}) {(IsPrimary ? "[Primary]" : "")}";
        }
        
        /// <summary>
        /// Compare if two monitor information objects are identical
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>Whether they are identical</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not MonitorInfo other) return false;
            
            return DeviceName == other.DeviceName &&
                   Width == other.Width &&
                   Height == other.Height &&
                   PositionX == other.PositionX &&
                   PositionY == other.PositionY &&
                   IsPrimary == other.IsPrimary &&
                   Orientation == other.Orientation;
        }
        
        /// <summary>
        /// Generate hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(DeviceName, Width, Height, PositionX, PositionY, IsPrimary, Orientation);
        }
    }
}