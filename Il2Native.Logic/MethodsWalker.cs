﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MethodsWalker.cs" company="">
//   
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Il2Native.Logic
{
    using System.Collections.Generic;
    using PEAssemblyReader;

    /// <summary>
    /// </summary>
    public class MethodsWalker
    {
        /// <summary>
        /// </summary>
        private readonly IMethod startMethod;

        /// <summary>
        /// </summary>
        private readonly ITypeResolver typeResolver;

        /// <summary>
        /// </summary>
        /// <param name="startMethod">
        /// </param>
        public MethodsWalker(IMethod startMethod, ITypeResolver typeResolver)
        {
            this.startMethod = startMethod;
            this.typeResolver = typeResolver;
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        public ISet<IType> DiscoverAllStaticFieldsDependencies()
        {
            // TODO: When you discover virtual method you need to discover all derived methods.
            var calledMethods = new NamespaceContainer<MethodKey>();
            var readStaticFields = new NamespaceContainer<IField>();

            this.WalkMethod(this.startMethod, calledMethods, readStaticFields);

            var typesWithStaticFields = new NamespaceContainer<IType>();
            foreach (var staticField in readStaticFields)
            {
                if (staticField.DeclaringType.TypeEquals(this.startMethod.DeclaringType))
                {
                    continue;
                }

                typesWithStaticFields.Add(staticField.DeclaringType);
            }

            return typesWithStaticFields;
        }

        /// <summary>
        /// </summary>
        /// <param name="method">
        /// </param>
        /// <param name="allCalledMethods">
        /// </param>
        /// <param name="allReadStaticFields">
        /// </param>
        private void WalkMethod(IMethod method, ISet<MethodKey> allCalledMethods, ISet<IField> allReadStaticFields)
        {
            var calledMethods = new NamespaceContainer<MethodKey>();
            method.DiscoverMethod(null, calledMethods, allReadStaticFields, typeResolver);

            foreach (var nextMethod in calledMethods)
            {
                if (allCalledMethods.Contains(nextMethod))
                {
                    continue;
                }

                allCalledMethods.Add(nextMethod);

                this.WalkMethod(nextMethod.Method, allCalledMethods, allReadStaticFields);
            }
        }
    }
}