using System;
using System.Collections.Generic;
using System.Security;
using Microsoft.Xna.Framework;
using Raven.Engine;
using Raven.Engine.Collision;
using Raven.Graphics;
using Raven.Graphics.Drawing2D;
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

    public Action? on_show;
    public Action? on_hide;
    
    public Action? start_of_draw_action;
    
    bool _render_targets_need_resize = false;
    private Vector2i old_size = Vector2i.Zero;
    
    public UIPanel(Vector2i position, Vector2i size) {
        setup(position.X, position.Y, size.X, size.Y);
        reconfigure_client_area();
        disable_focusing();
    }
    
    public void update() {
        start_of_update?.Invoke();
        
        update_collision();
        test_mouse();
        update_all_subforms();
        
        if (size != old_size) _render_targets_need_resize = true;
            
        if (_render_targets_need_resize) {
            _client_area = RenderTargetEx.create(client_size.X, client_size.Y);
            _render_targets_need_resize = false;
        }

        old_size = size;
    }

    public void render_internal() {
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

    public void draw() {
        if (!visible) return;
        start_of_draw_action?.Invoke();
        Draw2D.image(client_area, position + client_top_left, size);
    }
    
    public void parent_size_changed(Vector2i new_size) { }
}

public struct MenuPanelItem {
    private string text = "";
    public string Text => text;

    public Action? Pressed;

    public MenuPanelItem(string text, Action on_pressed) {
        this.text = text;
        Pressed = on_pressed;
    }
}

public partial class UIMenuPanel : UIPanel {
    MenuPanelItem[] menu_items;
    public int item_height = 30;
    public int menu_width = 300;

    public int header_gap = 50;
    public int footer_gap = 0;
    public int item_gap = 10;

    private string font = "profont";

    public Action<Vector2i>? DrawHeader;
    
    public UIMenuPanel(Vector2i position, string font, params MenuPanelItem[] items) : base(position, Vector2i.One) {
        menu_items = items;
        
        this.font = font;
        base.size = new Vector2i(menu_width, header_gap + item_gap + (items.Length * (item_height + item_gap)) + footer_gap + item_gap);

        foreground_draw += (menu) => {
            DrawHeader?.Invoke(new Vector2i(menu_width, header_gap));
            
            var col = Draw2D.ColorInterpolate(color_subfocus.multiply_color(UIColors.focus_fade), color_subfocus, base.window_focus_lerp);
            
            if (header_gap > 0) Draw2D.line(new Vector2i(0, header_gap), new Vector2i(menu_width, header_gap), col, 1f);
            
            for (var i = 0; i < menu_items.Length; i++) {
                var menu_item = menu_items[i];
                
                var top_left = new Vector2(0, header_gap + item_gap + (i * (item_height + item_gap)));
                var middle = top_left + (new Vector2(menu_width, item_height + item_gap) / 2f);
            
                var text_size = Draw2D.measure_string_i(font, menu_item.Text);
            
                Draw2D.text(font, menu_item.Text , middle - (text_size / 2f), col);
            }
        };
    }
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