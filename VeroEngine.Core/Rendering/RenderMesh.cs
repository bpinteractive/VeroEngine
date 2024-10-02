using System;
using System.Collections.Generic;
using System.IO;
using Assimp;
using OpenTK.Graphics.OpenGL4;
using VeroEngine.Core.Generic;
using VeroEngine.Core.Mathematics;
using Matrix4 = OpenTK.Mathematics.Matrix4; // Until I write my own
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using Vector4 = OpenTK.Mathematics.Vector4; // Until I write my own

namespace VeroEngine.Core.Rendering;

public class RenderMesh : IDisposable
    {
        private int _vao;
        private int _vbo;
        private int _ebo;
        private int _vertexCount;
        private RenderMaterial material;
        private bool disposed = false;

        public static RenderMesh FromModelFile(string file)
        {
            try
            {
                string fullpath = Path.Combine("Game", "Content", file);
                AssimpContext importer = new AssimpContext();

                Scene scene = importer.ImportFile(fullpath, PostProcessSteps.Triangulate
                                                            | PostProcessSteps.JoinIdenticalVertices
                                                            | PostProcessSteps.FlipUVs |
                                                            PostProcessSteps.TransformUVCoords);

                if (scene.MeshCount == 0)
                    throw new FileNotFoundException("No meshes found in the file.", fullpath);

                List<float> data = new List<float>();
                List<int> indices = new List<int>();
                int offset = 0;

                foreach (Mesh mesh in scene.Meshes)
                {
                    for (int i = 0; i < mesh.VertexCount; i++)
                    {
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

                    for (int i = 0; i < mesh.FaceCount; i++)
                    {
                        Face face = mesh.Faces[i];
                        for (int j = 0; j < face.IndexCount; j++)
                        {
                            indices.Add(face.Indices[j] + offset);
                        }
                    }

                    offset += mesh.VertexCount;
                }

                return new RenderMesh
                (
                    data.ToArray(),
                    indices.ToArray(),
                    RenderMaterial.FromSerialised(new SerialisedMaterial
                        { Shader = "VeroEngine.Basic", Uniforms = new Dictionary<string, SerialisedUniform>() })
                );
            }
            catch (Exception e)
            {
                return new RenderMesh();
            }
        }

        public RenderMesh(float[] data, int[] indices, RenderMaterial mat)
        {
            material = mat;
            InitializeBuffers(data, indices);
        }

        public RenderMesh() { }

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
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);

            int positionSize = 3;
            int texCoordSize = 2;
            int normalSize = 3;
            int colorSize = 4;

            int stride = (positionSize + texCoordSize + normalSize + colorSize) * sizeof(float);

            int offset = 0;

            GL.VertexAttribPointer(0, positionSize, VertexAttribPointerType.Float, false, stride, offset);
            GL.EnableVertexAttribArray(0);
            offset += positionSize * sizeof(float);

            GL.VertexAttribPointer(1, texCoordSize, VertexAttribPointerType.Float, false, stride, offset);
            GL.EnableVertexAttribArray(1);
            offset += texCoordSize * sizeof(float);

            GL.VertexAttribPointer(2, normalSize, VertexAttribPointerType.Float, false, stride, offset);
            GL.EnableVertexAttribArray(2);
            offset += normalSize * sizeof(float);

            GL.VertexAttribPointer(3, colorSize, VertexAttribPointerType.Float, false, stride, offset);
            GL.EnableVertexAttribArray(3);

            GL.BindVertexArray(0);
        }

        public void Render(Matrix4 model, Matrix4 view, Matrix4 projection, Vector3 color)
        {
            if (material != null)
            {
                material.Use();
                var c = color.ToOpenTK();
                GL.Uniform3(GL.GetUniformLocation(material.GetShader().Handle, "modulate_color"), ref c);
                GL.UniformMatrix4(GL.GetUniformLocation(material.GetShader().Handle, "model"), false, ref model);
                GL.UniformMatrix4(GL.GetUniformLocation(material.GetShader().Handle, "view"), false, ref view);
                GL.UniformMatrix4(GL.GetUniformLocation(material.GetShader().Handle, "projection"), false,
                    ref projection);

                GL.BindVertexArray(_vao);
                GL.DrawElements(PrimitiveType.Triangles, _vertexCount, DrawElementsType.UnsignedInt, 0);
                GL.BindVertexArray(0);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    material?.Dispose();
                }
                
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

                disposed = true;
            }
        }

        ~RenderMesh()
        {
            Dispose(false);
        }
    }