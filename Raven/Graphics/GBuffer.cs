using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Engine.Components;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.Drawing3D;
using Raven.Graphics.Effects;

namespace Raven.Graphics {
    [GuidManaged]
    public partial class GBuffer : IDisposable {
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

            public static void ClearAll2DLayers() {
                foreach (var gbuffer in gbuffers.Values) {
                    State.graphics_device.SetRenderTarget(gbuffer.rt_2D);
                    State.graphics_device.BlendState = BlendState.AlphaBlend;
                    State.graphics_device.Clear(Color.Transparent);
                }
            }

            private static Guid gbuffer_ui_draw;
            public static void SelectGBufferForUI(Guid buffer_id) => gbuffer_ui_draw = buffer_id;
            public static void SelectGBufferForUI(GBuffer buffer) => gbuffer_ui_draw = buffer.managed_guid;

            public static void DrawUIToSelectedGBuffer() {
                State.graphics_device.SetRenderTarget(gbuffers[gbuffer_ui_draw].rt_2D);
                State.UI.draw();
            }
            
            public static void DrawAllScreenBuffers() {
                State.graphics_device.SetRenderTarget(null);
                Drawing2D.Draw2D.end();
                    
                var window_size = State.window.ClientBounds.Size.ToVector2i();
                
                
                //Automatically draw all GBuffers which have draw_to_screen enabled, using their screen_draw_info
                foreach (var gbuffer in gbuffers.Values
                             .Where(buffer => buffer.draw_to_screen)
                             .OrderBy(buffer => buffer._screen_draw_info.layer)) {
                
                    
                    if (gbuffer._screen_draw_info.fullscreen) {
                        if (State.super_res_scale <= 1.0f) Drawing2D.Draw2D.begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None);
                        else Drawing2D.Draw2D.begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None);
                        
                        Drawing2D.Draw2D.image(gbuffer.rt_final, Vector2i.Zero, window_size);
                        
                        
                    } else {
                        Drawing2D.Draw2D.begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None);
                        Drawing2D.Draw2D.image(gbuffer.rt_final, gbuffer._screen_draw_info.position,
                            gbuffer._screen_draw_info.size);
                    }

                    Drawing2D.Draw2D.end();
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

        public void enable_screen_draw_fullscreen(int layer = -1) {
            _screen_draw_info = new ScreenDrawInfo();
            _screen_draw_info.layer = layer;
            draw_to_screen = true;
        }
        public void enable_screen_draw(Vector2i position, Vector2i size, int layer = -1) {
            _screen_draw_info = new ScreenDrawInfo(position, size, layer);
            draw_to_screen = true;
        }

        public Action Draw2DOverGame;
        public Action Draw2DOnTop;
        
        //public Action Draw3DOverGame;
        
        public ManagedRT2D rt_diffuse;
        public RenderTarget2D rt_normal;
        public RenderTarget2D rt_depth;
        public RenderTarget2D rt_lighting;
        public RenderTarget2D rt_final;

        public RenderTarget2D rt_composed_half;
        public RenderTarget2D rt_composed;
        public RenderTarget2D rt_2D;

        //public RenderTarget2D rt_fxaa;
        
        private bool FXAA => false;// gvars.get_bool("FXAA") && gvars.get_float("r_resolution_scale") == 1.0f;

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
            Dispose(false);
        }

        public void draw_UI_to_this_buffer() {
            Manager.SelectGBufferForUI(managed_guid);
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
                rt_final.SaveAsPng(fs, rt_final.Width, rt_final.Height);
            }

            screenshot = false;
        }
        
        public void Compose(Camera camera) {
            flip_diffuse();
            
            State.graphics_device.SetRenderTarget(rt_composed);
            
            State.graphics_device.Clear(Color.Transparent);
        
            State.graphics_device.SetVertexBuffer(State.quad_vb);
            State.graphics_device.Indices = State.quad_ib;
        
            State.graphics_device.BlendState = BlendState.AlphaBlend;
        
            State.e_compositor.Parameters["DiffuseLayer"].SetValue(rt_diffuse.FlipFlop || !rt_diffuse.DoubleBuffered ? rt_diffuse.flip : rt_diffuse.flop);
            State.e_compositor.Parameters["DepthLayer"].SetValue(rt_depth);
            State.e_compositor.Parameters["LightLayer"].SetValue(rt_lighting);
            State.e_compositor.Parameters["NormalLayer"].SetValue(rt_normal);
            //State.e_compositor.Parameters["sky_brightness"].SetValue(SkyboxState.sun_moon.sky_brightness);
            //e_compositor.Parameters["atmosphere_color"].SetValue(Skybox.sun_moon.atmosphere_color.ToVector3());
            State.e_compositor.Parameters["buffer"].SetValue(State.draw_debug_buffer);
        
            State.e_compositor.Techniques["draw"].Passes[0].Apply();
            State.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);

            State.graphics_device.SetRenderTarget(rt_final);
            Draw2D.end();
        
            Draw2D.begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None);
            Draw2D.image(rt_composed, Vector2i.Zero, resolution);
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
            rt_composed = new RenderTarget2D(State.graphics_device, (int)(width * res_scale), (int)(height * res_scale), false, SurfaceFormat.Color, DepthFormat.None);
            rt_composed_half = new RenderTarget2D(State.graphics_device, (int)(width / 2), (int)(height / 2), false, SurfaceFormat.Color, DepthFormat.None);
            rt_2D = new RenderTarget2D(State.graphics_device, (int)(width), (int)(height), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            rt_final = new RenderTarget2D(State.graphics_device, (int)(width), (int)(height), false, SurfaceFormat.Color, DepthFormat.None);
            
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

        private void ReleaseUnmanagedResources() {
            Manager.Remove(this.managed_guid);
        }

        private void Dispose(bool disposing) {
            ReleaseUnmanagedResources();
            if (disposing) {
                rt_normal?.Dispose();
                rt_depth?.Dispose();
                rt_lighting?.Dispose();
                rt_final?.Dispose();
                rt_composed_half?.Dispose();
                rt_composed?.Dispose();
                rt_2D?.Dispose();
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
