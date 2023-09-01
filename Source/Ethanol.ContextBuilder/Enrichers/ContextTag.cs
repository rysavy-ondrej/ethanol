using System;
using System.Text.Json.Serialization;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Represents an abstract base class for a context tag.
    /// </summary>
    public abstract class ContextTag
    {
        /// <summary>
        /// Gets or sets the name of the context tag.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the type of the key associated with the context tag.
        /// </summary>
        public abstract Type KeyType();

        /// <summary>
        /// Gets the type of the value associated with the context tag.
        /// </summary>
        public abstract Type ValueType();

        public bool HasKeyType<T>() => typeof(T) == KeyType();
        public bool HasValueType<T>() => typeof(T) == ValueType();

        /// <summary>
        /// Gets or sets the reliability score for the context tag.
        /// </summary>
        public double Reliability { get; set; }

        public abstract object KeyObject { get;  }

        /// <summary>
        /// Gets or sets the value associated with the key in the context tag.
        /// </summary>
        public abstract object ValueObject { get; }

        /// <summary>
        /// Casts the context tag to a specific key-value pair type.
        /// </summary>
        /// <typeparam name="K">The type of the key to cast to.</typeparam>
        /// <typeparam name="V">The type of the value to cast to.</typeparam>
        /// <returns>A context tag with the specified key-value pair types.</returns>
        public ContextTag<K, V> Cast<K, V>() => this as ContextTag<K, V>;
    }

    /// <summary>
    /// Represents a specific implementation of a context tag with defined key and value types.
    /// </summary>
    /// <typeparam name="TK">The type of the key.</typeparam>
    /// <typeparam name="TV">The type of the value.</typeparam>
    public class ContextTag<TK, TV> : ContextTag
    {
        /// <summary>
        /// Gets or sets the key of the context tag.
        /// </summary>
        public TK Key { get; set; }

        public override object KeyObject => this.Key;

        /// <summary>
        /// Gets or sets the value associated with the key in the context tag.
        /// </summary>
        public TV Value { get; set; }

        public override object ValueObject => this.Value;

        /// <summary>
        /// Gets the type of the key associated with the context tag.
        /// </summary>
        public override Type KeyType() => typeof(TK);

        /// <summary>
        /// Gets the type of the value associated with the context tag.
        /// </summary>
        public override Type ValueType() => typeof(TV);
    }

}
