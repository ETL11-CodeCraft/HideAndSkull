using HideAndSkull.Character;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

namespace HideAndSkull.Lobby.Workflow
{
    public class GamePlayWorkflow : MonoBehaviour
    {
        [SerializeField] Transform[] _spawnPoints;
        Player[] _playerList;


        private void Start()
        {
            if(PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(C_Workflow());
            }
        }

        IEnumerator C_Workflow()
        {
            _playerList = PhotonNetwork.PlayerList;
            for (int i=0; i< _playerList.Length; i++)
            {
                PhotonView photonView = PhotonNetwork.Instantiate("Character/Skull", _spawnPoints[i].position, Quaternion.identity).GetComponent<PhotonView>();
                photonView.TransferOwnership(_playerList[i].ActorNumber);
            }
            for (int i = _playerList.Length; i < 20; i++)
            {
                Skull AISkull = PhotonNetwork.Instantiate("Character/Skull", _spawnPoints[i].position, Quaternion.identity).GetComponent<Skull>();
                AISkull.InitAI();
            }
            yield return null;
        }
    }
}

