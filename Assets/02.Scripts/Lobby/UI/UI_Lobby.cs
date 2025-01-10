using HideAndSkull.Lobby.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.UI;

namespace HideAndSkull.Lobby.UI
{
    //Todo : "방만들기" 버튼 클릭시 방 생성하고 방으로 이동. (룸코드 생성)
    //Todo : "코드입력" 버튼 클릭시 코드 입력 팝업 띄우기
    //Todo : "빠른입장" 버튼 클릭시 이미 생성된 방 중 랜덤으로 입장. 없으면 방 자동 생성
    //Todo : "나가기" 버튼 클릭시 홈으로 이동 또는 앱 종료
    public class UI_Lobby : UI_Screen, ILobbyCallbacks, IMatchmakingCallbacks
    {
        [Resolve] Button _createRoom;
        [Resolve] Button _codeInput;
        [Resolve] Button _quickEnterRoom;
        [Resolve] Button _backHome;

        const int ROOM_PLAYER_COUNT = 8;    //입장할 수 있는 플레이어의 최대 인원수는 8명. 모든 방은 8명방으로 생성


        protected override void Start()
        {
            base.Start();

            _createRoom.onClick.AddListener(() => 
            {
                //Todo : Random한 코드 생성. 기존의 방들과 코드가 겹치면 안되며, 코드로 방에 접근할 수 있어야 함.
                string code = "";   
                RoomOptions roomOptions = new RoomOptions();
                roomOptions.MaxPlayers = ROOM_PLAYER_COUNT;
                PhotonNetwork.CreateRoom(code, roomOptions);
                Hide();
            });
            _codeInput.onClick.AddListener(() => { });
            _quickEnterRoom.onClick.AddListener(() => { });
            _backHome.onClick.AddListener(() => { });
        }

        public void OnCreatedRoom()
        {
        }

        public void OnCreateRoomFailed(short returnCode, string message)
        {
        }

        public void OnFriendListUpdate(List<FriendInfo> friendList)
        {
        }

        public void OnJoinedLobby()
        {
        }

        public void OnJoinedRoom()
        {
        }

        public void OnJoinRandomFailed(short returnCode, string message)
        {
        }

        public void OnJoinRoomFailed(short returnCode, string message)
        {
        }

        public void OnLeftLobby()
        {
        }

        public void OnLeftRoom()
        {
        }

        public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
        {
        }

        public void OnRoomListUpdate(List<RoomInfo> roomList)
        {
        }
    }
}