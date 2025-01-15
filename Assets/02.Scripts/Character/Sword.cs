using Photon.Pun;
using UnityEngine;

namespace HideAndSkull.Character
{
    [RequireComponent(typeof(BoxCollider))]
    public class Sword : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            //맞았는지 판단은 MasterClient에서만 함
            if(PhotonNetwork.IsMasterClient)
            {
                if (other.TryGetComponent(out Skull skull))
                {
                    //MasterClient에서 맞았다고 판단되면 모든 플레이어에게 해당 character가 죽었다고 호출함
                    skull.PhotonView.RPC(nameof(skull.Die), RpcTarget.AllViaServer);
                }
            }
        }
    }
}