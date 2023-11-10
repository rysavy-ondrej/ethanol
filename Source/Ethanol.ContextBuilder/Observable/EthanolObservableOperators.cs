using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Observable;
using Ethanol.ContextBuilder.Polishers;
using Ethanol.ContextBuilder.Readers;
using Ethanol.ContextBuilder.Writers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Pipeline
{
    public static class EthanolObservableOperators
    {
        public static IObservable<TFlow> LoadFlows<TFlow>(params IDataReader<TFlow>[] readers)
        {
            return readers.Merge();
        }

        public static Task StoreContextAsync<TContext>(this IObservable<TContext> observable, params IDataWriter<TContext>[] writers)
        {
            var connectable = observable.Publish();
            foreach(var writer in writers)
            {
                connectable.Subscribe(writer);
            }
            connectable.Connect();
            return Task.WhenAll(writers.Select(w => w.Completed));
        }

        public static IObservable<ObservableEvent<TPayload>> OrderSequence<TPayload>(this IObservable<ObservableEvent<TPayload>> observable, int sequenceLength)
        {
            var sequencer = new SequencerTransformer<TPayload>(sequenceLength);
            observable.Subscribe(sequencer);
            return sequencer;
        }
        public static IPAddress GetAddressOrDefault(this string ipString)
        {
            return IPAddress.TryParse(ipString, out var address) ? address : IPAddress.None;
        }

        public static IObservable<ObservableEvent<TContext>> IpHostContext<TContext>(this IObservable<ObservableEvent<IObservable<IpFlow>>> source,
            Func<KeyValuePair<IPAddress, IpFlow[]>, TContext> resultSelector)
        {
            KeyValuePair<IPAddress, IpFlow>[] GetKey(IpFlow flow)
            {
                return new[] { new KeyValuePair<IPAddress, IpFlow>(flow.SourceAddress, flow), new KeyValuePair<IPAddress, IpFlow>(flow.DestinationAddress, flow) };
            }
            return source.HostContext(GetKey, resultSelector);
        }

        public static IObservable<ObservableEvent<TContext>> HostContext<TFlow, TKey, TContext>(this IObservable<ObservableEvent<IObservable<TFlow>>> source,
            Func<TFlow, IEnumerable<KeyValuePair<TKey,TFlow>>> keySelector, 
            Func<KeyValuePair<TKey, TFlow[]>, TContext> resultSelector)
        {
            return source.SelectMany(window => 
                window.Payload
                    .SelectMany(keySelector)
                    .GroupByAggregate(k => k.Key, v => v.Value, g => new ObservableEvent<TContext>(resultSelector(g), window.StartTime, window.EndTime))
                );
        }

        public static Task Consume<T>(this IObservable<T> source, params IDataWriter<T>[] writers)
        {
            var connectable = source.Publish();
            foreach(var writer in writers)
            {
                connectable.Subscribe(writer);
            }
            connectable.Connect();
            return Task.WhenAll(writers.Select(x => x.Completed));
        }

        public static Task Run(IDataReader<IpFlow>[] readers, 
                    IDataWriter<ObservableEvent<IpTargetHostContext>>[] writers,
                    IObservableTransformer<ObservableEvent<IpHostContext>, ObservableEvent<IpHostContextWithTags>> enricher,
                    IObservableTransformer<ObservableEvent<IpHostContextWithTags>, ObservableEvent<IpTargetHostContext>> refiner)
        {
            var task = LoadFlows(readers)

                     .Select(t => new ObservableEvent<IpFlow>(t, t.TimeStart, t.TimeStart + t.TimeDuration))

                     .OrderSequence(16)

                     .HoppingWindow(TimeSpan.FromMinutes(5))

                     .IpHostContext(g => new IpHostContext { HostAddress = g.Key, Flows = g.Value })

                     .Where(c => c.Payload.HostAddress.Equals(IPAddress.Parse("192.168.111.19")))

                     .Transform(enricher)

                     .Transform(refiner)

                     .Consume(writers);

            readers.Select(x => x.ReadAllAsync(CancellationToken.None)).ToArray();

            return task;
        }

        public static IObservable<ObservableEvent<TResult>> Transform<TSource, TResult>(this IObservable<ObservableEvent<TSource>> source, IObservableTransformer<ObservableEvent<TSource>, ObservableEvent<TResult>> transformer)
        {
            source.Subscribe(transformer);
            return transformer;
        }
    }
}
