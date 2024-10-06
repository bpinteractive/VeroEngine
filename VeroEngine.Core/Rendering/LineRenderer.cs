using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using VeroEngine.Core.Generic;
using Vector3 = VeroEngine.Core.Mathematics.Vector3;

namespace VeroEngine.Core.Rendering;

public class LineRenderer
{
	private Vector3 _cachedEnd;
	private Vector3 _cachedStart;
	private bool _isDataChanged;
	private readonly int _vao;
	private readonly int _vbo;

	public LineRenderer()
	{
		// Initialize the VAO and VBO
		_vao = GL.GenVertexArray();
		_vbo = GL.GenBuffer();

		_cachedStart = Vector3.Zero;
		_cachedEnd = Vector3.Zero;
		_isDataChanged = true;
	}

	public void DrawLine(Shader shader, Vector3 start, Vector3 end, Vector3 color)
	{
		if (start != _cachedStart || end != _cachedEnd)
		{
			_cachedStart = start;
			_cachedEnd = end;
			_isDataChanged = true;
		}

		if (_isDataChanged)
		{
			float[] vertices =
			{
				_cachedStart.X, _cachedStart.Y, _cachedStart.Z,
				_cachedEnd.X, _cachedEnd.Y, _cachedEnd.Z
			};

			GL.BindVertexArray(_vao);

			GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
			GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices,
				BufferUsageHint.DynamicDraw);

			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

			_isDataChanged = false;
		}

		shader.Use();
		var c = color.ToOpenTK();
		GL.Uniform3(GL.GetUniformLocation(shader.Handle, "modulate_color"), ref c);

		var proj = Collections.RootTree.SceneCamera.GetProjectionMatrix();
		var view = Collections.RootTree.SceneCamera.GetViewMatrix();

		var identity = Matrix4.Identity;
		GL.UniformMatrix4(GL.GetUniformLocation(shader.Handle, "model"), false, ref identity);
		GL.UniformMatrix4(GL.GetUniformLocation(shader.Handle, "view"), false, ref view);
		GL.UniformMatrix4(GL.GetUniformLocation(shader.Handle, "projection"), false, ref proj);

		GL.BindVertexArray(_vao);

		GL.DrawArrays(PrimitiveType.Lines, 0, 2);

		GL.BindVertexArray(0);
	}

	public void Dispose()
	{
		GL.DeleteVertexArray(_vao);
		GL.DeleteBuffer(_vbo);
	}
}