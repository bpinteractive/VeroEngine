using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using VeroEngine.Core.Generic;

namespace VeroEngine.Core.Rendering;

public class RenderMaterial : IDisposable
{
    private Shader _shader;
    private SerialisedMaterial _serialisedMaterial;
    private bool _disposedValue = false;

    public static RenderMaterial FromSerialised(SerialisedMaterial material)
    {
        var inst = new RenderMaterial();
        inst._serialisedMaterial = material;
        inst._shader = Shader.Load(material.Shader); // Directly use the shader name

        foreach (var uniform in material.Uniforms)
        {
            switch (uniform.Value.Type.ToLower()) // Ensure case-insensitivity
            {
                case "int":
                    inst.SetUniformSafe(uniform.Key, uniform.Value.Value.GetInt32());
                    break;
                case "float":
                    inst.SetUniformSafe(uniform.Key, uniform.Value.Value.GetSingle());
                    break;
                case "tex":
                    string texturePath = uniform.Value.Value.GetString(); // Ensure it is a string
                    int textureId = LoadTexture(texturePath);
                    inst.SetUniformSafe(uniform.Key, textureId);
                    break;
                default:
                    Debug.WriteLine($"Warning: Unsupported uniform type '{uniform.Value.Type}' for '{uniform.Key}'");
                    break;
            }
        }

        return inst;
    }

    public static RenderMaterial Load(string name)
    {
        string filePath = Path.Combine("Game", "Content", $"{name}.json");

        try
        {
            string json = File.ReadAllText(filePath);
            SerialisedMaterial serialisedMaterial = SerialisedMaterial.Deserialize(json);
            return FromSerialised(serialisedMaterial);
        }
        catch (FileNotFoundException ex)
        {
            Log.Error($"Material file not found: {ex.Message}");
            return null;
        }
    }

    public void Use()
    {
        if (_shader != null)
        {
            _shader.Use();
        }
        else
        {
            Log.Error("Shader is null, cannot use material.");
        }
    }

    public Shader GetShader()
    {
        return _shader;
    }

    private static int LoadTexture(string path)
    {
        // Implement caching mechanism to avoid reloading the same texture multiple times
        // Placeholder implementation for texture loading
        return 0; // Replace with actual texture ID after loading the texture
    }

    private void SetUniformSafe(string name, int value)
    {
        int location = _shader.GetUniformLocation(name);
        if (location != -1)
        {
            _shader.SetUniform(name, value);
        }
        else
        {
            Debug.WriteLine($"Warning: Uniform '{name}' not found in shader.");
        }
    }

    private void SetUniformSafe(string name, float value)
    {
        int location = _shader.GetUniformLocation(name);
        if (location != -1)
        {
            _shader.SetUniform(name, value);
        }
        else
        {
            Debug.WriteLine($"Warning: Uniform '{name}' not found in shader.");
        }
    }

    public override string ToString()
    {
        var uniformInfo = string.Join(Environment.NewLine, _serialisedMaterial.Uniforms.Select(u =>
            $"{u.Key}: Type = {u.Value.Type}, Value = {u.Value.Value}"));

        return $"RenderMaterial: {Environment.NewLine}" +
               $"Shader: {_serialisedMaterial.Shader}{Environment.NewLine}" +
               $"Uniforms: {Environment.NewLine}{uniformInfo}";
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _shader?.Dispose();
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~RenderMaterial()
    {
        Dispose(false);
    }
}