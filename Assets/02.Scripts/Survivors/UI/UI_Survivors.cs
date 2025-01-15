using HideAndSkull.Lobby.UI;
using HideAndSkull.Lobby.Utilities;
using TMPro;

namespace HideAndSkull.Survivors.UI
{
    public class UI_Survivors : UI_Screen
    {
        public int survivorCount
        {
            get { return _survivorCountValue; }
            set
            {
                _survivorCountValue = value;
                _survivorCount.text = $"현재 생존한 플레이어 : <color=\"red\">{value}</color>명";
            }
        }

        int _survivorCountValue;
        [Resolve] TMP_Text _survivorCount;
    }
}

