using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Raven.Engine;
using Raven.Engine.Collision;
using Raven.Graphics.Drawing2D;
using Raven.UI;

namespace Raven.UI.Forms;

public partial class Panel : IUIForm {
    public Vector2i client_size => size;
    public Vector2i client_top_left => Vector2i.Zero;
    public Vector2i client_bottom_right => size;
    
    public Panel(Vector2i position, Vector2i size) {
        setup(position.X, position.Y, size.X, size.Y);
        reconfigure_client_area();
    }
    
    public void update() {
        update_collision();
        test_mouse();
        foreach (var subform in subforms) {
            subform.update();
        }
    }

    public void render_internal() {
        foreach (var subform in subforms) {
            if (subform.use_internal_rendering) {
                State.graphics_device.SetRenderTarget(subform.client_area);
                State.graphics_device.Clear(Color.Transparent);
                subform.render_internal();
                Draw2D.end();
            }
        }
        Draw2D.end();
        State.graphics_device.SetRenderTarget(client_area);
        State.graphics_device.Clear(color_background);
        
        //Draw2D.fill_rect_dither(Vector2i.Zero, client_top_left, client_size);
        
        foreach (var subform in subforms) {
            subform.draw();
        }
        Draw2D.rect(Vector2i.One, client_size, color_foreground, 1f);
    }

    public void draw() {
        Draw2D.image(client_area, position + client_top_left, size);
    }
}

public partial class TabbedPanel : IUIForm {
    public int tab_bar_height = 20;
    
    public Vector2i client_top_left => (Vector2i.UnitY * tab_bar_height);
    public Vector2i client_size => size - (Vector2i.UnitY * tab_bar_height);
    public Vector2i client_bottom_right => client_top_left + size;

    public TabbedPanel(Vector2i position, Vector2i size) {
        setup(position.X, position.Y, size.X, size.Y);
        reconfigure_client_area();
    }
    
    public void update() {
        update_collision();
        test_mouse();
        foreach (var subform in subforms) {
            subform.update();
        }
    }

    public void render_internal() {
        foreach (var subform in subforms) {
            subform.render_internal();
            
        }
        
        Draw2D.fill_rect(Vector2i.Zero, client_size, color_background);
        foreach (var subform in subforms) {
            subform.draw();
        }
        Draw2D.rect(Vector2i.One, client_size, color_foreground, 1f);
    }

    public void draw() {
        Draw2D.image(client_area, position + client_top_left, client_size);
    }
}