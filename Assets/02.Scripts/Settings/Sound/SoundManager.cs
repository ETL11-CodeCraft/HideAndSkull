using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HideAndSkull.Settings.Sound
{
    public static class SoundParameter
    {
        public static readonly string BGM_VOLUME = "BGMVolume";
        public static readonly string SFX_VOLUME = "SFXVolume";
    }

    public class SoundManager : MonoBehaviour
    {
        public static SoundManager instance;

        private AudioSource _bgmSource;
        private AudioSource _sfxSource;

        Dictionary<string, AudioClip> _bgmClips = new Dictionary<string, AudioClip>();
        Dictionary<string, AudioClip> _sfxClips = new Dictionary<string, AudioClip>();

        [System.Serializable]
        public struct NamedAudioClip
        {
            public string name;
            public AudioClip clip;
        }

        public NamedAudioClip[] bgmClipList;
        public NamedAudioClip[] sfxClipList;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioClips();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            _bgmSource = GetComponents<AudioSource>()[0];
            _sfxSource = GetComponents<AudioSource>()[1];
            _bgmSource.volume = PlayerPrefs.GetFloat(SoundParameter.BGM_VOLUME, 0.75f);
            _sfxSource.volume = PlayerPrefs.GetFloat(SoundParameter.SFX_VOLUME, 0.75f);

            PlayBGM("Home");

            StartCoroutine(C_TestSFXPlay());
        }

        IEnumerator C_TestSFXPlay()
        {
            while(true)
            {
                yield return new WaitForSeconds(1f);
                PlaySFX("ClickUI");
            }
        }

        void InitializeAudioClips()
        {
            foreach (var bgm in bgmClipList)
            {
                if (!_bgmClips.ContainsKey(bgm.name))
                {
                    _bgmClips.Add(bgm.name, bgm.clip);
                }
            }
            foreach (var sfx in sfxClipList)
            {
                if (!_sfxClips.ContainsKey(sfx.name))
                {
                    _sfxClips.Add(sfx.name, sfx.clip);
                }
            }
        }

        public void PlayBGM(string name)
        {
            if (_bgmClips.ContainsKey(name))
            {
                _bgmSource.clip = _bgmClips[name];
                _bgmSource.Play();
            }
        }

        public void PlaySFX(string name)
        {
            if (_sfxClips.ContainsKey(name))
            {
                _sfxSource.PlayOneShot(_sfxClips[name]);
            }
        }

        public void PlaySFX(string name, Vector3 position)
        {
            if (_sfxClips.ContainsKey(name))
            {
                AudioSource.PlayClipAtPoint(_sfxClips[name], position);
            }
        }

        public void SetBGMVolume(float volume)
        {
            _bgmSource.volume = Mathf.Clamp(volume, 0f, 1f);
        }

        public void SetSFXVolume(float volume)
        {
            _sfxSource.volume = Mathf.Clamp(volume, 0f, 1f);
        }

        public void StopBGM()
        {
            _bgmSource.Stop();
        }

        public void StopSFX()
        {
            _sfxSource.Stop();
        }

        public void PlayButtonSound()
        {
            PlaySFX("ButtonClick");
        }
    }
}
