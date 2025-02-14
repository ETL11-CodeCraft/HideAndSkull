using HideAndSkull.Lobby.UI;
using HideAndSkull.Lobby.Utilities;
using HideAndSkull.Settings.Sound;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HideAndSkull.Lobby.UI
{

    public class UI_ConfirmWindow : UI_Popup
    {
        [Resolve] TextMeshProUGUI _message;
        [Resolve] Button _confirm;

        public void Show(string message, UnityAction onConfirmed = null)
        {
            base.Show();

            _message.text = message;
            _confirm.onClick.RemoveAllListeners();
            _confirm.onClick.AddListener(() =>
           {
               SoundManager.instance.PlayButtonSound();
               Hide();
           });

            if (onConfirmed != null)
                _confirm.onClick.AddListener(onConfirmed);
        }
    }
}
