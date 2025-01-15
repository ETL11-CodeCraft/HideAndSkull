using HideAndSkull.Lobby.UI;
using HideAndSkull.Lobby.Utilities;
using System.Collections.Generic;
using UnityEngine;


namespace HideAndSkull.Survivors.UI
{

    public class UI_ToastPanel : UI_Screen
    {
        const int MAX_TOAST_COUNT = 5;
        Queue<UI_ToastMessageBox> _uiToastQueue;
        [Resolve] RectTransform _panel;
        [SerializeField] UI_ToastMessageBox _prefab;

        protected override void Awake()
        {
            base.Awake();
            _uiToastQueue = new Queue<UI_ToastMessageBox>(MAX_TOAST_COUNT);
        }

        public void ShowToast(string message)
        {
            if (_uiToastQueue.Count >= MAX_TOAST_COUNT)
            {
                if (_uiToastQueue.TryPeek(out var peek))
                {
                    peek.CancelToast();
                }
            }

            UI_ToastMessageBox uiToast = Instantiate(_prefab, _panel);
            _uiToastQueue.Enqueue(uiToast);
            uiToast.Show(message);
            uiToast.OnHide = () =>
            {
                _uiToastQueue.Dequeue();
            };
        }
    }
}
