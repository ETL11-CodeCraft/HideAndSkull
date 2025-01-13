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

            //�ϴ� �� Canvas���� ���𰡸� Ŭ���ߴ��� Ȯ��
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

                    //������ �ٸ� �˾��� Ŭ��. ���õ� �˾��� �ֻ������ ����
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