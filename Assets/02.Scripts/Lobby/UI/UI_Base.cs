using HideAndSkull.Lobby.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace HideAndSkull.Lobby.UI
{
    [RequireComponent(typeof(Canvas))]
    public class UI_Base : ComponentResolvingBehavior
    {
        public int sortingOrder
        {
            get => _canvas.sortingOrder;
            set => _canvas.sortingOrder = value;
        }

        public bool InputActionsEnabled
        {
            get => playerInputActions.asset.enabled;
            set
            {
                if (value)
                    playerInputActions.Enable();
                else
                    playerInputActions.Disable();
            }
        }

        protected UI_Manager manager;
        protected PlayerInputActions playerInputActions;
        Canvas _canvas;
        GraphicRaycaster _graphicRaycaster;
        EventSystem _eventSystem;
        PointerEventData _pointerEventData;
        List<RaycastResult> _raycastResultBuffer;

        public event Action onShow;
        public event Action onHide;
        public InputAction Drag;


        protected override void Awake()  //UI overide �ؾ� �� ���� �����Ƿ�
        {
            base.Awake();

            _canvas = GetComponent<Canvas>();
            _graphicRaycaster = GetComponent<GraphicRaycaster>();
            _eventSystem = EventSystem.current;
            _pointerEventData = new PointerEventData(_eventSystem);
            _raycastResultBuffer = new List<RaycastResult>(1);  //raycastresult�� �� ���� ���� ���������� ũ�⸦ 1�� ����
            playerInputActions = new PlayerInputActions();
            manager = UI_Manager.instance;
            manager.Register(this);
        }

        protected virtual void Start() { }

        public virtual void Show()
        {
            _canvas.enabled = true;
            onShow?.Invoke();
        }

        public virtual void Hide()
        {
            _canvas.enabled = false;
            onHide?.Invoke();
        }

        /// <summary>
        /// ���� Canvas�� Ư�� ������Ʈ�� �����ϴ��� Ž���ϴ� �Լ�
        /// </summary>
        /// <typeparam name="T">Ž���ϰ� ���� ������Ʈ Ÿ��</typeparam>
        /// <param name="pointerPosition">Ž���ϰ� ���� ��ġ</param>
        /// <param name="result">Ž�� ���</param>
        /// <returns>Ž�� ���� ����</returns>
        public bool TryGraphicRaycast<T>(Vector2 pointerPosition, out T result)
            where T : Component
        {
            _pointerEventData.position = pointerPosition;
            _raycastResultBuffer.Clear();
            _graphicRaycaster.Raycast(_pointerEventData, _raycastResultBuffer);

            if (_raycastResultBuffer.Count > 0)
            {
                if (_raycastResultBuffer[0].gameObject.TryGetComponent(out result))
                    return true;
            }

            result = default;
            return false;
        }
    }
}
