using System;

namespace AttTest.AppStates.RelatedInterfaces
{
    public interface IAttTestWindow
    {
        public void UpdateState(BaseAppState newState);
        
        public void HideFocusPoint();

        public void ShowFocusPoint();

        public void ShowFailure(string failureText, string note);

        public void ShowSuccess(string success, string note);

        public void SaveKeyPressTime(DateTime focus, DateTime keyPress, string type);

        public void HideResult();

        public void EndTest();
    }
}