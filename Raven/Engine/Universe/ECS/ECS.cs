using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Mail;
using CSScripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Raven.Graphics;
using Raven.Graphics.Drawing3D;

namespace Raven.Engine;

#region ENTITIES
public interface Entity {
    public ChunkPosition chunk_position { get; set; }
    public ChunkPosition chunk_position_stable { get; set; }
    
    public List<DynamicLight> lights { get; set; }
    
    public void Update();
    public void AfterCollision();
    public void UpdateGraphics();

    ComponentManager Components { get; set; }

    //public render_info RenderInfo { get; set; }
    //public Action<Entity> Draw2D { get; set;}
}
#endregion
#region COMPONENTS

public class ComponentManager {
    private ConcurrentDictionary<string, Component> Components = new();
    
    public void AddComponent(Component component) {
        //component with the same name exists, so add a number to the end and
        //iterate it until no component with the exact same name exists
        int c = 0; 
        string orig_name = component.name;
        while (Components.ContainsKey(component.name)) 
            component.name = orig_name + (++c).ToString();
        
        Components.TryAdd(component.name, component);
    }

    public void RenameComponent(string name, string new_name) {
        //If new name already exists in the dictionary,
        //add a numeral to the end
        int c = 0; string orig_name = new_name;
        while (Components.ContainsKey(new_name)) 
            new_name = orig_name + (++c).ToString();
        
        //Rename the component
        Components[name].name = new_name;
        
        //Remove and re-add to the dictionary to change the key 
        var comp = Components[name];
        Components.Remove(name, out _);
        Components.TryAdd(new_name, comp);
    }
    
    public bool HasComponent() {
        return false;
    }
}
public abstract class Component {
    public string name { get; set; }

    protected Dictionary<string, ComponentData> data { get; set; } = new();

    protected void add_data<T>(string name, T cdata) { 
        data.Add(name, new ComponentData<T>(name, cdata));
    }

    protected T get_value<T>(string name) {
        if (data.ContainsKey(name) && data[name] != null && data[name] is ComponentData<T> component_data)
            return component_data.data;
        return default;
    }

    protected void set_value<T>(string name, T value) {
        if (data.ContainsKey(name) && data[name] != null && data[name] is ComponentData<T> component_data) {
            ((ComponentData<T>)data[name]).data = value;
        }
    }
    
    protected bool try_get_value<T>(string name, out T value) {
        if (data.ContainsKey(name) && data[name] != null && data[name] is ComponentData<T> component_data) {
            value = component_data.data;
            return true;
        }

        value = default;
        return false;
    }
}

public class HealthComponent : Component {
    public string name { get; set; } = "Health";
    protected Dictionary<string, ComponentData> data { get; set; } = new();

    public float Health {
        get { return get_value<float>("Health"); }
        set { set_value("Health", value); }
    }

    public HealthComponent(float starting_health = 1.0f) {
        add_data("Health", starting_health);        
    }
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

#endregion

#region SYSTEMS
#endregion