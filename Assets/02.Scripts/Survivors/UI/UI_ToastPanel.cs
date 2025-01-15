using HideAndSkull.Lobby.UI;
using HideAndSkull.Lobby.Utilities;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HideAndSkull.Survivors.UI
{

    public class UI_ToastPanel : UI_Screen
    {
        const int MAX_TOAST_COUNT = 5;
        UI_ToastMessageBox[] _uiToastPool;
        [Resolve] RectTransform _panel;
        [SerializeField] UI_ToastMessageBox _prefab;
        PhotonView _photonView;

        protected override void Awake()
        {
            base.Awake();
            _uiToastPool = new UI_ToastMessageBox[MAX_TOAST_COUNT];
            for (int i = 0; i < MAX_TOAST_COUNT; i++)
            {
                _uiToastPool[i] = Instantiate(_prefab, _panel);
                _uiToastPool[i].gameObject.SetActive(false);
            }
            _photonView = GetComponent<PhotonView>();
            _photonView.ViewID = 3;
        }

        public void ShowToast(string message)
        {
            _photonView.RPC("ShowToastRPC", RpcTarget.All, message);
        }

        [PunRPC]
        private void ShowToastRPC(string message)
        {
            bool isFound = false;

            for (int i = 0; i < _uiToastPool.Length; i++)
            {
                if (_uiToastPool[i].gameObject.activeSelf == false)
                {
                    isFound = true;
                    _uiToastPool[i].Show(message);
                    _uiToastPool[i].transform.SetParent(_panel);
                    break;
                }
            }

            if (isFound == false)
            {
                _uiToastPool[0].CancelToast();
                _uiToastPool[0].Show(message);
                _uiToastPool[0].transform.SetParent(_panel);
            }
        }
    }
}
