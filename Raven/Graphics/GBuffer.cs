using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Engine.Components;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.Drawing3D;
using Raven.Graphics.Effects;

namespace Raven.Graphics {
    [GuidManagedClass]
    public partial class GBuffer {
        public static partial class Manager {
            public static string ListAllBuffers {
                get {
                    string output = "[GBuffers]\n";
                    foreach (var gbuffer in gbuffers) {
                        output += $"  [{gbuffer.Value.ManagedGuid.ToString()}]\n";
                        output += $"   | resolution > {gbuffer.Value.resolution.ToXString()}\n";
                        output += $"   | resolution scale > {gbuffer.Value.resolution_scale:0.00} ({gbuffer.Value.resolution_super.ToXString()})\n";
                        output += $"   | camera > {(gbuffer.Value.AttachedCameraGuid != Guid.Empty ? gbuffer.Value.AttachedCameraGuid.ToString() : "none")}\n";
                        
                        output += "\n";
                    }
                    return output;
                }
            }

            public static void PrepareAllBuffers(Camera camera) {
                foreach (var gbuffer in gbuffers.Values) {
                    gbuffer.RenderUniverse(camera);
                }
            }
            
            public static void DrawAll3DLayers(Camera camera) {
                foreach (var gbuffer in gbuffers.Values) {
                    gbuffer.Draw3DLayer?.Invoke();
                }
            }
            
            public static void DrawAll2DLayers() {
                foreach (var gbuffer in gbuffers.Values) {
                    gbuffer.Draw2DLayer?.Invoke();
                }
            }

            public static void ClearAll2DLayers() {
                foreach (var gbuffer in gbuffers.Values) {
                    State.graphics_device.SetRenderTarget(gbuffer.rt_2D);
                    State.graphics_device.BlendState = BlendState.AlphaBlend;
                    State.graphics_device.Clear(Color.Transparent);
                }
            }

            public static void ComposeAllLayers(Camera camera) {
                foreach (var gbuffer in gbuffers.Values) {
                    gbuffer.Compose(camera);
                }
            }
            
            public static void DrawAllScreenBuffers() {
                State.graphics_device.SetRenderTarget(null);
                Draw2D.end();
                foreach (var gbuffer in gbuffers.Values
                             .Where(buffer => buffer.draw_to_screen)
                             .OrderBy(buffer => buffer._screen_draw_info.layer)) {
                    
                    if (gbuffer._screen_draw_info.fullscreen) {
                        if (State.super_res_scale <= 1.0f) Draw2D.begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None);
                        else Draw2D.begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None);
                        Draw2D.image(gbuffer.rt_composed, Vector2i.Zero, State.resolution);
                    } else {
                        Draw2D.begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None);
                        Draw2D.image(gbuffer.rt_composed, gbuffer._screen_draw_info.position,
                            gbuffer._screen_draw_info.size);
                    }

                    Draw2D.end();
                }
            }

            public static void UpdateFullscreenBufferResolutions() {
                foreach (var gbuffer in gbuffers.Values
                             .Where(buffer => buffer.draw_to_screen)) {
                    if (gbuffer._screen_draw_info.fullscreen) {
                        gbuffer.change_resolution(State.resolution, State.super_res_scale);
                    }
                }
            }
        }
        
        public struct ScreenDrawInfo {
            public bool fullscreen = false;
            public Vector2i position, size;
            public int layer;

            public ScreenDrawInfo() => fullscreen = true;
            
            public ScreenDrawInfo(Vector2i position, Vector2i size, int layer = -1) {
                this.position = position;
                this.size = size;
                this.layer = layer;
                fullscreen = false;
            }
        }

        bool draw_to_screen = false;
        ScreenDrawInfo _screen_draw_info;

        public ScreenDrawInfo screen_draw_info => _screen_draw_info;

        public bool DrawToScreen => draw_to_screen;

        public void EnableScreenDrawFullscreen(int layer = -1) {
            _screen_draw_info = new ScreenDrawInfo();
            _screen_draw_info.layer = layer;
            draw_to_screen = true;
        }
        public void EnableScreenDraw(Vector2i position, Vector2i size, int layer = -1) {
            _screen_draw_info = new ScreenDrawInfo(position, size, layer);
            draw_to_screen = true;
        }

        public Action Draw3DLayer;
        public Action Draw2DLayer;
        
        public ManagedRT2D rt_diffuse;
        public RenderTarget2D rt_normal;
        public RenderTarget2D rt_depth;
        public RenderTarget2D rt_lighting;
        public RenderTarget2D rt_composed;

        public RenderTarget2D rt_final_half;
        public RenderTarget2D rt_final;
        public RenderTarget2D rt_2D;

        //public RenderTarget2D rt_fxaa;
        
        private bool FXAA => false;// gvars.get_bool("FXAA") && gvars.get_float("super_resolution_scale") == 1.0f;

        private int _width;
        private int _height;

        public Vector2i position;

        public int width => _width;
        public int height => _height;

        public int width_scaled => (int)(_width * resolution_scale);
        public int height_scaled => (int)(_height * resolution_scale);

        public float width_scaled_f => _width * resolution_scale;
        public float height_scaled_f => _height * resolution_scale;

        public Vector2 shader_position_offset;
        public Vector2 shader_size_scale;

        public Vector2i resolution => (Vector2i.UnitX * _width) + (Vector2i.UnitY * _height);
        public Vector2i resolution_super => (Vector2i.UnitX * width_scaled) + (Vector2i.UnitY * width_scaled);

        public float aspect_ratio => (float)width / (float)height;

        private float _resolution_scale;
        public float resolution_scale => _resolution_scale;

        public RenderTargetBinding[] target_bindings { get; private set; }
        public RenderTargetBinding[] target_bindings_dl { get; private set; }
        public RenderTargetBinding[] target_bindings_dln { get; private set; }

        public Viewport viewport;

        protected Guid managed_guid;
        public Guid ManagedGuid => managed_guid;

        public void AttachCamera(Camera camera) => attached_camera = camera.ManagedGuid;
        public Guid AttachedCameraGuid => attached_camera;
        private Guid attached_camera;

        public GBuffer() => managed_guid = Manager.Add(this);
        public GBuffer(int width, int height, float res_scale = 1.0f) {
            managed_guid = Manager.Add(this);
            CreateInPlace(width, height, res_scale);
        }

        ~GBuffer() {
            Manager.Remove(managed_guid);
        }

        public void change_resolution(Vector2i res) {
            _width = res.X;
            _height = res.Y;
            
            CreateInPlace(res.X, res.Y, 1);
        }
        public void change_resolution(Vector2i res, float super_res_scale) {
            _width = res.X;
            _height = res.Y;
            
            CreateInPlace(res.X, res.Y, super_res_scale);
        }
        public void change_resolution(int W, int H) {
            _width = W;
            _height = H;
            
            CreateInPlace(W, H, 1);
        }
        public void change_resolution(int W, int H, float super_res_scale) {
            _width = W;
            _height = H;

            CreateInPlace(W, H, super_res_scale);
        }

        public void draw_to_bindings() {
            target_bindings[0] = !rt_diffuse.FlipFlop || !rt_diffuse.DoubleBuffered ? rt_diffuse.flip : rt_diffuse.flop;
            target_bindings[1] = rt_normal;
            target_bindings[2] = rt_depth;
            target_bindings[3] = rt_lighting;
            State.graphics_device.SetRenderTargets(target_bindings);
        }

        public void RenderUniverse(Camera camera) {
            camera.update_projection(resolution);
            camera.update();
            
            Renderer.create_visibility_lists(camera);
        
            Renderer.build_lighting(camera, this);
        
            Renderer.clear_to_skybox(camera, this);
            
            State.universe.RenderUniverse(camera, this);
        }

        public void flip_diffuse() {
            rt_diffuse.FlipTargets();    
        }

        private bool screenshot = false;
        public void TakeScreenshot() {
            screenshot = true;
        }

        void write_screenshot() {
            System.Console.WriteLine(Directory.GetCurrentDirectory());
            if (!Directory.Exists("scr")) Directory.CreateDirectory("scr");

            using (FileStream fs = new FileStream("scr/scr" + DateTime.Now.ToFileTime() + ".png", FileMode.Create)) {
                rt_composed.SaveAsPng(fs, rt_composed.Width, rt_composed.Height);
            }

            screenshot = false;
        }
        
        public void Compose(Camera camera) {
            flip_diffuse();
            
            State.graphics_device.SetRenderTarget(rt_final);
            
            State.graphics_device.Clear(Color.Transparent);
        
            State.graphics_device.SetVertexBuffer(State.quad_vb);
            State.graphics_device.Indices = State.quad_ib;
        
            State.graphics_device.BlendState = BlendState.AlphaBlend;
        
            State.e_compositor.Parameters["DiffuseLayer"].SetValue(rt_diffuse.FlipFlop || !rt_diffuse.DoubleBuffered ? rt_diffuse.flip : rt_diffuse.flop);
            State.e_compositor.Parameters["DepthLayer"].SetValue(rt_depth);
            State.e_compositor.Parameters["LightLayer"].SetValue(rt_lighting);
            State.e_compositor.Parameters["NormalLayer"].SetValue(rt_normal);
            //State.e_compositor.Parameters["sky_brightness"].SetValue(State.Skybox.sun_moon.sky_brightness);
            //e_compositor.Parameters["atmosphere_color"].SetValue(Skybox.sun_moon.atmosphere_color.ToVector3());
            State.e_compositor.Parameters["buffer"].SetValue(State.draw_debug_buffer);
        
            State.e_compositor.Techniques["draw"].Passes[0].Apply();
            State.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);

            State.graphics_device.SetRenderTarget(rt_composed);
            Draw2D.end();
        
            Draw2D.begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None);
            Draw2D.image(rt_final, Vector2i.Zero, resolution);
            Draw2D.end();
            
            Draw2D.begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None);
            Draw2D.image(rt_2D, Vector2i.Zero, resolution);
            Draw2D.end();
            
            if (screenshot) write_screenshot();
        }
        
        public void CreateInPlace(int width, int height, float res_scale = 1.0f) {
            target_bindings = new RenderTargetBinding[4];
            target_bindings_dl = new RenderTargetBinding[2];
            target_bindings_dln = new RenderTargetBinding[3];
            
            position = Vector2i.Zero;
            
            this._width = width; this._height = height;

            _resolution_scale = res_scale;

            viewport = new Viewport(position.X, position.Y, width, height);

            shader_position_offset = Vector2.Zero;
            shader_size_scale = Vector2.One;

            if (rt_diffuse != null) {
                ManagedRT2D.Manager.Remove(rt_diffuse.ManagedGuid);
                rt_diffuse = null;
            }

            rt_diffuse = new ManagedRT2D((int)(width * res_scale), (int)(height * res_scale), true, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            rt_normal = new RenderTarget2D(State.graphics_device, (int)(width * res_scale), (int)(height * res_scale), false, SurfaceFormat.Vector4, DepthFormat.None, 0, RenderTargetUsage.PlatformContents);
            rt_depth = new RenderTarget2D(State.graphics_device, (int)(width * res_scale), (int)(height * res_scale), false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PlatformContents);
            rt_lighting = new RenderTarget2D(State.graphics_device, (int)(width * res_scale), (int)(height * res_scale), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PlatformContents);
            rt_final = new RenderTarget2D(State.graphics_device, (int)(width * res_scale), (int)(height * res_scale), false, SurfaceFormat.Color, DepthFormat.None);
            rt_final_half = new RenderTarget2D(State.graphics_device, (int)(width / 2), (int)(height / 2), false, SurfaceFormat.Color, DepthFormat.None);
            rt_2D = new RenderTarget2D(State.graphics_device, (int)(width), (int)(height), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            rt_composed = new RenderTarget2D(State.graphics_device, (int)(width), (int)(height), false, SurfaceFormat.Color, DepthFormat.None);
            
            target_bindings[0] = !rt_diffuse.FlipFlop || !rt_diffuse.DoubleBuffered ? rt_diffuse.flip : rt_diffuse.flop;
            target_bindings[1] = rt_normal;
            target_bindings[2] = rt_depth;
            target_bindings[3] = rt_lighting;
            
            target_bindings_dl[0] = rt_depth;
            target_bindings_dl[1] = rt_lighting;
            
            target_bindings_dl[0] = rt_depth;
            target_bindings_dln[1] = rt_lighting;
            target_bindings_dln[2] = rt_normal;

            if (FXAA) {
                //rt_fxaa = new RenderTarget2D(gd, (int)(width * res_scale), (int)(height * res_scale), false, SurfaceFormat.Color, DepthFormat.None);
            }
        }

        public void EnableFXAA(bool enable = true) {
            if (enable && !FXAA && resolution_scale == 1.0f) {
                //rt_fxaa = new RenderTarget2D(gd, _width, _height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                //gvars.set("FXAA", true);
            }
            else if (!enable) {
                //rt_fxaa.Dispose();
                //rt_fxaa = null;
                //gvars.set("FXAA", false);
            }
        }
    }
}
