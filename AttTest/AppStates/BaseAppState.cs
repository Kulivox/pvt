using System;
using System.Collections.Generic;
using AttTest.AppStates.RelatedInterfaces;
using Avalonia.Threading;

namespace AttTest.AppStates
{
    public abstract class BaseAppState
    {
        protected (DateTime, Action<DateTime>)[] TimedTransitions { get; set; }
        
        protected (DateTime, Action<DateTime>)[] KeyPressInvokedTransitions { get; set; }
        
        public int RoundId { get; set; }

        public void HandleKeyPress(DateTime now)
        {
           HandleOutsideInput(KeyPressInvokedTransitions, now);
        }
        
        public void HandleTimerTick(DateTime now)
        {
            HandleOutsideInput(TimedTransitions, now);
        }
        

        private void HandleOutsideInput((DateTime, Action<DateTime>)[] transitions, DateTime currentTime) 
        {
            if (transitions.Length == 0)
            {
                return;
            }

            for (var i = 0; i < transitions.Length; i++)
            {
                var (time, action) = transitions[i];
                if (currentTime > time)
                {
                    action(currentTime);
                    return;
                }
            }
        }
        
        protected readonly IAttTestWindow AttTestWindow;
        protected readonly TimerConstants Constants;

        protected BaseAppState(IAttTestWindow attTestWindow, TimerConstants constants, int roundId)
        {
            AttTestWindow = attTestWindow;
            Constants = constants;
            TimedTransitions = Array.Empty<(DateTime, Action<DateTime>)>();
            KeyPressInvokedTransitions = Array.Empty<(DateTime, Action<DateTime>)>();
            RoundId = roundId;
        }
        
        
    }
}