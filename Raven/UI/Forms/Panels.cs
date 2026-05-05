using System;
using System.Collections.Generic;
using System.Security;
using Microsoft.Xna.Framework;
using Raven.Engine;
using Raven.Engine.Collision;
using Raven.Graphics.Drawing2D;
using Raven.UI;

namespace Raven.UI.Forms;

public partial class UIPanel : IUIForm {
    public Vector2i client_size => size;
    public Vector2i client_top_left => Vector2i.Zero;
    public Vector2i client_bottom_right => size;

    public Action<UIPanel> background_draw = null;
    public Action<UIPanel> foreground_draw = null;
    
    public UIPanel(Vector2i position, Vector2i size) {
        setup(position.X, position.Y, size.X, size.Y);
        reconfigure_client_area();
    }
    
    public void update() {
        update_collision();
        test_mouse();
        update_all_subforms();
    }

    public void render_internal() {
        if (!visible) return; 
        render_all_subform_internals();
        
        State.graphics_device.SetRenderTarget(client_area);
        State.graphics_device.Clear(color_background);

        background_draw?.Invoke(this);
        draw_all_subforms();
        foreground_draw?.Invoke(this);
        
        Draw2D.rect(Vector2i.One, client_size, color_foreground, 1f);
        Draw2D.end();
    }

    public void draw() {
        if (!visible) return;
        Draw2D.image(client_area, position + client_top_left, size);
    }
    
    public void parent_size_changed(Vector2i new_size) { }
}

public partial class UITabbedPanel : IUIForm {
    public int tab_bar_height = 20;
    
    public Vector2i client_top_left => (Vector2i.UnitY * tab_bar_height);
    public Vector2i client_size => size - (Vector2i.UnitY * tab_bar_height);
    public Vector2i client_bottom_right => client_top_left + size;

    public Action<UITabbedPanel> background_draw = null;
    public Action<UITabbedPanel> foreground_draw = null;
    
    public UITabbedPanel(Vector2i position, Vector2i size) {
        setup(position.X, position.Y, size.X, size.Y);
        reconfigure_client_area();
    }
    
    public void update() {
        update_collision();
        test_mouse();
        update_all_subforms();
    }

    public void render_internal() {
        render_all_subform_internals();
        Draw2D.fill_rect(Vector2i.Zero, client_size, color_background);
        State.graphics_device.SetRenderTarget(client_area);
        State.graphics_device.Clear(color_background);
        background_draw?.Invoke(this);
        draw_all_subforms();
        foreground_draw?.Invoke(this);
        Draw2D.rect(Vector2i.One, client_size, color_foreground, 1f);
        Draw2D.end();
    }

    public void draw() {
        Draw2D.image(client_area, position + client_top_left, client_size);
    }
    
    public void parent_size_changed(Vector2i new_size) { }
}