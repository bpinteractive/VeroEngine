namespace VeroEngine.Core.Mathematics;

public class Vector2
{
    public float X { get; set; }
    public float Y { get; set; }

    public Vector2() : this(0, 0) { }

    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    // Magnitude of the vector
    public float Magnitude => (float)System.Math.Sqrt(X * X + Y * Y);

    // Normalize the vector
    public Vector2 Normalized => Magnitude > 0 ? new Vector2(X / Magnitude, Y / Magnitude) : new Vector2(0, 0);

    // Dot product
    public static float Dot(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;

    // Add two vectors
    public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);

    // Subtract two vectors
    public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);

    // Multiply a vector by a scalar
    public static Vector2 operator *(Vector2 v, float scalar) => new Vector2(v.X * scalar, v.Y * scalar);

    // Divide a vector by a scalar
    public static Vector2 operator /(Vector2 v, float scalar) => new Vector2(v.X / scalar, v.Y / scalar);

    // Override ToString for easy display
    public override string ToString() => $"({X}, {Y})";
}