﻿using HideAndSkull.Lobby.UI;
using HideAndSkull.Survivors.UI;
using Photon.Pun;
using UnityEngine;

namespace HideAndSkull.Character
{
    [RequireComponent(typeof(BoxCollider))]
    public class Sword : MonoBehaviour
    {
        public Skull SwordOwner { get; set; }
        private void OnTriggerEnter(Collider other)
        {
            //맞았는지 판단은 맞은 Skull에서 함
            if(other.TryGetComponent(out PhotonView photonView) && 
               photonView.IsMine)
            {
                if (other.TryGetComponent(out Skull attackedSkull))
                {
                    if (attackedSkull.isDead)
                        return;

                    //모든 플레이어에게 해당 character가 죽었다고 호출함
                    attackedSkull.PhotonView.RPC(nameof(attackedSkull.Die), RpcTarget.All);

                    if (attackedSkull.PlayMode == PlayMode.Player)
                    {
                        int killcount = (int)SwordOwner.PhotonView.Owner.CustomProperties["KillCount"];
                        SwordOwner.PlayerCustomProperty["KillCount"] = killcount + 1;
                        SwordOwner.PhotonView.Owner.SetCustomProperties(SwordOwner.PlayerCustomProperty);

                        attackedSkull.PlayerCustomProperty["IsDead"] = true;
                        attackedSkull.PhotonView.Owner.SetCustomProperties(attackedSkull.PlayerCustomProperty);
                        UI_ToastPanel uI_ToastPanel = UI_Manager.instance.Resolve<UI_ToastPanel>();
                        uI_ToastPanel.ShowToast($"{photonView.Owner.NickName}님이 사망하였습니다.");
                    }
                }
            }
        }
    }
}
