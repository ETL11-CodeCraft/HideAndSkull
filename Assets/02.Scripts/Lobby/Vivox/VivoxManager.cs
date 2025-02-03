using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Vivox;
using Unity.Services.Authentication;
using Photon.Pun;
using HideAndSkull.Lobby.UI;
using UnityEngine.Android;
using UnityEngine.SceneManagement;
using HideAndSkull.Lobby.Network;
using System.Threading.Tasks;

namespace HideAndSkull.Lobby.Vivox
{
    public class VivoxManager : MonoBehaviour
    {
        private string _roomName;
        public static VivoxManager instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new GameObject(nameof(VivoxManager)).AddComponent<VivoxManager>();
                }

                return s_instance;
            }
        }

        static VivoxManager s_instance;

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

#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }
#endif
            InitializeVivoxAsync();

            DontDestroyOnLoad(gameObject);
        }

        //인증
        public async void InitializeVivoxAsync()
        {
            await UnityServices.InitializeAsync();
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("UnityServices 인증 성공");
            }
            catch
            {
                Debug.Log("UnityServices 인증 실패");
            }

            await VivoxService.Instance.InitializeAsync();

            Debug.Log("Initialize Vivox");
        }

        //vivox 로그인
        public async Task LoginToVivoxAsync()
        {
            if (VivoxService.Instance.IsLoggedIn) return;

            LoginOptions options = new LoginOptions();
            options.DisplayName = PhotonNetwork.LocalPlayer.NickName;
            await VivoxService.Instance.LoginAsync(options);

            Debug.Log("Login Vivox");
        }

        //보이스 채널 참가
        public async Task JoinVoiceChannelAsync()
        {
            _roomName = PhotonNetwork.CurrentRoom.Name;
            await VivoxService.Instance.JoinGroupChannelAsync(_roomName, ChatCapability.AudioOnly);

            Debug.Log("JoinChannel Vivox");
        }

        //보이스 채널 나가기
        public async void LeaveVoiceChannelAsync()
        {
            await VivoxService.Instance.LeaveChannelAsync(_roomName);

            Debug.Log("LeaveChannel Vivox");
        }

        //로그아웃
        public async void LogoutOfVivoxAsync()
        {
            if (!VivoxService.Instance.IsLoggedIn) return;

            await VivoxService.Instance.LogoutAsync();

            Debug.Log("Logout Vivox");
        }

        //본인 음소거 사용 및 해제
        public void Mute()
        {
            if (VivoxService.Instance.IsInputDeviceMuted)
                VivoxService.Instance.UnmuteInputDevice();
            else
                VivoxService.Instance.MuteInputDevice();

            Debug.Log("Mute Toggle");
        }
    }
}
