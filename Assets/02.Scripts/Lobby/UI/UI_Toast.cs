using HideAndSkull.Lobby.Utilities;
using System.Collections;
using TMPro;
using UnityEngine;

namespace HideAndSkull.Lobby.UI
{

    public class UI_Toast : UI_Popup
    {
        [Resolve] TextMeshProUGUI _message;

        public void Show(string message, float duration = 0.25f)
        {
            base.Show();

            _message.text = message;
            StartCoroutine(C_HideAfterDuration(duration));
        }

        IEnumerator C_HideAfterDuration(float duration)
        {
            yield return new WaitForSeconds(duration);
            Hide();
        }
    }
}
