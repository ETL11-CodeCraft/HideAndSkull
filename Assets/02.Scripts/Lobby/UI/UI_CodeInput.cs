using HideAndSkull.Lobby.Utilities;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

namespace HideAndSkull.Lobby.UI
{
    public class UI_CodeInput : UI_Popup
    {
        [Resolve] TMP_InputField _code;
        [Resolve] Button _codeEnter;
        [Resolve] Button _codeExit;

        protected override void Start()
        {
            base.Start();

            _codeEnter.onClick.AddListener(() => 
            {
                PhotonNetwork.JoinRoom(_code.text);
                Hide();
            });

            _codeExit.onClick.AddListener(Hide);
        }

        public override void Show()
        {
            base.Show();

            _code.text = string.Empty;
        }
    }
}