# Tasks and Exceptions

## Properly Cancelled Tasks

Using asynchronous methods with cancellation tokens in C# allows you to handle task cancellation in a responsive and controlled manner. 
Proper use involves passing a `CancellationToken` to your async methods and periodically checking whether cancellation has been requested inside those methods. 

Here's how to handle it correctly:

### 1. Passing a CancellationToken

When calling an async method, pass a `CancellationToken` as an argument. This token is typically obtained from a `CancellationTokenSource`.

```csharp
CancellationTokenSource cts = new CancellationTokenSource();
CancellationToken token = cts.Token;

await MyAsyncMethod(token);
```

### 2. Handling CancellationToken in Async Methods

Within the asynchronous method, check for cancellation at appropriate points. If cancellation is requested, throw a `OperationCanceledException`.

```csharp
async Task MyAsyncMethod(CancellationToken cancellationToken)
{
    // Periodically check for cancellation
    if (cancellationToken.IsCancellationRequested)
    {
        // Clean up if necessary
        cancellationToken.ThrowIfCancellationRequested();
    }

    // Perform asynchronous operation
    await Task.Delay(1000, cancellationToken); // This method internally checks the token
}
```

### 3. Handling the Cancellation

When you `await` the async method, handle the `OperationCanceledException` to distinguish between normal completion and cancellation.

```csharp
try
{
    await MyAsyncMethod(token);
}
catch (OperationCanceledException)
{
    // Handle the cancellation scenario
}
catch (Exception ex)
{
    // Handle other exceptions
}
```

### Deciding Whether the Method Finished Normally or Was Cancelled

- **Method Completed Normally**: If the method completes its execution without any exceptions, and without the cancellation token being triggered, it has finished normally.
- **Method Was Cancelled**: If a `OperationCanceledException` is thrown (and caught), it indicates that the method was cancelled.

### Best Practices and Tips

- **Cooperative Cancellation**: Cancellation in .NET is cooperative, meaning the code has to periodically check the `CancellationToken` and opt-in to respond to cancellation.
- **Pass CancellationToken to Async I/O Operations**: Many built-in async I/O operations accept a `CancellationToken` and handle cancellation internally.
- **Avoid Blocking Calls**: Don't use blocking calls (like `Task.Wait` or `Task.Result`) on async methods as they can lead to deadlocks. Always use `await`.
- **Free Resources on Cancellation**: If you have resources that need to be freed or certain actions that need to be performed on cancellation, do it before throwing the `OperationCanceledException`.
- **Idempotency and Safety**: Ensure that your async methods are safe to cancel. This often means making sure operations are idempotent or that you can roll back changes if cancellation occurs midway.
- **Consider Timeouts**: Sometimes, you may want to use a cancellation token with a timeout. This can be done by passing a timespan to `CancellationTokenSource`.
- **Logging and Cleanup**: Consider adding logging for cancellations for diagnostic purposes and ensure proper cleanup in case of cancellation.

## Exception stored on Task and Thrown

In C# and the .NET Task Parallel Library, exceptions that occur in an asynchronous task are handled in a specific manner. Understanding when and how these exceptions are stored and thrown is crucial for effective error handling in asynchronous programming.

### When the Exception is Stored

When an exception occurs within a `Task`, it is captured and stored in the `Task` object. This happens in the following scenarios:

- An exception is thrown and not caught within the task.
- The task is running on a separate thread or thread pool (which is usually the case with `Task.Run` or when using `async` and `await`).

The exception is wrapped in an `AggregateException` object. This object can potentially contain multiple exceptions if more than one exception was thrown during the task's execution.

### When the Exception is Thrown

The stored exception is thrown when you interact with the `Task` in ways that require its result or completion status:

1. **Awaiting the Task**: When you `await` a task, any stored exception is re-thrown automatically. This is the common scenario in `async` methods.

   ```csharp
   try
   {
       await someTask;
   }
   catch (Exception ex)
   {
       // Handle exception
   }
   ```

   In this case, `await` unwraps the `AggregateException`, and you can catch the specific exception directly.

2. **Accessing Task.Result**: If you access the `Result` property of a `Task<T>`, any stored exception is re-thrown. 

   ```csharp
   try
   {
       var result = someTask.Result;
   }
   catch (AggregateException ex)
   {
       // Handle exception
   }
   ```

   Here, the exception is thrown as an `AggregateException`.

3. **Task.Wait()**: If you use the `Wait()` method on a task, an `AggregateException` will be thrown if the task encountered an exception.

   ```csharp
   try
   {
       someTask.Wait();
   }
   catch (AggregateException ex)
   {
       // Handle exception
   }
   ```

### Important Notes

- **Unobserved Exceptions**: If you don't observe (i.e., catch or otherwise interact with) the exception, it will remain stored in the `Task`. Unobserved exceptions in tasks can lead to application crashes in certain versions of .NET Framework, but in .NET Core and later versions of the .NET Framework, they are ignored by default.
- **AggregateException**: This exception type can wrap multiple exceptions. Use the `AggregateException.Flatten()` method or iterate through `AggregateException.InnerExceptions` to handle all the underlying exceptions.
- **Best Practices**: It's generally a good practice to handle exceptions where you await the task or immediately after starting a task if you're using continuation tasks. Avoiding unobserved task exceptions is crucial for robust error handling in asynchronous programming.
