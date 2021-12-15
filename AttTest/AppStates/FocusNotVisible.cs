using System;
using AttTest.AppStates.RelatedInterfaces;
using JetBrains.Annotations;

namespace AttTest.AppStates
{
    public class FocusNotVisible : BaseAppState
    {
        public FocusNotVisible([NotNull] IAttTestWindow attTestWindow, TimerConstants constants, int roundId) : base(attTestWindow, constants, roundId)
        {
            TimedTransitions = new (DateTime, Action<DateTime>)[]
            {
                (DateTime.Now.AddSeconds(
                    new Random().NextDouble()
                    * (Constants.RoundLengthMaxSeconds - Constants.RoundLengthMinSeconds)
                    + Constants.RoundLengthMinSeconds), ShowFocus)
            };
        }

        private void ShowFocus(DateTime dateTime)
        {
            AttTestWindow.ShowFocusPoint();
            AttTestWindow.UpdateState(new FocusVisible(AttTestWindow, Constants, RoundId));
        }
        
    }
}