using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using ProjectRaven.Graphics.Drawing3D;
using ProjectRaven.Graphics;

namespace ProjectRaven.Engine;

public interface Entity {
    public ChunkPosition chunk_position { get; set; }
    
    public List<DynamicLight> lights { get; set; }
    
    public void Update();
    public void AfterCollision();
    public void UpdateGraphics();

    //public render_info RenderInfo { get; set; }
    
    public Action<Entity> Draw2D { get; set;}
}

public class ComponentManager {
    
}

public abstract class ComponentData {
    string _name;
    public string name { get; }
    public abstract Type type { get; }
    internal void change_name(string name) {
        this._name = name;
    } 
}
public class ComponentData<T> : ComponentData {
    string _name;
    public T data;
    
    public string name => _name;
    public override Type type => typeof(T);
    
    public ComponentData(string name, T data) {
        this._name = name;
        this.data = data;
    }

    internal void change_name(string name) {
        this._name = name;
    } 
}


public class Component {
    string _name = "";
    public string name => _name;

    private Dictionary<string, ComponentData> data = new();

    public void add_data(string name, ComponentData data) {
        data.change_name(name);
        this.data.Add(name, data);
    }
    public void add_data<T>(string name, ComponentData<T> data) {
        data.change_name(name);
        this.data.Add(name, data);
    }

    public bool get_value<T>(string name, out T value) {
        if (data[name] != null && data[name] is ComponentData<T> d) {
            value = d.data;
            return true;
        }

        value = default;
        return false;
    }
}