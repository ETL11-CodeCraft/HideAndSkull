using HideAndSkull.Lobby.UI;
using HideAndSkull.Settings.Sound;
using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace HideAndSkull.Lobby.Workflow
{
    public class LobbySceneWorkflow : MonoBehaviour
    {
        private void Start()
        {
            SoundManager.instance.PlayBGM("Home");

            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.DestroyAll();

            StartCoroutine(C_Workflow());
        }

        IEnumerator C_Workflow()
        {
            UI_Manager uiManager = UI_Manager.instance;

            //Photon server에 접속완료 될 때까지 대기
            yield return new WaitUntil(() => PhotonNetwork.IsConnected);

            if (PhotonNetwork.InRoom)
            {
                uiManager.Resolve<UI_Home>()
                    .Show();

                uiManager.Resolve<UI_Lobby>()
                    .Show();

                uiManager.Resolve<UI_Room>()
                    .Show();
            }
            else
            {
                uiManager.Resolve<UI_Home>()
                         .Show();
            }
        }
    }
}