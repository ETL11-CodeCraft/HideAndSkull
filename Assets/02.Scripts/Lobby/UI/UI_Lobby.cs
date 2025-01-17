using HideAndSkull.Lobby.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HideAndSkull.Lobby.UI
{
    //Todo : "방만들기" 버튼 클릭시 방 생성하고 방으로 이동 (확인)
    //Todo : "코드입력" 버튼 클릭시 코드 입력 팝업 띄우기 (확인)
    //Todo : 해당 코드를 가진 방이 있는지 검사. 입장할 수 있으면 방 입장, 아니면 에러 출력(popup) 후 로비로 (확인)
    //Todo : "빠른입장" 버튼 클릭시 이미 생성된 방 중 랜덤으로 입장, 없으면 방 자동 생성 (확인)
    //Todo : "나가기" 버튼 클릭시 홈으로 이동 또는 앱 종료 (확인)
    //Todo : MasterClient인데 로비에 있을 때, 더 이상 MasterClient가 아니도록 갱신 - 현재는 LeftRoom으로 해결(추후 수정)
    //Todo : 플레이 중인 방에 들어갈 수 없도록 조정
    public class UI_Lobby : UI_Screen, ILobbyCallbacks, IMatchmakingCallbacks
    {
        [Resolve] Button _createRoom;
        [Resolve] Button _codeInput;
        [Resolve] Button _quickEnterRoom;
        [Resolve] Button _backHome;

        const int ROOM_PLAYER_COUNT = 8;    //입장할 수 있는 플레이어의 최대 인원수는 8명. 모든 방은 8명방으로 생성
        const int ROOM_CODE_LENGTH = 6;     //랜덤으로 생성되는 코드의 길이


        protected override void Start()
        {
            base.Start();

            //최대 인원 8명, 랜덤한 6자리의 코드를 가진 방 생성
            _createRoom.onClick.AddListener(() =>
            {
                //Random한 코드 생성. 기존의 방들과 코드가 겹치면 안되며, 코드로 방에 접근할 수 있어야 함.
                CreateRoomWithRandomCode();
            });

            //코드입력 팝업 띄우기 - 팝업 띄우기 확인 / 코드입력으로 룸 입장하기
            _codeInput.onClick.AddListener(() =>
            {
                if (PhotonNetwork.InRoom)
                {
                    PhotonNetwork.LeaveRoom();
                    PhotonNetwork.JoinLobby();
                }

                UI_CodeInput codeInputPopup = UI_Manager.instance.Resolve<UI_CodeInput>();
                codeInputPopup.Show();
            });

            //입장할 수 있는 방 검사 후 있으면 입장, 없으면 새로운 방 생성
            _quickEnterRoom.onClick.AddListener(() =>
            {
                PhotonNetwork.JoinRandomRoom();
            });

            // 홈으로 돌아가기
            _backHome.onClick.AddListener(() =>
            {
                UI_Manager.instance.Resolve<UI_Home>()
                               .Show();
            }
            );
        }

        public override void Show()
        {
            base.Show();

            PhotonNetwork.JoinLobby();

            Debug.Log(PhotonNetwork.LocalPlayer.NickName + "님이 로비에 입장하였습니다.");
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        /// <summary>
        /// 고유한 랜덤 코드를 생성하여 방을 생성합니다.
        /// </summary>
        private void CreateRoomWithRandomCode()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.JoinLobby();
            }

            string randomRoomCode = GenerateRandomRoomCode(ROOM_CODE_LENGTH);

            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = ROOM_PLAYER_COUNT,
                CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "RoomCode", randomRoomCode }, // 커스텀 프로퍼티에 랜덤 코드 추가
            },
            };

            PhotonNetwork.CreateRoom(randomRoomCode, roomOptions);
        }

        /// <summary>
        /// 지정된 길이의 고유한 랜덤 코드를 생성합니다.
        /// </summary>
        /// <param name="codeLength">코드 길이</param>
        /// <returns>랜덤 문자열 코드</returns>
        private string GenerateRandomRoomCode(int codeLength)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] randomChars = new char[codeLength];
            System.Random random = new System.Random();

            for (int i = 0; i < codeLength; i++)
            {
                randomChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(randomChars);
        }


        public void OnCreatedRoom()
        {
            Debug.Log("OnCreatedRoom");
        }

        public void OnCreateRoomFailed(short returnCode, string message)
        {
            // 중복된 방 이름 발생: 새로운 랜덤 코드 생성 및 재시도
            if (returnCode == ErrorCode.GameIdAlreadyExists)
            {
                Debug.Log("방 코드가 중복됩니다. 새로운 코드 생성 중...");
                CreateRoomWithRandomCode();
            }
            else
            {
                Debug.LogError("방 생성이 실패하였습니다 : " + message);
            }
        }

        public void OnFriendListUpdate(List<FriendInfo> friendList)
        {
        }

        public void OnJoinedLobby()
        {
            Debug.Log("로비에 입장하였습니다.");
        }

        public void OnJoinedRoom()
        {
            UI_Manager.instance.Resolve<UI_Room>()
                               .Show();

            Debug.Log("룸에 입장하였습니다.");
        }

        public void OnJoinRandomFailed(short returnCode, string message)
        {
            CreateRoomWithRandomCode();
        }

        public void OnJoinRoomFailed(short returnCode, string message)
        {
            UI_ConfirmWindow confirmWindow = UI_Manager.instance.Resolve<UI_ConfirmWindow>();

            confirmWindow.Show("올바른 코드가 아닙니다.");
            return;
        }

        public void OnLeftLobby()
        {
        }

        public void OnLeftRoom()
        {
            PhotonNetwork.JoinLobby();
        }

        public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
        { }

        public void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            foreach (RoomInfo roomInfo in roomList)
            {
                if (roomInfo.CustomProperties.ContainsKey("RoomCode"))
                {
                    string roomCode = roomInfo.CustomProperties["RoomCode"].ToString();
                    Debug.Log($"Room found: {roomInfo.Name}, Code: {roomCode}");
                }
            }
        }
    }
}