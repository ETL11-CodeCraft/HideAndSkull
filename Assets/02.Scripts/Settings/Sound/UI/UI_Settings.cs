using HideAndSkull.Lobby.UI;
using HideAndSkull.Lobby.Utilities;
using UnityEngine.UI;
using UnityEngine;

namespace HideAndSkull.Settings.UI
{
    class UI_Settings : UI_Popup
    {
        [Resolve] Button _exitGame;
        [Resolve] Button _close;

        public override void Show()
        {
            base.Show();

            _exitGame.onClick.RemoveAllListeners();
            _close.onClick.RemoveAllListeners();

            _exitGame.onClick.AddListener(() =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });

            _close.onClick.AddListener(() =>
            {
                Hide();
            });
        }
    }
}