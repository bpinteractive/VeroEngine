namespace VeroEngine.Core.Mathematics;

public class AABB
{
	// Constructors
	public AABB(Vector3 min, Vector3 max)
	{
		Min = min;
		Max = max;
	}

	public AABB()
	{
		Min = Vector3.Zero;
		Max = Vector3.Zero;
	}

	public Vector3 Min { get; set; }
	public Vector3 Max { get; set; }

	// Override ToString for better debugging
	public override string ToString()
	{
		return "(" + Min + ", " + Max + ")";
	}

	// Method to get the 8 corners of the AABB in 3D space
	public Vector3[] GetCorners()
	{
		return new[]
		{
			new Vector3(Min.X, Min.Y, Min.Z), // Bottom-left-front
			new Vector3(Max.X, Min.Y, Min.Z), // Bottom-right-front
			new Vector3(Min.X, Max.Y, Min.Z), // Top-left-front
			new Vector3(Max.X, Max.Y, Min.Z), // Top-right-front
			new Vector3(Min.X, Min.Y, Max.Z), // Bottom-left-back
			new Vector3(Max.X, Min.Y, Max.Z), // Bottom-right-back
			new Vector3(Min.X, Max.Y, Max.Z), // Top-left-back
			new Vector3(Max.X, Max.Y, Max.Z) // Top-right-back
		};
	}

	// Check if a point is inside the AABB
	public bool Contains(Vector3 point)
	{
		return point.X >= Min.X && point.X <= Max.X &&
		       point.Y >= Min.Y && point.Y <= Max.Y &&
		       point.Z >= Min.Z && point.Z <= Max.Z;
	}

	// Expand the AABB to include another point
	public void ExpandToInclude(Vector3 point)
	{
		Min = Vector3.ComponentMin(Min, point);
		Max = Vector3.ComponentMax(Max, point);
	}

	// Merge another AABB into this one (expands to include both)
	public void Merge(AABB other)
	{
		Min = Vector3.ComponentMin(Min, other.Min);
		Max = Vector3.ComponentMax(Max, other.Max);
	}

	// Check for intersection with another AABB
	public bool Intersects(AABB other)
	{
		return Min.X <= other.Max.X && Max.X >= other.Min.X &&
		       Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
		       Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;
	}

	// Calculate the size of the AABB
	public Vector3 GetSize()
	{
		return Max - Min;
	}

	// Calculate the center of the AABB
	public Vector3 GetCenter()
	{
		return (Min + Max) / 2f;
	}
}