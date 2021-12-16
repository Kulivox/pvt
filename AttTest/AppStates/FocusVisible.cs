using System;
using AttTest.AppStates.RelatedInterfaces;
using JetBrains.Annotations;

namespace AttTest.AppStates
{
    public class FocusVisible : BaseAppState
    {
        // time of construction of this state
        private readonly DateTime _focusStart;
        // real time when focus should become visible
        private readonly DateTime _focusVisible;
        public FocusVisible([NotNull] IAttTestWindow attTestWindow, [NotNull] TimerConstants constants, int roundId, DateTime focusVisible) : base(attTestWindow, constants, roundId)
        {
            _focusStart = DateTime.Now;
            _focusVisible = focusVisible;
            TimedTransitions = new (DateTime, Action<DateTime>)[]
            {
                (_focusStart.AddMilliseconds(Constants.FocusPointVisibilityLength), MissedRound)
            };
            
            // order of these two actions matters, we need the most distant action to be first
            KeyPressInvokedTransitions = new (DateTime, Action<DateTime>)[]
            {
                (_focusStart.AddMilliseconds(100), CorrectHit),
                (_focusStart, FalseStart)
            };
        }

        private void MissedRound(DateTime dateTime)
        {
            AttTestWindow.HideFocusPoint();
            AttTestWindow.ShowFailure($"{(dateTime - _focusStart).Milliseconds}","Missed round");
            AttTestWindow.SaveKeyPressTime(_focusVisible, dateTime, "miss");

            AttTestWindow.UpdateState(new FocusNotVisible(AttTestWindow, Constants, RoundId + 1));
        }

        private void FalseStart(DateTime dateTime)
        {
            AttTestWindow.HideFocusPoint();
            AttTestWindow.ShowFailure($"{(dateTime - _focusStart).Milliseconds}","Too fast");
            AttTestWindow.SaveKeyPressTime(_focusVisible, dateTime, "visible-early");

            AttTestWindow.UpdateState(new FocusNotVisible(AttTestWindow, Constants, RoundId + 1));
        }

        private void CorrectHit(DateTime dateTime)
        {
            var hitTime = (dateTime - _focusStart).Milliseconds;
            if (hitTime <= Constants.FocusPointVisibilityLength)
            {
                AttTestWindow.ShowSuccess(hitTime.ToString(), "");
                AttTestWindow.SaveKeyPressTime(_focusVisible, dateTime, "correct-hit");
            }
            else
            {
                AttTestWindow.ShowFailure(hitTime.ToString(), "miss");
                AttTestWindow.SaveKeyPressTime(_focusVisible, dateTime, "miss");
            }
            AttTestWindow.HideFocusPoint();
            AttTestWindow.UpdateState(new FocusNotVisible(AttTestWindow, Constants, RoundId + 1));
        }
    }
}