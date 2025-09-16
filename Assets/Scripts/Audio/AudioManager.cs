using System.Collections;
using Kanbarudesu.Utility;
using UnityEngine;

namespace Kanbarudesu.Audio
{
    public class AudioManager : Singleton<AudioManager>
    {
        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private AudioSource sfxAudioSource;

        public bool IsMusicMuted { get; private set; }
        public bool IsSfxMuted { get; private set; }

        private bool isMusicPaused = false;

        private float musicVolume;
        private float sfxVolume;

        private readonly float k_volumeSteps = 0.01f;

        protected override void Awake()
        {
            base.Awake();
            InitializeAudioSourceState();
        }

        private void InitializeAudioSourceState()
        {
            LoadAudioSourcesState();
            musicAudioSource.mute = IsMusicMuted;
            musicAudioSource.volume = musicVolume;

            sfxAudioSource.mute = IsSfxMuted;
            sfxAudioSource.volume = sfxVolume;
        }

        public void PlayMusicClip(AudioClip audioClip, bool isLooping = true)
        {
            //Check if theres music currently playing.
            if (musicAudioSource.isPlaying)
            {
                //We Switch current music to new music clip by fading it in and out.
                StartCoroutine(FadeInOutMusic(audioClip));
            }
            else
            {
                musicAudioSource.clip = audioClip;
                musicAudioSource.volume = musicVolume;
                musicAudioSource.Play();
            }

            musicAudioSource.loop = isLooping;
        }

        public void PlaySfxClip(AudioClip audioClip)
        {
            sfxAudioSource.PlayOneShot(audioClip, sfxVolume);
        }

        public void StopMusicClip()
        {
            musicAudioSource.Stop();
        }

        public void TogglePauseMusicClip()
        {
            isMusicPaused = !isMusicPaused;
            if (isMusicPaused)
                musicAudioSource.Pause();
            else
                musicAudioSource.UnPause();
        }

        public void ToggleMuteMusicAudio()
        {
            musicAudioSource.mute = !musicAudioSource.mute;
            IsMusicMuted = musicAudioSource.mute;
        }

        public void ToggleMuteSfxAudio()
        {
            sfxAudioSource.mute = !sfxAudioSource.mute;
            IsSfxMuted = sfxAudioSource.mute;
        }

        public void SetMusicVolume(float newValue)
        {
            musicAudioSource.volume = newValue;
        }

        public void SetSfxVolume(float newValue)
        {
            sfxAudioSource.volume = newValue;
        }

        public void SaveAudiSourcesState()
        {
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);

            PlayerPrefs.SetInt("MusicMute", IsMusicMuted ? 1 : 0);
            PlayerPrefs.SetInt("SFXMute", IsSfxMuted ? 1 : 0);
        }

        private void LoadAudioSourcesState()
        {
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1);

            IsMusicMuted = PlayerPrefs.GetInt("MusicMute", 0) == 1 ? true : false;
            IsSfxMuted = PlayerPrefs.GetInt("SFXMute", 0) == 1 ? true : false;
        }

        private IEnumerator FadeInOutMusic(AudioClip audioClip)
        {
            float currentVolume = musicVolume;

            while (currentVolume > 0)
            {
                musicAudioSource.volume -= k_volumeSteps;
                currentVolume -= k_volumeSteps;
                yield return new WaitForEndOfFrame();
            }

            musicAudioSource.Stop();
            musicAudioSource.clip = audioClip;
            musicAudioSource.volume = musicVolume;
            musicAudioSource.Play();

            while (currentVolume < musicVolume)
            {
                musicAudioSource.volume += k_volumeSteps;
                currentVolume += k_volumeSteps;
                yield return new WaitForEndOfFrame();
            }
        }
    }
}

