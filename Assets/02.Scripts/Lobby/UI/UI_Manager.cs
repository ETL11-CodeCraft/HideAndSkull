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
            _uis = new Dictionary<Type, UI_Base>(EXPECTED_MAX_UI_COUNT_IN_SCENE);   //Reserve �� : �� ��鿡�� ����� �� �ִ� �ִ� UI ����
            _popupStack = new List<UI_Popup>(EXPECTED_MAX_POPUP_COUNT_IN_SCENE);  //Reserve �� : �� ��鿡�� ���ÿ� ��� �� �ִ� �ִ� Popup ����
        }


        public IEnumerable<UI_Popup> popups => _popupStack;

        const int EXPECTED_MAX_UI_COUNT_IN_SCENE = 30;
        const int EXPECTED_MAX_POPUP_COUNT_IN_SCENE = 10;
        Dictionary<Type, UI_Base> _uis; //���� ��鿡�� ���� �� �ִ� UI ���
        UI_Screen _screen;  //���� ����� ��ũ���� �����ϴ� UI
        List<UI_Popup> _popupStack;    //���� Ȱ��ȭ �Ǿ��ִ� �˾����� ���������� �����ϴ� ����

        public void Register(UI_Base ui)
        {
            if (_uis.TryAdd(ui.GetType(), ui))
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
                return (T)result;
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
            //�̹� Ȱ��ȭ�� ��ũ�� UI�� ������ ��
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

            //�̹� �� �˾��� Ȱ��ȭ�Ǿ��ִٸ�, �����ϰ� ���� �ڷ� ��������
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

            //�����°� �������̾����� ���� ���� Ȱ��ȭ
            if (popupIndex == _popupStack.Count - 1)
            {
                _popupStack[popupIndex].InputActionsEnabled = false;

                //���� �˾��� �����Ѵٸ�
                if (popupIndex > 0)
                    _popupStack[popupIndex - 1].InputActionsEnabled = true;
            }

            Debug.Log($"Popped {popup.name}");
        }
    }
}