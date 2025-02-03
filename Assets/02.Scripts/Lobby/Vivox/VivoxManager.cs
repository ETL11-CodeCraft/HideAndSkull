using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Vivox;
using Unity.Services.Authentication;
using Photon.Pun;
using HideAndSkull.Lobby.UI;

namespace HideAndSkull.Lobby.Vivox
{
    public class VivoxManager : MonoBehaviour
    {
        public static VivoxManager Instance { get; private set; }

        private string _roomName;


        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            InitializeAsync();
        }

        private void OnDestroy()
        {
            if (VivoxService.Instance.IsLoggedIn)
                LogoutOfVivoxAsync();
        }


        //인증
        public async void InitializeAsync()
        {
            await UnityServices.InitializeAsync();
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("로그인 성공");
            }
            catch
            {
                Debug.Log("로그인 실패");
            }

            await VivoxService.Instance.InitializeAsync();

            if (VivoxService.Instance.IsLoggedIn)
                LogoutOfVivoxAsync();

            Debug.Log("Initialize Vivox");
        }

        //vivox 로그인
        public async void LoginToVivoxAsync()
        {
            if (VivoxService.Instance.IsLoggedIn) return;

            LoginOptions options = new LoginOptions();
            options.DisplayName = PhotonNetwork.LocalPlayer.NickName;
            options.EnableTTS = false;
            await VivoxService.Instance.LoginAsync(options);

            UI_Manager.instance.Resolve<UI_Lobby>()
                .Show();
        }

        //보이스 채널 참가
        public async void JoinVoiceChannelAsync()
        {
            _roomName = PhotonNetwork.CurrentRoom.Name;
            await VivoxService.Instance.JoinGroupChannelAsync(_roomName, ChatCapability.AudioOnly);

            UI_Manager.instance.Resolve<UI_Room>()
                               .Show();
        }

        //보이스 채널 나가기
        public async void LeaveVoiceChannelAsync()
        {
            await VivoxService.Instance.LeaveChannelAsync(_roomName);

            Debug.Log("Vivox Leave Channel");
        }

        //로그아웃
        public async void LogoutOfVivoxAsync()
        {
            if (!VivoxService.Instance.IsLoggedIn) return;

            await VivoxService.Instance.LogoutAsync();

            UI_Manager.instance.Resolve<UI_Home>()
               .Show();
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
