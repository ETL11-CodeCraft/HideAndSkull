using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using HideAndSkull.Lobby.UI;

namespace HideAndSkull.Lobby.UI
{
    public class UI_Popup : UI_Base
    {
        [SerializeField] Image _panel;
        private bool _onDragging;
        private Vector2 _mouseDelta;

        protected override void Start()
        {
            base.Start();

            playerInputActions.UI.Click.performed += CheckOtherUIClicked;
            //playerInputActions.UI.RightClick.performed -= CheckOtherUIClicked;
            playerInputActions.UI.Drag.performed += OnDrag;
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

        //private void Update()
        //{
        //    //if(_onDragging)
        //    {
        //        ((RectTransform)_panel.transform).anchoredPosition += _mouseDelta;
        //    }
        //}

        void OnDrag(InputAction.CallbackContext context)
        {
            if (TryGraphicRaycast(Mouse.current.position.ReadValue(), out Image result))
            {
                if (result == _panel)
                {
                    StartCoroutine(C_OnDrag(context, Mouse.current.position.ReadValue() - (Vector2)_panel.transform.position));

                    //Vector2 mouseDelta = context.ReadValue<Vector2>();
                    //Debug.Log($"mouse delta difference : {mouseDelta} , {Mouse.current.position}");

                    //_onDragging = mouseDelta.magnitude > 0;
                    //_mouseDelta = mouseDelta;
                }
            }
        }

        IEnumerator C_OnDrag(InputAction.CallbackContext context, Vector2 offset)
        {
            while (context.action.ReadValue<Vector2>().magnitude > 0)
            {
                _panel.transform.position = Mouse.current.position.ReadValue() + offset;
                yield return null;
            }
        }
    }
}