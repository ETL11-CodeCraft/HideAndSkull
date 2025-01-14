using ExitGames.Client.Photon;
using HideAndSkull.Lobby.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HideAndSkull.Lobby.UI
{
    //Todo : 룸코드 띄우기 (확인)
    //Todo : 플레이어 목록 갱신하기
    //Todo : 채팅창에 플레이어 입장 / 퇴장 띄우기 (확인)
    //Todo : 채팅창에 입력하기 (확인)
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
        TMP_Text[] _chatArray;   //List -> Array로 변경할까
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

            //방장 한 명만 룸에 있을 때, 게임 시작하기 버튼을
            //interaction = false로 지정하고 color.gray로 설정할까.
            //ConfirmWindow를 사용하여 게임 시작할 수 없음을 표기할까.
            _gameStart.onClick.AddListener(() =>
            {
                if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
                {
                    UI_ConfirmWindow confirmWindow = UI_Manager.instance.Resolve<UI_ConfirmWindow>();

                    confirmWindow.Show("플레이어가 2명 이상일 때,\n플레이 가능합니다.");
                    return;
                }

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
            _photonView.RPC("PlayerListRPC", RpcTarget.All);
            ChattingClear();
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

            _photonView.RPC("PlayerListRPC", RpcTarget.All);
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            _photonView.RPC("ChatRPC", RpcTarget.All, $"<color=yellow>{otherPlayer.NickName}님이 퇴장하셨습니다</color>");

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
        //플레이어 리스트는 방장과 다른 플레이어들을 구별해서 표기하며, 입장 및 퇴장시 재정렬한다.
        [PunRPC]
        private void PlayerListRPC()
        {
            //기존에 있는 플레이어 닉네임이면 삭제.
            //기존에 없는 플레이어 닉네임이면 추가.
            //List 갱신할 때 마스터 클라이언트 바뀌면 [방장] 플레이어도 갱신

            int index = 0;

            ICollection<Player> players = PhotonNetwork.CurrentRoom.Players.Values;

            List<Player> sortedPlayers = players.OrderBy(player => player.ActorNumber).ToList();

            foreach (Player player in sortedPlayers)
            {
                if (player.IsMasterClient)
                    _playerArray[index].text = $"[방장] {player.NickName}";
                else
                    _playerArray[index].text = $"           {player.NickName}";

                index++;
            }

            for (int i = index; i < _playerArray.Length; i++)
            {
                _playerArray[i].text = $"";
            }
        }
        #endregion

        #region Chatting
        private void ChattingClear()
        {
            for (int i = 0; i < _chatArray.Length; i++)
            {
                _chatArray[i].text = "";
            }
        }

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

            if (!isInput) // 꽉차면 한칸씩 위로 올림
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