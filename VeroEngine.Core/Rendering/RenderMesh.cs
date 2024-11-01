using System;
using System.Collections.Generic;
using System.IO;
using Assimp;
using OpenTK.Graphics.OpenGL4;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Mathematics;
using Matrix4 = OpenTK.Mathematics.Matrix4; // Until I write my own
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;

// Until I write my own

namespace VeroEngine.Core.Rendering;

public class RenderMesh : IDisposable
{
	public readonly List<Vector3> Vertices = new();
	private bool _boundingBoxDirty = true;

	private AABB _cachedBoundingBox;
	private int _ebo;
	private int _vao;
	private int _vbo;
	private int _vertexCount;
	private bool disposed;
	public RenderMaterial Material;
	private RenderMaterial ShadowMaterial;

	public RenderMesh(float[] data, int[] indices, RenderMaterial mat, List<Vector3> vertices)
	{
		Material = mat;
		InitializeBuffers(data, indices);
		Vertices = vertices;
		ShadowMaterial = RenderMaterial.FromSerialised(new SerialisedMaterial() { Shader = "Core.DepthDraw", Uniforms = new Dictionary<string, SerialisedUniform>()});
	}

	public RenderMesh()
	{
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	// Cached bounding box method
	public AABB GetBoundingBox()
	{
		if (_boundingBoxDirty)
		{
			_cachedBoundingBox = CalculateBoundingBox();
			_boundingBoxDirty = false;
		}

		return _cachedBoundingBox;
	}

	private AABB CalculateBoundingBox()
	{
		if (Vertices.Count == 0)
			throw new InvalidOperationException("No vertices in the mesh.");

		var min = Vertices[0];
		var max = Vertices[0];

		foreach (var vertex in Vertices)
		{
			if (vertex.X < min.X) min.X = vertex.X;
			if (vertex.Y < min.Y) min.Y = vertex.Y;
			if (vertex.Z < min.Z) min.Z = vertex.Z;

			if (vertex.X > max.X) max.X = vertex.X;
			if (vertex.Y > max.Y) max.Y = vertex.Y;
			if (vertex.Z > max.Z) max.Z = vertex.Z;
		}

		return new AABB(min, max);
	}

	public static RenderMesh FromModelFile(string file)
{
    try
    {
        var fullpath = Path.Combine("Game", "Content", file);
        var importer = new AssimpContext();

        var scene = importer.ImportFile(fullpath, PostProcessSteps.Triangulate
                                                  | PostProcessSteps.JoinIdenticalVertices
                                                  | PostProcessSteps.FlipUVs |
                                                  PostProcessSteps.TransformUVCoords);

        if (scene.MeshCount == 0)
            throw new FileNotFoundException("No meshes found in the file.", fullpath);

        var data = new List<float>();
        var indices = new List<int>();
        var offset = 0;

        var verts = new List<Vector3>();

        foreach (var mesh in scene.Meshes)
        {

            // Add vertex data including tangents and bitangents
            for (var i = 0; i < mesh.VertexCount; i++)
            {
                verts.Add(new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z));
                data.Add(mesh.Vertices[i].X);
                data.Add(mesh.Vertices[i].Y);
                data.Add(mesh.Vertices[i].Z);

                if (mesh.HasTextureCoords(0))
                {
                    data.Add(mesh.TextureCoordinateChannels[0][i].X);
                    data.Add(mesh.TextureCoordinateChannels[0][i].Y);
                }
                else
                {
                    data.Add(0.0f);
                    data.Add(0.0f);
                }

                if (mesh.HasNormals)
                {
                    data.Add(mesh.Normals[i].X);
                    data.Add(mesh.Normals[i].Y);
                    data.Add(mesh.Normals[i].Z);
                }
                else
                {
                    data.Add(0.0f);
                    data.Add(0.0f);
                    data.Add(0.0f);
                }
                

                if (mesh.HasVertexColors(0))
                {
                    var color = mesh.VertexColorChannels[0][i];
                    data.Add(color.R);
                    data.Add(color.G);
                    data.Add(color.B);
                    data.Add(color.A);
                }
                else
                {
                    data.Add(1.0f);
                    data.Add(1.0f);
                    data.Add(1.0f);
                    data.Add(1.0f);
                }
            }

            for (var i = 0; i < mesh.FaceCount; i++)
            {
                var face = mesh.Faces[i];
                for (var j = 0; j < face.IndexCount; j++) indices.Add(face.Indices[j] + offset);
            }

            offset += mesh.VertexCount;
        }

        return new RenderMesh
        (
            data.ToArray(),
            indices.ToArray(),
            RenderMaterial.FromSerialised(new SerialisedMaterial
                { Shader = "VeroEngine.Basic", Uniforms = new Dictionary<string, SerialisedUniform>() }),
            verts
        );
    }
    catch (Exception e)
    {
        return new RenderMesh();
    }
}
	


	private void InitializeBuffers(float[] data, int[] indices)
	{
		_vertexCount = indices.Length;

		_vao = GL.GenVertexArray();
		_vbo = GL.GenBuffer();
		_ebo = GL.GenBuffer();

		GL.BindVertexArray(_vao);

		GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
		GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.StaticDraw);

		GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
		GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices,
			BufferUsageHint.StaticDraw);
		
		var positionSize = 3;
		var texCoordSize = 2;
		var normalSize = 3;
		var colorSize = 4;

		var stride = (positionSize + texCoordSize + normalSize + colorSize) * sizeof(float);

		var offset = 0;

		GL.VertexAttribPointer(0, positionSize, VertexAttribPointerType.Float, false, stride, offset);
		GL.EnableVertexAttribArray(0);
		offset += positionSize * sizeof(float);

		GL.VertexAttribPointer(1, texCoordSize, VertexAttribPointerType.Float, false, stride, offset);
		GL.EnableVertexAttribArray(1);
		offset += texCoordSize * sizeof(float);

		GL.VertexAttribPointer(2, normalSize, VertexAttribPointerType.Float, false, stride, offset);
		GL.EnableVertexAttribArray(2);
		offset += normalSize * sizeof(float);
		
		// Color
		GL.VertexAttribPointer(3, colorSize, VertexAttribPointerType.Float, false, stride, offset);
		GL.EnableVertexAttribArray(3);

	}

	public void Render(Matrix4 model, Matrix4 view, Matrix4 projection, Vector3 color, bool wireframe = false, int shaded = 0, bool shadowmap = false)
	{
	    // Bind the appropriate framebuffer or texture based on whether we're rendering a shadow map
	    if (shadowmap)
	    {
	        Material = ShadowMaterial;
	    }

	    if (Material != null)
	    {
	        Material.Use();
	        var c = color.ToOpenTK();
	        GL.Uniform3(GL.GetUniformLocation(Material.GetShader().Handle, "modulate_color"), c);
	        try
	        {
		        var ae = Collections.Registry.Get<Vector3>("ambient_col");
		        GL.Uniform3(GL.GetUniformLocation(Material.GetShader().Handle, "ambient_col"), ae.ToOpenTK());
	        }
	        catch (Exception e)
	        {
	        }
	        
	        
	        GL.Uniform1(Material.GetShader().GetUniformLocation("shade"), shaded);
	        GL.Uniform1(Material.GetShader().GetUniformLocation("num_lights"), Collections.SceneLights.Count);

	        var i = 0;
	        foreach (var light in Collections.SceneLights)
	        {
	            var lightPosition = light.GlobalPosition.ToOpenTK();
	            var lightColor = light.Color.ToOpenTK() * light.Intensity;

	            // Set light position
	            GL.Uniform3(GL.GetUniformLocation(Material.GetShader().Handle, "light_positions[" + i + "]"), lightPosition);
	            
	            // Set light color
	            GL.Uniform3(GL.GetUniformLocation(Material.GetShader().Handle, "light_colors[" + i + "]"), lightColor);
	            
	            GL.ActiveTexture(TextureUnit.Texture0 + i);
	            GL.BindTexture(TextureTarget.TextureCubeMap, light.DepthCubemap);
	            GL.Uniform1(GL.GetUniformLocation(Material.GetShader().Handle, "light_depthCubemap[" + i + "]"), i);

	            i++;
	        }

	        try
	        {
		        bool a = Collections.Registry.Get<bool>("dirlight_enabled");
		        GL.Uniform1(Material.GetShader().GetUniformLocation("dirlight_enabled"), a ? 1 : 0);
		        if (a)
		        {
			        var e = Collections.Registry.Get<Vector3>("dirlight_rot");
			        GL.Uniform3(Material.GetShader().GetUniformLocation("dirlight_rot"), e.ToOpenTK());
			        var b = Collections.Registry.Get<Vector3>("dirlight_col");
			        GL.Uniform3(Material.GetShader().GetUniformLocation("dirlight_col"), b.ToOpenTK());
		        }
	        }
	        catch (Exception e)
	        {
		        // ignored
	        }


	        GL.UniformMatrix4(GL.GetUniformLocation(Material.GetShader().Handle, "model"), false, ref model);
	        GL.UniformMatrix4(GL.GetUniformLocation(Material.GetShader().Handle, "view"), false, ref view);
	        GL.UniformMatrix4(GL.GetUniformLocation(Material.GetShader().Handle, "projection"), false, ref projection);

	        GL.BindVertexArray(_vao);
	        if (wireframe)
	            GL.DrawElements(PrimitiveType.LineStrip, _vertexCount, DrawElementsType.UnsignedInt, 0);
	        else
	            GL.DrawElements(PrimitiveType.Triangles, _vertexCount, DrawElementsType.UnsignedInt, 0);

	        GL.BindVertexArray(0);
	    }
	}


	protected virtual void Dispose(bool disposing)
	{
		if (!disposed && disposing)
		{
			Material?.Dispose();
			if (_vao != 0)
			{
				GL.DeleteVertexArray(_vao);
				_vao = 0;
			}

			if (_vbo != 0)
			{
				GL.DeleteBuffer(_vbo);
				_vbo = 0;
			}

			if (_ebo != 0)
			{
				GL.DeleteBuffer(_ebo);
				_ebo = 0;
			}
		}

		disposed = true;
	}

	~RenderMesh()
	{
		Dispose(false);
	}
}