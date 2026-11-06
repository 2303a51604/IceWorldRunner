using UnityEngine;

namespace IceWorldRunner
{
    /// <summary>
    /// Procedural Sound Generator & Audio Manager.
    /// Synthesizes dynamic retro/crystalline sound effects in memory.
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Sources")]
        private AudioSource sfxSource;
        private AudioSource loopSource;

        [Header("Volume Settings")]
        [Range(0f, 1f)] public float sfxVolume = 0.8f;
        [Range(0f, 1f)] public float musicVolume = 0.5f;

        private AudioClip jumpClip;
        private AudioClip doubleJumpClip;
        private AudioClip slideClip;
        private AudioClip crystalClip;
        private AudioClip crackClip;
        private AudioClip shatterClip;
        private AudioClip powerUpClip;
        private AudioClip bounceClip;
        private AudioClip blizzardAmbience;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            loopSource = gameObject.AddComponent<AudioSource>();
            loopSource.playOnAwake = false;
            loopSource.loop = true;

            GenerateAllProceduralClips();
        }

        private void Start()
        {
            PlayBlizzardAmbience();
        }

        private void GenerateAllProceduralClips()
        {
            jumpClip = CreateJumpSound();
            doubleJumpClip = CreateDoubleJumpSound();
            slideClip = CreateSlideSound();
            crystalClip = CreateCrystalSound();
            crackClip = CreateCrackSound();
            shatterClip = CreateShatterSound();
            powerUpClip = CreatePowerUpSound();
            bounceClip = CreateBounceSound();
            blizzardAmbience = CreateBlizzardAmbience();
        }

        public void PlayJump() => PlayOneShot(jumpClip, 0.8f);
        public void PlayDoubleJump() => PlayOneShot(doubleJumpClip, 0.9f);
        public void PlayCrystal() => PlayOneShot(crystalClip, 0.75f);
        public void PlayCrack() => PlayOneShot(crackClip, 0.9f);
        public void PlayShatter() => PlayOneShot(shatterClip, 1.0f);
        public void PlayPowerUp() => PlayOneShot(powerUpClip, 0.9f);
        public void PlayBounce() => PlayOneShot(bounceClip, 0.85f);

        public void StartSlide()
        {
            if (slideClip != null && !loopSource.isPlaying)
            {
                loopSource.clip = slideClip;
                loopSource.volume = sfxVolume * 0.4f;
                loopSource.Play();
            }
        }

        public void StopSlide()
        {
            if (loopSource.clip == slideClip && loopSource.isPlaying)
            {
                loopSource.Stop();
            }
        }

        private void PlayOneShot(AudioClip clip, float volumeScale = 1.0f)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
            }
        }

        private void PlayBlizzardAmbience()
        {
            if (blizzardAmbience != null)
            {
                AudioSource amb = gameObject.AddComponent<AudioSource>();
                amb.clip = blizzardAmbience;
                amb.loop = true;
                amb.volume = musicVolume * 0.35f;
                amb.Play();
            }
        }

        #region Sound Synthesizers

        private AudioClip CreateJumpSound()
        {
            int sampleRate = 44100;
            float duration = 0.18f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;
                // Ascending sweep from 260Hz to 620Hz
                float freq = Mathf.Lerp(260f, 620f, progress);
                float envelope = 1f - progress;
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.5f;
            }

            AudioClip clip = AudioClip.Create("SFX_Jump", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateDoubleJumpSound()
        {
            int sampleRate = 44100;
            float duration = 0.22f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;
                // Ascending crystalline chime sweep
                float freq = Mathf.Lerp(450f, 1100f, Mathf.Pow(progress, 0.7f));
                float harmonic = freq * 1.5f;
                float envelope = (1f - progress) * Mathf.Min(progress * 15f, 1f);
                samples[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f +
                              Mathf.Sin(2f * Mathf.PI * harmonic * t) * 0.3f) * envelope * 0.45f;
            }

            AudioClip clip = AudioClip.Create("SFX_DoubleJump", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateCrystalSound()
        {
            int sampleRate = 44100;
            float duration = 0.35f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            // Sparkling bell arpeggio chord (E6, G#6, B6, E7)
            float[] freqs = new float[] { 1318.5f, 1661.2f, 1975.5f, 2637.0f };

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;
                float envelope = Mathf.Exp(-7f * progress);

                float signal = 0f;
                for (int f = 0; f < freqs.Length; f++)
                {
                    signal += Mathf.Sin(2f * Mathf.PI * freqs[f] * t);
                }

                samples[i] = (signal / freqs.Length) * envelope * 0.4f;
            }

            AudioClip clip = AudioClip.Create("SFX_Crystal", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateSlideSound()
        {
            int sampleRate = 44100;
            float duration = 0.8f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            // Filtered white noise with low rumble for ice/snow skate scrape
            float last = 0f;
            for (int i = 0; i < sampleCount; i++)
            {
                float white = (Random.value * 2f - 1f);
                last = Mathf.Lerp(last, white, 0.15f); // Low pass filter
                samples[i] = last * 0.35f;
            }

            AudioClip clip = AudioClip.Create("SFX_Slide", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateCrackSound()
        {
            int sampleRate = 44100;
            float duration = 0.25f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;
                float snap = (Random.value * 2f - 1f) * Mathf.Exp(-20f * progress);
                float rumble = Mathf.Sin(2f * Mathf.PI * 90f * t) * (1f - progress);
                samples[i] = (snap * 0.7f + rumble * 0.3f) * 0.6f;
            }

            AudioClip clip = AudioClip.Create("SFX_Crack", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateShatterSound()
        {
            int sampleRate = 44100;
            float duration = 0.45f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;
                float burst = (Random.value * 2f - 1f) * Mathf.Exp(-8f * progress);
                float chime1 = Mathf.Sin(2f * Mathf.PI * 1800f * t) * Mathf.Exp(-12f * progress);
                float chime2 = Mathf.Sin(2f * Mathf.PI * 2400f * t) * Mathf.Exp(-15f * progress);
                samples[i] = (burst * 0.5f + chime1 * 0.3f + chime2 * 0.2f) * 0.7f;
            }

            AudioClip clip = AudioClip.Create("SFX_Shatter", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreatePowerUpSound()
        {
            int sampleRate = 44100;
            float duration = 0.4f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;
                float noteStep = Mathf.Floor(progress * 4f);
                float freq = 440f * Mathf.Pow(1.2599f, noteStep * 2f);
                float envelope = 1f - (progress * 4f - noteStep);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.4f;
            }

            AudioClip clip = AudioClip.Create("SFX_PowerUp", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateBounceSound()
        {
            int sampleRate = 44100;
            float duration = 0.25f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;
                float freq = Mathf.Lerp(160f, 480f, Mathf.Sin(progress * Mathf.PI * 0.5f));
                float envelope = 1f - progress;
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.5f;
            }

            AudioClip clip = AudioClip.Create("SFX_Bounce", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateBlizzardAmbience()
        {
            int sampleRate = 22050;
            float duration = 4.0f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            float filter = 0f;
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float noise = (Random.value * 2f - 1f);
                float windSwell = 0.5f + 0.5f * Mathf.Sin(t * 1.5f);
                filter = Mathf.Lerp(filter, noise, 0.08f + 0.04f * windSwell);
                samples[i] = filter * (0.2f + 0.2f * windSwell);
            }

            AudioClip clip = AudioClip.Create("SFX_BlizzardAmbience", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        #endregion
    }
}
