using System;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ListManaged : Attribute {}

[AttributeUsage(AttributeTargets.Class)]
public sealed class GuidManaged : Attribute {}

[AttributeUsage(AttributeTargets.Class)]
public sealed class HashSetManaged : Attribute {}

[AttributeUsage(AttributeTargets.Class)]
public sealed class ManagedPollingLoop : Attribute {}