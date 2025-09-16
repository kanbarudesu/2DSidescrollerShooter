using UnityEngine;
using UnityEngine.UI;

namespace Kanbarudesu.Audio
{
    public class AudioPlayer : MonoBehaviour
    {
        public enum AudioType
        {
            BgMusic,
            SFX
        }

        [SerializeField] private AudioClip[] audioClip;
        [SerializeField] private AudioType audioType;

        [Tooltip("This will try to find Button component on awake if set to true")]
        [SerializeField] private bool playOnButtonPressed;

        private float clipLength;
        private bool canPlay;

        private void Awake()
        {
            if (playOnButtonPressed && TryGetComponent(out Button button))
            {
                button.onClick.AddListener(PlayClip);
            }
            else if (playOnButtonPressed)
            {
                Debug.Log($"Button Compoenent doesn't exist on {this.gameObject.name} Gameobject.", this.gameObject);
            }
        }

        private void Update()
        {
            if (canPlay) return;

            clipLength -= Time.deltaTime;
            if (clipLength <= 0)
            {
                canPlay = true;
            }
        }

        public void PlayClip()
        {
            if (!canPlay) return;

            var randomClip = audioClip[Random.Range(0, audioClip.Length)];
            clipLength = randomClip.length;
            canPlay = false;

            switch (audioType)
            {
                case AudioType.BgMusic: AudioManager.Instance.PlayMusicClip(randomClip); break;
                case AudioType.SFX: AudioManager.Instance.PlaySfxClip(randomClip); break;
                default: Debug.Log("Audio Type Not Found"); break;
            }
        }
    }
}
