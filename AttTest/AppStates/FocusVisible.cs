using System;
using AttTest.AppStates.RelatedInterfaces;
using JetBrains.Annotations;

namespace AttTest.AppStates
{
    public class FocusVisible : BaseAppState
    {
        private readonly DateTime _focusStart;
        public FocusVisible([NotNull] IAttTestWindow attTestWindow, [NotNull] TimerConstants constants, int roundId) : base(attTestWindow, constants, roundId)
        {
            _focusStart = DateTime.Now;
            TimedTransitions = new (DateTime, Action<DateTime>)[]
            {
                (_focusStart.AddMilliseconds(Constants.FocusPointVisibilityLength), MissedRound)
            };
            
            // order of these two actions matters, we need the most distant action to be first
            KeyPressInvokedTransitions = new (DateTime, Action<DateTime>)[]
            {
                (_focusStart.AddMilliseconds(constants.FalseStartPointVisibleLength), CorrectHit),
                (_focusStart, FalseStart)
            };
        }

        private void MissedRound(DateTime dateTime)
        {
            AttTestWindow.HideFocusPoint();
            AttTestWindow.ShowFailure("Missed round");
            
            AttTestWindow.UpdateState(new FocusNotVisible(AttTestWindow, Constants, RoundId + 1));
        }

        private void FalseStart(DateTime dateTime)
        {
            AttTestWindow.HideFocusPoint();
            AttTestWindow.ShowFailure("Too fast");
            
            AttTestWindow.UpdateState(new FocusNotVisible(AttTestWindow, Constants, RoundId + 1));
        }

        private void CorrectHit(DateTime dateTime)
        {
            AttTestWindow.ShowSuccess((dateTime - _focusStart).Milliseconds.ToString());
            AttTestWindow.HideFocusPoint();
            AttTestWindow.UpdateState(new FocusNotVisible(AttTestWindow, Constants, RoundId + 1));
        }
    }
}