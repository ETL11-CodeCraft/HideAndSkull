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
    //Todo : ���� ���ӽ� 3�ʰ� "���� ���ӵǾ����ϴ�." �޼��� ��� (Ȯ��)
    //Todo : "�����ϱ�" ��ư Ŭ���� �г��� ��ǲ�ʵ带 �÷��̾��� �г������� �����ϰ� �κ�� �̵� - �г����� �ԷµǾ������� ���� Ȯ�� �ʿ�
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

        //Todo : Lobby Canvas Active true�ǵ��� ����
        private void Connect()
        {
            PhotonNetwork.LocalPlayer.NickName = _nickName.text;
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " �г����� ��ϵǾ����ϴ�.");

            UI_Manager.instance.Resolve<UI_Lobby>()
                               .Show();
        }

        public void OnJoinedLobby()
        {
            StartCoroutine(C_ServerConnectText());
        }

        /// <summary>
        /// ���� ���� �� 3�ʰ� ���� ���� �޼��� ���
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