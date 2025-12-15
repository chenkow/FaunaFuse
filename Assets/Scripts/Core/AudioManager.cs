using UnityEngine;
using System.Collections.Generic;

namespace Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Clips")]
        public AudioClip sfxSpawn;
        public AudioClip sfxMerge;
        public AudioClip sfxMove;
        public AudioClip sfxButton;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Ensure sources exist
                if (!sfxSource) sfxSource = gameObject.AddComponent<AudioSource>();
                if (!musicSource) 
                {
                    musicSource = gameObject.AddComponent<AudioSource>();
                    musicSource.loop = true;
                }
            }
            else Destroy(gameObject);
        }

        public void PlaySFX(AudioClip clip, float volume = 1.0f)
        {
            if (clip && sfxSource) sfxSource.PlayOneShot(clip, volume);
        }

        public void PlaySpawn() => PlaySFX(sfxSpawn);
        public void PlayMerge() => PlaySFX(sfxMerge);
        public void PlayMove() => PlaySFX(sfxMove, 0.5f);
        public void PlayButton() => PlaySFX(sfxButton);
    }
}