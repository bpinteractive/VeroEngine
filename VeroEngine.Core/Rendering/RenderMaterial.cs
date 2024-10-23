using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;
using VeroEngine.Core.Generic;

namespace VeroEngine.Core.Rendering;

public class RenderMaterial : IDisposable
{
    private bool _disposedValue;
    private SerialisedMaterial _serialisedMaterial;
    private Shader _shader;

    private static readonly Dictionary<string, int> TextureCache = new(); // Cache for loaded textures
    private readonly Dictionary<string, int> _uniformTextureIds = new(); // Store texture IDs for uniforms

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public static RenderMaterial FromSerialised(SerialisedMaterial material)
    {
        var inst = new RenderMaterial();
        inst._serialisedMaterial = material;
        inst._shader = Shader.Load(material.Shader); // Directly use the shader name

        foreach (var uniform in material.Uniforms)
            switch (uniform.Value.Type.ToLower()) // Ensure case-insensitivity
            {
                case "int":
                    inst.SetUniformSafe(uniform.Key, uniform.Value.Value.GetInt32());
                    break;
                case "float":
                    inst.SetUniformSafe(uniform.Key, uniform.Value.Value.GetSingle());
                    break;
                case "tex":
                    var texturePath = uniform.Value.Value.GetString(); // Ensure it is a string
                    var textureId = LoadTexture(texturePath);
                    inst._uniformTextureIds[uniform.Key] = textureId; // Store the texture ID for later binding
                    break;
                default:
                    Debug.WriteLine($"Warning: Unsupported uniform type '{uniform.Value.Type}' for '{uniform.Key}'");
                    break;
            }

        return inst;
    }

    public static string DefaultMaterial = "{\n  \"Shader\": \"VeroEngine.Basic\",\n  \"Uniforms\": {\n    \"Albedo\": {\n      \"Type\": \"tex\",\n      \"Value\": \"Textures/blank.png\"\n    }\n  }\n}";

    public static RenderMaterial Load(string name)
    {
        Log.Info($"Loading material {name}");
        var filePath = Path.Combine("Game", "Content", $"{name}.mat");

        try
        {
            var json = File.ReadAllText(filePath);
            var serialisedMaterial = SerialisedMaterial.Deserialize(json);
            return FromSerialised(serialisedMaterial);
        }
        catch (FileNotFoundException ex)
        {
            return FromSerialised(SerialisedMaterial.Deserialize(DefaultMaterial));
        }
    }

    public void Use()
    {
        if (_shader != null)
        {
            _shader.Use();
            
            int textureUnit = 0;
            foreach (var uniform in _uniformTextureIds)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
                GL.BindTexture(TextureTarget.Texture2D, uniform.Value);
                
                _shader.SetUniform(uniform.Key, textureUnit);

                textureUnit++;
            }
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
        path = Path.Combine("Game", "Content", path);
        if (TextureCache.TryGetValue(path, out int existingTextureId))
        {
            return existingTextureId;
        }
        
        ImageResult imageResult = ImageResult.FromStream(File.Open(path, FileMode.Open), ColorComponents.RedGreenBlueAlpha);
        if (imageResult == null)
        {
            Log.Error($"Error loading texture: {path}");
            return 0; // Handle error (0 texture ID)
        }

        int textureId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureId);
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, imageResult.Width, imageResult.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, imageResult.Data);

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        GL.BindTexture(TextureTarget.Texture2D, 0);
        
        TextureCache[path] = textureId;

        return textureId;
    }

    private void SetUniformSafe(string name, int value)
    {
        var location = _shader.GetUniformLocation(name);
        if (location != -1)
            _shader.SetUniform(name, value);
        else
            Debug.WriteLine($"Warning: Uniform '{name}' not found in shader.");
    }

    private void SetUniformSafe(string name, float value)
    {
        var location = _shader.GetUniformLocation(name);
        if (location != -1)
            _shader.SetUniform(name, value);
        else
            Debug.WriteLine($"Warning: Uniform '{name}' not found in shader.");
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
        if (!_disposedValue && disposing)
        {
            _shader?.Dispose();
            _disposedValue = true;
        }
    }

    ~RenderMaterial()
    {
        Dispose(false);
    }
}
