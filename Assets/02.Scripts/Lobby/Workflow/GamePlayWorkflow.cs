using HideAndSkull.Character;
using System.Collections;
using UnityEngine;

namespace HideAndSkull.Lobby.Workflow
{
    public class GamePlayWorkflow : MonoBehaviour
    {
        private Skull _skull;

        private void Awake()
        {
            if (!_skull)
            {
                _skull = Resources.Load<Skull>("Character/Skull");
            }
        }

        private void Start()
        {
            StartCoroutine(C_Workflow());
        }

        IEnumerator C_Workflow()
        {
            SpawnCharacter(new Vector3(0, 0, -5));
            SpawnAI(new Vector3(0, 0, 0));
            yield return null;
        }

        private void SpawnCharacter(Vector3 transform)
        {
            if (!_skull)
                throw new System.Exception($"[{nameof(GamePlayWorkflow)}] Can't Find Skull Prefab");

            Skull playerSkull = Instantiate(_skull, transform, Quaternion.identity);
            playerSkull.PlayMode = Character.PlayMode.Player;
            playerSkull.StartPlayerAct();
        }

        private void SpawnAI(Vector3 transform)
        {
            if (!_skull)
                throw new System.Exception($"[{nameof(GamePlayWorkflow)}] Can't Find Skull Prefab");

            Skull AISkull = Instantiate(_skull, transform, Quaternion.identity);
            AISkull.PlayMode = Character.PlayMode.AI;
            //AISkull.StartAIAct();
        }
    }
}
