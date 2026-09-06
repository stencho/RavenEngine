using System;
using RavenEdit.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Engine.Controls;
using Raven.Graphics;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.Geometry2D;
using Raven.UI;
using Raven.UI.Forms;
using Raven.UI.Forms.Layout;

namespace RavenEdit;

public static class Interface {
    static MenuStrip menu_strip;
    
    static UIPanel tool_panel;
    
    static UIWindow test_window;
    static UIWindow controller_window;
    static UIWindow test_dialog;
    static UIWindow test_window_two;
    static UIWindow inspector;

    public static FullResolutionRenderTarget render_target;
    
    private static DrawShapesToSurface shape_drawing;
    
    public static Action<DrawShapesToSurface> Draw2DOverCanvas;
    static void draw_over_canvas_layer() => Draw2DOverCanvas.Invoke(shape_drawing);
        
    public static Action<DrawShapesToSurface> Draw2DOnTop;
    static void draw_on_top_layer() => Draw2DOnTop?.Invoke(shape_drawing);
    
    private static Vector2i cursor_shadow_offset = Vector2i.One * 3 + Vector2i.Down;
    
    public static void Load() {
        UIGraphics.Load();

        render_target = new FullResolutionRenderTarget();
        
        shape_drawing = new DrawShapesToSurface(() => State.resolution);
        
        State.resolution_changed += () => {
            State.UI.change_render_target(render_target.rt2D);
        };
        
        //debug text
        Draw2DOverCanvas += (DrawShapesToSurface draw_shapes) => {
            Draw2D.text_shadow(
                Clock.frame_rate + "/" + Clock.tick_rate + " [Frames/Ticks] Per Sec\n\n" 
                + gvars.list_all() +
                
                "\n\n[current_window]\n  " +
                State.UI.current_window_info(), 
                
                (Vector2i.One * 5) + (Vector2i.Down * 20) + (Vector2i.Right * 30), Color.White);
        };

        // draw mouse cursor
        Draw2DOnTop += (DrawShapesToSurface draw_shapes) => {
            UIGraphics.cursor.render_position = MouseWatcher.Position - Vector2.UnitY;
            draw_shapes.draw_shape_single_color(UIGraphics.cursor, cursor_shadow_offset, UIColors.Shadow, Color.Transparent, 0, sdf_pattern.DITHER, 1);
            draw_shapes.draw_shape(UIGraphics.cursor);
        };
        
        State.UI = new UIWindowManager(render_target.rt2D);
        
        menu_strip = new MenuStrip();

        menu_strip.menu_buttons.Add(new ButtonFlat("File"));                
        menu_strip.menu_buttons.Add(new ButtonFlat("Edit"));       
            
        test_dialog = new UIWindow(new Vector2i(50, 50), new Vector2i(380, 260));
        test_dialog.hide();
        test_dialog.change_text("DIALOG");
        
        test_window = new UIWindow(new Vector2i(50, 50), new Vector2i(420, 260));
        
        var button = new UIButton(200, 25, "test", "profont");

        button.set_action(() => {
            test_dialog.show();
        });
        
        var slider = new UISlider(Vector2i.One * 25, (Vector2i.Down * 23) + (Vector2i.Right * 140), 0f, 1f);
        var knob_one = new UIKnob(Vector2i.One * 50 + (Vector2i.Down * 63), 32);
        var knob_two = new UIKnob(Vector2i.One * 50 + (Vector2i.Down * 63) + (Vector2i.Right * 155), 32,
            ("zero", 0.0f),
            ("half", .5f),
            ("max", 1.0f)
        );
        
        knob_one.change_text("knob\nhehehe");
        knob_two.change_text("labelled knob");
        
        test_window.add_subform(button);
        test_window.add_subform(slider);
        test_window.add_subform(knob_one);
        test_window.add_subform(knob_two);
        
        test_window_two = new UIWindow(new Vector2i(90, 90), new Vector2i(160, 160));
        test_window_two.allow_resize = false;
        
        var button_two = new UIButton(10, 10, "click me", "profont");
        var test_panel = new UIPanel(Vector2i.One * 45, (Vector2i.Right * 80) + (Vector2i.Down * 50));

        test_panel.hide();
        test_panel.background_draw = (panel) => {
            Draw2D.text("good boy!\n  :^)", Vector2i.One * 15, panel.color_foreground);
        };
        
        button_two.set_action(test_panel.toggle_visibility);
        
        test_window_two.add_subform(button_two);
        test_window_two.add_subform(test_panel);
        
        inspector = new UIWindow(new Vector2i(320,200), new Vector2i(400, 320));
        inspector.change_text("Performance Inspector");
        inspector.hide();
        
        LayoutStripManager lm = new LayoutStripManager(inspector);
        
        lm.add_strip(new UIButton(0, 0, "test button"));
        lm.add_strip(new UIButton(0,0, "test button"), new UIButton(0,0, "test button"));
        lm.add_strip(new UIButton(0,0, "test button"), new UIButton(0,0, "test button"), new UIButton(0,0, "test button"));
        
        inspector.add_subform(lm);
        
        controller_window = new UIWindow(new Vector2i(50, 50), new Vector2i(420, 260));
        
        
        
        State.UI.add_window(menu_strip);
        State.UI.add_window(test_window);
        State.UI.add_window(test_window_two);
        State.UI.add_window_dialog(test_dialog, true);
        State.UI.add_window(inspector);
        
        test_dialog.show_hide_button = true;
    }

    public static void Update() {}
    
    public static void Render() {
        State.graphics_device.SetRenderTarget(render_target.rt2D);
        State.graphics_device.Clear(Color.Transparent);
        
        draw_over_canvas_layer();
        UIWindowManager.Manager.render_UIs_to_their_buffers();
        draw_on_top_layer();
    }
}