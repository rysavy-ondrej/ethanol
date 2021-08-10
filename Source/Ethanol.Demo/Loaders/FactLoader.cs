using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Ethanol.Demo
{

    /// <summary>
    /// A base class of all context fact loaders.
    /// <para/>
    /// <see cref="FactLoader"/> is responsible for accessing artifacts and loading them via 
    /// <see cref="Query(IpfixArtifact, IQueryable{IpfixArtifact})"/> function.
    /// </summary>
    public abstract class FactLoader
    {
        public abstract Type ArtifactType { get; }
        public abstract Type InputType { get; }
        /// <summary>
        /// Queries the <paramref name="input"/> data source to obtain facts relevant to the <paramref name="target"/> artifact.
        /// </summary>
        /// <param name="target">The target artifact.</param>
        /// <param name="input">The queryable input of artifacts.</param>
        /// <returns>Yields a collection of facts drawn from <paramref name="input"/> artifacts relevant to the <paramref name="target"/> artifact.</returns>
        public abstract IEnumerable<Fact> Query(Artifact target, IQueryable<Artifact> input);

        /// <summary>
        /// Creates a new loader using the given expression.
        /// </summary>
        /// <typeparam name="TArtifact"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="loaderExpression"></param>
        /// <returns></returns>
        public static FactLoader<TArtifact, TInput> Create<TArtifact, TInput>(FactLoaderFunction<TArtifact, TInput> loaderExpression) 
            where TArtifact : Artifact
            where TInput : Artifact
        {
            return new FactLoader<TArtifact, TInput>(loaderExpression);
        }
    }


   
    /// <summary>
    /// Represents a context fact builder. It includes the expression that can be used either as
    /// a function to be executed on input collection of type <typeparamref name="OutputType"/> or
    /// to generate query string against the database with data.
    /// </summary>
    /// <typeparam name="TArtifact">The type of artifact that owns the builder.</typeparam>
    /// <typeparam name="TOutput">The type of artifact that is being selected by the builder from the datasource.</typeparam>
    public class FactLoader<TArtifact, TInput> : FactLoader where TArtifact : Artifact
                                                            where TInput    : Artifact
    {    
        internal FactLoader(FactLoaderFunction<TArtifact, TInput> loaderExpression)
        {
            _queryDelegate = loaderExpression;
        }
        /// <summary>
        /// The predicate to be executed on the input data source.
        /// </summary>        
        FactLoaderFunction<TArtifact, TInput> _queryDelegate{ get; }

        public override Type ArtifactType => typeof(TArtifact);

        public override Type InputType => typeof(TInput);

        public override IEnumerable<Fact> Query(Artifact target, IQueryable<Artifact> input)
        {
            var typedTarget = target as TArtifact;
            var typedInput = input.Cast<TInput>();
            if (typedTarget == null || typedInput == null) throw new InvalidCastException("Target type or input type does not match.");
            return _queryDelegate.Invoke(typedTarget, typedInput);
        }
    }

    public static class FactLoaderExtensions
    {
        public static FactLoader GetLoader<TA, TI>(this FactLoaderFunction<TA, TI> func) where TA : IpfixArtifact where TI : IpfixArtifact
            => FactLoader.Create<TA, TI>(func);
    }
}