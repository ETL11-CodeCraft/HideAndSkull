using ExitGames.Client.Photon;
using HideAndSkull.Lobby.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HideAndSkull.Lobby.UI
{
    //Todo : ���ڵ� ����
    //Todo : �÷��̾� ��� �����ϱ�
    //Todo : ä��â�� �÷��̾� ���� / ���� ����
    //Todo : ä��â�� �Է��ϱ�
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
        List<TMP_Text> _chatList;
        List<TMP_Text> _playerList;
        PhotonView _photonView;

        const int CHAT_LIST_LIMIT_MAX = 12;
        const int PLAYER_LIST_LIMIT_MAX = 8;


        protected override void Awake()
        {
            base.Awake();

            _photonView = GetComponent<PhotonView>();

            _chatList = new List<TMP_Text>(CHAT_LIST_LIMIT_MAX);
            _playerList = new List<TMP_Text>(PLAYER_LIST_LIMIT_MAX);

            RefreshChatList(_chatList);
            RefreshPlayerList(_playerList);
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
        }

        private void RefreshChatList(List<TMP_Text> chatList)
        {
            //PoolingChatList

            for (int i = 0; i < CHAT_LIST_LIMIT_MAX; i++)
            {
                TMP_Text chat = Instantiate(_chatText, _chatListContent);
                chat.text = "";
                chat.gameObject.SetActive(true);
                _chatList.Add(chat);
            }
        }

        private void RefreshPlayerList(List<TMP_Text> playerList)
        {
            //PoolingPlayerList
            for (int i = 0; i < PLAYER_LIST_LIMIT_MAX; i++)
            {
                TMP_Text playerNickName = Instantiate(_playerNickName, _playerListContent);
                playerNickName.text = "";
                playerNickName.gameObject.SetActive(true);
                _playerList.Add(playerNickName);
            }
        }

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
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            _photonView.RPC("ChatRPC", RpcTarget.All, $"<color=yellow>{otherPlayer.NickName}���� �����ϼ̽��ϴ�</color>");
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
        }

        #region PlayerList  
        //�÷��̾� ����Ʈ�� ����� �ٸ� �÷��̾���� �����ؼ� ǥ���ϸ�, ����� �߰��ϰ� ����� �����Ѵ�.
        private void PlayerListRenewal(Player player)
        {
            if (player.IsMasterClient)
                _photonView.RPC("PlayerListRPC", RpcTarget.All, "[����]" + PhotonNetwork.NickName);
            else
                _photonView.RPC("PlayerListRPC", RpcTarget.All, "      " + PhotonNetwork.NickName);

        }

        [PunRPC]
        private void PlayerListRPC(string nickName)
        {
            //������ �ִ� �÷��̾� �г����̸� ����.
            //������ ���� �÷��̾� �г����̸� �߰�.
            //List ������ �� ������ Ŭ���̾�Ʈ �ٲ�� [����] �÷��̾ ����

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

            for (int i = 0; i < _chatList.Count; i++)
            {
                if (_chatList[i].text == "")
                {
                    isInput = true;
                    _chatList[i].text = message;
                    break;
                }
            }

            if (!isInput) // ������ ��ĭ�� ���� �ø�
            {
                for (int i = 1; i < _chatList.Count; i++)
                {
                    _chatList[i - 1].text = _chatList[i].text;
                }

                _chatList[_chatList.Count - 1].text = message;
            }
        }
        #endregion
    }
}