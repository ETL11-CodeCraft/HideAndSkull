using HideAndSkull.Lobby.Singleton;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HideAndSkull.Lobby.UI
{
    public class UI_Manager : Singleton<UI_Manager>
    {
        public UI_Manager()
        {
            _uis = new Dictionary<Type, UI_Base>(EXPECTED_MAX_UI_COUNT_IN_SCENE);   //Reserve 수 : 한 장면에서 사용할 수 있는 최대 UI 개수
            _popupStack = new List<UI_Popup>(EXPECTED_MAX_POPUP_COUNT_IN_SCENE);  //Reserve 수 : 한 장면에서 동시에 띄울 수 있는 최대 Popup 개수
        }


        public IEnumerable<UI_Popup> popups => _popupStack;

        const int EXPECTED_MAX_UI_COUNT_IN_SCENE = 30;
        const int EXPECTED_MAX_POPUP_COUNT_IN_SCENE = 10;
        Dictionary<Type, UI_Base> _uis; //현재 장면에서 열릴 수 있는 UI 목록
        UI_Screen _screen;  //현재 장면의 스크린을 차지하는 UI
        List<UI_Popup> _popupStack;    //현재 활성화 되어있는 팝업들을 순차적으로 관리하는 스택

        public void Register(UI_Base ui)
        {
            if (_uis.ContainsKey(ui.GetType()))
            {
                Debug.LogWarning($"UI {ui.GetType()} is already registered. Replacing the old instance.");
                _uis[ui.GetType()] = ui; // 기존 UI를 새 UI로 교체

                if (ui is UI_Popup)
                {
                    ui.onShow += () => Push((UI_Popup)ui);
                    ui.onHide += () => Pop((UI_Popup)ui);
                }
            }
            else if (_uis.TryAdd(ui.GetType(), ui))
            {
                Debug.Log($"Registered UI {ui.GetType()}");

                if (ui is UI_Popup)
                {
                    ui.onShow += () => Push((UI_Popup)ui);
                    ui.onHide += () => Pop((UI_Popup)ui);
                }
            }
            else
            {
                throw new Exception($"Failed to register ui {ui.GetType()}. already exist");
            }
        }

        public T Resolve<T>()
            where T : UI_Base
        {
            if (_uis.TryGetValue(typeof(T), out UI_Base result))
            {
                return (T)result;
            }
            else
            {
                string path = $"UI/Canvas - {typeof(T).Name.Substring(3)}";
                UI_Base prefab = Resources.Load<UI_Base>(path);

                if (prefab == null)
                    throw new Exception($"Failed to resolve ui {typeof(T)}. Not exist");

                return (T)GameObject.Instantiate(prefab);
            }
        }

        public void SetScreen(UI_Screen screen)
        {
            //이미 활성화된 스크린 UI가 있으면 끔
            if (_screen != null)
            {
                _screen.InputActionsEnabled = false;
                _screen.Hide();
            }

            _screen = screen;
            _screen.sortingOrder = 0;
            _screen.InputActionsEnabled = true;
        }

        public void Push(UI_Popup popup)
        {
            int popupIndex = _popupStack.FindLastIndex(ui => ui == popup);

            //이미 이 팝업이 활성화되어있다면, 제거하고 가장 뒤로 보내야함
            if (popupIndex > 0)
            {
                _popupStack.RemoveAt(popupIndex);
            }

            int sortingOrder = 1;

            if (_popupStack.Count > 0)
            {
                UI_Popup prevPopup = _popupStack[^1];
                prevPopup.InputActionsEnabled = false;
                sortingOrder = prevPopup.sortingOrder + 1;
            }

            popup.sortingOrder = sortingOrder;
            popup.InputActionsEnabled = true;
            _popupStack.Add(popup);
            Debug.Log($"Pushed {popup.name}");
        }

        public void Pop(UI_Popup popup)
        {
            UI_Popup latest = _popupStack[^1];

            int popupIndex = _popupStack.FindLastIndex(ui => ui == popup);

            if (popupIndex < 0)
                throw new Exception($"Failed to remove popup. {popup.name}");

            //빼려는게 마지막이었으면 이전 꺼를 활성화
            if (popupIndex == _popupStack.Count - 1)
            {
                _popupStack[popupIndex].InputActionsEnabled = false;

                //이전 팝업이 존재한다면
                if (popupIndex > 0)
                    _popupStack[popupIndex - 1].InputActionsEnabled = true;
            }

            Debug.Log($"Popped {popup.name}");
        }

        public void Unresister<T>(T uiType)
            where T : UI_Base
        {
            if (_uis.ContainsValue(uiType))
            {
                _uis.Remove(typeof(T));
                return;
            }
        }
    }
}