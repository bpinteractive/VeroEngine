using System;
using OpenTK.Mathematics;

namespace VeroEngine.Core.Mathematics;

public struct Util
{
    public static Quaternion RadiansToQuaternion(Vector3 euler)
    {
        float roll = euler.X;
        float pitch = euler.Y;
        float yaw = euler.Z;
        
        float halfRoll = roll * 0.5f;
        float halfPitch = pitch * 0.5f;
        float halfYaw = yaw * 0.5f;
        
        float sinRoll = MathF.Sin(halfRoll);
        float cosRoll = MathF.Cos(halfRoll);
        float sinPitch = MathF.Sin(halfPitch);
        float cosPitch = MathF.Cos(halfPitch);
        float sinYaw = MathF.Sin(halfYaw);
        float cosYaw = MathF.Cos(halfYaw);
        
        float x = sinRoll * cosPitch * cosYaw - cosRoll * sinPitch * sinYaw;
        float y = cosRoll * sinPitch * cosYaw + sinRoll * cosPitch * sinYaw;
        float z = cosRoll * cosPitch * sinYaw - sinRoll * sinPitch * cosYaw;
        float w = cosRoll * cosPitch * cosYaw + sinRoll * sinPitch * sinYaw;
        
        return new Quaternion(x, y, z, w);
    }
    public static double Deg2Rad(double deg)
    {
        return deg * Math.PI / 180;
    }

    public static Vector3 Deg2Rad(Vector3 deg)
    {
        return new Vector3((float)Deg2Rad(deg.X), (float)Deg2Rad(deg.Y), (float)Deg2Rad(deg.Z));
    }

    public static Vector3 Rad2Deg(Vector3 rad)
    {
        return new Vector3((float)Rad2Deg(rad.X), (float)Rad2Deg(rad.Y), (float)Rad2Deg(rad.Z));
    }

    public static double Rad2Deg(double rad)
    {
        return rad * 180.0 / Math.PI;
    }
}