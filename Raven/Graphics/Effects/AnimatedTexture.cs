using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;

namespace Raven.Graphics.Effects;

public class AnimatedTexture {
    public static class Manager {
        static List<AnimatedTexture> animated_textures = new();

        public static void Add(AnimatedTexture texture) {
            animated_textures.Add(texture);
        } 
        public static void Remove(AnimatedTexture texture) {
            animated_textures.Remove(texture);
        }

        public static void GraphicsUpdate(double delta_s) {
            foreach (var animated_texture in animated_textures) {
                animated_texture.GraphicsUpdate();
            }
        }
    }

    public string TextureName { get; set; } = "OnePXWhite";
    public Texture2D Texture => Resources.GetTexture(TextureName);

    public Vector2 ScrollSpeedPerSecond { get; set; } = Vector2.One;
    public Vector2 ScrollPosition {  get; set; } = Vector2.Zero;
    
    public Vector2 ScrollDirection { get; set; } = Vector2.One;
    
    public AnimatedTexture(string TextureName) {
        this.TextureName = TextureName;
    }

    public void GraphicsUpdate() {
        ScrollPosition += (ScrollDirection * ScrollSpeedPerSecond) * (float)Clock.delta_time;
        ScrollPosition = new  Vector2(ScrollPosition.X - MathF.Floor(ScrollPosition.X), ScrollPosition.Y - MathF.Floor(ScrollPosition.Y));
    }
}