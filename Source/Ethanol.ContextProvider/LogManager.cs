using Microsoft.Extensions.Logging;

namespace Ethanol
{
    /// <summary>
    /// Provides centralized logging management for the application.
    /// </summary>
    /// <remarks>
    /// The LogManager is designed to simplify the process of retrieving and 
    /// setting up loggers throughout the application. It supports both 
    /// generic and non-generic logger retrieval, offering flexibility 
    /// based on the requirements.
    /// </remarks>
    public static class LogManager
    {
        // Holds a reference to the global logger instance.
        private static ILogger globalLogger;

        // Holds a reference to the logger factory instance, used to create logger instances.
        private static ILoggerFactory loggerFactory;


        public static void SetGlobalLogger(ILogger logger)
        {
            globalLogger = logger;
        }
        /// <summary>
        /// Configures the LogManager with a specified logger factory and initializes the global logger.
        /// </summary>
        /// <param name="loggerFactory">The logger factory to be used for creating logger instances.</param>
        public static void SetLoggerFactory(ILoggerFactory loggerFactory)
        {
            LogManager.loggerFactory = loggerFactory;
            LogManager.globalLogger = loggerFactory.CreateLogger("Global");
        }

        /// <summary>
        /// Provides access to the global logger instance.
        /// </summary>
        /// <value>The global logger.</value>
        public static ILogger Logger => globalLogger;

        /// <summary>
        /// Retrieves a logger instance for the specified class type.
        /// </summary>
        /// <typeparam name="T">The class type for which a logger instance should be retrieved.</typeparam>
        /// <returns>A logger instance associated with the given class type.</returns>
        public static ILogger<T> GetClassLogger<T>() where T : class => loggerFactory.CreateLogger<T>();

        /// <summary>
        /// Retrieves a logger instance based on the specified category name or defaults to the "Global" category.
        /// </summary>
        /// <param name="categoryName">The category name for the logger. If not specified, defaults to "Global".</param>
        /// <returns>A logger instance associated with the given category name.</returns>
        public static ILogger GetCurrentClassLogger() => loggerFactory?.CreateLogger("Global") ?? globalLogger;
    }
}
