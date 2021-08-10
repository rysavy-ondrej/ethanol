using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Ethanol.Demo
{
    /// <summary>
    /// Represents a single fact in the context.
    /// </summary>
    public record Fact(string Label, Artifact Artifact);

    /// <summary>
    /// The delegate that defines the type for fact loader functions.
    /// </summary>
    /// <typeparam name="TArtifact">The type of target artifact.</typeparam>
    /// <typeparam name="TInput">The type of input artifacts.</typeparam>
    /// <param name="target">The target artifact.</param>
    /// <param name="input">The queryable collection of input artifacts.</param>
    /// <returns></returns>
    public delegate IEnumerable<Fact> FactLoaderFunction<in TArtifact, in TInput>(TArtifact target, IQueryable<TInput> input)   
        where TArtifact : Artifact
        where TInput : Artifact;

    public delegate IEnumerable<Fact> FactLoaderFunction<in TArtifact, in TInput1, in TInput2>(TArtifact target, IQueryable<TInput1> input1, IQueryable<TInput2> input2)
        where TArtifact : Artifact
        where TInput1 : Artifact
        where TInput2 : Artifact;

    public delegate IEnumerable<Fact> FactLoaderFunction<in TArtifact, in TInput1, in TInput2, in TInput3>(TArtifact target, IQueryable<TInput1> input1, IQueryable<TInput2> input2, IQueryable<TInput3> input3)
        where TArtifact : Artifact
        where TInput1 : Artifact
        where TInput2 : Artifact
        where TInput3 : Artifact;
}