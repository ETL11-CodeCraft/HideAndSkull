using System;
using System.Collections;
using System.ComponentModel;
using TMPro;
using UnityEngine;

namespace HideAndSkull.Survivors.UI
{

    public class UI_ToastMessageBox : MonoBehaviour
    {
        [SerializeField] TMP_Text _message;
        CanvasGroup _canvasGroup;
        public float displayDuration = 2f;
        public float fadeDuration = 0.2f;
        Coroutine toastCoroutine;
        public Action OnHide;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
        }

        public void Show(string message)
        {
            _message.text = message;

            if (toastCoroutine != null)
            {
                StopCoroutine(toastCoroutine);
            }

            toastCoroutine = StartCoroutine(ShowToastCoroutine());
        }

        private IEnumerator ShowToastCoroutine()
        {
            // 페이드 인
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                yield return null;
            }

            yield return new WaitForSeconds(displayDuration);

            // 페이드 아웃
            elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                _canvasGroup.alpha = 1 - Mathf.Clamp01(elapsedTime / fadeDuration);
                yield return null;
            }

            _canvasGroup.alpha = 0;
            Destroy(gameObject);
            OnHide?.Invoke();
        }

        public void CancelToast()
        {
            if (toastCoroutine != null)
            {
                StopCoroutine(toastCoroutine);
            }
            Destroy(gameObject);
            OnHide?.Invoke();
        }
    }
}
