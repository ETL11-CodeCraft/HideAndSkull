using HideAndSkull.Lobby.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.UI;

namespace HideAndSkull.Lobby.UI
{
    //Todo : "�游���" ��ư Ŭ���� �� �����ϰ� ������ �̵�. (���ڵ� ����)
    //Todo : "�ڵ��Է�" ��ư Ŭ���� �ڵ� �Է� �˾� ����
    //Todo : "��������" ��ư Ŭ���� �̹� ������ �� �� �������� ����. ������ �� �ڵ� ����
    //Todo : "������" ��ư Ŭ���� Ȩ���� �̵� �Ǵ� �� ����
    public class UI_Lobby : UI_Screen, ILobbyCallbacks, IMatchmakingCallbacks
    {
        [Resolve] Button _createRoom;
        [Resolve] Button _codeInput;
        [Resolve] Button _quickEnterRoom;
        [Resolve] Button _backHome;

        const int ROOM_PLAYER_COUNT = 8;    //������ �� �ִ� �÷��̾��� �ִ� �ο����� 8��. ��� ���� 8������� ����


        protected override void Start()
        {
            base.Start();

            _createRoom.onClick.AddListener(() => 
            {
                //Todo : Random�� �ڵ� ����. ������ ���� �ڵ尡 ��ġ�� �ȵǸ�, �ڵ�� �濡 ������ �� �־�� ��.
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