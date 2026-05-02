using System;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;

namespace Raven.Graphics.Drawing2D;

[GuidManaged]
public partial class ManagedRT2D {
    public static partial class Manager {
        public static string ListAllBuffers {
            get {
                string output = "[ManagedRT2Ds]\n";
                foreach (var rt2d in managedrt2ds) {
                    output += $"  [{rt2d.Value.ManagedGuid}]\n";
                    output += $"   | resolution > {rt2d.Value.Resolution.ToXString()}\n";
                    output += $"   | double buffered > {rt2d.Value.double_buffered}\n";
                    output += $"   | depth format > {rt2d.Value.depth_format.ToString()}\n";
                    output += $"   | surface format > {rt2d.Value.surface_format.ToString()}\n";
                    
                    output += "\n";
                }
                return output;
            }
        }

        public static void FlipAll() {
            foreach (var rt2d in managedrt2ds) {
                if (rt2d.Value.DoubleBuffered) 
                    rt2d.Value.FlipTargets();
            }
        }    
    }
    
    protected Guid managed_guid;
    public Guid ManagedGuid => managed_guid;
    
    public RenderTarget2D flip, flop;

    public RenderTarget2D present => flipflop || !double_buffered ? flip : flop;
    public RenderTarget2D offscreen => !flipflop || !double_buffered ? flip : flop;
    
    bool flipflop = true;
    public bool FlipFlop => flipflop;
    public void FlipTargets() => flipflop = !flipflop;
    
    private bool mipmap = false;

    public bool MipMap {
        get {
            return mipmap;
        }
        set {
            mipmap = value;
            update_render_targets();
        }
    }
    
    SurfaceFormat surface_format;
    DepthFormat depth_format;
    
    //RESOLUTION HANDLING
    Vector2i resolution;
    public int Width => resolution.X; public int Height => resolution.Y;
    public Vector2i Resolution {
        get { return resolution; }
        set {
            resolution = value;
            update_render_targets();
        }
    }

    //DOUBLE BUFFERING
    bool double_buffered = false;
    public bool DoubleBuffered {
        get { return double_buffered; }
        set {
            if (double_buffered && !value) {
                flop = null;
            } else if (!double_buffered && value) {
                flop = new RenderTarget2D(State.graphics_device, Width, Height, mipmap, surface_format, depth_format);
            }
            double_buffered = value;
        }
    }

    public ManagedRT2D(int width, int height, bool double_buffered, bool mipmap = false, SurfaceFormat surface_format = SurfaceFormat.Color, DepthFormat depth_format = DepthFormat.Depth24Stencil8) {
        resolution =  new Vector2i(width, height);
        this.double_buffered = double_buffered;
        this.mipmap = mipmap;
        this.surface_format = surface_format;
        this.depth_format = depth_format;
        
        update_render_targets();
        
        managed_guid = Manager.Add(this);
    }

    ~ManagedRT2D() {
        Manager.Remove(ManagedGuid);
    }
    
    private void update_render_targets() {
        flip = new RenderTarget2D(State.graphics_device, Width, Height, mipmap, surface_format, depth_format, 0, RenderTargetUsage.DiscardContents);
        if (double_buffered) flop = new RenderTarget2D(State.graphics_device, Width, Height, mipmap, surface_format, depth_format, 0, RenderTargetUsage.DiscardContents);
        else flop = null;
    }
}