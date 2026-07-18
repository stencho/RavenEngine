using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Raven.Engine;
using Raven.Engine.Collision;
using Raven.Engine.Collision.Shapes2D;
using Raven.Engine.Controls;
using Raven.Graphics.Drawing2D;
using Raven.Graphics.InterpolatedTypes;
namespace Raven.UI.Forms;

public partial class UIDropDown : IUIForm {
    public Vector2i client_size => size;
    
    public Vector2i client_top_left => top_left;
    public Vector2i client_bottom_right => bottom_right;

    private UISelectionList drop_down;

    public Action<SelectionListItem> item_picked { get; set; }
    public Action<int> index_changed { get; set; }
    
    public UIDropDown() {
        drop_down = new UISelectionList(this);
    }
    
    public void update() {
        
    }

    public void render_internal() {
        
    }

    public void draw() {
        
    }
    
    public void parent_size_changed(Vector2i new_size) { }
}

/// <summary>
/// While this is designed for use as the drop-down menu for UIDropDown, it's also designed to be used as a generic text list selection box.
/// UIDropDown is just a wrapper which shows/hides it and reports its results
/// </summary>
public partial class UISelectionList : IUIForm {
    public Vector2i client_size => size;
    
    public Vector2i client_top_left => top_left;
    public Vector2i client_bottom_right => bottom_right;
    
    private List<SelectionListItem> items = new List<SelectionListItem>();

    private UIDropDown parent;
    
    public Action<SelectionListItem> item_picked { get; set; }
    public Action<int> index_changed { get; set; }
    
    public UISelectionList(UIDropDown parent) {
        this.parent = parent;
    }

    public void add_item(string text) {
        items.Add(new SelectionListItem(text, this));
    }
    
    public void update() {
        if (!visible) return;
        
    }

    public void render_internal() {
        
    }

    public void draw() {
        Draw2D.text_centered(font_name, text, position.OnlyX() / 2, UIColors.Foreground);
    }
    
    public void parent_size_changed(Vector2i new_size) { }
}

public class SelectionListItem {
    public string text { get; set; } = "";
    public bool mouse_over { get; set; } = false;

    private UISelectionList parent;
    
    public SelectionListItem(string text, UISelectionList parent) {
        this.text = text;
        this.parent = parent;
    }
}

