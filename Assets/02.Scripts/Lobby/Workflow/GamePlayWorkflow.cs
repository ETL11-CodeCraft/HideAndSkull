using HideAndSkull.Character;
using HideAndSkull.Lobby.UI;
using HideAndSkull.Survivors.UI;
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


        [SerializeField] Transform[] _spawnPoints;
        List<Player> _playerList;
        UI_ToastPanel uI_ToastPanel;
        UI_Survivors uI_Survivors;


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
                    GameObject gameObject = PhotonNetwork.Instantiate("Character/Skull", _spawnPoints[i].position, Quaternion.identity);

                    Skull skull = gameObject.GetComponent<Skull>();

                    PhotonView photonView = gameObject.GetComponent<PhotonView>();
                    photonView.TransferOwnership(_playerList[i].ActorNumber);

                    _playerList[i].CustomProperties = new ExitGames.Client.Photon.Hashtable
                    {
                        {"IsDead", false},
                    };
                }

                for (int i = _playerList.Count; i < 20; i++)
                {
                    Skull AISkull = PhotonNetwork.Instantiate("Character/Skull", _spawnPoints[i].position, Quaternion.identity).GetComponent<Skull>();
                    AISkull.InitAI();
                }
            }
        }

        private void ShowWinner()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                //TODO :: 결과 창 띄우기
                //TODO :: 일정 시간이 지난후 씬 변경하기 (씬 변경은 MasterClient만)
            }
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

        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
            //AI Character 소유권 넘기기
        }
    }
}

