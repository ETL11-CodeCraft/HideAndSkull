using HideAndSkull.Character;
using HideAndSkull.Lobby.UI;
using HideAndSkull.Settings.Sound;
using HideAndSkull.Survivors.UI;
using HideAndSkull.Winner.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using UnityEngine;
using PlayMode = HideAndSkull.Character.PlayMode;

namespace HideAndSkull.Lobby.Workflow
{
    public class GamePlayWorkflow : MonoBehaviour, IInRoomCallbacks
    {
        public int SurvivePlayerCount 
        {
            get => _survivePlayerCount;
            set
            {
                bool isChanged = _survivePlayerCount != value ? true : false;

                _survivePlayerCount = value;

                if (isChanged && PhotonNetwork.IsMasterClient)
                    _uISurvivors.SetSurvivorCount(_survivePlayerCount);

                if (_survivePlayerCount == 1)
                    ShowWinner();
            }
        }
        private int _survivePlayerCount;


        private const int MAX_CHARACTER_COUNT = 20;

        private List<Player> _playerList;
        private UI_ToastPanel _uIToastPanel;
        private UI_Survivors _uISurvivors;
        private List<GameObject> _characters = new List<GameObject>(MAX_CHARACTER_COUNT);
        private List<Vector3> _spawnPoints;
        private HashSet<Vector3> _usedPositions;
        private bool _isCharacterSpawned = false;
        private bool _isSpawnPointsCached = false;


        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }
        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void Start()
        {
            SoundManager.instance.PlayBGM("GamePlay");

            _uIToastPanel = UI_Manager.instance.Resolve<UI_ToastPanel>();
            _uISurvivors = UI_Manager.instance.Resolve<UI_Survivors>();

            _playerList = PhotonNetwork.PlayerList.ToList();
            SurvivePlayerCount = _playerList.Count;

            if (PhotonNetwork.IsMasterClient)
            {
                for (int i = 0; i < _playerList.Count; i++)
                {
                    GameObject characterObject = PhotonNetwork.Instantiate("Character/Skull", Vector3.forward * i, Quaternion.identity);
                    characterObject.GetComponent<Rigidbody>().isKinematic = true;

                    Skull skull = characterObject.GetComponent<Skull>();

                    _characters.Add(characterObject);
                }

                for (int i = _playerList.Count; i < MAX_CHARACTER_COUNT; i++)
                {
                    GameObject characterObject = PhotonNetwork.Instantiate("Character/Skull", Vector3.forward * i, Quaternion.identity);
                    Skull AISkull = characterObject.GetComponent<Skull>();
                    characterObject.GetComponent<Rigidbody>().isKinematic = true;

                    //AISkull.InitAI();

                    _characters.Add(characterObject);
                }
                if(!_isSpawnPointsCached)
                {
                    _isCharacterSpawned = true;
                }
                else
                {
                    SetCharacterPosition();
                }
            }
        }

        public void CachedCharacterPosition(List<Vector3> spawnPoints, HashSet<Vector3> usedPositions)
        {
            int n = spawnPoints.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                Vector3 value = spawnPoints[k];
                spawnPoints[k] = spawnPoints[n];
                spawnPoints[n] = value;
            }

            _spawnPoints = spawnPoints;
            _usedPositions = usedPositions;

            if (!_isCharacterSpawned)
            {
                _isSpawnPointsCached = true;
            }
            else
            {
                SetCharacterPosition();
            }
        }

        private void SetCharacterPosition()
        {
            int cnt = 0;
            foreach (Vector3 spawnPoint in _spawnPoints)
            {
                if (RandomMapGenerator.isPositionTooClose(spawnPoint, _usedPositions, 1.5f))
                {
                    continue;
                }

                _characters[cnt].transform.position = spawnPoint + Vector3.up;
                _usedPositions.Add(spawnPoint);

                PhotonView photonView = _characters[cnt].GetComponent<PhotonView>();
                
                if(cnt >= 0 && cnt < _playerList.Count)
                {
                    photonView.TransferOwnership(_playerList[cnt].ActorNumber);
                    
                    PhotonNetwork.RaiseEvent(Lobby.Network.PhotonEventCode.SYNC_PLAYMODE,
                        new object[] { PlayMode.Player, photonView.ViewID },
                        new RaiseEventOptions { Receivers = ReceiverGroup.All },
                        SendOptions.SendReliable);
                }
                else
                {
                    PhotonNetwork.RaiseEvent(Lobby.Network.PhotonEventCode.SYNC_PLAYMODE,
                        new object[] { PlayMode.AI, photonView.ViewID },
                        new RaiseEventOptions { Receivers = ReceiverGroup.All },
                        SendOptions.SendReliable);
                }

                _characters[cnt].GetComponent<Rigidbody>().isKinematic = false;

                if (cnt++ == MAX_CHARACTER_COUNT - 1)
                    break;
            }

            if (cnt != MAX_CHARACTER_COUNT)
                throw new System.Exception("스폰할 공간이 없음");
        }

        private void ShowWinner()
        {
            Player winner = null;
            foreach(Player player in PhotonNetwork.PlayerList)
            {
                if ((bool)player.CustomProperties["IsDead"] == false)
                {
                    winner = player;
                    break;
                }
            }

            if (winner == null) return;

            SoundManager.instance.PlayBGM("Result");

            UI_Manager.instance.Resolve<UI_Winner>().SetWinnerInfo(winner.NickName, (int)winner.CustomProperties["KillCount"]);
        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (!(bool)otherPlayer.CustomProperties["IsDead"])
            {
                SurvivePlayerCount--;
            }
            
            if (PhotonNetwork.IsMasterClient)
            {
                _uIToastPanel.ShowToast($"{otherPlayer.NickName}님이 접속을 종료하였습니다.");
            }

            _playerList.Remove(otherPlayer);
        }

        public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {

        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            if ((bool)changedProps["IsDead"])
            {
                SurvivePlayerCount--;
                if(targetPlayer == PhotonNetwork.LocalPlayer)
                {
                    UI_Manager.instance.Resolve<UI_Dead>().Show();
                }
            }
        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
            
        }
    }
}

