namespace HideAndSkull.Lobby.UI
{
    public class UI_Screen : UI_Base
    {
        public override void Show()
        {
            base.Show();

            manager.SetScreen(this);
        }
    }
}