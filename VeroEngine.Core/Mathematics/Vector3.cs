using System;
using OpenTK.Mathematics;

namespace VeroEngine.Core.Mathematics;

public class Vector3
{
    
    public static Vector3 Zero = new Vector3(0, 0, 0);
    public static Vector3 One = new Vector3(1, 1, 1);
    public Vector3() : this(0, 0, 0)
    {
    }

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public float Magnitude => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

    public Vector3 Normalize()
    {
        var magnitude = Magnitude;
        return magnitude > 0 ? this / magnitude : new Vector3();
    }

    public static float Dot(Vector3 v1, Vector3 v2)
    {
        return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
    }

    public static Vector3 Cross(Vector3 v1, Vector3 v2)
    {
        return new Vector3(
            v1.Y * v2.Z - v1.Z * v2.Y,
            v1.Z * v2.X - v1.X * v2.Z,
            v1.X * v2.Y - v1.Y * v2.X
        );
    }

    public Matrix4 GetRotationMatrix()
    {
        return Matrix4.CreateFromQuaternion(Util.RadiansToQuaternion(this));
    }


    public Matrix4 GetTranslationMatrix()
    {
        return Matrix4.CreateTranslation(X, Y, Z);
    }

    public OpenTK.Mathematics.Vector3 ToOpenTK()
    {
        return new OpenTK.Mathematics.Vector3(X, Y, Z);
    }

    public static Vector3 FromSystem(System.Numerics.Vector3 from)
    {
        return new Vector3(from.X, from.Y, from.Z);
    }

    public System.Numerics.Vector3 ToSystem()
    {
        return new System.Numerics.Vector3(X, Y, Z);
    }

    public static Vector3 operator +(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
    }

    public static Vector3 operator -(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
    }

    public static Vector3 operator *(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
    }

    public static Vector3 operator /(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);
    }

    public static Vector3 operator +(Vector3 v, float scalar)
    {
        return new Vector3(v.X + scalar, v.Y + scalar, v.Z + scalar);
    }

    public static Vector3 operator -(Vector3 v, float scalar)
    {
        return new Vector3(v.X - scalar, v.Y - scalar, v.Z - scalar);
    }

    public static Vector3 operator *(Vector3 v, float scalar)
    {
        return new Vector3(v.X * scalar, v.Y * scalar, v.Z * scalar);
    }

    public static Vector3 operator /(Vector3 v, float scalar)
    {
        return new Vector3(v.X / scalar, v.Y / scalar, v.Z / scalar);
    }

    public static bool operator ==(Vector3 v1, Vector3 v2)
    {
        return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
    }

    public static bool operator !=(Vector3 v1, Vector3 v2)
    {
        return !(v1 == v2);
    }

    public override string ToString()
    {
        return $"Vector3({X}, {Y}, {Z})";
    }

    public override bool Equals(object obj)
    {
        if (obj is Vector3 other) return this == other;
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public Vector3 Transform(Matrix4 matrix)
    {
        var x = X * matrix.M11 + Y * matrix.M21 + Z * matrix.M31 + matrix.M41;
        var y = X * matrix.M12 + Y * matrix.M22 + Z * matrix.M32 + matrix.M42;
        var z = X * matrix.M13 + Y * matrix.M23 + Z * matrix.M33 + matrix.M43;
        return new Vector3(x, y, z);
    }
}