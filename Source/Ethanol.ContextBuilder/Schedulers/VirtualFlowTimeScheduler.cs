using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Globalization;

namespace Ethanol.ContextBuilder.Schedulers
{
    /// <summary>
    /// Represents a virtual flow time scheduler that implements the <see cref="IScheduler"/>, <see cref="IServiceProvider"/>, and <see cref="IStopwatchProvider"/> interfaces.
    /// </summary>
    /// <remarks>
    /// This scheduler is similar to <see cref="System.Reactive.Concurrency.HistoricalScheduler"/> class.
    /// It enables to schedule actions to be executed at a given virtual time derived from input flow data.
    /// </remarks>
    public class VirtualFlowTimeScheduler : IScheduler, IServiceProvider, IStopwatchProvider
    {
        private readonly SchedulerQueue<DateTimeOffset> _queue = new SchedulerQueue<DateTimeOffset>();

        private sealed class VirtualTimeStopwatch : IStopwatch
        {
            private readonly VirtualFlowTimeScheduler _parent;

            private readonly DateTimeOffset _start;

            public TimeSpan Elapsed => _parent.ClockToDateTimeOffset() - _start;

            public VirtualTimeStopwatch(VirtualFlowTimeScheduler parent, DateTimeOffset start)
            {
                _parent = parent;
                _start = start;
            }
        }

        public bool IsEnabled { get; private set; }

        protected IComparer<DateTimeOffset> Comparer { get; }

        public DateTimeOffset Clock { get; protected set; }

        public DateTimeOffset Now => ToDateTimeOffset(Clock);

        public VirtualFlowTimeScheduler()
            : this(default(DateTimeOffset), (IComparer<DateTimeOffset>)Comparer<DateTimeOffset>.Default)
        {
        }

        public VirtualFlowTimeScheduler(DateTimeOffset initialClock, IComparer<DateTimeOffset> comparer)
        {
            Clock = initialClock;
            Comparer = comparer ?? throw new ArgumentNullException("comparer");
        }


        protected DateTimeOffset Add(DateTimeOffset absolute, TimeSpan relative)
        {
            return absolute.Add(relative);
        }

        protected DateTimeOffset ToDateTimeOffset(DateTimeOffset absolute) => absolute;

        protected TimeSpan ToRelative(TimeSpan timeSpan) => timeSpan;

        public IDisposable ScheduleAbsolute<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            Func<IScheduler, TState, IDisposable> action2 = action;
            if (action2 == null)
            {
                throw new ArgumentNullException("action");
            }

            ScheduledItem<DateTimeOffset, TState>? si = null;
            Func<IScheduler, TState, IDisposable> action3 = delegate (IScheduler scheduler, TState state1)
            {
                if (si!=null) _queue.Remove(si);
                return action2(scheduler, state1);
            };
            si = new ScheduledItem<DateTimeOffset, TState>(this, state, action3, dueTime, Comparer);
            _queue.Enqueue(si);
            return si;
        }

        public IDisposable ScheduleRelative<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            DateTimeOffset dueTime2 = Add(Clock, dueTime);
            return ScheduleAbsolute(state, dueTime2, action);
        }

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            return ScheduleAbsolute(state, Clock, action);
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            return ScheduleRelative(state, ToRelative(dueTime), action);
        }

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            return ScheduleRelative(state, ToRelative(dueTime - Now), action);
        }

        public void Start()
        {
            if (IsEnabled)
            {
                return;
            }

            IsEnabled = true;
            do
            {
                IScheduledItem<DateTimeOffset>? next = GetNext();
                if (next != null)
                {
                    if (Comparer.Compare(next.DueTime, Clock) > 0)
                    {
                        Clock = next.DueTime;
                    }

                    next.Invoke();
                }
                else
                {
                    IsEnabled = false;
                }
            }
            while (IsEnabled);
        }

        public void Stop()
        {
            IsEnabled = false;
        }

        public void AdvanceTo(DateTimeOffset time)
        {
            int num = Comparer.Compare(time, Clock);
            if (num < 0)
            {
                throw new ArgumentOutOfRangeException("time");
            }

            if (num == 0)
            {
                return;
            }

            if (!IsEnabled)
            {
                IsEnabled = true;
                do
                {
                    IScheduledItem<DateTimeOffset>? next = GetNext();
                    if (next != null && Comparer.Compare(next.DueTime, time) <= 0)
                    {
                        if (Comparer.Compare(next.DueTime, Clock) > 0)
                        {
                            Clock = next.DueTime;
                        }

                        next.Invoke();
                    }
                    else
                    {
                        IsEnabled = false;
                    }
                }
                while (IsEnabled);
                Clock = time;
                return;
            }

            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "CANT_ADVANCE_WHILE_RUNNING", "AdvanceTo"));
        }

        public void AdvanceBy(TimeSpan time)
        {
            var val = Add(Clock, time);
            int num = Comparer.Compare(val, Clock);
            if (num < 0)
            {
                throw new ArgumentOutOfRangeException("time");
            }

            if (num != 0)
            {
                if (IsEnabled)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "CANT_ADVANCE_WHILE_RUNNING", "AdvanceBy"));
                }

                AdvanceTo(val);
            }
        }

        /// <summary>
        /// Skips the scheduler's clock to the specified time. It does not involve to execute any scheduled actions. 
        /// This method is suitable in the case when the scheduler's clock is behind the actual time and needs to 
        /// quickly move forward discarding any actions that should have been executed in the past.
        /// </summary>
        /// <param name="time">The time to skip to.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the specified time is less than the current time.</exception>
        public void SkipTo(DateTimeOffset time)
        {
            if (Comparer.Compare(time, Clock) < 0)
            {
                throw new ArgumentOutOfRangeException("time", "The time must be grater than the current time.");
            }

            Clock = time;
        }

        public void Sleep(TimeSpan time)
        {
            var val = Add(Clock, time);
            if (Comparer.Compare(val, Clock) < 0)
            {
                throw new ArgumentOutOfRangeException("time");
            }

            Clock = val;
        }

        protected IScheduledItem<DateTimeOffset>? GetNext()
        {
            while (_queue.Count > 0)
            {
                ScheduledItem<DateTimeOffset> scheduledItem = _queue.Peek();
                if (scheduledItem.IsCanceled)
                {
                    _queue.Dequeue();
                    continue;
                }

                return scheduledItem;
            }

            return null;
        }

        object? IServiceProvider.GetService(Type serviceType)
        {
            return GetService(serviceType);
        }

        protected virtual object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IStopwatchProvider))
            {
                return this;
            }

            return null;
        }

        public IStopwatch StartStopwatch()
        {
            DateTimeOffset start = ClockToDateTimeOffset();
            return new VirtualTimeStopwatch(this, start);
        }

        private DateTimeOffset ClockToDateTimeOffset()
        {
            return ToDateTimeOffset(Clock);
        }
    }
}