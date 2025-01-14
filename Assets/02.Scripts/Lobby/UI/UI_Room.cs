using ExitGames.Client.Photon;
using HideAndSkull.Lobby.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HideAndSkull.Lobby.UI
{
    //Todo : ���ڵ� ���� (Ȯ��)
    //Todo : �÷��̾� ��� �����ϱ�
    //Todo : ä��â�� �÷��̾� ���� / ���� ���� (Ȯ��)
    //Todo : ä��â�� �Է��ϱ� (Ȯ��)
    [RequireComponent(typeof(PhotonView))]
    public class UI_Room : UI_Screen, IInRoomCallbacks
    {
        [Resolve] TMP_Text _roomCode;
        [Resolve] RectTransform _chatListContent;
        [Resolve] TMP_Text _chatText;
        [Resolve] TMP_InputField _chatInput;
        [Resolve] Button _chatEnter;
        [Resolve] Button _gameStart;
        [Resolve] Button _exitRoom;
        [Resolve] RectTransform _playerListContent;
        [Resolve] TMP_Text _playerNickName;
        TMP_Text[] _chatArray;   //List -> Array�� �����ұ�
        TMP_Text[] _playerArray;
        PhotonView _photonView;

        const int CHAT_LIST_LIMIT_MAX = 12;
        const int PLAYER_LIST_LIMIT_MAX = 8;


        protected override void Awake()
        {
            base.Awake();

            _photonView = GetComponent<PhotonView>();

            _chatArray = new TMP_Text[CHAT_LIST_LIMIT_MAX];
            _playerArray = new TMP_Text[PLAYER_LIST_LIMIT_MAX];

            PoolingChatList();
            PoolingPlayerList();
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        protected override void Start()
        {
            base.Start();

            _chatEnter.onClick.AddListener(MessageSend);

            _gameStart.onClick.AddListener(() =>
            {
                SceneManager.LoadScene(1);
            });

            _exitRoom.onClick.AddListener(() =>
            {
                PhotonNetwork.LeaveRoom();
            });
        }

        public override void Show()
        {
            base.Show();

            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("RoomCode"))
            {
                string roomCode = PhotonNetwork.CurrentRoom.CustomProperties["RoomCode"].ToString();
                _roomCode.text = "���ڵ� : " + roomCode;
            }
            else
            {
                _roomCode.text = "���ڵ带 ã�� �� �����ϴ�.";
            }

            TogglePlayerButtons(PhotonNetwork.LocalPlayer);
            _photonView.RPC("ChatRPC", RpcTarget.All, $"<color=yellow>{PhotonNetwork.MasterClient.NickName}���� �����ϼ̽��ϴ�</color>");
            _photonView.RPC("PlayerListRPC", RpcTarget.All);
        }

        private void PoolingChatList()
        {
            //PoolingChatList
            for (int i = 0; i < CHAT_LIST_LIMIT_MAX; i++)
            {
                TMP_Text chat = Instantiate(_chatText, _chatListContent);
                chat.text = "";
                chat.gameObject.SetActive(true);
                _chatArray[i] = chat;
            }
        }

        private void PoolingPlayerList()
        {
            //PoolingPlayerList
            for (int i = 0; i < PLAYER_LIST_LIMIT_MAX; i++)
            {
                TMP_Text playerNickName = Instantiate(_playerNickName, _playerListContent);
                playerNickName.text = "";
                playerNickName.gameObject.SetActive(true);
                _playerArray[i] = playerNickName;
            }
        }

        //�÷��̾� ����Ʈ�� ��� �ʱ�ȭ�� ��, ���� �÷��̾ �ϳ��� �Է��ұ�?
        //private void RefreshPlayerList()
        //{
        //    for (int i = 0; i < PLAYER_LIST_LIMIT_MAX; i++)
        //    {
        //        _playerArray[i].text = "";
        //    }

        //    int index = 0;

        //    foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
        //    {
        //        if (player.IsMasterClient)
        //            _playerArray[index].text = $"[����] {player.NickName}";
        //        else
        //            _playerArray[index].text = $"       {player.NickName}";

        //        index++;
        //    }
        //}

        void TogglePlayerButtons(Player player)
        {
            if (player.IsMasterClient)
            {
                _gameStart.gameObject.SetActive(true);
            }
            else
            {
                _gameStart.gameObject.SetActive(false);
            }
        }

        //Todo : Player ��� ���Ž� Ȯ���Ͽ� ������ ����
        public void OnMasterClientSwitched(Player newMasterClient)
        {
            if (newMasterClient.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                TogglePlayerButtons(newMasterClient);
        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            _photonView.RPC("ChatRPC", RpcTarget.All, $"<color=yellow>{newPlayer.NickName}���� �����ϼ̽��ϴ�</color>");

            _photonView.RPC("PlayerListRPC", RpcTarget.All);

            //RefreshPlayerList();
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            _photonView.RPC("ChatRPC", RpcTarget.All, $"<color=yellow>{otherPlayer.NickName}���� �����ϼ̽��ϴ�</color>");

            _photonView.RPC("PlayerListRPC", RpcTarget.All);

            //RefreshPlayerList();
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
        }

        #region PlayerList  
        //�÷��̾� ����Ʈ�� ����� �ٸ� �÷��̾���� �����ؼ� ǥ���ϸ�, ����� �߰��ϰ� ����� �����Ѵ�.
        [PunRPC]
        private void PlayerListRPC()
        {
            //������ �ִ� �÷��̾� �г����̸� ����.
            //������ ���� �÷��̾� �г����̸� �߰�.
            //List ������ �� ������ Ŭ���̾�Ʈ �ٲ�� [����] �÷��̾ ����

            int index = 0;

            foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                if (player.IsMasterClient)
                    _playerArray[index].text = $"[����] {player.NickName}";
                else
                    _playerArray[index].text = $"       {player.NickName}";

                index++;
            }

            for (int i = index; i < _playerArray.Length; i++)
            {
                _playerArray[i].text = $"";
            }
        }
        #endregion

        #region Chatting
        private void MessageSend()
        {
            _photonView.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + _chatInput.text);
            _chatInput.text = "";
        }

        [PunRPC]
        private void ChatRPC(string message)
        {
            bool isInput = false;

            for (int i = 0; i < _chatArray.Length; i++)
            {
                if (_chatArray[i].text == "")
                {
                    isInput = true;
                    _chatArray[i].text = message;
                    break;
                }
            }

            if (!isInput) // ������ ��ĭ�� ���� �ø�
            {
                for (int i = 1; i < _chatArray.Length; i++)
                {
                    _chatArray[i - 1].text = _chatArray[i].text;
                }

                _chatArray[_chatArray.Length - 1].text = message;
            }
        }
        #endregion
    }
}