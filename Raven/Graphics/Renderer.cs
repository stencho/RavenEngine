using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Engine.Collision;
using Raven.Engine.Collision.Shapes3D;
using Raven.Engine.Components;
using Raven.Engine.Geometry3D;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.Drawing3D.Effects;
using Raven.Graphics.Effects;
using Raven.Graphics.Geometry2D;
using Raven.Graphics.Skybox;
using Raven.UI;
using Color = Microsoft.Xna.Framework.Color;
using static Raven.Engine.State;

namespace Raven.Graphics.Drawing3D {
    public static class Renderer {
        public static Effect e_directionallight;
        static volatile List<light> visible_lights = new List<light>();
        static volatile List<Entity> visible_entities = new List<Entity>();

        public static ZPrepass z_prepass = new ZPrepass();
        public static DrawBuffersDeferred deferred = new DrawBuffersDeferred();
        public static DrawBuffersForward forward = new DrawBuffersForward();
        public static Compositor compositor = new Compositor();
        
        public static BlendState light_accumulation_blend_state = new BlendState {
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.Add,

            AlphaSourceBlend = Blend.Zero,
            AlphaDestinationBlend = Blend.One, 
            AlphaBlendFunction = BlendFunction.Add
        };
        
        enum RenderPhase {
            build_visibility,
            build_lighting,
            draw_skybox,
            render_deferred,
            render_forward,
            render_2D,
            render_UI,
            compose,
            sleep
        }

        private static RenderPhase render_phase = RenderPhase.sleep;
        
        public static volatile string VisibilityString = "";
        
        public class render_obj {
            public VertexBuffer vertex_buffer;
            public IndexBuffer index_buffer;

            public Matrix world;

            public Texture2D texture;
        }

        private static Camera camera => Camera.current_render_camera;

        public static class fullscreen_quad {
            static VertexPositionTexture[] vb_data = {
                new VertexPositionTexture(Vector3.Up + Vector3.Left, Vector2.Zero),
                new VertexPositionTexture(Vector3.Up + Vector3.Right, Vector2.UnitX),
                new VertexPositionTexture(Vector3.Down + Vector3.Left, Vector2.UnitY),
                new VertexPositionTexture(Vector3.Down + Vector3.Right, Vector2.One)
            };
            static int[] ib_data = { 0, 1, 2, 1, 3, 2 };
            
            static VertexBuffer quad_vb;
            static IndexBuffer quad_ib;

            public static VertexBuffer vertex_buffer => quad_vb;
            public static IndexBuffer index_buffer => quad_ib;
            
            public static void Load() {
                quad_vb = new VertexBuffer(graphics_device, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.None);
                quad_ib = new IndexBuffer(graphics_device, IndexElementSize.ThirtyTwoBits, 6, BufferUsage.None);
                
                quad_vb.SetData(vb_data);
                quad_ib.SetData(ib_data);
            }

            public static void UseBuffers() {
                graphics_device.SetVertexBuffer(vertex_buffer);
                graphics_device.Indices = index_buffer;
            }
        }

        public static void Load() {
            fullscreen_quad.Load();
        }
        
        public static void render_scene_to_gbuffer() {
            render_scene_to_gbuffer(camera);    
        }
        
        public static void render_scene_to_gbuffer(Camera camera) {
            render_phase = RenderPhase.build_visibility;
            camera.parent_scene.ClearVisibilityLists();
            camera.parent_scene.BuildVisibilityLists(camera);
            
            //build shadows
            
            compositor.clear_buffers(camera); 
            render_prepass(camera);
            
            render_phase = RenderPhase.draw_skybox;

            camera.environment.skybox.render(camera);
            
            render_phase = RenderPhase.render_deferred;
            render_deferred(camera);
            
            camera.environment.sunlight.draw_lighting(camera);
            
            //draw deferred lighting here
            
            compositor.compose(camera);
            
            render_phase = RenderPhase.render_forward;
            render_forward(camera);
            
            camera.gbuffer.draw_over_game_layer();
            
            render_phase = RenderPhase.render_forward;
            
            graphics_device.SetRenderTarget(camera.gbuffer.rt_2D);
            graphics_device.Clear(Color.Transparent);
            
            UIWindowManager.Manager.render_UI_for_current_buffer(camera.gbuffer.GUID);
            camera.gbuffer.draw_on_top_layer();
            
            compositor.finalize(camera);
        }
        
        static List<Component> prepassed_components = new List<Component>();
        static List<(float distance, Component c)> prepassed_components_need_forward_render = new();

        static void render_prepass(Camera camera) {
            prepassed_components.Clear();
            prepassed_components_need_forward_render.Clear();
            
            z_prepass.batch_render_setup(camera);
            foreach (var e in camera.parent_scene.entity_visibility_list.Where(
                         a => a.camera.GUID == camera.GUID)) {
                e.entity.Components.ForAllComponentsWithFlag(ComponentFlags.Render, component => {
                    var has_opacity_data = component.TryGetData("Opacity", out float opacity);
                    var has_forward_flag = component.TryGetData("HasTransparentTexture", out bool always_render_foward);
                    
                    if ((has_opacity_data && opacity < 1.0f) ||  (has_forward_flag && always_render_foward)) { 
                        prepassed_components_need_forward_render.Add((e.distance, component));
                    } else {
                        component.RenderZPrePass(camera);
                        prepassed_components.Add(component);
                    }
                });
            }
        }
        
        //VertexBuffer deferred_batching_vertex_buffer 
        
        static void render_deferred(Camera camera) {
            deferred.batch_render_setup(camera);
            foreach (var component in prepassed_components) {
                component.Render(camera);                
            }
        }

        static void render_forward(Camera camera) {
            prepassed_components_need_forward_render = 
                prepassed_components_need_forward_render.OrderByDescending(a => a.distance).ToList();
            
            forward.batch_render_setup(camera);
            foreach (var component in prepassed_components_need_forward_render) {
                component.c.RenderForward(camera);                
            }
        }
        
        public static void render_entity() {
            if (render_phase == RenderPhase.render_deferred) {
                
            } else if (render_phase == RenderPhase.render_forward) {
                
            }
        }
        
        public static void update_point_light(ref light l, Camera camera) {
            l.world = Matrix.CreateScale(l.point_info.radius) * Matrix.CreateTranslation(l.point_info.position);
        }

        public static void update_spot_light(ref light l, Camera camera) {
            spot_info si = l.spot_info;

            si.view
                = Matrix.CreateLookAt(l.position, l.position + si.orientation.Forward, si.orientation.Up);
            si.projection
                = Matrix.CreatePerspectiveFieldOfView(si.fov, 1f, si.near_clip, si.far_clip);

            si.radial_scale = (float)Math.Tan((double)si.fov) * si.far_clip;

            si.actual_scale = Matrix.CreateScale(si.radial_scale, si.radial_scale, si.far_clip);


            si.bounds = new BoundingFrustum(si.view * si.projection);

            l.spot_info = si;
            l.world = si.actual_scale * si.orientation * Matrix.CreateTranslation(si.position);
        }

        
        private static void build_shadows(Camera camera) {
            foreach (light light in visible_lights) {
                // need to iterate through each light's visibility list
                // and render out their depth textures
                
                /*
                if (light.type == LightType.SPOT) {
                    graphics_device.SetRenderTarget(light.spot_info.depth_map);

                    graphics_device.BlendState = BlendState.Opaque;
                    graphics_device.DepthStencilState = DepthStencilState.Default;

                    graphics_device.Clear(Color.Transparent);

                    e_exp_light_depth.Parameters["View"].SetValue(light.spot_info.view);
                    e_exp_light_depth.Parameters["Projection"].SetValue(light.spot_info.projection);
                    //create_spot_light_visibility_list(map, light);

                    e_exp_light_depth.Parameters["LightPosition"].SetValue(light.position);
                    e_exp_light_depth.Parameters["LightDirection"].SetValue(light.spot_info.orientation.Forward);
                    e_exp_light_depth.Parameters["LightClip"].SetValue(light.spot_info.far_clip);
                    e_exp_light_depth.Parameters["C"].SetValue(light.spot_info.C);

                    foreach (int i in light.spot_info.visible) {
                        //map.game_objects[i].draw_to_light(light);
                    }


                } else if (light.type == LightType.POINT) {
                }*/
            }
        }
        
        private static void draw_lighting(Camera camera) {
            foreach(light light in visible_lights) {
                /* TURN E_SPOTLIGHT AND E_POINTLIGHT INTO MANAGEDEFFECTS
                    ALSO ADD SHADOWS TO POINT LIGHTS
                if (light.type == LightType.SPOT) {
                    e_spotlight.Parameters["World"].SetValue(light.world);

                    e_spotlight.Parameters["NORMAL"].SetValue(gbuffer.rt_normal);
                    e_spotlight.Parameters["DEPTH"].SetValue(gbuffer.rt_depth);
                    e_spotlight.Parameters["COOKIE"].SetValue(light.spot_info.cookie);
                    e_spotlight.Parameters["SHADOW"].SetValue(light.spot_info.depth_map);

                    e_spotlight.Parameters["LightViewProjection"].SetValue(light.spot_info.view * light.spot_info.projection);
                    e_spotlight.Parameters["LightColor"].SetValue(light.color.ToVector4());
                    e_spotlight.Parameters["LightPosition"].SetValue(light.position);
                    e_spotlight.Parameters["LightDirection"].SetValue(light.spot_info.orientation.Forward);
                    e_spotlight.Parameters["LightAngleCos"].SetValue(light.spot_info.angle_cos);
                    e_spotlight.Parameters["LightClip"].SetValue(light.spot_info.far_clip);
                    e_spotlight.Parameters["DepthBias"].SetValue(light.spot_info.bias);
                    e_spotlight.Parameters["C"].SetValue(light.spot_info.C);

                    e_spotlight.Parameters["Shadows"].SetValue(light.spot_info.shadows);

                    graphics_device.SetVertexBuffer(Resources.GetModel("cone").Meshes[0].MeshParts[0].VertexBuffer);
                    graphics_device.Indices = Resources.GetModel("cone").Meshes[0].MeshParts[0].IndexBuffer;

                    float SL = Math.Abs(Vector3.Dot(Vector3.Normalize(light.position - camera.position), light.spot_info.orientation.Forward));

                    if (SL <= (light.spot_info.angle_cos)) {
                        graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
                    } else {
                        graphics_device.RasterizerState = RasterizerState.CullClockwise;
                    }

                    e_spotlight.CurrentTechnique.Passes[0].Apply();
                    graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Resources.GetModel("cone").Meshes[0].MeshParts[0].VertexBuffer.VertexCount);


                } else if (light.type == LightType.POINT) {
                    e_pointlight.Parameters["World"].SetValue(
                    Matrix.CreateScale(light.point_info.radius) * Matrix.CreateTranslation(light.point_info.position));

                    e_pointlight.Parameters["NORMAL"].SetValue(gbuffer.rt_normal);
                    e_pointlight.Parameters["DEPTH"].SetValue(gbuffer.rt_depth);

                    e_pointlight.Parameters["LightColor"].SetValue(light.color.ToVector4());
                    e_pointlight.Parameters["LightPosition"].SetValue(light.position);
                    e_pointlight.Parameters["LightIntensity"].SetValue(1f);
                    e_pointlight.Parameters["LightRadius"].SetValue(light.point_info.radius);

                    e_pointlight.Parameters["Shadows"].SetValue(false);
                    e_pointlight.Parameters["quantized"].SetValue(light.point_info.quantize);

                    graphics_device.SetVertexBuffer(Resources.GetModel("sphere").Meshes[0].MeshParts[0].VertexBuffer);
                    graphics_device.Indices =       Resources.GetModel("sphere").Meshes[0].MeshParts[0].IndexBuffer;

                    Vector3 sdiff = (camera.position) - light.position;
                    float skyCameraToLight = (float)Math.Sqrt((float)Vector3.Dot(sdiff, sdiff)) / 100.0f;

                    if (skyCameraToLight <= light.point_info.radius) {
                        graphics_device.RasterizerState = RasterizerState.CullClockwise;
                    } else {
                        graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
                    }

                    e_pointlight.CurrentTechnique.Passes[0].Apply();
                    graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Resources.GetModel("sphere").Meshes[0].MeshParts[0].VertexBuffer.VertexCount);
                }
                */
            }
        }
    }
}
