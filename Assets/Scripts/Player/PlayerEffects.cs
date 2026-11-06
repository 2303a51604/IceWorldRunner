using UnityEngine;

namespace IceWorldRunner
{
    /// <summary>
    /// Manages visual particle effects and visual juice for the player.
    /// Builds required Particle Systems procedurally if not assigned.
    /// </summary>
    public class PlayerEffects : MonoBehaviour
    {
        [Header("Particle Systems")]
        public ParticleSystem slideSprayParticles;
        public ParticleSystem jumpDustParticles;
        public ParticleSystem deathShatterParticles;

        [Header("Shield Visual")]
        public GameObject shieldVisual;

        private void Awake()
        {
            SetupProceduralEffects();
        }

        private void SetupProceduralEffects()
        {
            Material particleMat = null;
            Shader spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader != null)
            {
                particleMat = new Material(spriteShader);
                particleMat.mainTexture = ProceduralTextureGenerator.CreateSnowFlakeSprite(16).texture;
            }

            // Setup Slide Spray Particles
            if (slideSprayParticles == null)
            {
                GameObject sprayObj = new GameObject("SlideSprayParticles");
                sprayObj.transform.parent = transform;
                sprayObj.transform.localPosition = new Vector3(-0.3f, -0.4f, 0);

                slideSprayParticles = sprayObj.AddComponent<ParticleSystem>();
                var main = slideSprayParticles.main;
                main.startLifetime = 0.35f;
                main.startSpeed = 3.5f;
                main.startSize = 0.2f;
                main.startColor = new Color(0.8f, 0.95f, 1f, 0.8f);
                main.loop = true;
                main.playOnAwake = false;

                var emission = slideSprayParticles.emission;
                emission.rateOverTime = 30;

                var shape = slideSprayParticles.shape;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 25f;
                shape.radius = 0.1f;
                sprayObj.transform.localRotation = Quaternion.Euler(0, 0, 160f); // Spray backwards

                if (particleMat != null)
                {
                    sprayObj.GetComponent<ParticleSystemRenderer>().material = particleMat;
                }
            }

            // Setup Jump / Land Dust Particles
            if (jumpDustParticles == null)
            {
                GameObject dustObj = new GameObject("JumpDustParticles");
                dustObj.transform.parent = transform;
                dustObj.transform.localPosition = new Vector3(0, -0.5f, 0);

                jumpDustParticles = dustObj.AddComponent<ParticleSystem>();
                var main = jumpDustParticles.main;
                main.startLifetime = 0.25f;
                main.startSpeed = 2.0f;
                main.startSize = 0.22f;
                main.startColor = new Color(0.9f, 0.98f, 1f, 0.9f);
                main.loop = false;
                main.playOnAwake = false;

                var emission = jumpDustParticles.emission;
                emission.rateOverTime = 0;

                if (particleMat != null)
                {
                    dustObj.GetComponent<ParticleSystemRenderer>().material = particleMat;
                }
            }

            // Setup Death Shatter Particles
            if (deathShatterParticles == null)
            {
                GameObject deathObj = new GameObject("DeathShatterParticles");
                deathObj.transform.parent = transform;
                deathObj.transform.localPosition = Vector3.zero;

                deathShatterParticles = deathObj.AddComponent<ParticleSystem>();
                var main = deathShatterParticles.main;
                main.startLifetime = 0.8f;
                main.startSpeed = 6.0f;
                main.startSize = 0.28f;
                main.startColor = new Color(0.4f, 0.85f, 1f, 1f);
                main.loop = false;
                main.playOnAwake = false;

                var shape = deathShatterParticles.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;

                if (particleMat != null)
                {
                    deathObj.GetComponent<ParticleSystemRenderer>().material = particleMat;
                }
            }

            // Setup Warmth Shield Glow
            if (shieldVisual == null)
            {
                shieldVisual = new GameObject("ShieldAura");
                shieldVisual.transform.parent = transform;
                shieldVisual.transform.localPosition = new Vector3(0, 0.2f, 0);
                shieldVisual.transform.localScale = new Vector3(2.2f, 2.2f, 1f);

                SpriteRenderer sr = shieldVisual.AddComponent<SpriteRenderer>();
                sr.sprite = ProceduralTextureGenerator.CreatePowerUpSprite(48, new Color(1f, 0.6f, 0.1f, 0.45f));
                sr.sortingOrder = 5;
                shieldVisual.SetActive(false);
            }
        }

        public void SetSlideEffect(bool active)
        {
            if (slideSprayParticles == null) return;
            if (active) slideSprayParticles.Play();
            else slideSprayParticles.Stop();
        }

        public void PlayJumpDust()
        {
            if (jumpDustParticles != null) jumpDustParticles.Emit(8);
        }

        public void PlayLandDust()
        {
            if (jumpDustParticles != null) jumpDustParticles.Emit(12);
        }

        public void PlayDoubleJumpBurst()
        {
            if (jumpDustParticles != null) jumpDustParticles.Emit(16);
        }

        public void SetShieldGlow(bool active)
        {
            if (shieldVisual != null) shieldVisual.SetActive(active);
        }

        public void PlayShieldBreakBurst()
        {
            if (jumpDustParticles != null) jumpDustParticles.Emit(25);
        }

        public void PlayDeathShatter()
        {
            if (deathShatterParticles != null)
            {
                deathShatterParticles.transform.parent = null;
                deathShatterParticles.Emit(40);
                Destroy(deathShatterParticles.gameObject, 1.5f);
            }
        }
    }
}
