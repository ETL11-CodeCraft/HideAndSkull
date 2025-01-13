using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using HideAndSkull.Lobby.Utilities;

namespace HideAndSkull.Lobby.UI
{
    public class UI_Popup : UI_Base
    {
        [Resolve] Image _panel;
        private bool _onDragging;
        private Vector2 _mouseDelta;

        protected override void Start()
        {
            base.Start();

            playerInputActions.UI.Click.performed += CheckOtherUIClicked;
        }

        void CheckOtherUIClicked(InputAction.CallbackContext context)
        {
            if (context.ReadValueAsButton() == false)
                return;

            Vector2 mousePosition = Mouse.current.position.ReadValue();

            //일단 이 Canvas에서 무언가를 클릭했는지 확인
            if (TryGraphicRaycast(mousePosition, out CanvasRenderer renderer))
            {
                //Nothing todo
            }
            else
            {
                IEnumerable<UI_Popup> popups = manager.popups;

                foreach (UI_Popup popup in popups)
                {
                    if (popup == this)
                        continue;

                    //유저가 다른 팝업을 클릭. 선택된 팝업을 최상단으로 보냄
                    if (popup.TryGraphicRaycast(mousePosition, out renderer))
                    {
                        popup.Show();
                        break;
                    }
                }
            }
        }
    }
}