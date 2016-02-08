﻿namespace Il2Native.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Symbols;

    public static class CodeAnalysisHelpers
    {
        public static IEnumerable<ITypeSymbol> EnumAllTypes(this IModuleSymbol module)
        {
            foreach (var metadataTypeAdapter in module.GlobalNamespace.EnumAllNamespaces().SelectMany(n => n.GetTypeMembers()))
            {
                yield return metadataTypeAdapter;
                foreach (var nestedType in metadataTypeAdapter.EnumAllNestedTypes())
                {
                    yield return nestedType;
                }
            }
        }

        public static IEnumerable<INamespaceOrTypeSymbol> EnumAllNamespaces(this INamespaceOrTypeSymbol source)
        {
            yield return source;
            foreach (var namespaceSymbolSub in source.GetMembers().OfType<INamespaceOrTypeSymbol>().SelectMany(EnumAllNamespaces))
            {
                yield return namespaceSymbolSub;
            }
        }

        public static IEnumerable<ITypeSymbol> EnumAllNestedTypes(this INamespaceOrTypeSymbol source)
        {
            return source.GetTypeMembers().SelectMany(nestedType => EnumAllNestedTypes(nestedType));
        }

        public static bool IsDerivedFrom(this ITypeSymbol source, ITypeSymbol from)
        {
            var current = source.BaseType;
            while (current != null && current != from)
            {
                current = current.BaseType;
            }

            return current != null;
        }

        public static IEnumerable<IMethodSymbol> IterateAllMethodsWithTheSameNames(this ITypeSymbol type)
        {
            return IterateAllMethodsWithTheSameNames((INamedTypeSymbol)type);
        }

        private static IEnumerable<IMethodSymbol> IterateAllMethodsWithTheSameNames(INamedTypeSymbol type)
        {
            if (type.TypeKind != TypeKind.Interface && type.BaseType == null)
            {
                return new IMethodSymbol[0];
            }

            var methods = new List<IMethodSymbol>();

            var groupsByName = type.EnumerateAllMethodsRecursevly().GroupBy(m => m.Name);
            foreach (var groupByName in groupsByName)
            {
                var groupByType = groupByName.GroupBy(g => g.ContainingType);
                if (groupByType.Count() < 2)
                {
                    continue;
                }

                methods.AddRange(groupByName.Distinct(new KeyStringEqualityComparer()).Where(m => m.ContainingType != type));
            }

            return methods;
        }

        public class KeyStringEqualityComparer : IEqualityComparer<IMethodSymbol>
        {
            public bool Equals(IMethodSymbol x, IMethodSymbol y)
            {
                return string.Compare(((MethodSymbol)x).ToKeyString(), ((MethodSymbol)y).ToKeyString(), StringComparison.Ordinal) == 0;
            }

            public int GetHashCode(IMethodSymbol obj)
            {
                return obj.GetHashCode();
            }
        }

        public static IEnumerable<IMethodSymbol> EnumerateAllMethodsRecursevly(this INamedTypeSymbol type)
        {
            if (type.TypeKind == TypeKind.Interface)
            {
                foreach (var memberBase in type.Interfaces.SelectMany(i => i.EnumerateAllMethodsRecursevly()))
                {
                    yield return memberBase;
                }
            }

            if (type.BaseType != null)
            {
                foreach (var memberBase in type.BaseType.EnumerateAllMethodsRecursevly())
                {
                    yield return memberBase;
                }
            }

            foreach (var member in type.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind != MethodKind.Constructor))
            {
                yield return member;
            }
        }
    }
}
