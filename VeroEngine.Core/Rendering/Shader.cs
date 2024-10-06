using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Mathematics;
using Matrix4 = OpenTK.Mathematics.Matrix4; // Until I write my own

namespace VeroEngine.Core.Rendering;

public class Shader : IDisposable
{
	private readonly string _name;
	private bool _disposedValue;

	private Shader(string name)
	{
		_name = name;
		Handle = -1; // Default invalid handle
		Log.Info($"Shader {name} Constructed");
	}

	public int Handle { get; private set; }

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public static Shader FromSerialized(SerializedShader serializedShader, string name)
	{
		var shader = new Shader(name);
		shader.CompileShaders(serializedShader);
		return shader;
	}

	public static Shader Load(string name)
	{
		Log.Info($"Loading shader {name}");
		try
		{
			var cachePath = GetCachePath(name);
			if (File.Exists(cachePath))
			{
				var shader = new Shader(name);
				shader.LoadFromCache(cachePath);
				return shader;
			}

			var json = File.ReadAllText(GetShaderFilePath(name));
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
		var json = File.ReadAllText(GetShaderFilePath("VeroEngine.Missing"));
		var serializedShader = SerializedShader.Deserialize(json);
		return FromSerialized(serializedShader, "VeroEngine.Missing");
	}

	public static string GetCachePath(string name)
	{
		return Path.Combine(Collections.GetUserDirectory(), "PipelineCache", $"{name}.bin");
	}

	public static string GetCachePath()
	{
		return Path.Combine(Collections.GetUserDirectory(), "PipelineCache");
	}

	private static string GetShaderFilePath(string name)
	{
		return Path.Combine("Game", "Shaders", $"{name}.json");
	}

	private void CompileShaders(SerializedShader serializedShader)
	{
		Handle = GL.CreateProgram();
		var vertsource = File.ReadAllText(Path.Combine("Game", "Shaders", "dat", serializedShader.VertexShader));
		var fragsource = File.ReadAllText(Path.Combine("Game", "Shaders", "dat", serializedShader.FragmentShader));

		AttachShader(vertsource, ShaderType.VertexShader);
		AttachShader(fragsource, ShaderType.FragmentShader);

		GL.LinkProgram(Handle);
		CheckLinkStatus();

		SaveToCache();
	}

	private void AttachShader(string shaderSource, ShaderType shaderType)
	{
		if (string.IsNullOrEmpty(shaderSource)) return;

		var shader = CompileShader(shaderSource, shaderType);
		GL.AttachShader(Handle, shader);
		GL.DeleteShader(shader); // Safe to delete as it's now part of the program
	}

	private int CompileShader(string source, ShaderType type)
	{
		var shader = GL.CreateShader(type);
		GL.ShaderSource(shader, source);
		GL.CompileShader(shader);

		GL.GetShader(shader, ShaderParameter.CompileStatus, out var success);
		if (success == 0)
		{
			var infoLog = GL.GetShaderInfoLog(shader);
			GL.DeleteShader(shader); // Clean up the shader if it fails
			throw new Exception($"Shader compilation failed ({type}): {infoLog}");
		}

		return shader;
	}

	private void CheckLinkStatus()
	{
		GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out var linkStatus);
		if (linkStatus == 0)
		{
			var infoLog = GL.GetProgramInfoLog(Handle);
			throw new Exception($"Shader program linking failed: {infoLog}");
		}
	}

	private void LoadFromCache(string cachePath)
	{
		Log.Info($"Loading pipeline cache from {cachePath}");
		var binary = File.ReadAllBytes(cachePath);
		Handle = GL.CreateProgram();
		var format = (BinaryFormat)GetBinaryFormat(cachePath);
		GL.ProgramBinary(Handle, format, binary, binary.Length);

		ValidateCache();
	}

	private void ValidateCache()
	{
		GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out var success);
		if (success == 0)
		{
			var infoLog = GL.GetProgramInfoLog(Handle);
			throw new Exception($"Shader program loading from cache failed: {infoLog}");
		}
	}

	private void SaveToCache()
	{
		Log.Info($"Writing pipeline cache for {_name}");
		GL.GetProgram(Handle, GetProgramParameterName.ProgramBinaryLength, out var binaryLength);
		var binary = new byte[binaryLength];
		GL.GetProgramBinary(Handle, binaryLength, out _, out var format, binary);

		var cachePath = GetCachePath(_name);
		Directory.CreateDirectory(Path.GetDirectoryName(cachePath));
		File.WriteAllBytes(cachePath, binary);
		File.WriteAllText($"{cachePath}.format", format.ToString());
	}

	private int GetBinaryFormat(string cachePath)
	{
		var formatPath = $"{cachePath}.format";
		if (File.Exists(formatPath)) return int.Parse(File.ReadAllText(formatPath));

		throw new Exception("Shader binary format not found.");
	}

	public int GetUniformLocation(string name)
	{
		return GL.GetUniformLocation(Handle, name);
	}

	public void SetUniform(string name, float value)
	{
		var location = GetUniformLocation(name);
		if (location != -1) GL.Uniform1(location, value);
	}

	public void SetUniform(string name, int value)
	{
		var location = GetUniformLocation(name);
		if (location != -1) GL.Uniform1(location, value);
	}
	

	public void SetUniform(string name, Vector3 vector)
	{
		var location = GetUniformLocation(name);
		if (location != -1) GL.Uniform3(location, vector.X, vector.Y, vector.Z);
	}

	public void SetUniform(string name, Matrix4 matrix)
	{
		var location = GetUniformLocation(name);
		if (location != -1) GL.UniformMatrix4(location, false, ref matrix);
	}

	public void Use()
	{
		SetUniform("cam_dir", (Vector3.Zero-Collections.RootTree.SceneCamera.GetFront()).Normalize());
		GL.UseProgram(Handle);
	}

	protected virtual void Dispose(bool disposing)
	{
		Log.Info($"Shader {_name} Disposed");
		if (!_disposedValue && disposing)
		{
			if (Handle != -1) // Ensure handle is valid before deleting
			{
				GL.DeleteProgram(Handle);
				Handle = -1; // Invalidate handle after deletion
			}

			_disposedValue = true;
		}
	}

	~Shader()
	{
		Dispose(false);
	}
}