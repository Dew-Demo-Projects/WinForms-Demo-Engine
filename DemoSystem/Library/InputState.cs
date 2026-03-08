namespace DemoSystem.Library
{
    /// <summary>One frame of input data.</summary>
    public readonly struct InputState
    {
        public static readonly InputState Empty = default;

        public PointF Mouse { get; init; } // screen coords
        public bool LeftDown { get; init; }
        public bool RightDown { get; init; }
        public int WheelDelta { get; init; } // accumulated since last frame
    }
}