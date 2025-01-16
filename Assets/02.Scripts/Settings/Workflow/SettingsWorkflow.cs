using HideAndSkull.Lobby.UI;
using HideAndSkull.Settings.UI;
using UnityEngine;
using UnityEngine.UI;

namespace HideAndSkull.Lobby.Workflow
{
    public class SettingsWorkflow : MonoBehaviour
    {
        [SerializeField] Button _settings;

        private void Start()
        {
            _settings.onClick.AddListener(() =>
            {
                UI_Settings uiSettings = UI_Manager.instance.Resolve<UI_Settings>();
                uiSettings.Show();
            });
        }
    }
}