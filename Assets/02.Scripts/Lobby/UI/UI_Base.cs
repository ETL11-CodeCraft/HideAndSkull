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


        protected override void Awake()  //UI overide 해야 할 수도 있으므로
        {
            base.Awake();

            _canvas = GetComponent<Canvas>();
            _graphicRaycaster = GetComponent<GraphicRaycaster>();
            _eventSystem = EventSystem.current;
            _pointerEventData = new PointerEventData(_eventSystem);
            _raycastResultBuffer = new List<RaycastResult>(1);  //raycastresult의 맨 위의 값만 가져오도록 크기를 1로 지정
            playerInputActions = new PlayerInputActions();
            manager = UI_Manager.instance;
            manager.Register(this);
        }

        protected virtual void Start() { }

        public virtual void Show()
        {
            if (_canvas != null)
            {
                _canvas.enabled = true;
                onShow?.Invoke();
            }
            else
            {
                Debug.Log("Canvas가 없습니다.");
            }
        }

        public virtual void Hide()
        {
            _canvas.enabled = false;
            onHide?.Invoke();
        }

        /// <summary>
        /// 현재 Canvas에 특정 컴포넌트가 존재하는지 탐색하는 함수
        /// </summary>
        /// <typeparam name="T">탐색하고 싶은 컴포넌트 타입</typeparam>
        /// <param name="pointerPosition">탐색하고 싶은 위치</param>
        /// <param name="result">탐색 결과</param>
        /// <returns>탐색 성공 여부</returns>
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
