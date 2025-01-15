using HideAndSkull.Lobby.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HideAndSkull.Lobby.UI
{
    //Todo : 서버 접속시 3초간 "서버 접속되었습니다." 메세지 출력 (확인)
    //Todo : "접속하기" 버튼 클릭시 닉네임 인풋필드를 플레이어의 닉네임으로 설정하고 로비로 이동 (확인)
    //Todo : "나가기" 버튼 클릭시 앱 종료 (확인)
    public class UI_Home : UI_Screen, ILobbyCallbacks
    {
        [Resolve] TMP_InputField _nickName;
        [Resolve] Button _connect;
        [Resolve] Button _exit;
        [Resolve] TMP_Text _serverConnect;

        const int PLAYER_NICKNAME_MAX_LENGTH = 11;


        protected override void Start()
        {
            base.Start();

            _serverConnect.gameObject.SetActive(false);
            _connect.onClick.AddListener(Connect);
            _exit.onClick.AddListener(Application.Quit);
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        //NickName 확인 후 입력된 글자가 있으면 로비로 이동
        private void Connect()
        {
            string nickName = _nickName.text.Trim();

            if(nickName == "")
            {
                UI_ConfirmWindow confirmWindow = UI_Manager.instance.Resolve<UI_ConfirmWindow>();

                confirmWindow.Show("닉네임은 공백으로 이루어질 수 없습니다.\n숫자나 영어, 한글을 이용해 입력해주세요.");
                return;
            }

            if(nickName.Length > 11)
            {
                nickName = nickName.Substring(0, PLAYER_NICKNAME_MAX_LENGTH);
            }

            PhotonNetwork.LocalPlayer.NickName = nickName;
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " 닉네임이 등록되었습니다.");

            UI_Manager.instance.Resolve<UI_Lobby>()
                               .Show();
        }

        public void OnJoinedLobby()
        {
            StartCoroutine(C_ServerConnectText());
        }

        /// <summary>
        /// 서버 접속 후 3초간 서버 접속 메세지 출력
        /// </summary>
        /// <returns></returns>
        public IEnumerator C_ServerConnectText()
        {
            _serverConnect.gameObject.SetActive(true) ;

            yield return new WaitForSeconds(3);

            _serverConnect.gameObject.SetActive(false);
        }

        public void OnLeftLobby()
        {
        }

        public void OnRoomListUpdate(List<RoomInfo> roomList)
        {
        }

        public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
        {
        }
    }
}