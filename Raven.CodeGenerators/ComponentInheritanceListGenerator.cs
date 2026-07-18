using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace RavenCodeGenerators;

[Generator]
public class EntityInheritanceListGenerator : IIncrementalGenerator {
    private const string target_interface = "Raven.Engine.Entity";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. Pivot the pipeline to watch the entire Compilation state
        var collectedClassNames = context.CompilationProvider.Select((compilation, cancellationToken) =>
        {
            var builder = ImmutableArray.CreateBuilder<string>();

            // Retrieve the semantic symbol for the target interface (e.g. MyEngine.IComponent)
            var interfaceSymbol = compilation.GetTypeByMetadataName(target_interface);
            if (interfaceSymbol is null) return ImmutableArray<string>.Empty;

            // 2. Scan the current assembly AND all referenced DLLs/Projects
            var assembliesToScan = compilation.SourceModule.ReferencedAssemblySymbols
                .Concat([compilation.Assembly]);

            foreach (var assembly in assembliesToScan)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                // Recursively look up types from the assembly's global namespace
                FindImplementationsInNamespace(assembly.GlobalNamespace, interfaceSymbol, builder, cancellationToken);
            }

            return builder.ToImmutable();
        });

        // 3. Emit the dictionary file
        context.RegisterSourceOutput(collectedClassNames, (productionContext, classNames) => Execute(productionContext, classNames));
    }

    // Helper method to recursively dive into sub-namespaces to find classes
    private static void FindImplementationsInNamespace(
        INamespaceSymbol namespaceSymbol, 
        INamedTypeSymbol interfaceSymbol, 
        ImmutableArray<string>.Builder builder,
        System.Threading.CancellationToken cancellationToken)
    {
        // Check all types declared inside this namespace level
        foreach (var member in namespaceSymbol.GetTypeMembers())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (member is not { IsAbstract: false, TypeKind: TypeKind.Class }) continue;

            // Check if this class implements the engine interface
            var implementsInterface = member.AllInterfaces
                .Any(i => SymbolEqualityComparer.Default.Equals(i, interfaceSymbol));

            if (implementsInterface)
            {
                builder.Add(member.ToDisplayString());
            }
        }

        // Deep-dive into sub-namespaces (e.g. MyEngine.Core.SubNamespace)
        foreach (var subNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            FindImplementationsInNamespace(subNamespace, interfaceSymbol, builder, cancellationToken);
        }
    }
        
    private static void Execute(SourceProductionContext context, ImmutableArray<string> classes) {
        var sb = new StringBuilder();
        
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Text;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("namespace Raven.Engine;");
        sb.AppendLine("public static partial class Inherited {");
        sb.AppendLine("    public static readonly Dictionary<string, Func<object>> Entities = new() {");
            
        foreach (var type in classes) {
            
        sb.AppendLine($"        {{ \"{type}\", () => Entity.Create<{type}>(null, null) }},");
        }
            
        sb.AppendLine("    };");
        sb.AppendLine();
        sb.AppendLine("    public static string ListEntities() {");
        sb.AppendLine("        var sb = new StringBuilder();");
        sb.AppendLine("        sb.AppendLine(\"[Entity Types]\");");
        sb.AppendLine("        foreach (string type in Entities.Keys) {");
        sb.AppendLine("            var last_period_index = type.LastIndexOf('.')+1;");
        sb.AppendLine("            var type_name = type.Remove(0, last_period_index);");
        sb.AppendLine("            sb.AppendLine($\"| {type_name}\");");
        sb.AppendLine("        }");
        sb.AppendLine("        return sb.ToString();");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();
            
        context.AddSource("EntityTypes.g.cs", sb.ToString());
    }
}


[Generator]
public class ComponentInheritanceListGenerator : IIncrementalGenerator {
    private const string target_interface = "Raven.Engine.Component";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. Hook into the compilation state
        var collectedClassNames = context.CompilationProvider.Select((compilation, cancellationToken) =>
        {
            var builder = ImmutableArray.CreateBuilder<string>();

            // Retrieve the semantic symbol for your abstract class (e.g. MyEngine.Component)
            var baseClassSymbol = compilation.GetTypeByMetadataName(target_interface);
            if (baseClassSymbol is null) return ImmutableArray<string>.Empty;

            // 2. Combine the current project with all referenced assemblies
            var assembliesToScan = compilation.SourceModule.ReferencedAssemblySymbols
                .Concat([compilation.Assembly]);

            foreach (var assembly in assembliesToScan)
            {
                cancellationToken.ThrowIfCancellationRequested();
                FindInheritorsInNamespace(assembly.GlobalNamespace, baseClassSymbol, builder, cancellationToken);
            }

            return builder.ToImmutable();
        });

        // 3. Emit the dictionary file
        context.RegisterSourceOutput(collectedClassNames, (productionContext, classNames) => Execute(productionContext,classNames));
    }

    private static void FindInheritorsInNamespace(
        INamespaceSymbol namespaceSymbol, 
        INamedTypeSymbol baseClassSymbol, 
        ImmutableArray<string>.Builder builder,
        System.Threading.CancellationToken cancellationToken)
    {
        foreach (var member in namespaceSymbol.GetTypeMembers())
        {
            cancellationToken.ThrowIfCancellationRequested();

            // We only want concrete classes (skip abstract classes inheriting from your abstract class)
            if (member is not { IsAbstract: false, TypeKind: TypeKind.Class }) continue;

            // Walk up the inheritance tree to see if it derives from the target base class
            if (IsDerivedFrom(member, baseClassSymbol))
            {
                builder.Add(member.ToDisplayString());
            }
        }

        // Deep-dive into sub-namespaces
        foreach (var subNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            FindInheritorsInNamespace(subNamespace, baseClassSymbol, builder, cancellationToken);
        }
    }

    // Helper to traverse the class inheritance chain
    private static bool IsDerivedFrom(INamedTypeSymbol? currentSymbol, INamedTypeSymbol baseClassSymbol)
    {
        var walker = currentSymbol?.BaseType;
        
        while (walker is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(walker, baseClassSymbol))
            {
                return true;
            }
            walker = walker.BaseType; // Move up one level (e.g. PlayerComponent -> PhysicsComponent -> Component)
        }
        
        return false;
    }
    
    private static void Execute(SourceProductionContext context, System.Collections.Immutable.ImmutableArray<string> classes) {
        var sb = new StringBuilder();
        
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Text;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("namespace Raven.Engine;");
        sb.AppendLine("public static partial class Inherited {");
        sb.AppendLine("    public static readonly Dictionary<string, Func<object>> Components = new() {");
        foreach (var type in classes) { 
        sb.AppendLine($"        {{ \"{type}\", () => ComponentManager.Create<{type}>(null, null) }},");
        }
        sb.AppendLine("    };");
        sb.AppendLine();
        sb.AppendLine("    public static string ListComponents() {");
        sb.AppendLine("        var sb = new StringBuilder();");
        sb.AppendLine("        sb.AppendLine(\"[Component Types]\");");
        sb.AppendLine("        foreach (string type in Components.Keys) {");
        sb.AppendLine("            var last_period_index = type.LastIndexOf('.')+1;");
        sb.AppendLine("            var type_name = type.Remove(0, last_period_index);");
        sb.AppendLine("            sb.AppendLine($\"| {type_name}\");");
        sb.AppendLine("        }");
        sb.AppendLine("        return sb.ToString();");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();
            
        context.AddSource("ComponentTypes.g.cs", sb.ToString());
    }
}
