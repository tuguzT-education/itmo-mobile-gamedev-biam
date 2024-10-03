using ProceduralToolkit;

namespace BiaM
{
    public static class DirectionsExtensions
    {
        public static Directions Inverse(this Directions directions)
        {
            var result = Directions.None;
            if (directions.HasFlag(Directions.Left)) result |= Directions.Right;
            if (directions.HasFlag(Directions.Right)) result |= Directions.Left;
            if (directions.HasFlag(Directions.Up)) result |= Directions.Down;
            if (directions.HasFlag(Directions.Down)) result |= Directions.Up;
            if (directions.HasFlag(Directions.Forward)) result |= Directions.Back;
            if (directions.HasFlag(Directions.Back)) result |= Directions.Forward;
            return result;
        }
    }
}