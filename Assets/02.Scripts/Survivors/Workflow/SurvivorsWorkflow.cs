using HideAndSkull.Lobby.UI;
using HideAndSkull.Survivors.UI;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace HideAndSkull.Survivors.Workflow
{
    public class SurvivorsWorkflow : MonoBehaviour
    {
        [SerializeField] Button _testButton; // 테스트용 추후삭제
        int _testIndex;

        UI_Survivors uI_Survivors;
        UI_ToastPanel uI_ToastPanel;

        private void Awake()
        {
            uI_Survivors = UI_Manager.instance.Resolve<UI_Survivors>();
            uI_ToastPanel = UI_Manager.instance.Resolve<UI_ToastPanel>();
        }

        private void Start()
        {
            _testButton.onClick.AddListener(() =>
            {
                uI_ToastPanel.ShowToast($"테스트 {_testIndex++}");
            });

            uI_Survivors.SetSurvivorCount(PhotonNetwork.PlayerList.Length);
        }
    }
}
