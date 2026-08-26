using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Graphics.Effects;

namespace Raven.Graphics.Drawing2D;

public enum FlipMode { // is the greatest
    None, Horizontal, Vertical, Both
}

public struct RenderPacket2D {
    public Vector2i position = Vector2i.Zero;
    public Vector2i size = Vector2i.Zero;
    
    public float rotation = 0f;
    public FlipMode flip_mode = FlipMode.None;
    
    public Guid effect_id;
    public Action<ManagedEffect>? configure_effect;
    
    public Texture2D texture;

    public RenderTarget2D target;
    
    public RenderPacket2D(Texture2D texture, Vector2i size, Vector2i position, Guid effect_id) {
        position = default;
        size = default;
        this.effect_id = effect_id;
        this.texture = texture;
    }

    public void use_target() => target.use();
    public void apply_effect() => ManagedEffect.Manager.managedeffects[effect_id].apply_passes();
}

public class Draw2DPacketEffect : ManagedEffect {
    public Draw2DPacketEffect() : base(Resources.GetShader("draw_2d")) {}
    
    public void draw_packet(RenderPacket2D packet) {
        
    }
}

public static class NewGenDraw2D {
    public static class Batching {
        private static List<RenderPacket2D> packets = new();

        public static void Queue(RenderPacket2D packet) {
            packets.Add(packet);
        }

        static void create_batches() {
            
        }
        
        public static void render_all_2D() {
            
        }
    }
    
    private static Draw2DPacketEffect draw_2d;
    
    public static void Line(Vector2i A, Vector2i B, Color color, float thickness) {
        var tan = B - A;
        var rot = (float)Math.Atan2(tan.Y, tan.X);

        var middlePoint = new Vector2(0, 0.5f);
        var scale = new Vector2(tan.Length(), thickness);
        
        //Batching.Queue();
    }
}
