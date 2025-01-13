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
    //Todo : 룸코드 띄우기
    //Todo : 플레이어 목록 갱신하기
    //Todo : 채팅창에 플레이어 입장 / 퇴장 띄우기
    //Todo : 채팅창에 입력하기
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
                _roomCode.text = "룸코드 : " + roomCode;
            }
            else
            {
                _roomCode.text = "룸코드를 찾을 수 없습니다.";
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

        //Todo : Player 목록 갱신시 확인하여 마스터 변경
        public void OnMasterClientSwitched(Player newMasterClient)
        {
            if (newMasterClient.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                TogglePlayerButtons(newMasterClient);
        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            _photonView.RPC("ChatRPC", RpcTarget.All, $"<color=yellow>{newPlayer.NickName}님이 참가하셨습니다</color>");
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            _photonView.RPC("ChatRPC", RpcTarget.All, $"<color=yellow>{otherPlayer.NickName}님이 퇴장하셨습니다</color>");
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
        }

        #region PlayerList  
        //플레이어 리스트는 방장과 다른 플레이어들을 구별해서 표기하며, 입장시 추가하고 퇴장시 제거한다.
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
            //기존에 있는 플레이어 닉네임이면 삭제.
            //기존에 없는 플레이어 닉네임이면 추가.
            //List 갱신할 때 마스터 클라이언트 바뀌면 [방장] 플레이어도 갱신

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

            if (!isInput) // 꽉차면 한칸씩 위로 올림
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