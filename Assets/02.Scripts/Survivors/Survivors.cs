using HideAndSkull.Lobby.UI;
using HideAndSkull.Survivors.UI;
using UnityEngine;
using UnityEngine.UI;

public class Survivors : MonoBehaviour
{
    [SerializeField] Button _button;
    int _index;

    private void Start()
    {
        _button.onClick.AddListener(() =>
        {
            UI_ToastPanel uI_ToastPanel = UI_Manager.instance.Resolve<UI_ToastPanel>();
            uI_ToastPanel.ShowToast($"Å×½ºÆ® {_index++}");
        });
    }
}
