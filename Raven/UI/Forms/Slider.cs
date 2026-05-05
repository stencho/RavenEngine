using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Engine.Collision;
using Raven.Graphics.Drawing2D;

namespace Raven.UI.Forms;

public partial class UISlider : IUIForm {
    public Vector2i client_size => client_bottom_right - client_top_left;
    public Vector2i client_top_left => top_left + (Vector2i.One * border_gap);
    public Vector2i client_bottom_right => bottom_right - (Vector2i.One * border_gap);

    private int border_gap = 2;
    
    public float value = 0.5f;

    public float minimum = 0f;
    public float maximum = 1f;

    private float actual_value = 0.5f;

    public UISlider(Vector2i position, Vector2i size, float minimum, float maximum) {
        setup(position.X, position.Y, size.X, size.Y);

        this.minimum = minimum;
        this.maximum = maximum;
        
        reconfigure_client_area();
    }
    
    public void update() {
        
    }

    public void render_internal() {
        Draw2D.fill_rect(Vector2i.Zero, (Vector2i.Down * (client_size.Y)) + (Vector2i.Right * (1f * (client_size.X ))), 
           color_foreground );
    }

    public void draw() {
        if (!visible) return;
        Draw2D.image(client_area, client_top_left, client_size);
        Draw2D.rect(position, position + size + Vector2i.One, UIColors.Foreground, 1f);
    }
    
    public void parent_size_changed(Vector2i new_size) { }
}