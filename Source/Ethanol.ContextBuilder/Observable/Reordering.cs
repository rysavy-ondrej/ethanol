using System;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.Generic;
using Ethanol.ContextBuilder.Observable;

public static class ReorderingTransformer
{
    public static IObservable<ObservableEvent<T>> ReorderStream<T>(
        this IObservable<ObservableEvent<T>> stream,
        int queueLength)
    {
        return Observable.Create<ObservableEvent<T>>(observer =>
        {
            // This SortedList is acting as our priority queue.
            SortedList<DateTime, Queue<ObservableEvent<T>>> buffer = new SortedList<DateTime, Queue<ObservableEvent<T>>>();
            var bufferedElements = 0;
            return stream
                // Materialize/Dematerialize are used to capture and propagate OnCompleted and OnError notifications.
                .Materialize()
                .Subscribe(notification =>
                {
                    if (notification.Kind == System.Reactive.NotificationKind.OnNext)
                    {
                        // On each incoming item, buffer it according to its timestamp.
                        var item = notification.Value;
                        if (!buffer.ContainsKey(item.StartTime))
                        {
                            buffer[item.StartTime] = new Queue<ObservableEvent<T>>();
                        }
                        buffer[item.StartTime].Enqueue(item);
                        bufferedElements++;

                        // If the buffer exceeds the specified length, dequeue the earliest item.
                        if (bufferedElements > queueLength)
                        {
                            var oldest = buffer.First();
                            observer.OnNext(oldest.Value.Dequeue());
                            bufferedElements--;
                            if (oldest.Value.Count == 0)
                            {
                                buffer.Remove(oldest.Key);
                            }
                        }
                    }
                    else if (notification.Kind == System.Reactive.NotificationKind.OnError)
                    {
                        // Propagate error notification
                        observer.OnError(notification.Exception);
                    }
                    else if (notification.Kind == System.Reactive.NotificationKind.OnCompleted)
                    {
                        // When the stream completes, flush any remaining items in the buffer in order.
                        foreach (var queue in buffer.Values)
                        {
                            while (queue.Count > 0)
                            {
                                observer.OnNext(queue.Dequeue());
                            }
                        }
                        observer.OnCompleted();
                    }
                });
        });
    }
}

