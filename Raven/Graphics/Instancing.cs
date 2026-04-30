using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Raven.Engine;

namespace Raven.Graphics {
    public static class Instancing {
        
        public static InstancedVertexData[] Instance_buffer_temp { get => instance_buffer_temp; set => instance_buffer_temp = value; }
        
        public struct InstancedVertex : IVertexType {
            public VertexDeclaration VertexDeclaration => throw new NotImplementedException();

            public static readonly VertexElement[] VertexElements = {
                //R1 - R4
                new VertexElement(sizeof(float) * 0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
                new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3),
                new VertexElement(sizeof(float) * 12, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4),
                new VertexElement(sizeof(float) * 16, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 5),
                new VertexElement(sizeof(float) * 20, VertexElementFormat.Color, VertexElementUsage.Color, 0)
            };

            //boilerplate
            public static VertexDeclaration Declaration {
                get { return new VertexDeclaration(VertexElements); }
            }
            VertexDeclaration IVertexType.VertexDeclaration {
                get { return new VertexDeclaration(VertexElements); }
            }
        }


        //packed world matrix
        [StructLayout(LayoutKind.Explicit)]
        public struct InstancedVertexData {
            [FieldOffset(0)] public Vector4 tex_offset_flip;
            [FieldOffset(sizeof(float) * 4)] public Vector4 r1;
            [FieldOffset(sizeof(float) * 8)] public Vector4 r2;
            [FieldOffset(sizeof(float) * 12)] public Vector4 r3;
            [FieldOffset(sizeof(float) * 16)] public Vector4 r4;
            [FieldOffset(sizeof(float) * 20)] public Color tint;
        }
        
        public enum transform_type {
            scale_orientation_translation,
            billboard,
            constrained_billboard
        }

        public struct instance {
            public Matrix world;
            public Vector2 texture_offset;
            public bool flip_h;
            public bool flip_v;
            public Color tint;
        }

        private static InstancedVertexData[] instance_buffer_temp;
        private static DynamicVertexBuffer temp_return_buffer;

        public static DynamicVertexBuffer pack_instance_data(List<instance> instances) {
            instance_buffer_temp = new InstancedVertexData[instances.Count];

            for (int y = 0; y < instance_buffer_temp.Length; y++) {
                instance_buffer_temp[y].tex_offset_flip.X = instances[y].texture_offset.X;
                instance_buffer_temp[y].tex_offset_flip.Y = instances[y].texture_offset.Y;
                instance_buffer_temp[y].tex_offset_flip.Z = (instances[y].flip_h ? 1 : 0);
                instance_buffer_temp[y].tex_offset_flip.W = (instances[y].flip_v ? 1 : 0);

                instance_buffer_temp[y].r1.X = instances[y].world.M11;
                instance_buffer_temp[y].r1.Y = instances[y].world.M21;
                instance_buffer_temp[y].r1.Z = instances[y].world.M31;
                instance_buffer_temp[y].r1.W = instances[y].world.M41;

                instance_buffer_temp[y].r2.X = instances[y].world.M12;
                instance_buffer_temp[y].r2.Y = instances[y].world.M22;
                instance_buffer_temp[y].r2.Z = instances[y].world.M32;
                instance_buffer_temp[y].r2.W = instances[y].world.M42;

                instance_buffer_temp[y].r3.X = instances[y].world.M13;
                instance_buffer_temp[y].r3.Y = instances[y].world.M23;
                instance_buffer_temp[y].r3.Z = instances[y].world.M33;
                instance_buffer_temp[y].r3.W = instances[y].world.M43;

                instance_buffer_temp[y].r4.X = instances[y].world.M14;
                instance_buffer_temp[y].r4.Y = instances[y].world.M24;
                instance_buffer_temp[y].r4.Z = instances[y].world.M34;
                instance_buffer_temp[y].r4.W = instances[y].world.M44;

                instance_buffer_temp[y].tint = instances[y].tint;

                //instance_buffer_temp[y].opacity = instances[y].opacity;
            }

            temp_return_buffer = new DynamicVertexBuffer(State.graphics_device, InstancedVertex.Declaration, instance_buffer_temp.Length, BufferUsage.WriteOnly);

            temp_return_buffer.SetData(instance_buffer_temp);

            return temp_return_buffer;
        }

        public class model_instance_info {
            public instance instance => new instance() { world = world, texture_offset = texture_offset, flip_h = flip_texture_h, flip_v = flip_texture_v, tint = tint };

            public Matrix orientation = Matrix.Identity;

            public transform_type world_type = transform_type.scale_orientation_translation;

            public Matrix world {
                get {
                    switch (world_type) {
                        case transform_type.scale_orientation_translation:
                            return world_normal;
                        case transform_type.billboard:
                            return world_billboard;
                        case transform_type.constrained_billboard:
                            return world_constrained_billboard;

                        default: return world_normal;
                    }
                }
            }

            private Matrix world_normal => Matrix.Identity * Matrix.CreateScale(scale) * orientation * Matrix.CreateTranslation(position);

            private Matrix world_billboard => Matrix.Identity * Matrix.CreateScale(scale) * Matrix.CreateBillboard(position, Camera.current_render_camera.position, Camera.current_render_camera.up_direction, Camera.current_render_camera.direction);

            private Matrix world_constrained_billboard => Matrix.Identity * Matrix.CreateScale(scale) * Matrix.CreateBillboard(position, Camera.current_render_camera.position, Vector3.Up, Camera.current_render_camera.direction);

            public Vector3 position = Vector3.Zero;
            public Vector3 scale = Vector3.One;

            public Vector3 velocity_normal = Vector3.Zero;
            public float velocity_delta = 0;
            public Vector4 velocity_v4() { return new Vector4(velocity_normal, velocity_delta); }

            public Color tint = Color.White;
            //public Vector3 bounds;

            public Vector2 texture_offset = Vector2.Zero;

            public int system_id;

            public float camera_distance;
            public bool camera_frustum_hit = false;
            public void update_camera_distance_and_frustum(Camera cam) {
                camera_distance = Vector3.Distance(cam.position, this.position);

                if (cam.frustum.Contains(position) != ContainmentType.Disjoint) {
                    camera_frustum_hit = true;
                }
            }

            public void update(Camera cam) {
                update_camera_distance_and_frustum(cam);


            }

            public float opacity;
            public float texture_rotation;

            public bool flip_texture_h;
            public bool flip_texture_v;

        }
    }
}
