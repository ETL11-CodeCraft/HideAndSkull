using UnityEngine;
using UnityEngine.UI;

namespace HideAndSkull.Settings.Sound
{
    public class SetVolume : MonoBehaviour
    {
        private string _parameterName;
        private Slider _slider;

        private void Awake()
        {
            _parameterName = gameObject.name.Substring("Slider - ".Length);
            _slider = GetComponent<Slider>();
        }

        void Start()
        {
            _slider.value = PlayerPrefs.GetFloat(_parameterName, 0.75f);
            _slider.onValueChanged.AddListener((sliderValue) =>
            {
                SetLevel(sliderValue);
            });
        }

        public void SetLevel(float sliderValue)
        {
            if (_parameterName.Equals(SoundParameter.BGM_VOLUME))
            {
                SoundManager.instance.SetBGMVolume(sliderValue);
                PlayerPrefs.SetFloat(_parameterName, sliderValue);
            }
            else if (_parameterName.Equals(SoundParameter.SFX_VOLUME))
            {
                SoundManager.instance.SetSFXVolume(sliderValue);
                PlayerPrefs.SetFloat(_parameterName, sliderValue);
            }
        }
    }
}
