using HideAndSkull.Character;
using HideAndSkull.Lobby.UI;
using HideAndSkull.Survivors.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HideAndSkull.Lobby.Workflow
{
    public class GamePlayWorkflow : MonoBehaviour
    {
        public int SurvivePlayerCount 
        {
            get => _survivePlayerCount;
            set
            {
                bool isChanged = _survivePlayerCount != value ? true : false;

                _survivePlayerCount = value;

                if (isChanged)
                    uI_Survivors.SetSurvivorCount(_survivePlayerCount);

                if (_survivePlayerCount == 1)
                    ShowWinner();
            }
        }
        private int _survivePlayerCount;


        [SerializeField] Transform[] _spawnPoints;
        Player[] _playerList;
        UI_ToastPanel uI_ToastPanel;
        UI_Survivors uI_Survivors;


        private void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                uI_Survivors = UI_Manager.instance.Resolve<UI_Survivors>();
                StartCoroutine(C_Workflow());
            }
        }

        IEnumerator C_Workflow()
        {
            _playerList = PhotonNetwork.PlayerList;
            SurvivePlayerCount = _playerList.Length;
            for (int i=0; i< _playerList.Length; i++)
            {
                GameObject gameObject = PhotonNetwork.Instantiate("Character/Skull", _spawnPoints[i].position, Quaternion.identity);
                
                Skull skull = gameObject.GetComponent<Skull>();

                PhotonView photonView = gameObject.GetComponent<PhotonView>();
                photonView.TransferOwnership(_playerList[i].ActorNumber);
            }
            for (int i = _playerList.Length; i < 20; i++)
            {
                Skull AISkull = PhotonNetwork.Instantiate("Character/Skull", _spawnPoints[i].position, Quaternion.identity).GetComponent<Skull>();
                AISkull.InitAI();
            }
            yield return null;
        }

        private void ShowWinner()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                //SceneManager.LoadScene(0);
            }
        }
    }
}

