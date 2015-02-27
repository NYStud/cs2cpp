﻿namespace Il2Native.Logic.Gencode.SynthesizedMethods.MultiDimArray
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using PEAssemblyReader;

    /// <summary>
    /// </summary>
    public class SynthesizedMultiDimArraySetMethod : SynthesizedThisMethod
    {
        private readonly IMethodBody _methodBody;

        private readonly IList<IParameter> _parameters;

        private readonly IList<object> _tokenResolutions;

        /// <summary>
        /// </summary>
        /// <param name="type">
        /// </param>
        /// <param name="typeResolver">
        /// </param>
        public SynthesizedMultiDimArraySetMethod(IType type, ITypeResolver typeResolver)
            : base("Set", type, typeResolver.System.System_Void)
        {
            object[] code;
            IList<object> tokenResolutions;
            IList<IType> locals;
            IList<IParameter> parameters;
            ArrayMultiDimensionGen.GetMultiDimensionArraySet(type, typeResolver, out code, out tokenResolutions, out locals, out parameters);

            this._methodBody = new SynthesizedMethodBodyDecorator(
                null,
                locals,
                MethodBodyBank.Transform(code).ToArray());

            this._parameters = parameters;
            this._tokenResolutions = tokenResolutions;
        }

        public override IEnumerable<IParameter> GetParameters()
        {
            return this._parameters;
        }

        public override IMethodBody GetMethodBody(IGenericContext genericContext = null)
        {
            return this._methodBody;
        }

        public override IModule Module
        {
            get { return new SynthesizedModuleResolver(null, this._tokenResolutions); }
        }
    }
}
