using System;

namespace VeroEngine.Core.Mathematics;

public struct Util
{
    public static double Deg2Rad(double deg)
    {
        return deg * Math.PI / 180;
    }
    
    public static Vector3 Deg2Rad(Vector3 deg)
    {
        return new((float)Deg2Rad(deg.X), (float)Deg2Rad(deg.Y), (float)Deg2Rad(deg.Z));
    }
    
    public static Vector3 Rad2Deg(Vector3 rad)
    {
        return new((float)Rad2Deg(rad.X), (float)Rad2Deg(rad.Y), (float)Rad2Deg(rad.Z));
    }
    
    public static double Rad2Deg(double rad)
    {
        return rad * 180.0 / Math.PI;
    }
}