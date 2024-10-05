using System;
using VeroEngine.Core.Mathematics;
using Matrix4 = OpenTK.Mathematics.Matrix4;

namespace VeroEngine.Core.Rendering;

public class Camera
{
    private float _aspectRatio;
    private float _farClip;
    private float _fieldOfView; // In radians
    private float _nearClip;
    private float _pitch; // Rotation around the X-axis (in radians)
    private Vector3 _position;
    private float _roll; // Rotation around the Z-axis (in radians)
    private Vector3 _up; // The up direction
    private float _yaw; // Rotation around the Y-axis (in radians)

    public Camera(Vector3 position, float yaw, float pitch, float roll, Vector3 up, float fieldOfView,
        float aspectRatio, float nearClip, float farClip)
    {
        _position = position;
        _yaw = yaw;
        _pitch = pitch;
        _roll = roll;
        _up = up.Normalize(); // Ensure the up vector is normalized
        _fieldOfView = fieldOfView;
        _aspectRatio = aspectRatio;
        _nearClip = nearClip;
        _farClip = farClip;
    }

    public Matrix4 GetProjectionMatrix()
    {
        // Perspective projection matrix
        return Matrix4.CreatePerspectiveFieldOfView(_fieldOfView, _aspectRatio, _nearClip, _farClip);
    }

    public Matrix4 GetViewMatrix()
    {
        // Calculate the front vector based on yaw (Y-axis), pitch (X-axis)
        var front = new Vector3(
            MathF.Cos(_yaw) * MathF.Cos(_pitch),
            MathF.Sin(_pitch),
            MathF.Sin(_yaw) * MathF.Cos(_pitch)
        ).Normalize();

        // Calculate the right and up vectors using cross product
        var right = Vector3.Cross(front, _up).Normalize();
        var up = Vector3.Cross(right, front).Normalize();

        // Apply roll to right and up vectors
        var rollMatrix = Matrix4.CreateFromAxisAngle(front.ToOpenTK(), _roll);
        right = right.Transform(rollMatrix).Normalize();
        up = up.Transform(rollMatrix).Normalize();

        // Return the LookAt matrix using the updated vectors
        return Matrix4.LookAt(_position.ToOpenTK(), _position.ToOpenTK() + front.ToOpenTK(), up.ToOpenTK());
    }


    public Vector3 GetFront()
    {
        return new Vector3(
            MathF.Cos(_yaw) * MathF.Cos(_pitch),
            MathF.Sin(_pitch),
            MathF.Sin(_yaw) * MathF.Cos(_pitch)
        ).Normalize();
    }

    public Vector3 GetRight()
    {
        var front = new Vector3(
            MathF.Cos(_yaw) * MathF.Cos(_pitch),
            MathF.Sin(_pitch),
            MathF.Sin(_yaw) * MathF.Cos(_pitch)
        ).Normalize();
        
        var right = Vector3.Cross(front, _up).Normalize();
        return right;
    }

    public Vector3 GetLeft()
    {
        return new Vector3(0, 0, 0) - GetRight();
    }


    public void SetRotation(float yaw, float pitch, float roll)
    {
        _yaw = pitch; // Y-axis
        _pitch = roll; // Z-axis
        _roll = yaw; // X-axis
    }

    public void SetRotation(Vector3 rot)
    {
        _yaw = -rot.Y;   // Yaw on Y-axis
        _pitch = -rot.Z; // Pitch on X-axis
        _roll = -rot.X;  // Roll on Z-axis
    }


    public void UpdatePosition(Vector3 position)
    {
        _position = position;
    }

    public Vector3 GetPosition()
    {
        return _position;
    }

    public Vector3 GetRotation()
    {
        return new Vector3(-_roll, -_yaw, -_pitch);
    }

    public void SetUp(Vector3 up)
    {
        _up = up.Normalize();
    }

    public void SetAspectRatio(float aspectRatio)
    {
        _aspectRatio = aspectRatio;
    }

    public void SetFieldOfView(float fieldOfView)
    {
        _fieldOfView = fieldOfView;
    }

    public float GetFieldOfView()
    {
        return _fieldOfView;
    }

    public void SetNearClip(float nearClip)
    {
        _nearClip = nearClip;
    }

    public void SetFarClip(float farClip)
    {
        _farClip = farClip;
    }
}