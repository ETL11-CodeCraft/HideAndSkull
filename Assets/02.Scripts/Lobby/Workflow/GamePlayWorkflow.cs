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
                //TODO :: ��� â ����
                //TODO :: ���� �ð��� ������ �� �����ϱ� (�� ������ MasterClient��)
            }
        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            _playerList.Remove(otherPlayer);
            //���� otherPlayer�� ����־��ٸ� SurvivePlayerCount--;
            if (PhotonNetwork.IsMasterClient)
                uI_ToastPanel.ShowToast($"{otherPlayer.NickName}���� ������ �����Ͽ����ϴ�.");
        }

        public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {

        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {

        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
            //AI Character ������ �ѱ��
        }
    }
}

