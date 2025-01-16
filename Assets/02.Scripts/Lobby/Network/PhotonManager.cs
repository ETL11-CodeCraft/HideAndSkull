using HideAndSkull.Lobby.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace HideAndSkull.Lobby.Network
{
    public class PhotonManager : MonoBehaviourPunCallbacks
    {
        public static PhotonManager instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new GameObject(nameof(PhotonManager)).AddComponent<PhotonManager>();
                }

                return PhotonManager.instance;
            }
        }

        static PhotonManager s_instance;

        private void Awake()
        {
            if (s_instance)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                s_instance = this;
            }

            ConnectToPhotonServer();
         
            DontDestroyOnLoad(s_instance);
        }

        private void ConnectToPhotonServer()
        { 
            if (PhotonNetwork.IsConnected == false)
            {
#if UNITY_EDITOR
                PhotonNetwork.LogLevel = PunLogLevel.Full;
                Application.runInBackground = true;
#endif

                PhotonNetwork.AuthValues = new Photon.Realtime.AuthenticationValues(Random.Range(0, 999999999).ToString());
                PhotonNetwork.NickName = Random.Range(0, 999999999).ToString();

                bool isConnected = PhotonNetwork.ConnectUsingSettings();
                Debug.Assert(isConnected, $"[{nameof(PhotonManager)}] Failed to connect to photon pun server");
            }
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();

            PhotonNetwork.AutomaticallySyncScene = true;    //현재 속해있는 방의 방장이 씬을 전환하면 따라서 전환하는 옵션
            //PhotonNetwork.NickName = "";  //닉네임 설정 가능
            Debug.Log($"[{nameof(PhotonManager)}] Connected to master server");
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();

            Debug.Log($"[{nameof(PhotonManager)}] Joined lobby");
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            base.OnDisconnected(cause);

            Debug.LogWarning($"Disconnected from Photon server. Cause: {cause}");

            UI_ConfirmWindow confirmWindow = UI_Manager.instance.Resolve<UI_ConfirmWindow>();

            confirmWindow.Show("서버 연결에 실패하였습니다. 재시도하시겠습니까?", ConnectToPhotonServer);
            return;
        }
    }
}