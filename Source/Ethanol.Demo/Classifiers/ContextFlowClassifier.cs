using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.StreamProcessing;

namespace Ethanol.Demo
{
    public record ClassificationResult(string Label, double Score);

    /// <summary>
    /// A base class of all context flow classifiers.
    /// <para/>
    /// The classifier needs to implement <see cref="Score(ContextFlow{TContext})"/> method, which computes the likelihood that the given context flow is of the given class.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public abstract class ContextFlowClassifier<TContext>
    {
        /// <summary>
        /// Creates an expression that represents a classification methods for applying a collection of the classifiers.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <param name="classifiers">The classifiers to use for the classification.</param>
        /// <returns>An expression that can be used as an operator in <see cref="Streamable.Select{TKey, TPayload, TResult}(IStreamable{TKey, TPayload}, Expression{Func{TPayload, TResult}})"/> method.</returns>
        public static Expression<Func<ContextFlow<TContext>,ClassifiedContextFlow<TContext>>> Classify(params ContextFlowClassifier<TContext>[] classifiers)
        {
            return (arg) =>
              new ClassifiedContextFlow<TContext>(arg.Flow,classifiers.Select(classifier => new ClassificationResult(classifier.Label, classifier.Score(arg))).ToArray(),arg.Context);
        }
        /// <summary>
        /// Provides an expression that can be used to compute the score of the context flow.
        /// </summary>
        /// <returns>An expression that computes the score for the current classifier.</returns>
        public abstract double Score(ContextFlow<TContext> arg);
        /// <summary>
        /// A label of the current classifier.
        /// </summary>
        public abstract string Label { get; }
    }
}