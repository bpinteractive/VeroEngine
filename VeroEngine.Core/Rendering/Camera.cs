using System;
using OpenTK.Mathematics;
using VeroEngine.Core.Mathematics;
using Matrix4 = OpenTK.Mathematics.Matrix4;
using Vector2 = VeroEngine.Core.Mathematics.Vector2;
using Vector3 = VeroEngine.Core.Mathematics.Vector3;

namespace VeroEngine.Core.Rendering;

public class Camera
    {
        private float _aspectRatio;
        private float _farClip;
        private float _fieldOfView; // In radians
        private float _nearClip;
        private float _pitch; // Rotation around the X-axis (in radians)
        private float _yaw;   // Rotation around the Y-axis (in radians)
        private float _roll;  // Rotation around the Z-axis (in radians)
        private Vector3 _position;
        private Vector3 _up = Vector3.UnitY; // Default up vector

        public Camera(Vector3 position, float yaw, float pitch, float roll, float fieldOfView, float aspectRatio, float nearClip, float farClip)
        {
            _position = position;
            _yaw = yaw;
            _pitch = pitch;
            _roll = roll;
            _fieldOfView = fieldOfView;
            _aspectRatio = aspectRatio;
            _nearClip = nearClip;
            _farClip = farClip;
        }

        public Matrix4 GetProjectionMatrix()
        {
            _fieldOfView = Math.Clamp(_fieldOfView, 0.01f, (float)Math.PI);
            return Matrix4.CreatePerspectiveFieldOfView(_fieldOfView, _aspectRatio, _nearClip, _farClip);
        }

        public Matrix4 GetViewMatrix()
        {
            Vector3 front = GetFront();
            var Front = front.Normalize();
            var Right = Vector3.Cross(Front, _up).Normalize();

            return Matrix4.LookAt(_position.ToOpenTK(), (_position + front).ToOpenTK(), _up.ToOpenTK());
        }

        public Vector3 GetFront()
        {
            /*if (Util.Rad2Deg(_pitch) > 360)
            {
                _pitch -= (float)Util.Deg2Rad(360);
            }
            if (Util.Rad2Deg(_pitch) < -360)
            {
                var amnt = (Util.Rad2Deg(_pitch) - 360);
                _pitch += (float)amnt;
            }*/
            return new Vector3(
                MathF.Cos(_yaw) * MathF.Cos(_pitch),
                MathF.Sin(_pitch),
                MathF.Sin(_yaw) * MathF.Cos(_pitch)
            ).Normalize();
        }

        public void SetRotation(float yaw, float pitch, float roll)
        {
            _yaw = yaw % (2 * MathF.PI); // Wrap yaw to prevent overflow
            _pitch = pitch;
            _roll = roll % (2 * MathF.PI); // Wrap roll to prevent overflow
        }
        
        public void SetRotation(Vector3 e)
        {
            SetRotation(e.X, e.Y, e.Z);
        }

        public void Rotate(float deltaYaw, float deltaPitch, float deltaRoll)
        {
            _yaw += deltaYaw;
            _pitch += deltaPitch;
            _roll += deltaRoll;
            
            _yaw = (_yaw + MathF.PI * 2) % (MathF.PI * 2);
            _roll = (_roll + MathF.PI * 2) % (MathF.PI * 2);
            
            _pitch = Math.Clamp(_pitch, -MathF.PI / 2 + 0.01f, MathF.PI / 2 - 0.01f);
        }

        public Vector3 GetRight()
        {
            return Vector3.Cross(GetFront(), _up).Normalize();
        }

        public Vector3 GetPosition()
        {
            return _position;
        }

        public void SetPosition(Vector3 position)
        {
            _position = position;
        }

        public Quaternion GetRotationQuaternion()
        {
            // Return a quaternion representing the camera's rotation, including roll
            return Quaternion.FromEulerAngles(-_pitch, _yaw, _roll);
        }

        public Vector3 GetRotation()
        {
            return new Vector3(_pitch, _yaw, _roll);
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