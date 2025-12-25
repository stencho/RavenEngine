using System;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ListManagedClass : Attribute {}

[AttributeUsage(AttributeTargets.Class)]
public sealed class GuidManagedClass : Attribute {}