using System;
using AttTest.AppStates.RelatedInterfaces;
using JetBrains.Annotations;

namespace AttTest.AppStates
{
    public class FocusNotVisible : BaseAppState
    {
        private readonly DateTime _focusStart;

        private readonly DateTime _focusVisible;

        public FocusNotVisible([NotNull] IAttTestWindow attTestWindow, TimerConstants constants, int roundId) : base(attTestWindow, constants, roundId)
        {
            _focusStart = DateTime.Now;
            _focusVisible = DateTime.Now.AddSeconds(
                new Random().NextDouble()
                * (Constants.RoundLengthMaxSeconds - Constants.RoundLengthMinSeconds)
                + Constants.RoundLengthMinSeconds);
            
            TimedTransitions = new (DateTime, Action<DateTime>)[]
            {
                (_focusVisible, ShowFocus)
            };

            KeyPressInvokedTransitions = new (DateTime, Action<DateTime>)[]
            {
                (_focusStart, FalseStart)
            };
        }
        
        private void FalseStart(DateTime dateTime)
        {
            AttTestWindow.HideFocusPoint();
            AttTestWindow.ShowFailure($"0","Too fast");
            AttTestWindow.SaveKeyPressTime(_focusVisible, dateTime, "not-visible-early");
            
            AttTestWindow.UpdateState(new FocusNotVisible(AttTestWindow, Constants, RoundId + 1));
        }

        private void ShowFocus(DateTime dateTime)
        {
            AttTestWindow.ShowFocusPoint();
            AttTestWindow.UpdateState(new FocusVisible(AttTestWindow, Constants, RoundId, _focusVisible));
        }
        
    }
}