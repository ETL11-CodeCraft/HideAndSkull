using HideAndSkull.Lobby.UI;
using HideAndSkull.Lobby.Utilities;
using Photon.Pun;
using TMPro;

namespace HideAndSkull.Survivors.UI
{
    public class UI_Survivors : UI_Screen
    {
        public int survivorCount
        {
            get { return _survivorCountValue; }
            private set
            {
                _survivorCountValue = value;
                _survivorCount.text = $"현재 생존한 플레이어 : <color=\"red\">{value}</color>명";
            }
        }

        int _survivorCountValue;
        [Resolve] TMP_Text _survivorCount;
        PhotonView _photonView;

        protected override void Awake()
        {
            base.Awake();

            _photonView = GetComponent<PhotonView>();
            _photonView.ViewID = 2;
        }

        public void SetSurvivorCount(int count)
        {
            _photonView.RPC(nameof(SetSurvivorCountRPC), RpcTarget.All, count);
        }

        [PunRPC]
        private void SetSurvivorCountRPC(int survivorCount)
        {
            this.survivorCount = survivorCount;
        }
    }
}

