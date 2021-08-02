using System;
using System.Linq.Expressions;

namespace Ethanol.Demo
{
    /// <summary>
    /// A base class of all context fact builders. 
    /// </summary>
    public abstract class FactBuilder
    {
        public abstract string Name { get; }
        public abstract Type ArtifactType { get; }
        public abstract Type OutputType { get; }

        public abstract Func<Artifact, bool> GetPredicate(Artifact target);

    }
    /// <summary>
    /// Represents a context fact builder. It is expression that can be used either as
    /// a function to be executed on input collection of type <typeparamref name="OutputType"/> or
    /// to generate query string against the database with data.
    /// </summary>
    /// <typeparam name="TArtifact">The type of artifact that owns the builder.</typeparam>
    /// <typeparam name="TOutput">The type of artifact that is being selected by the builder from the datasource.</typeparam>
    public class FactBuilder<TArtifact, TOutput> : FactBuilder where TArtifact : Artifact
                                                                       where TOutput : Artifact
    {
        readonly string _name; 
        public FactBuilder(string name, Expression<Func<TArtifact, TOutput, bool>> predicate)
        {
            _name = name;
            Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <summary>
        /// The predicate to be executed on the input data source.
        /// </summary>        
        public Expression<Func<TArtifact, TOutput, bool>> Predicate { get; }

        public override Type ArtifactType => typeof(TArtifact);

        public override Type OutputType => typeof(TOutput);

        public override string Name => _name;



        /// <summary>
        /// Compiles the predicate expression to a function.
        /// </summary>
        /// <returns>A function representing the predicate.</returns>
        public Func<TArtifact, TOutput, bool> Compile() => Predicate.Compile();

        public override Func<Artifact, bool> GetPredicate(Artifact target)
        {
            var compiledFunc = Compile();
            bool predicate(Artifact x)
            {
                var obj1 = (TArtifact)target;
                var obj2 = (TOutput)x;
                return compiledFunc(obj1, obj2);
            }
            return predicate;
        }
    }
}