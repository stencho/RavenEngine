using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using CSScripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Raven.Console;
using Raven.Engine.Collision;
using Raven.Graphics;
using Raven.Graphics.Drawing3D;
using Raven.Graphics.Effects;

namespace Raven.Engine;


#region ENTITIES
public interface Entity {
    public string name { get; set; }
    public Guid GUID { get; }
    
    public EntityPosition position { get; }
    public void SetPosition(Vector3 position);
    
    public ComponentManager Components { get; set; }
    
    public Scene parent_scene { get; set; }
    
    public void Initialize();
    public void Initialized();
    public void Update();
    public void StabilizePosition();
    public void AfterCollision();
    public void UpdateGraphics();
    public void UpdateInterpolatedPosition();
    
    public static T CreateInstance<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(Action? default_constructor_action, Action<ParameterInfo[]>? longest_constructor_action) 
        where T : Entity {
        // get default constructor info if it exists
        ConstructorInfo? ctor_default = typeof(T).GetConstructor(Type.EmptyTypes);
        
        // no default ctor
        if (ctor_default == null) {
            ConstructorInfo? ctor_complex = 
                typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                    .OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();

            if (ctor_complex != null) {
                ParameterInfo[] parameters = ctor_complex.GetParameters();
                longest_constructor_action?.Invoke(parameters);
            }
        }
        
        // default ctor is present 
        default_constructor_action?.Invoke();
        return (T)ctor_default.Invoke(null);
    }
}

#endregion
#region COMPONENTS
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class ComponentManager {
    private ConcurrentDictionary<string, Component> Components = new();
    Entity parent;
    public void set_parent(Entity p) => parent = p;
    
    public ComponentManager() {}
    
    public ComponentManager(Entity parent) {
        this.parent = parent;
    }
    
    public static T CreateInstance<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(Action? default_constructor_action, Action<ParameterInfo[]>? longest_constructor_action) 
        where T : Component {
        // get default constructor info if it exists
        ConstructorInfo? ctor_default = typeof(T).GetConstructor(Type.EmptyTypes);
        
        // no default ctor
        if (ctor_default == null) {
            ConstructorInfo? ctor_complex = 
                typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();

            if (ctor_complex != null) {
                ParameterInfo[] parameters = ctor_complex.GetParameters();
                longest_constructor_action?.Invoke(parameters);
            }
        }
        
        // default ctor is present 
        default_constructor_action?.Invoke();
        return (T)ctor_default.Invoke(null);
    }

    public Component AddComponent(Entity parent, Component component) {
        //component with the same name exists, so add a number to the end and
        //iterate it until no component with the exact same name exists
        int c = 0; 
        string orig_name = component.Name;
        while (Components.ContainsKey(component.Name)) 
            component.Name = orig_name + (++c).ToString();
        
        component.set_parent(parent);
        Components.TryAdd(component.Name, component);

        return component;
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
    public bool HasComponentWithFlag(ComponentFlags flag, out Component component) {
        foreach (var c in Components) {
            if (c.Value.flags.HasFlag(flag)) {
                component = c.Value;
                return true;
            }
        }

        component = default;
        return false;
    }

    public Component[] AllComponentsWithFlag(ComponentFlags flag) {
        List<Component> component_list = new List<Component>();
        
        foreach (var c in Components) {
            if (c.Value.flags.HasFlag(flag)) {
                component_list.Add(c.Value);
            }
        }

        return component_list.ToArray();
    }

    public void ForAllComponentsWithFlag(ComponentFlags flag, Action<Component> method) {
        foreach (var c in Components) {
            if (c.Value.flags.HasFlag(flag)) {
                method.Invoke(c.Value);
            }
        }
    }
    
    public T[] AllComponentsOfType<T>() where T : Component {
        List<T> component_list = new List<T>();
        
        foreach (var c in Components) {
            if ((c.Value.Type) == typeof(T)) {
                component_list.Add(c.Value as T);
            }
        }

        return component_list.ToArray();
    }
    
    public void ForAllComponentsOfType<T>(Action<Component> method) where T : Component {
        foreach (var c in Components) {
            if ((c.Value.Type) == typeof(T)) {
                method.Invoke(c.Value);
            }
        }
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

[Flags]
public enum ComponentFlags {
    None = 0,
    Render = 1,
    Collide = 2,
    Light = 4,
    Camera = 8
}

public interface IComponentGlobalMethods {
}

public abstract class Component : IComponentGlobalMethods {
    public abstract string Name { get; set; }
    public virtual Type Type { get; set; }
    
    protected Entity parent;
    protected Dictionary<string, ComponentData> data { get; set; } = new();
    
    public abstract ComponentFlags flags { get; }
    
    public abstract void RenderZPrepass(Camera camera);
    public abstract void RenderDeferred(Camera camera);
    public abstract void RenderForward(Camera camera);

    public bool force_forward_rendering { get; set; } = false;
    
    public Action? OverrideRenderZPrepass { get; set; }
    public Action? OverrideRenderDeferred { get; set; }
    public Action? OverrideRenderForward { get; set; }

    public Action? RunBeforeRenderDeferred { get; set; }
    public Action? RunBeforeRenderForward { get; set; }
    public Action? RunBeforeRenderBoth { get; set; }
    
    public Action? RenderBeforeForwardPass { get; set; }
    
    public abstract Shape3D? GetShape();
    public abstract collision_result? TestGJKAgainstShape(Shape3D test_shape, Matrix world);
    public abstract BoundingBox? GetBounds();

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
    
    public bool TryGetData<T>(string name, out T value) {
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
[HashSetManaged]
public abstract partial class GameSystem : IDisposable {
    public partial class Manager {
        public static void AllUpdate() {
            foreach (var game_system in gamesystems) {
                game_system.Update();
            }
        }
        public static void AllUpdateGraphics() {
            foreach (var game_system in gamesystems) {
                game_system.UpdateGraphics();
            }
        }
        public static void AllUpdateEndOfFrame() {
            foreach (var game_system in gamesystems) {
                game_system.UpdateEndOfFrame();
            }
        }
        public static void AllUpdateGraphicsEndOfFrame() {
            foreach (var game_system in gamesystems) {
                game_system.UpdateGraphicsEndOfFrame();
            }
        }
    }
    
    public GameSystem() => Manager.Add(this);
    public void Dispose() => Manager.Remove(this);
    
    ~GameSystem() => Dispose();

    public abstract void Update();
    public abstract void UpdateEndOfFrame();
    public abstract void UpdateGraphics();
    public abstract void UpdateGraphicsEndOfFrame();
}

#endregion