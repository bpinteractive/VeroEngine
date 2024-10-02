using System;
using System.IO;
using System.Net;
using OpenTK.Graphics.OpenGL4;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Mathematics;
using Matrix4 = OpenTK.Mathematics.Matrix4; // Until I write my own

namespace VeroEngine.Core.Rendering;

public class Shader : IDisposable
    {
        public int Handle { get; private set; }
        private bool _disposedValue = false;
        private readonly string _name;

        public static Shader FromSerialized(SerializedShader serializedShader, string name)
        {
            var shader = new Shader(name);
            shader.CompileShaders(serializedShader);
            return shader;
        }

        private Shader(string name)
        {
            _name = name;
        }

        public static Shader Load(string name)
        {
            Log.Info($"Loading shader {name}");
            try
            {
                string cachePath = GetCachePath(name);
                if (File.Exists(cachePath))
                {
                    var shader = new Shader(name);
                    shader.LoadFromCache(cachePath);
                    return shader;
                }

                string json = File.ReadAllText(GetShaderFilePath(name));
                var serializedShader = SerializedShader.Deserialize(json);
                return FromSerialized(serializedShader, name);
            }
            catch (FileNotFoundException ex)
            {
                Log.Error($"Shader file not found: {ex.Message}");
                return LoadMissingShader();
            }
        }

        private static Shader LoadMissingShader()
        {
            string json = File.ReadAllText(GetShaderFilePath("VeroEngine.Missing"));
            var serializedShader = SerializedShader.Deserialize(json);
            return FromSerialized(serializedShader, "VeroEngine.Missing");
        }

        private static string GetCachePath(string name) =>
            Path.Combine(Collections.GetUserDirectory(), "PipelineCache", $"{name}.bin");

        private static string GetShaderFilePath(string name) =>
            Path.Combine("Game", "Shaders", $"{name}.json");

        private void CompileShaders(SerializedShader serializedShader)
        {
            Handle = GL.CreateProgram();
            string vertsource = File.ReadAllText(Path.Combine("Game", "Shaders", "dat", serializedShader.VertexShader));
            string fragsource = File.ReadAllText(Path.Combine("Game", "Shaders", "dat", serializedShader.FragmentShader));
            AttachShader(vertsource, ShaderType.VertexShader);
            AttachShader(fragsource, ShaderType.FragmentShader);

            GL.LinkProgram(Handle);
            CheckLinkStatus();

            SaveToCache();
        }

        private void AttachShader(string shaderSource, ShaderType shaderType)
        {
            if (string.IsNullOrEmpty(shaderSource)) return;

            int shader = CompileShader(shaderSource, shaderType);
            GL.AttachShader(Handle, shader);
            GL.DeleteShader(shader);
        }

        private int CompileShader(string source, ShaderType type)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Shader compilation failed ({type}): {infoLog}");
            }
            return shader;
        }

        private void CheckLinkStatus()
        {
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int linkStatus);
            if (linkStatus == 0)
            {
                string infoLog = GL.GetProgramInfoLog(Handle);
                throw new Exception($"Shader program linking failed: {infoLog}");
            }
        }

        private void LoadFromCache(string cachePath)
        {
            Log.Info($"Loading pipeline cache from {cachePath}");
            byte[] binary = File.ReadAllBytes(cachePath);
            Handle = GL.CreateProgram();
            BinaryFormat format = (BinaryFormat)GetBinaryFormat(cachePath);
            GL.ProgramBinary(Handle, format, binary, binary.Length);

            ValidateCache();
        }

        private void ValidateCache()
        {
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(Handle);
                throw new Exception($"Shader program loading from cache failed: {infoLog}");
            }
        }

        private void SaveToCache()
        {
            Log.Info($"Writing pipeline cache for {_name}");
            GL.GetProgram(Handle, GetProgramParameterName.ProgramBinaryLength, out int binaryLength);
            byte[] binary = new byte[binaryLength];
            GL.GetProgramBinary(Handle, binaryLength, out _, out BinaryFormat format, binary);

            string cachePath = GetCachePath(_name);
            Directory.CreateDirectory(Path.GetDirectoryName(cachePath));
            File.WriteAllBytes(cachePath, binary);
            File.WriteAllText($"{cachePath}.format", format.ToString());
        }

        private int GetBinaryFormat(string cachePath)
        {
            string formatPath = $"{cachePath}.format";
            if (File.Exists(formatPath))
            {
                return int.Parse(File.ReadAllText(formatPath));
            }

            throw new Exception("Shader binary format not found.");
        }
        
        public int GetUniformLocation(string name)
        {
            return GL.GetUniformLocation(Handle, name);
        }
        public void SetUniform(string name, float value)
        {
            int location = GL.GetUniformLocation(Handle, name);
            if (location != -1) GL.Uniform1(location, value);
        }

        public void SetUniform(string name, int value)
        {
            int location = GL.GetUniformLocation(Handle, name);
            if (location != -1) GL.Uniform1(location, value);
        }

        public void SetUniform(string name, Vector3 vector)
        {
            int location = GL.GetUniformLocation(Handle, name);
            if (location != -1) GL.Uniform3(location, vector.X, vector.Y, vector.Z);
        }

        public void SetUniform(string name, Matrix4 matrix)
        {
            int location = GL.GetUniformLocation(Handle, name);
            if (location != -1) GL.UniformMatrix4(location, false, ref matrix);
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // Cleanup managed resources if any
                }

                if (Handle != -1 && Handle != 0)
                {
                    if(GL.IsProgram(Handle))
                    {
                        GL.DeleteProgram(Handle);
                    }
                }
                _disposedValue = true;
            }
        }

        ~Shader()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }