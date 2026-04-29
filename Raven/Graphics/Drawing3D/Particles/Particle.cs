using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;

namespace Raven.Graphics.Particles {
    [StructLayout(LayoutKind.Sequential)]
    public struct InstanceDataDec : IVertexType {
        public static readonly VertexDeclaration VertexDeclaration;

        public Vector4 r1;
        public Vector4 r2;
        public Vector4 r3;
        public Vector4 r4;
        public Vector3 normal;
        public Color tint;

        VertexDeclaration IVertexType.VertexDeclaration {
            get { return VertexDeclaration; }
        }

        static InstanceDataDec() {
            var elements = new VertexElement[]
                {
                    // PARTICLE WORLD MATRIX
                    new VertexElement(sizeof(float) * 0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
                    new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
                    new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3),
                    new VertexElement(sizeof(float) * 12, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4),

                    // NORMAL
                    new VertexElement(sizeof(float) * 16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),

                    // COLOR
                    new VertexElement(sizeof(float) * 19, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                };
            VertexDeclaration = new VertexDeclaration(elements);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InstanceData {
        [FieldOffset(sizeof(float) * 0)] public Vector4 r1;
        [FieldOffset(sizeof(float) * 4)] public Vector4 r2;
        [FieldOffset(sizeof(float) * 8)] public Vector4 r3;
        [FieldOffset(sizeof(float) * 12)] public Vector4 r4;

        [FieldOffset(sizeof(float) * 16)] public Vector3  normal;
        
        [FieldOffset(sizeof(float) * 19)] public Color tint;  
    }

    public class Particle {
        public static Effect e_particle;
    }
    public class ParticleQuad {
        VertexPositionNormalTexture[] _quad = new VertexPositionNormalTexture[4] {
                new VertexPositionNormalTexture(new Vector3(-1, -1, 0), -Vector3.UnitZ, new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(1, -1, 0), -Vector3.UnitZ,  new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(1, 1, 0), -Vector3.UnitZ,   new Vector2(1, 1)),
                new VertexPositionNormalTexture(new Vector3(-1, 1, 0), -Vector3.UnitZ,  new Vector2(0, 1))
            };

        ushort[] _indices = { 0, 1, 2, 2, 3, 0 };
        VertexBuffer vb;
        IndexBuffer ib;

        VertexBufferBinding[] vertex_buffer_binding = new VertexBufferBinding[2];

        string _texture;

        public Texture2D texture => Resources.GetTexture(_texture);

        public ParticleQuad(string texture) {
            if (Particle.e_particle == null)
                Particle.e_particle = Resources.GetShader("particle");

            this._texture = texture;
            
            vb = new VertexBuffer(State.graphics_device, VertexPositionNormalTexture.VertexDeclaration, _quad.Length, BufferUsage.None);
            ib = new IndexBuffer(State.graphics_device, IndexElementSize.SixteenBits, _indices.Length, BufferUsage.None);

            vertex_buffer_binding[0] = new VertexBufferBinding(vb, 0, 0);

            vb.SetData(_quad);
            ib.SetData(_indices);
        }

        public void instance_onto_point_cloud(Camera camera, PointCloud pc) {
            //lock (pc) {
            //lock (vertex_buffer_binding) {
            //State.graphics_device.RasterizerState = RasterizerState.CullNone;
            State.graphics_device.BlendState = BlendState.NonPremultiplied;

            pc.GenerateWorldMatrixBuffer(camera);
            vertex_buffer_binding[1] = new VertexBufferBinding(pc.instance_buffer, 0, 1);

            //Particle.e_particle.Parameters["World"].SetValue(Matrix.Identity);
            Particle.e_particle.Parameters["View"].SetValue(camera.view);
            Particle.e_particle.Parameters["Projection"].SetValue(camera.projection);

            Particle.e_particle.Parameters["particle"].SetValue(texture);

            Particle.e_particle.Techniques["Instanced"].Passes[0].Apply();

            State.graphics_device.SetVertexBuffers(vertex_buffer_binding);
            State.graphics_device.Indices = ib;

            State.graphics_device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 2, pc.vert_positions.Length);
            //}
            //}
        }
    }
    internal class Particle3D {
        int mesh_index;        
        string _model;

        public Model model => Resources.GetModel(_model);
        public ModelMesh mesh => model.Meshes[mesh_index];

        string _texture;
        public Texture2D texture => Resources.GetTexture(_texture);

        public Particle3D(string model, string texture) {
            if (Particle.e_particle == null)
                Particle.e_particle = Resources.GetShader("particle");

            this._model = model;
            this._texture = texture;
        }

        public void instance_onto_point_cloud(PointCloud pc) {

        }
    }
}
