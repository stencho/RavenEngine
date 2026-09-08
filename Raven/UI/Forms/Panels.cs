using System;
using System.Collections.Generic;
using System.Security;
using Microsoft.Xna.Framework;
using Raven.Engine;
using Raven.Engine.Collision;
using Raven.Engine.Controls;
using Raven.Graphics;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.InterpolatedTypes;
using Raven.UI;

namespace Raven.UI.Forms;

public partial class UIPanel : IUIForm {
    public Vector2i client_size => size;
    public Vector2i client_top_left => Vector2i.Zero;
    public Vector2i client_bottom_right => size;

    public Action<UIPanel> background_draw = null;
    public Action<UIPanel> foreground_draw = null;
    
    public Action? start_of_update;
    public Action? end_of_update;

    public Action? start_of_draw_action;
    
    bool _render_targets_need_resize = false;
    private Vector2i old_size = Vector2i.Zero;
    
    public UIPanel(Vector2i position, Vector2i size) {
        setup(position.X, position.Y, size.X, size.Y);
        reconfigure_client_area();
        disable_focusing();
    }

    protected void disable_client_area() {
        _client_area = null;
    }
    
    public virtual void update() {
        start_of_update?.Invoke();
        
        update_collision();
        test_mouse();
        update_all_subforms();
        
        if (size != old_size) _render_targets_need_resize = true;
            
        if (_render_targets_need_resize && use_internal_rendering) {
            _client_area = RenderTargetEx.create(client_size.X, client_size.Y);
            _render_targets_need_resize = false;
        }

        old_size = size;
        
        end_of_update?.Invoke();
    }

    public virtual void render_internal() {
        if (!visible) return; 
        render_all_subform_internals();
        
        State.graphics_device.SetRenderTarget(client_area);
        State.graphics_device.Clear(color_background);

        background_draw?.Invoke(this);
        draw_all_subforms();
        foreground_draw?.Invoke(this);
        
        if (!is_child) Draw2D.rect(Vector2i.One, client_size, color_window_focus, 1f);
        else Draw2D.rect(Vector2i.One, client_size, color_foreground, 1f);
        Draw2D.end();
    }

    public virtual void draw() {
        if (!visible) return;
        start_of_draw_action?.Invoke();
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
        disable_focusing();
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