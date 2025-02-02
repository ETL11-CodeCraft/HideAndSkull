using HideAndSkull.Lobby.UI;
using HideAndSkull.Lobby.Utilities;
using HideAndSkull.Settings.Sound;
using UnityEngine;
using UnityEngine.UI;

namespace HideAndSkull.Settings.UI
{
    public static class SettingsParameter
    {
        public static readonly string IS_REVERSED_BUTTON = "IsReversedButton";
        public static readonly string CAMERA_SENSIBILITY = "CameraSensibility";
    }

    class UI_Settings : UI_Popup
    {
        [SerializeField] Toggle _isReversedKey;
        [SerializeField] Slider _cameraSensibility;
        [Resolve] Button _exitGame;
        [Resolve] Button _close;

        protected override void Start()
        {
            _cameraSensibility.maxValue = 110;
            _cameraSensibility.minValue = 10;
        }

        public override void Show()
        {
            base.Show();

            SoundManager.instance.PlayButtonSound();

            // 조작키 반전 토글
            _isReversedKey.isOn = PlayerPrefs.GetInt(SettingsParameter.IS_REVERSED_BUTTON, 0) == 1;
            _isReversedKey.onValueChanged.AddListener((value) =>
            {
                PlayerPrefs.SetInt(SettingsParameter.IS_REVERSED_BUTTON, value == true ? 1 : 0);
            });

            // 카메라 감도 슬라이더
            _cameraSensibility.value = PlayerPrefs.GetFloat(SettingsParameter.CAMERA_SENSIBILITY, 0.5f);
            _cameraSensibility.onValueChanged.AddListener((value) =>
            {
                PlayerPrefs.SetFloat(SettingsParameter.CAMERA_SENSIBILITY, value);
            });

            // 종료 버튼
            _exitGame.onClick.AddListener(() =>
            {
                SoundManager.instance.PlayButtonSound();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });

            // 닫기 버튼
            _close.onClick.AddListener(() =>
            {
                SoundManager.instance.PlayButtonSound();
                Hide();
            });
        }

        public override void Hide()
        {
            base.Hide();

            _exitGame.onClick.RemoveAllListeners();
            _close.onClick.RemoveAllListeners();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _exitGame.onClick.RemoveAllListeners();
            _close.onClick.RemoveAllListeners();
        }
    }
}