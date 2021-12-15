using System;
using System.Collections.Generic;
using AttTest.AppStates.RelatedInterfaces;

namespace AttTest.AppStates
{
    public abstract class BaseAppState
    {
        protected (DateTime, Action<DateTime>)[] TimedTransitions { get; set; }
        
        protected (DateTime, Action<DateTime>)[] KeyPressInvokedTransitions { get; set; }
        
        public int RoundId { get; set; }

        public void HandleKeyPress()
        {
           HandleOutsideInput(KeyPressInvokedTransitions);
        }
        
        public void HandleTimerTick()
        {
            HandleOutsideInput(TimedTransitions);
        }

        private void HandleOutsideInput((DateTime, Action<DateTime>)[] transitions) 
        {
            if (transitions.Length == 0)
            {
                return;
            }
            
            var currentTime = DateTime.Now;

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