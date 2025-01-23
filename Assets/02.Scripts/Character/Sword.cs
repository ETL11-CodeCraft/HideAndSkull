using HideAndSkull.Lobby.UI;
using HideAndSkull.Survivors.UI;
using Photon.Pun;
using UnityEngine;

namespace HideAndSkull.Character
{
    [RequireComponent(typeof(BoxCollider))]
    public class Sword : MonoBehaviour
    {
        public Skull Owner { get; set; }
        private void OnTriggerEnter(Collider other)
        {
            //맞았는지 판단은 맞은 Skull에서 함
            if(other.TryGetComponent(out PhotonView photonView) && 
               photonView.IsMine)
            {
                if (other.TryGetComponent(out Skull skull))
                {
                    if (skull.isDead)
                        return;

                    //모든 플레이어에게 해당 character가 죽었다고 호출함
                    skull.PhotonView.RPC(nameof(skull.Die), RpcTarget.All);

                    if (skull.PlayMode == PlayMode.Player)
                    {
                        skull.PlayerCustomProperty["IsDead"] = true;
                        photonView.Owner.SetCustomProperties(skull.PlayerCustomProperty);
                        UI_ToastPanel uI_ToastPanel = UI_Manager.instance.Resolve<UI_ToastPanel>();
                        uI_ToastPanel.ShowToast($"{photonView.Owner.NickName}님이 사망하였습니다.");
                        int killcount = (int)Owner.PlayerCustomProperty["KillCount"];
                        Owner.PlayerCustomProperty["KillCount"] = killcount + 1;
                        Owner.PhotonView.Owner.SetCustomProperties(Owner.PlayerCustomProperty);
                    }
                }
            }
        }
    }
}
