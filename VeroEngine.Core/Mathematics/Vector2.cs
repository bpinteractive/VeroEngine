using System;

namespace VeroEngine.Core.Mathematics;

public class Vector2
{
	public Vector2() : this(0, 0)
	{
	}

	public Vector2(float x, float y)
	{
		X = x;
		Y = y;
	}


	public float X { get; set; }
	public float Y { get; set; }

	// Magnitude of the vector
	public float Magnitude => (float)Math.Sqrt(X * X + Y * Y);

	// Normalize the vector
	public Vector2 Normalized => Magnitude > 0 ? new Vector2(X / Magnitude, Y / Magnitude) : new Vector2(0, 0);

	public OpenTK.Mathematics.Vector2 ToOpenTK()
	{
		return new OpenTK.Mathematics.Vector2(X, Y);
	}

	public static Vector2 FromSystem(System.Numerics.Vector2 from)
	{
		return new Vector2(from.X, from.Y);
	}

	public System.Numerics.Vector2 ToSystem()
	{
		return new System.Numerics.Vector2(X, Y);
	}

	// Dot product
	public static float Dot(Vector2 a, Vector2 b)
	{
		return a.X * b.X + a.Y * b.Y;
	}

	// Add two vectors
	public static Vector2 operator +(Vector2 a, Vector2 b)
	{
		return new Vector2(a.X + b.X, a.Y + b.Y);
	}

	// Subtract two vectors
	public static Vector2 operator -(Vector2 a, Vector2 b)
	{
		return new Vector2(a.X - b.X, a.Y - b.Y);
	}

	// Multiply a vector by a scalar
	public static Vector2 operator *(Vector2 v, float scalar)
	{
		return new Vector2(v.X * scalar, v.Y * scalar);
	}

	// Divide a vector by a scalar
	public static Vector2 operator /(Vector2 v, float scalar)
	{
		return new Vector2(v.X / scalar, v.Y / scalar);
	}

	// Override ToString for easy display
	public override string ToString()
	{
		return $"({X}, {Y})";
	}
}