using HideAndSkull.Lobby.UI;
using HideAndSkull.Settings.UI;
using UnityEngine;
using UnityEngine.UI;

namespace HideAndSkull.Lobby.Workflow
{
    public class SettingsWorkflow : MonoBehaviour
    {
        [SerializeField] Button _settings;
        [SerializeField] RectTransform _default;
        [SerializeField] RectTransform _reversed;

        UI_Settings uiSettings;

        private void Start()
        {
            uiSettings = UI_Manager.instance.Resolve<UI_Settings>();
            uiSettings.GetComponent<Canvas>().enabled = false;

            _settings.onClick.AddListener(() =>
            {
                uiSettings.Show();
            });

            uiSettings.onHide.AddListener(SwitchButtonsReplacement);

            SwitchButtonsReplacement();
        }

        private void SwitchButtonsReplacement()
        {
            if (PlayerPrefs.GetInt(SettingsParameter.IS_REVERSED_BUTTON) == 0)
            {
                _default.gameObject.SetActive(true);
                _reversed.gameObject.SetActive(false);
            }
            else
            {
                _default.gameObject.SetActive(false);
                _reversed.gameObject.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            uiSettings.onHide.RemoveListener(SwitchButtonsReplacement);
        }
    }
}