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
    //Todo : ���� ���ӽ� 3�ʰ� "���� ���ӵǾ����ϴ�." �޼��� ���
    //Todo : "�����ϱ�" ��ư Ŭ���� �г��� ��ǲ�ʵ带 �÷��̾��� �г������� �����ϰ� �κ�� �̵�
    //Todo : "������" ��ư Ŭ���� �� ����
    public class UI_Home : UI_Screen, ILobbyCallbacks
    {
        [Resolve] TMP_InputField _nickName;
        [Resolve] Button _connect;
        [Resolve] Button _exit;
        [Resolve] TMP_Text _serverConnect;


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

        private void Connect()
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        public void OnJoinedLobby()
        {
            StartCoroutine(C_ServerConnectText());
            PhotonNetwork.LocalPlayer.NickName = _nickName.text;
        }

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