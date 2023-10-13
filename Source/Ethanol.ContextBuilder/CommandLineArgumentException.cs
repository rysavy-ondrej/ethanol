using System;

namespace Ethanol.ContextBuilder
{
    /// <summary>
    /// Represents errors that occur due to invalid command-line arguments.
    /// </summary>
    /// <remarks>
    /// This exception provides details about the specific argument that caused the issue and the reason for the error.
    /// It is designed to give detailed feedback to users or developers when encountering issues with command-line inputs.
    /// </remarks>
    public class CommandLineArgumentException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineArgumentException"/> class with the specified argument and reason.
        /// </summary>
        /// <param name="argument">The argument that caused the exception.</param>
        /// <param name="reason">The reason why the argument is considered invalid.</param>
        public CommandLineArgumentException(string argument, string reason)
            : base($"Invalid Argument: Argument:{argument}, Error: {reason}.")
        {
            Argument = argument;
            Reason = reason;
        }

        /// <summary>
        /// Gets the argument that caused the exception.
        /// </summary>
        public string Argument { get; }

        /// <summary>
        /// Gets the reason why the argument is considered invalid.
        /// </summary>
        public string Reason { get; }
    }
}
