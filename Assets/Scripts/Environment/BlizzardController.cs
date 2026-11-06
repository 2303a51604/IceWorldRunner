using UnityEngine;

namespace IceWorldRunner
{
    /// <summary>
    /// Controls atmospheric snow storm, blizzard gusts, and ambient frost particles.
    /// </summary>
    public class BlizzardController : MonoBehaviour
    {
        [Header("Wind Mechanics")]
        public float gustInterval = 12f;
        public float gustDuration = 4f;
        public float gustForce = -2.5f; // Opposing headwind

        [Header("Particle System")]
        private ParticleSystem snowParticles;
        private Transform targetCamera;
        private float gustTimer;
        private bool isGustActive = false;

        private void Awake()
        {
            SetupSnowParticleSystem();
        }

        private void Start()
        {
            if (Camera.main != null)
            {
                targetCamera = Camera.main.transform;
            }
            gustTimer = gustInterval;
        }

        private void Update()
        {
            // Follow camera horizontally so snowflakes continually blanket the screen
            if (targetCamera != null)
            {
                transform.position = new Vector3(targetCamera.position.x, targetCamera.position.y + 4f, 0);
            }

            HandleWindGusts();
        }

        private void HandleWindGusts()
        {
            gustTimer -= Time.deltaTime;
            if (!isGustActive && gustTimer <= 0f)
            {
                StartWindGust();
            }
            else if (isGustActive && gustTimer <= -gustDuration)
            {
                EndWindGust();
            }
        }

        private void StartWindGust()
        {
            isGustActive = true;
            if (snowParticles != null)
            {
                var vel = snowParticles.velocityOverLifetime;
                vel.x = new ParticleSystem.MinMaxCurve(-18f, -28f);
                vel.y = new ParticleSystem.MinMaxCurve(-6f, -11f);
                vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
                var main = snowParticles.main;
                main.simulationSpeed = 1.6f;
            }
        }

        private void EndWindGust()
        {
            isGustActive = false;
            gustTimer = gustInterval;
            if (snowParticles != null)
            {
                var vel = snowParticles.velocityOverLifetime;
                vel.x = new ParticleSystem.MinMaxCurve(-6f, -12f);
                vel.y = new ParticleSystem.MinMaxCurve(-4f, -8f);
                vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
                var main = snowParticles.main;
                main.simulationSpeed = 1.0f;
            }
        }

        private void SetupSnowParticleSystem()
        {
            snowParticles = gameObject.AddComponent<ParticleSystem>();
            var main = snowParticles.main;
            main.loop = true;
            main.playOnAwake = true;
            main.maxParticles = 500;
            main.startLifetime = 3.5f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(6f, 10f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
            main.startColor = new Color(0.92f, 0.98f, 1f, 0.85f);

            var emission = snowParticles.emission;
            emission.rateOverTime = 80;

            var shape = snowParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(32f, 2f, 1f);

            var vel = snowParticles.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(-6f, -12f);
            vel.y = new ParticleSystem.MinMaxCurve(-4f, -8f);
            vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            ParticleSystemRenderer rend = GetComponent<ParticleSystemRenderer>();
            rend.sortingOrder = 10;

            // Assign crisp white snowflake sprite material to eliminate any pink/magenta squares
            Shader spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader != null)
            {
                Material snowMat = new Material(spriteShader);
                Sprite snowSprite = ProceduralTextureGenerator.CreateSnowFlakeSprite(16);
                snowMat.mainTexture = snowSprite.texture;
                rend.material = snowMat;
            }
        }
    }
}
