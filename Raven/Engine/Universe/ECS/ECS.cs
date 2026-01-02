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
    public string name { get; set; }
    
    public ChunkPosition position { get;  }
    
    public ComponentManager Components { get; set; }
    
    public Universe parent_universe { get; set; }
    public Chunk parent_chunk { get; set; }
    
    public Threads.ThreadRequestPacket update_packet{ get; set; }
    
    public void Initialize();
    public void Initialized();
    public void Update();
    public void StabilizeChunkPosition();
    public void AfterCollision();
    public void UpdateGraphics();
    public void UpdateInterpolatedPosition();

    public void SetPosition(ChunkPosition position);
}
#endregion
#region COMPONENTS

public class ComponentManager {
    private ConcurrentDictionary<string, Component> Components = new();
    Entity parent;
    public void set_parent(Entity p) => parent = p;
    
    public ComponentManager() {}
    
    public ComponentManager(Entity parent) {
        this.parent = parent;
    }
    
    public void AddComponent(Entity parent, Component component) {
        //component with the same name exists, so add a number to the end and
        //iterate it until no component with the exact same name exists
        int c = 0; 
        string orig_name = component.Name;
        while (Components.ContainsKey(component.Name)) 
            component.Name = orig_name + (++c).ToString();
        
        component.set_parent(parent);
        Components.TryAdd(component.Name, component);
    }

    public void RenameComponent(string name, string new_name) {
        //If new name already exists in the dictionary,
        //add a numeral to the end
        int c = 0; string orig_name = new_name;
        while (Components.ContainsKey(new_name)) 
            new_name = orig_name + (++c).ToString();
        
        //Rename the component
        Components[name].Name = new_name;
        
        //Remove and re-add to the dictionary to change the key 
        var comp = Components[name];
        Components.Remove(name, out _);
        Components.TryAdd(new_name, comp);
    }

    public T Get<T>(string name) where T : Component {
        return (T)Components[name];
        
        if (HasComponent(name)) {
            if (Components.TryGetValue(name, out var c)) {
                return c as T;
            }
        }

        return null;
    }
    public T GetFirst<T>() where T : Component {
        if (HasComponentOfType<T>(out var component)) {
            return component as T;
        }
        return null;
    }
    
    public bool HasComponent(string name) {
        return Components.ContainsKey(name);
    }
    
    public bool HasComponentOfType<T>() {
        foreach (var c in Components) {
            if ((c.Value.Type) == typeof(T)) return true;
        }
        return false;
    }
    
    public bool HasComponentOfType<T>(out T component) where T : Component {
        foreach (var c in Components) {
            if ((c.Value.Type) == typeof(T)) {
                component = c.Value as T;
                return true;
            }
        }

        component = default;
        return false;
    }

    public string ListAllComponents(int spaces_at_start_of_each_line = 0) {
        string output = new string(' ', spaces_at_start_of_each_line);
        output += "[Components]\n";
        foreach (var c in Components) {
            output += new string(' ', spaces_at_start_of_each_line +  2);
            output += $"[{c.Value.Name} : {c.Value.Type.Name}]\n{c.Value.list_all_data(spaces_at_start_of_each_line +  4)}";
            output += "\n";
            //output += $" | {c.Value.name} > ]n".PadLeft(spaces_at_start_of_each_line + 3) + "";
        }
        
        return output;
    }
}

public abstract class Component {
    public abstract string Name { get; set; }
    public virtual Type Type { get; set; }
    protected Entity parent;
    protected Dictionary<string, ComponentData> data { get; set; } = new();

    public void set_parent(Entity p) {
        parent = p;
    }
    
    protected void add_data<T>(string name, T cdata) { 
        data.Add(name, new ComponentData<T>(name, cdata));
    }

    public T GetData<T>(string name) {
        if (data.ContainsKey(name) && data[name] != null && data[name] is ComponentData<T> component_data)
            return component_data.data;
        return default;
    }

    public void SetData<T>(string name, T value) {
        if (data.ContainsKey(name) && data[name] != null && data[name] is ComponentData<T> component_data) {
            ((ComponentData<T>)data[name]).data = value;
        }
    }
    
    public bool try_get_value<T>(string name, out T value) {
        if (data.ContainsKey(name) && data[name] != null && data[name] is ComponentData<T> component_data) {
            value = component_data.data;
            return true;
        }

        value = default;
        return false;
    }

    public string list_all_data(int spaces_at_start_of_each_line = 0) {
        string output = "";
        foreach (var cd in data) {
            output += new string(' ', spaces_at_start_of_each_line);
            var t = cd.Value.type;
            output += $"[{cd.Value.name} :: {cd.Value.type.Name}] ";
            output += "\n";
        }

        return output;
    }
}

public abstract class ComponentData {
    public abstract string name { get; set; }
    public abstract Type type { get; }
    internal void change_name(string name) {
        this.name = name;
    } 
}
public class ComponentData<T> : ComponentData {
    public override string name { get; set; } 
    public T data;
    
    public override Type type => typeof(T);
    
    public ComponentData(string name, T data) {
        this.name = name;
        this.data = data;
    }

    internal void change_name(string name) {
        this.name = name;
    }

    public T get_data() => data;
}

#endregion

#region SYSTEMS
#endregion