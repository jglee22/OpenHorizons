using UnityEngine;

namespace AudioSystem
{
    [System.Serializable]
    public class SoundEffect
    {
        [Header("Sound Settings")]
        public string soundName;
        public AudioClip audioClip;
        public float volume = 1.0f;
        public float pitch = 1.0f;
        public bool loop = false;
        public bool playOnAwake = false;
        
        [Header("3D Sound Settings")]
        public bool is3D = false;
        public float minDistance = 1.0f;
        public float maxDistance = 500.0f;
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
        
        [Header("Randomization")]
        public bool randomizePitch = false;
        public float pitchVariation = 0.1f;
        public bool randomizeVolume = false;
        public float volumeVariation = 0.1f;
        
        private AudioSource audioSource;
        private float baseVolume;
        private float basePitch;
        
        public void Initialize(GameObject parent)
        {
            if (audioClip == null)
            {
                Debug.LogWarning($"SoundEffect '{soundName}' has no audio clip assigned!");
                return;
            }
            
            // Create audio source
            audioSource = parent.AddComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.loop = loop;
            audioSource.playOnAwake = playOnAwake;
            
            // Store base values for randomization
            baseVolume = volume;
            basePitch = pitch;
            
            // Configure 3D sound settings
            if (is3D)
            {
                audioSource.spatialBlend = 1.0f; // 3D sound
                audioSource.minDistance = minDistance;
                audioSource.maxDistance = maxDistance;
                audioSource.rolloffMode = rolloffMode;
            }
            else
            {
                audioSource.spatialBlend = 0.0f; // 2D sound
            }
        }
        
        public void Play()
        {
            if (audioSource == null || audioClip == null) return;
            
            // Apply randomization if enabled
            if (randomizePitch)
            {
                audioSource.pitch = basePitch + Random.Range(-pitchVariation, pitchVariation);
            }
            
            if (randomizeVolume)
            {
                audioSource.volume = baseVolume + Random.Range(-volumeVariation, volumeVariation);
            }
            
            audioSource.Play();
        }
        
        public void PlayOneShot()
        {
            if (audioSource == null || audioClip == null) return;
            
            float finalVolume = volume;
            float finalPitch = pitch;
            
            // Apply randomization if enabled
            if (randomizePitch)
            {
                finalPitch = basePitch + Random.Range(-pitchVariation, pitchVariation);
            }
            
            if (randomizeVolume)
            {
                finalVolume = baseVolume + Random.Range(-volumeVariation, volumeVariation);
            }
            
            audioSource.pitch = finalPitch;
            audioSource.PlayOneShot(audioClip, finalVolume);
        }
        
        public void Stop()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }
        
        public void Pause()
        {
            if (audioSource != null)
            {
                audioSource.Pause();
            }
        }
        
        public void Resume()
        {
            if (audioSource != null)
            {
                audioSource.UnPause();
            }
        }
        
        public bool IsPlaying()
        {
            return audioSource != null && audioSource.isPlaying;
        }
        
        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
            if (audioSource != null)
            {
                audioSource.volume = volume;
            }
        }
        
        public void SetPitch(float newPitch)
        {
            pitch = Mathf.Clamp(newPitch, 0.1f, 3.0f);
            if (audioSource != null)
            {
                audioSource.pitch = pitch;
            }
        }
        
        public void SetLoop(bool loop)
        {
            this.loop = loop;
            if (audioSource != null)
            {
                audioSource.loop = loop;
            }
        }
    }
}
