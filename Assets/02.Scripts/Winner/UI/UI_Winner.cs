using HideAndSkull.Lobby.UI;
using HideAndSkull.Lobby.Utilities;
using System.Collections;
using TMPro;
using UnityEngine;

namespace HideAndSkull.Winner.UI
{
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
                _killCount.text = $"óġ : {value}��";
            }
        }

        [Resolve] TMP_Text _playerNickname;
        [Resolve] TMP_Text _killCount;
        [Resolve] TMP_Text _infoMessage;

        private int _killCountValue;


        public override void Show()
        {
            base.Show();

            StartCoroutine(C_CoundownThenHide(5));
        }

        IEnumerator C_CoundownThenHide(int seconds)
        {
            while (seconds > 0)
            {
                _infoMessage.text = $"{seconds--}�� �� ������ ���ư��ϴ�.";
                yield return new WaitForSeconds(1);
            }

            Hide();
        }
    }
}

