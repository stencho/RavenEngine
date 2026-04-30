using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Engine.Collision;
using Raven.Engine.Collision.Shapes3D;
using Raven.Engine.Components;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.Skybox;
using Color = Microsoft.Xna.Framework.Color;
using static Raven.Engine.State;

namespace Raven.Graphics.Drawing3D {
    public static class Renderer {
        static volatile List<light> visible_lights = new List<light>();
        static volatile List<Entity> visible_entities = new List<Entity>();

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

        public static void render_scene_to_gbuffer() {
            render_scene_to_gbuffer(camera);    
        }
        
        public static void render_scene_to_gbuffer(Camera camera) {
            render_phase = RenderPhase.build_visibility;
            camera.parent_scene.ClearVisibilityLists();
            camera.parent_scene.BuildVisibilityLists(camera);
            
            build_lighting(camera, camera.gbuffer);
            
            render_phase = RenderPhase.draw_skybox;
            clear_to_skybox(camera, camera.gbuffer);
            
            render_phase = RenderPhase.render_deferred;
            render_deferred(camera);
            draw_lighting(camera, camera.gbuffer);
            
            render_phase = RenderPhase.render_forward;
            render_forward(camera);
            camera.gbuffer.Draw3DOnTop?.Invoke();
            
            graphics_device.SetRenderTarget(camera.gbuffer.rt_2D);
            
            AutoRender2D.Manager.RenderAll();
            camera.gbuffer.Draw2DOverGame?.Invoke();
            
            GBuffer.Manager.DrawUIToSelectedGBuffer();
            camera.gbuffer.Draw2DOnTop?.Invoke();
            
            camera.gbuffer.Compose(camera);
        }
        
        static void render_deferred(Camera camera) {
            Draw3D.batch_draw_setup(camera, camera.gbuffer);
        
            foreach (var e in camera.parent_scene.render_list_deferred.Where(
                         a => a.camera.GUID == camera.GUID)) {
                if (e.entity.Components.HasComponentOfType<RenderModelStatic>(out var rm)) {
                    rm.DrawBasic(camera, camera.gbuffer);
                }
            }
        }

        static void render_forward(Camera camera) {
            
        }
        
        public static void render_entity() {
            if (render_phase == RenderPhase.render_deferred) {
                
            } else if (render_phase == RenderPhase.render_forward) {
                
            }
        }
        
        public static void clear_to_skybox(Camera camera, GBuffer gbuffer) {       
            graphics_device.DepthStencilState = DepthStencilState.None;

            gbuffer.draw_to_bindings();
            e_clear.Parameters["color"].SetValue(SkyboxState.sun_moon.atmosphere_color.ToVector4());
            e_clear.Techniques["Default"].Passes[0].Apply();


            graphics_device.SetVertexBuffer(quad_vb);
            graphics_device.Indices = quad_ib;

            graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);

            graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
            graphics_device.BlendState = BlendState.AlphaBlend;

            e_skybox.Parameters["atmosphere_color"].SetValue(SkyboxState.sun_moon.atmosphere_color.ToVector4());
            e_skybox.Parameters["sky_color"].SetValue(SkyboxState.sun_moon.sky_color.ToVector4());

            e_skybox.Parameters["World"].SetValue(Matrix.CreateScale(1f) * Matrix.Identity);
            e_skybox.Parameters["View"].SetValue(Matrix.CreateLookAt(Vector3.Zero, camera.direction, camera.up_direction));
            e_skybox.Parameters["Projection"].SetValue(camera.projection);

            e_skybox.Techniques["draw"].Passes[0].Apply();

            graphics_device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, SkyboxState.skybox_data, 0, 2, SkyboxState.skybox_indices, 0, SkyboxState.skybox_indices.Length / 3, VertexPositionNormalColorUv.VertexDeclaration);

            graphics_device.DepthStencilState = DepthStencilState.Default;
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

        
        public static void build_lighting(Camera camera, GBuffer buffer) {
            graphics_device.SetRenderTarget(buffer.rt_lighting);
            graphics_device.Clear(SkyboxState.sun_moon.atmosphere_color);

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

        public static void draw_lighting(Camera camera, GBuffer gbuffer) {
            graphics_device.SetRenderTarget(gbuffer.rt_lighting);

            e_pointlight.Parameters["View"].SetValue(camera.view);
            e_pointlight.Parameters["Projection"].SetValue(camera.projection);
            e_pointlight.Parameters["InverseView"].SetValue(Matrix.Invert(camera.view));
            e_pointlight.Parameters["InverseViewProjection"].SetValue(Matrix.Invert(camera.view * camera.projection));

            e_spotlight.Parameters["View"].SetValue(camera.view);
            e_spotlight.Parameters["Projection"].SetValue(camera.projection);
            e_spotlight.Parameters["InverseView"].SetValue(Matrix.Invert(camera.view));
            e_spotlight.Parameters["InverseViewProjection"].SetValue(Matrix.Invert(camera.view * camera.projection));

            graphics_device.BlendState = BlendState.AlphaBlend;
            graphics_device.DepthStencilState = DepthStencilState.DepthRead;

            graphics_device.SetVertexBuffer(quad_vb);
            graphics_device.Indices = quad_ib;

            SkyboxState.sun_moon.configure_dlight_shader(camera, gbuffer, e_directionallight);

            graphics_device.BlendState = DynamicLightRequirements.blend_state;
            graphics_device.DepthStencilState = DepthStencilState.DepthRead;
            
            foreach(light light in visible_lights) {
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
            }

        }
    }
}
