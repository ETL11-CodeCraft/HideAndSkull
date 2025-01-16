using HideAndSkull.Character;
using Photon.Pun;
using Photon.Realtime;
using System;
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
                bool isChanged = false;
                if(_survivePlayerCount != value)
                    isChanged = true;

                _survivePlayerCount = value;

                if (isChanged)
                    OnChangedSurvivePlayerCount?.Invoke();

                if(_survivePlayerCount == 1)
                {
                    ShowWinner();
                }
            }
        }
        private int _survivePlayerCount;
        public Action OnChangedSurvivePlayerCount;


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

