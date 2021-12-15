namespace AttTest.AppStates.RelatedInterfaces
{
    public interface IAttTestWindow
    {
        public void UpdateState(BaseAppState newState);
        
        public void HideFocusPoint();

        public void ShowFocusPoint();

        public void ShowFailure(string failureText);

        public void ShowSuccess(string success);

        public void HideResult();

        public void EndTest();
    }
}