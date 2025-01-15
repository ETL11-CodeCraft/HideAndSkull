﻿using HideAndSkull.Lobby.UI;
using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace HideAndSkull.Lobby.Workflow
{
    public class LobbySceneWorkflow : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(C_Workflow());
        }

        IEnumerator C_Workflow()
        {
            UI_Manager uiManager = UI_Manager.instance;

            //Photon server에 접속완료 될 때까지 대기
            yield return new WaitUntil(() => PhotonNetwork.IsConnected);

            uiManager.Resolve<UI_Home>()
                     .Show();
        }
    }
}