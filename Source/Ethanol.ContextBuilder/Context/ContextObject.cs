namespace Ethanol.ContextBuilder.Context
{
    /// <summary>
    /// The context object stores <see cref="Context"/> information about the target <see cref="Object"/>. 
    /// </summary>
    /// <typeparam name="TTarget">The type of target object.</typeparam>
    /// <typeparam name="TContext">The type that represents the context.</typeparam>
    public record ContextObject<TTarget, TContext>
    {
        /// <summary>
        /// Creates the context object.
        /// </summary>
        public ContextObject() { }
        /// <summary>
        /// Creaets the context object.
        /// </summary>
        /// <param name="id">The record ID.</param>
        /// <param name="window">The validity window.</param>
        /// <param name="targetObject">The target object of the context.</param>
        /// <param name="context">The context record for the target object.</param>
        public ContextObject(string id, WindowSpan window, TTarget targetObject, TContext context)
        {
            Id = id;
            Window = window;
            Object = targetObject;
            Context = context;
        }
        /// <summary>
        /// Gets or sets a unique identifier of the context object.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The window of validity of the entity context information.
        /// </summary>
        public WindowSpan Window { get; set; }
        /// <summary>
        /// Get the target object.
        /// </summary>
        public TTarget Object { get; set; }
        /// <summary>
        /// Get the context of the <see cref="Object"/>.
        /// </summary>
        public TContext Context { get; set; }
    }
}
