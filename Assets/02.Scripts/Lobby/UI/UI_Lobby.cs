using HideAndSkull.Lobby.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
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
        const int ROOM_CODE_LENGTH = 6;     //�������� �����Ǵ� �ڵ��� ����


        protected override void Start()
        {
            base.Start();

            //Todo : �ִ� �ο� 8��, ������ 6�ڸ��� �ڵ带 ���� �� ����
            _createRoom.onClick.AddListener(() =>
            {
                //Todo : Random�� �ڵ� ����. ������ ���� �ڵ尡 ��ġ�� �ȵǸ�, �ڵ�� �濡 ������ �� �־�� ��.
                CreateRoomWithRandomCode();
                Hide();
            });

            //Todo : �ش� �ڵ带 ���� ���� �ִ��� �˻�. ������ �� ������ �� ����. �ƴϸ� ���� ��� �� �κ��
            _codeInput.onClick.AddListener(() => { });

            //Todo : ������ �� �ִ� �� �˻� �� ������ ����, ������ ���ο� �� ����
            _quickEnterRoom.onClick.AddListener(() => { });

            //Todo : Ȩ���� ���ư���
            _backHome.onClick.AddListener(() => { });
        }

        /// <summary>
        /// ������ ���� �ڵ带 �����Ͽ� ���� �����մϴ�.
        /// </summary>
        private void CreateRoomWithRandomCode()
        {
            string randomRoomCode = GenerateRandomRoomCode(ROOM_CODE_LENGTH);

            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = ROOM_PLAYER_COUNT,
                CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "RoomCode", randomRoomCode } // Ŀ���� ������Ƽ�� ���� �ڵ� �߰�
            },
            };

            PhotonNetwork.CreateRoom(randomRoomCode, roomOptions, TypedLobby.Default);
        }

        /// <summary>
        /// ������ ������ ������ ���� �ڵ带 �����մϴ�.
        /// </summary>
        /// <param name="codeLength">�ڵ� ����</param>
        /// <returns>���� ���ڿ� �ڵ�</returns>
        private string GenerateRandomRoomCode(int codeLength)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] stringChars = new char[codeLength];
            System.Random random = new System.Random();

            for (int i = 0; i < codeLength; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }


        public void OnCreatedRoom()
        {
        }

        public void OnCreateRoomFailed(short returnCode, string message)
        {
            // �ߺ��� �� �̸� �߻�: ���ο� ���� �ڵ� ���� �� ��õ�
            if (returnCode == ErrorCode.GameIdAlreadyExists)
            {
                Debug.Log("Duplicate room code detected. Generating a new code...");
                CreateRoomWithRandomCode();
            }
            else
            {
                Debug.LogError("Room creation failed for another reason: " + message);
            }
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
        { }

        public void OnLeftRoom()
        { }

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