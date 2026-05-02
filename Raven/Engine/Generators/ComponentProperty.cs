
using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ComponentProperty : Attribute {
    public string Name { get; }
    public Type Type { get; }

    public ComponentProperty(string name, Type type) {
        Name = name;
        Type = type;
    }
}
