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
using Raven.Graphics.Geometry2D;

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
            
            public static void DrawAllScreenBuffers() {
                Draw2D.end();
                
                State.graphics_device.SetRenderTarget(null);
                    
                //Automatically draw all GBuffers which have draw_to_screen enabled, using their screen_draw_info
                foreach (var gbuffer in gbuffers.Values
                             .Where(buffer => buffer.draw_to_screen)
                             .OrderBy(buffer => buffer._screen_draw_info.layer)) {
                    Renderer.compositor.draw_buffer_to_screen(gbuffer);
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

        private DrawShapesToSurface shape_drawing;
        
        public Action<DrawShapesToSurface> Draw2DOverGame;
        public void draw_over_game_layer() => Draw2DOverGame?.Invoke(shape_drawing);
        
        public Action<DrawShapesToSurface> Draw2DOnTop;
        public void draw_on_top_layer() => Draw2DOnTop?.Invoke(shape_drawing);
        
        //public Action Draw3DOverGame;
        
        public RenderTarget2D rt_diffuse;
        public RenderTarget2D rt_normal;
        public RenderTarget2D rt_depth;
        public RenderTarget2D rt_lighting;
        public RenderTarget2D rt_final;
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
        public RenderTargetBinding[] target_bindings_for_clearing { get; private set; }
        public RenderTargetBinding[] target_bindings_for_skybox { get; private set; }
        public RenderTargetBinding[] target_bindings_dl { get; private set; }
        public RenderTargetBinding[] target_bindings_dln { get; private set; }

        public Viewport viewport;

        protected Guid managed_guid;
        public Guid ManagedGuid => managed_guid;

        public void AttachCamera(Camera camera) => attached_camera = camera.ManagedGuid;
        public Guid AttachedCameraGuid => attached_camera;
        private Guid attached_camera;

        public GBuffer() {
            managed_guid = Manager.Add(this);
            CreateInPlace(width, height, State.super_res_scale, false);
        }

        public GBuffer(int width, int height) {
            managed_guid = Manager.Add(this);
            CreateInPlace(width, height, State.super_res_scale, false);
        }
        public GBuffer(int width, int height, float res_scale, bool double_buffered) {
            managed_guid = Manager.Add(this);
            CreateInPlace(width, height, res_scale, double_buffered);
        }

        ~GBuffer() {
            Dispose(false);
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
            target_bindings[0] = rt_diffuse;
            target_bindings[1] = rt_normal;
            target_bindings[2] = rt_lighting;
            State.graphics_device.SetRenderTargets(target_bindings);
        }
        
        public void draw_to_bindings_for_skybox() {
            target_bindings_for_skybox[0] = rt_diffuse;
            target_bindings_for_skybox[1] = rt_lighting;
            State.graphics_device.SetRenderTargets(target_bindings_for_skybox);
        }
        
        public void draw_to_bindings_for_clear() {
            target_bindings_for_clearing[0] = rt_diffuse;
            target_bindings_for_clearing[1] = rt_normal;
            target_bindings_for_clearing[2] = rt_depth;
            target_bindings_for_clearing[3] = rt_lighting;
            State.graphics_device.SetRenderTargets(target_bindings_for_clearing);
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
        
        private bool use_srgb = false;
        private bool double_buffered = false;
        
        public void CreateInPlace(int width, int height, float res_scale = 1.0f, bool double_buffered = false) {
            this.double_buffered = double_buffered;
            
            target_bindings = new RenderTargetBinding[3];
            target_bindings_for_clearing = new RenderTargetBinding[4];
            target_bindings_for_skybox = new RenderTargetBinding[2];
            target_bindings_dl = new RenderTargetBinding[2];
            target_bindings_dln = new RenderTargetBinding[3];
            
            position = Vector2i.Zero;
            
            this._width = width; this._height = height;

            _resolution_scale = res_scale;

            viewport = new Viewport(position.X, position.Y, width, height);

            shader_position_offset = Vector2.Zero;
            shader_size_scale = Vector2.One;

            if (rt_diffuse != null) {
                rt_diffuse = null;
            }

            rt_diffuse = RenderTargetEx.create(width * res_scale, height * res_scale, SurfaceFormat.HalfVector4);
            rt_lighting = RenderTargetEx.create(width * res_scale, height * res_scale, SurfaceFormat.HalfVector4);
            rt_composed = RenderTargetEx.create(width * res_scale, height * res_scale, SurfaceFormat.HalfVector4);
            rt_2D = RenderTargetEx.create(width * res_scale, height * res_scale, SurfaceFormat.HalfVector4);
            rt_final = RenderTargetEx.create(width * res_scale, height * res_scale, SurfaceFormat.HalfVector4);
            
            rt_normal = RenderTargetEx.create(width * res_scale, height * res_scale, SurfaceFormat.Vector4);
            rt_depth = RenderTargetEx.create(width * res_scale, height * res_scale, SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8);
            
            shape_drawing = new DrawShapesToSurface(() => rt_2D.Bounds.Size.ToVector2i());
            
            target_bindings[0] = rt_diffuse;
            target_bindings[1] = rt_normal;
            target_bindings[2] = rt_lighting;
            
            target_bindings_for_skybox[0] = rt_diffuse;
            target_bindings_for_skybox[1] = rt_lighting;
            
            target_bindings_for_clearing[0] = rt_diffuse;
            target_bindings_for_clearing[1] = rt_normal;
            target_bindings_for_clearing[2] = rt_depth;
            target_bindings_for_clearing[3] = rt_lighting;
            
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
