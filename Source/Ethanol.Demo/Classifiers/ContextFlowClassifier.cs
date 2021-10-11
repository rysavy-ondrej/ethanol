using Microsoft.StreamProcessing;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Ethanol.Demo
{
    /// <summary>
    /// Result of the classification. 
    /// </summary>
    /// <param name="Label">The label representing the class.</param>
    /// <param name="Score">The score that gives the likelihood that the sample belongs to the class.</param>
    public record ClassificationResult(string Label, double Score);

    /// <summary>
    /// Defines an interface for context flow classifiers.
    /// </summary>
    /// <typeparam name="TContext">The type of context of the flow.</typeparam>
    public interface IContextFlowClassifier<TContext>
    {
        /// <summary>
        /// The classifier label.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Method computes the likelihood that the sample belongs to the class.
        /// </summary>
        /// <param name="arg">An input context flow to be classified.</param>
        /// <returns>The value of score represented as double.</returns>
        double Score(ContextFlow<TContext> arg);
    }

    /// <summary>
    /// A base class of all context flow classifiers.
    /// <para/>
    /// The classifier needs to implement <see cref="Score(ContextFlow{TContext})"/> method, which computes the likelihood that the given context flow is of the given class.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public abstract class ContextFlowClassifier<TContext> : IContextFlowClassifier<TContext>
    {
        /// <summary>
        /// Creates an expression that represents a funciton applying a collection of the classifiers to a context flow argument.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <param name="classifiers">The classifiers to use for the classification.</param>
        /// <returns>An expression that can be used as an operator in <see cref="Streamable.Select{TKey, TPayload, TResult}(IStreamable{TKey, TPayload}, Expression{Func{TPayload, TResult}})"/> method.</returns>
        public static Expression<Func<ContextFlow<TContext>, ClassifiedContextFlow<TContext>>> Classify(params IContextFlowClassifier<TContext>[] classifiers)
        {
            return (arg) =>
              new ClassifiedContextFlow<TContext>(arg.Flow, classifiers.Select(classifier => new ClassificationResult(classifier.Label, classifier.Score(arg))).ToArray(), arg.Context);
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