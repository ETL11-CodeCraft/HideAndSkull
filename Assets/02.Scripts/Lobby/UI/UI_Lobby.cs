using HideAndSkull.Lobby.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HideAndSkull.Lobby.UI
{
    //Todo : "�游���" ��ư Ŭ���� �� �����ϰ� ������ �̵� (Ȯ��)
    //Todo : "�ڵ��Է�" ��ư Ŭ���� �ڵ� �Է� �˾� ���� (Ȯ��)
    //Todo : �ش� �ڵ带 ���� ���� �ִ��� �˻�. ������ �� ������ �� ����, �ƴϸ� ���� ���(popup) �� �κ��
    //Todo : "��������" ��ư Ŭ���� �̹� ������ �� �� �������� ����, ������ �� �ڵ� ����
    //Todo : "������" ��ư Ŭ���� Ȩ���� �̵� �Ǵ� �� ���� (Ȯ��)
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
            });

            //Todo : �ڵ��Է� �˾� ���� - �˾� ���� Ȯ�� / �ڵ��Է����� �� �����ϱ� ��Ȯ��
            _codeInput.onClick.AddListener(() =>
            {
                UI_CodeInput codeInputPopup = UI_Manager.instance.Resolve<UI_CodeInput>();
                codeInputPopup.Show();
            });

            //Todo : ������ �� �ִ� �� �˻� �� ������ ����, ������ ���ο� �� ����
            _quickEnterRoom.onClick.AddListener(() =>
            {
                PhotonNetwork.JoinRandomRoom();
            });

            // Ȩ���� ���ư���
            _backHome.onClick.AddListener(OnLeftLobby);
        }

        public override void Show()
        {
            base.Show();
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + "���� �κ� �����Ͽ����ϴ�.");
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

            PhotonNetwork.CreateRoom(randomRoomCode, roomOptions);
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
            Debug.Log("OnCreatedRoom");
        }

        public void OnCreateRoomFailed(short returnCode, string message)
        {
            // �ߺ��� �� �̸� �߻�: ���ο� ���� �ڵ� ���� �� ��õ�
            if (returnCode == ErrorCode.GameIdAlreadyExists)
            {
                Debug.Log("�� �ڵ尡 �ߺ��˴ϴ�. ���ο� �ڵ� ���� ��...");
                CreateRoomWithRandomCode();
            }
            else
            {
                Debug.LogError("�� ������ �����Ͽ����ϴ� : " + message);
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
            UI_Manager.instance.Resolve<UI_Room>()
                               .Show();
        }

        public void OnJoinRandomFailed(short returnCode, string message)
        {
            CreateRoomWithRandomCode();
        }

        public void OnJoinRoomFailed(short returnCode, string message)
        {
            UI_ConfirmWindow confirmWindow = UI_Manager.instance.Resolve<UI_ConfirmWindow>();

            confirmWindow.Show("�ùٸ� �ڵ尡 �ƴմϴ�.");
            return;
        }

        public void OnLeftLobby()
        {
            UI_Manager.instance.Resolve<UI_Home>()
                               .Show();

            Hide();
        }

        public void OnLeftRoom()
        {
            //Tode : Hide Room UI
            Show();
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