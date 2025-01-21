using HideAndSkull.Character;
using HideAndSkull.Lobby.UI;
using HideAndSkull.Survivors.UI;
using HideAndSkull.Winner.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
                    uI_Survivors.SetSurvivorCount(_survivePlayerCount);

                if (_survivePlayerCount == 1)
                    ShowWinner();
            }
        }
        private int _survivePlayerCount;


        public const int MAX_CHARACTER_COUNT = 20;

        List<Player> _playerList;
        UI_ToastPanel uI_ToastPanel;
        UI_Survivors uI_Survivors;
        List<GameObject> _characters = new List<GameObject>(MAX_CHARACTER_COUNT);
        List<Vector3> _spawnPoints;
        HashSet<Vector3> _usedPositions;
        bool isCharacterSpawned = false;
        bool isSpawnPointsCached = false;


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
            uI_ToastPanel = UI_Manager.instance.Resolve<UI_ToastPanel>();
            uI_Survivors = UI_Manager.instance.Resolve<UI_Survivors>();

            _playerList = PhotonNetwork.PlayerList.ToList();
            SurvivePlayerCount = _playerList.Count;

            if (PhotonNetwork.IsMasterClient)
            {
                for (int i = 0; i < _playerList.Count; i++)
                {
                    GameObject characterObject = PhotonNetwork.Instantiate("Character/Skull", Vector3.forward * i, Quaternion.identity);

                    Skull skull = characterObject.GetComponent<Skull>();


                    _characters.Add(characterObject);
                }

                for (int i = _playerList.Count; i < MAX_CHARACTER_COUNT; i++)
                {
                    GameObject characterObject = PhotonNetwork.Instantiate("Character/Skull", Vector3.forward * i, Quaternion.identity);
                    Skull AISkull = characterObject.GetComponent<Skull>();
                    AISkull.InitAI();

                    _characters.Add(characterObject);
                }
                if(!isSpawnPointsCached)
                {
                    isCharacterSpawned = true;
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

            if (!isCharacterSpawned)
            {
                isSpawnPointsCached = true;
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

                _characters[cnt].transform.position = spawnPoint;
                _usedPositions.Add(spawnPoint);

                if(cnt >= 0 && cnt < _playerList.Count)
                {
                    PhotonView photonView = _characters[cnt].GetComponent<PhotonView>();
                    photonView.TransferOwnership(_playerList[cnt].ActorNumber);
                }

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

            UI_Manager.instance.Resolve<UI_Winner>().SetWinnerInfo(winner.NickName, (int)winner.CustomProperties["KillCount"]);
        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (!(bool)otherPlayer.CustomProperties["IsDead"])
                {
                    SurvivePlayerCount--;
                }
                uI_ToastPanel.ShowToast($"{otherPlayer.NickName}님이 접속을 종료하였습니다.");
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
            //AI Character 소유권 넘기기
            
        }
    }
}

