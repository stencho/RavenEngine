using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RavenRPG.Graphics.Drawing3D {
    public class render_info_model : render_info {
        public Vector3 render_offset { get; set; } = Vector3.Zero;
        public Vector3 scale { get; set; } = Vector3.One;
        public Model model => _model; Model _model;
        public Matrix world { get; set; } = Matrix.Identity;
        public Matrix orientation { get; set; } = Matrix.Identity;
        public string[] textures { get; set; }

        public BoundingSphere render_bounds { get; set; }

        // IMPLEMENT THIS
        // ALSO DIFFERENT TEXTURE TYPES
        public bool partial_transparency { get; set; } = false;

        public bool in_frustum(BoundingFrustum frustum) {
            foreach (ModelMesh mm in _model.Meshes) {
                if (frustum.Intersects(mm.BoundingSphere.Transform(world))) {
                    return true;
                }
            }

            return false;
        }

        public render_info_model(string model_name) {
            _model = ContentHandler.resources[model_name].value_gfx;
            textures = new string[1] { "OnePXWhite" };
        }
        public render_info_model(string model_name, string texture_name) {
            _model = ContentHandler.resources[model_name].value_gfx;
            textures = new string[1] { texture_name };
        }

        public Action preepass_action;
        public void prepass() {
            if (preepass_action != null) preepass_action();
        }

        public void draw() {
            foreach (ModelMesh mm in _model.Meshes) {
                foreach (ModelMeshPart mmp in mm.MeshParts) {
                    Renderer.e_gbuffer.Parameters["World"].SetValue(world);
                    Renderer.e_gbuffer.Parameters["WVIT"].SetValue(Matrix.Transpose(Matrix.Invert(world * EngineState.camera.view)));

                    Renderer.e_gbuffer.Parameters["DiffuseMap"].SetValue(ContentHandler.resources[textures[0]].value_tx);
                    Renderer.e_gbuffer.Parameters["tint"].SetValue(Color.White.ToVector3());

                    EngineState.graphics_device.SetVertexBuffer(mmp.VertexBuffer);
                    EngineState.graphics_device.Indices = mmp.IndexBuffer;

                    Renderer.e_gbuffer.CurrentTechnique.Passes[0].Apply();
                    EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, mmp.VertexBuffer.VertexCount);

                }
            }
        }

        public void draw_to_light(light light) {
            Renderer.e_exp_light_depth.Parameters["World"].SetValue(world);

            foreach (ModelMesh mm in _model.Meshes) {
                foreach (ModelMeshPart mmp in mm.MeshParts) {

                    Renderer.e_exp_light_depth.Parameters["DiffuseMap"].SetValue(ContentHandler.resources[textures[0]].value_tx);

                    EngineState.graphics_device.DepthStencilState = DepthStencilState.Default;

                    EngineState.graphics_device.SetVertexBuffer(mmp.VertexBuffer);
                    EngineState.graphics_device.Indices = mmp.IndexBuffer;

                    foreach (EffectTechnique tech in Renderer.e_exp_light_depth.Techniques) {
                        foreach (EffectPass pass in tech.Passes) {
                            pass.Apply();
                            EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, mmp.VertexBuffer.VertexCount);
                        }
                    }

                }
            }

        }

    }

    public class render_info_vertex_buffer : render_info {
        public BoundingSphere render_bounds { get; set; }
        public Vector3 render_offset { get; set; } = Vector3.Zero;
        public Vector3 scale { get; set; } = Vector3.One;
        public VertexBuffer vertex_buffer { get; }
        public IndexBuffer index_buffer { get; }


        public Matrix world { get; set; } = Matrix.Identity;
        public Matrix orientation { get; set; } = Matrix.Identity;

        public string[] textures { get; set; }

        public render_info_vertex_buffer(VertexBuffer vbuffer, IndexBuffer ibuffer) {
            vertex_buffer = vbuffer;
            index_buffer = ibuffer;
        }
        public void prepass() { }

        public void draw() { }
        public void draw_to_light(light light) { }
        public bool in_frustum(BoundingFrustum frustum) { return false; }
    }
    public class render_info_vertex_buffers : render_info {
        public BoundingSphere render_bounds { get; set; }
        public Vector3 render_offset { get; set; } = Vector3.Zero;
        public Vector3 scale { get; set; } = Vector3.One;
        public VertexBuffer[] vertex_buffer { get; }
        public IndexBuffer[] index_buffer { get; }
        public string[] textures { get; set; }


        public Matrix world { get; set; } = Matrix.Identity;
        public Matrix orientation { get; set; } = Matrix.Identity;


        public void prepass() { }
        public void draw() {}
        public void draw_to_light(light light) { }
        public bool in_frustum(BoundingFrustum frustum) { return false; }
    }

    public interface render_info {
        public BoundingSphere render_bounds { get; set; }

        public Vector3 render_offset { get; set; }
        public Vector3 scale { get; set; }

        public Matrix world { get; set; }
        public Matrix orientation { get; set; }

        public string[] textures { get; set; }

        public void prepass();
        public void draw();
        public void draw_to_light(light light);

        public bool in_frustum(BoundingFrustum frustum);
    }
}
