using UnityEngine;

namespace IceWorldRunner
{
    /// <summary>
    /// Self-constructing runtime bootstrap.
    /// If placed in a scene (or auto-run), ensures all core managers, player,
    /// camera, parallax background, and procedural systems exist and run seamlessly.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void AutoBootstrap()
        {
            if (FindAnyObjectByType<GameManager>() == null)
            {
                GameObject bootObj = new GameObject("=== GAME BOOTSTRAP ===");
                bootObj.AddComponent<GameBootstrap>().InitializeGame();
            }
        }

        public void InitializeGame()
        {
            Debug.Log("<color=cyan>[IceWorldRunner]</color> Initializing Frost Runner World...");

            // 1. Setup Camera
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                camObj.AddComponent<AudioListener>();
            }

            cam.orthographic = true;
            cam.orthographicSize = 6.5f;
            cam.backgroundColor = new Color(0.11f, 0.20f, 0.36f); // Deep Nordic twilight
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(0, 2f, -10f);

            CameraController camController = cam.GetComponent<CameraController>();
            if (camController == null)
            {
                camController = cam.gameObject.AddComponent<CameraController>();
            }

            // 2. Setup Sound Manager
            if (FindAnyObjectByType<SoundManager>() == null)
            {
                GameObject soundObj = new GameObject("SoundManager");
                soundObj.AddComponent<SoundManager>();
            }

            // 3. Setup Player
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player == null)
            {
                GameObject playerObj = new GameObject("Player");
                playerObj.transform.position = new Vector3(0, 1.5f, 0);
                playerObj.tag = "Player";

                SpriteRenderer sr = playerObj.AddComponent<SpriteRenderer>();
                Sprite standSp = ProceduralTextureGenerator.CreatePlayerSprite(36, 56);
                Sprite slideSp = ProceduralTextureGenerator.CreatePlayerSlideSprite(56, 28);
                sr.sprite = standSp;
                sr.sortingOrder = 5;

                Rigidbody2D rb = playerObj.AddComponent<Rigidbody2D>();
                rb.freezeRotation = true;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

                BoxCollider2D col = playerObj.AddComponent<BoxCollider2D>();
                col.size = new Vector2(0.9f, 1.6f);
                col.offset = new Vector2(0, 0.8f);

                player = playerObj.AddComponent<PlayerController>();
                player.spriteRenderer = sr;
                player.standingSprite = standSp;
                player.slidingSprite = slideSp;

                playerObj.AddComponent<PlayerEffects>();
            }

            camController.target = player.transform;

            // 4. Setup GameManager
            GameManager gm = FindAnyObjectByType<GameManager>();
            if (gm == null)
            {
                GameObject gmObj = new GameObject("GameManager");
                gm = gmObj.AddComponent<GameManager>();
            }
            gm.playerTransform = player.transform;

            // 5. Setup UI Manager
            if (FindAnyObjectByType<UIManager>() == null)
            {
                GameObject uiObj = new GameObject("UIManager");
                uiObj.AddComponent<UIManager>();
            }

            // 6. Setup Blizzard Particles
            if (FindAnyObjectByType<BlizzardController>() == null)
            {
                GameObject blizObj = new GameObject("BlizzardController");
                blizObj.AddComponent<BlizzardController>();
            }

            // 7. Setup Parallax Background
            SetupParallaxMountains();

            // 8. Setup Chunk Manager (Infinite Level Spawner)
            if (FindAnyObjectByType<ChunkManager>() == null)
            {
                GameObject chunkObj = new GameObject("ChunkManager");
                ChunkManager cm = chunkObj.AddComponent<ChunkManager>();
                cm.playerTransform = player.transform;
            }

            Debug.Log("<color=cyan>[IceWorldRunner]</color> Frost Runner World successfully initialized!");
        }

        private void SetupParallaxMountains()
        {
            if (FindAnyObjectByType<ParallaxBackground>() != null) return;

            GameObject parallaxRoot = new GameObject("ParallaxEnvironment");
            ParallaxBackground pb = parallaxRoot.AddComponent<ParallaxBackground>();

            Sprite mountainFarSprite = ProceduralTextureGenerator.CreateMountainSprite(128, 80, new Color(0.18f, 0.28f, 0.46f, 0.85f));
            Sprite mountainMidSprite = ProceduralTextureGenerator.CreateMountainSprite(128, 80, new Color(0.24f, 0.38f, 0.58f, 0.95f));

            // Layer 1: Far Mountains
            GameObject layerFar = new GameObject("Layer_FarMountains");
            layerFar.transform.parent = parallaxRoot.transform;
            layerFar.transform.position = new Vector3(0, 1.0f, 5f);
            for (int i = -1; i <= 3; i++)
            {
                GameObject m = new GameObject($"MountFar_{i}");
                m.transform.parent = layerFar.transform;
                m.transform.localPosition = new Vector3(i * 16f, 0, 0);
                m.transform.localScale = new Vector3(4f, 4f, 1f);
                SpriteRenderer sr = m.AddComponent<SpriteRenderer>();
                sr.sprite = mountainFarSprite;
                sr.sortingOrder = -10;
            }

            // Layer 2: Mid Mountains
            GameObject layerMid = new GameObject("Layer_MidMountains");
            layerMid.transform.parent = parallaxRoot.transform;
            layerMid.transform.position = new Vector3(0, 0f, 3f);
            for (int i = -1; i <= 3; i++)
            {
                GameObject m = new GameObject($"MountMid_{i}");
                m.transform.parent = layerMid.transform;
                m.transform.localPosition = new Vector3(i * 14f, 0, 0);
                m.transform.localScale = new Vector3(3.5f, 3f, 1f);
                SpriteRenderer sr = m.AddComponent<SpriteRenderer>();
                sr.sprite = mountainMidSprite;
                sr.sortingOrder = -8;
            }

            pb.layers = new ParallaxBackground.ParallaxLayer[]
            {
                new ParallaxBackground.ParallaxLayer { transform = layerFar.transform, parallaxSpeedX = 0.08f, tileWidth = 32f },
                new ParallaxBackground.ParallaxLayer { transform = layerMid.transform, parallaxSpeedX = 0.22f, tileWidth = 28f }
            };
        }
    }
}
