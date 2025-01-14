using HideAndSkull.Character;
using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace HideAndSkull.Lobby.Workflow
{
    public class GamePlayWorkflow : MonoBehaviour
    {
        [SerializeField] Transform[] _spawnPoints;
        static int spawnedCnt = 0;

        private void Start()
        {
            StartCoroutine(C_Workflow());
        }

        IEnumerator C_Workflow()
        {
            SpawnCharacter(_spawnPoints[spawnedCnt++].position);
            if(PhotonNetwork.IsMasterClient)
            {
                for(int i= PhotonNetwork.PlayerList.Length; i < 20; i++)
                {
                    SpawnAI(_spawnPoints[spawnedCnt++].position);
                }
            }
            yield return null;
        }

        private void SpawnCharacter(Vector3 transform)
        {
            Skull playerSkull = PhotonNetwork.Instantiate("Character/Skull", transform, Quaternion.identity).GetComponent<Skull>();
            playerSkull.PlayMode = Character.PlayMode.Player;
            playerSkull.StartPlayerAct();
        }

        private void SpawnAI(Vector3 transform)
        {
            Skull AISkull = PhotonNetwork.Instantiate("Character/Skull", transform, Quaternion.identity).GetComponent<Skull>();
            AISkull.PlayMode = Character.PlayMode.AI;
        }
    }
}

