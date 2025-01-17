﻿using HideAndSkull.Lobby.UI;
using HideAndSkull.Lobby.Utilities;
using UnityEngine.UI;
using UnityEngine;

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

        public override void Show()
        {
            base.Show();

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
            _exitGame.onClick.RemoveAllListeners();
            _exitGame.onClick.AddListener(() =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
            
            // 닫기 버튼
            _close.onClick.RemoveAllListeners();
            _close.onClick.AddListener(() =>
            {
                Hide();
            });
        }
    }
}