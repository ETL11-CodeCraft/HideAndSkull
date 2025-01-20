using HideAndSkull.Lobby.UI;
using HideAndSkull.Lobby.Utilities;
using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;

namespace HideAndSkull.Winner.UI
{
    [RequireComponent(typeof(PhotonView))]
    public class UI_Winner : UI_Base
    {
        public string playerNickname
        {
            get { return _playerNickname.text; }
            set { _playerNickname.text = value; }
        }

        public int killCount
        {
            get { return _killCountValue; }
            set
            {
                _killCountValue = value;
                _killCount.text = $"처치 : {value}명";
            }
        }

        [Resolve] TMP_Text _playerNickname;
        [Resolve] TMP_Text _killCount;
        [Resolve] TMP_Text _infoMessage;
        PhotonView _photonView;

        private int _killCountValue;


        protected override void Awake()
        {
            base.Awake();

            _photonView = GetComponent<PhotonView>();
        }

        public override void Show()
        {
            base.Show();

            StartCoroutine(C_CoundownThenHide(5));
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void SetWinnerInfo(string playerNickname, int killCount)
        {
            this.playerNickname = playerNickname;
            this.killCount = killCount;

            Show();
        }

        IEnumerator C_CoundownThenHide(int seconds)
        {
            while (seconds > 0)
            {
                _infoMessage.text = $"{seconds--}초 후 룸으로 돌아갑니다.";
                yield return new WaitForSeconds(1);
            }

            Hide();

            if (PhotonNetwork.IsMasterClient)
            {

                PhotonNetwork.LoadLevel(0);
            }
        }
    }
}

