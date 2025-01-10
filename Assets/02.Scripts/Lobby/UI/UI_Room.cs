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

            _chatList = new List<TMP_Text>(CHAT_LIST_LIMIT_MAX);
            _playerList = new List<TMP_Text>(PLAYER_LIST_LIMIT_MAX);
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
                _roomCode.text = "룸코드 : " + roomCode;
            }

            //Pooling ChatText, PlayerText
            for (int i = 0; i < _chatList.Count; i++)
            {
                TMP_Text chat = Instantiate(_chatText, _chatListContent);
                chat.text = "";
                chat.gameObject.SetActive(true);
                _chatList.Add(chat);
            }

            for (int i = 0; i < _playerList.Count; i++)
            {
                TMP_Text playerNickName = Instantiate(_playerNickName, _playerListContent);
                playerNickName.text = "";
                playerNickName.gameObject.SetActive(true);
                _playerList.Add(playerNickName);
            }

            TogglePlayerButtons(PhotonNetwork.LocalPlayer);
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

        //Todo : Player 목록 갱신시 확인하여 마스터 변경
        public void OnMasterClientSwitched(Player newMasterClient)
        {
        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            ChatRPC("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            ChatRPC("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
        }

        #region PlayerList
        [PunRPC]
        private void PlayerListRenewal(Player player)
        {
            if (player.IsMasterClient)
                _photonView.RPC("PlayerListRPC", RpcTarget.All, "[방장]" + PhotonNetwork.NickName);
            else
                _photonView.RPC("PlayerListRPC", RpcTarget.All, "      " + PhotonNetwork.NickName);

        }

        [PunRPC]
        private void PlayerListRPC(string nickName)
        {

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

        }
        #endregion
    }
}