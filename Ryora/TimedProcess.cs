using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Ryora.Client
{
    public class TimedProcessor
    {
        public int TimesElapsed { get; set; }

        public bool IsRunning => Timer.Enabled;

        private bool IsStopped { get; set; }

        public void Start()
        {
            Timer.Start();
            IsStopped = false;
        }

        public void Stop()
        {
            Timer.Stop();
            IsStopped = true;
        }

        public TimedProcessor(int interval, Func<Task> processor, bool whenStartedFireImmediately = true)
        {
            AsyncProcessor = processor;
            HasAsyncProcessor = true;
            InitializeTimedProcessor(interval, whenStartedFireImmediately);
        }
        public TimedProcessor(int interval, Action processor, bool whenStartedFireImmediately = true)
        {
            Processor = processor;
            HasProcessor = true;
            InitializeTimedProcessor(interval, whenStartedFireImmediately);
        }

        private bool HasFired { get; set; }
        private int Interval { get; set; }
        private Stopwatch Duration = new Stopwatch();
        internal bool HasProcessor = false;
        private Action Processor { get; set; }
        internal bool HasAsyncProcessor = false;
        private Func<Task> AsyncProcessor { get; set; }
        private Timer Timer { get; set; }
        private void InitializeTimedProcessor(int interval, bool whenStartedFireImmediately)
        {
            Interval = interval;
            HasFired = !whenStartedFireImmediately;
            TimesElapsed = 0;
            Duration.Start();
            Timer = new Timer() { AutoReset = false };
            if (!whenStartedFireImmediately) Timer.Interval = interval;
            Timer.Elapsed += TimerElapsed;
        }

        private async void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            TimesElapsed++;
            var timer = (sender as Timer);
            if (HasAsyncProcessor)
            {
                await AsyncProcessor.Invoke().ConfigureAwait(false);
            }
            if (HasProcessor)
            {
                Processor.Invoke();
            }
            if (!HasFired)
            {
                HasFired = true;
                if (timer != null) timer.Interval = Interval;
            }
            if (!IsStopped)
                timer?.Start();
        }

        public override string ToString()
        {
            return $"TimedProcessor executed {TimesElapsed} times over {Duration}, average of {Math.Round((TimesElapsed / Duration.Elapsed.TotalSeconds), 2)} per second";
        }
    }
}
