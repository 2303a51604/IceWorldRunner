using System.Collections.Generic;
using UnityEngine;

namespace IceWorldRunner
{
    public enum ChunkType
    {
        Runway,
        SlideGauntlet,
        CrevasseLeap,
        FragileIceSteps,
        IcicleDropAlley,
        BouncyGeyser,
        RampSlope
    }

    /// <summary>
    /// Procedural Infinite Chunk Generator and Recycler.
    /// Dynamically constructs ice platforms, hazards, and crystal arcs ahead of the player.
    /// </summary>
    public class ChunkManager : MonoBehaviour
    {
        public static ChunkManager Instance { get; private set; }

        [Header("Spawn Configuration")]
        public float chunkLength = 28.0f;
        public int chunksAhead = 4;
        public float despawnDistanceBehind = 35.0f;

        [Header("References")]
        public Transform playerTransform;

        private float nextSpawnX = -10.0f;
        private List<GameObject> activeChunks = new List<GameObject>();
        private int chunkCounter = 0;

        // Shared Procedural Sprites
        private Sprite iceTileSprite;
        private Sprite crackedIceSprite;
        private Sprite icicleSprite;
        private Sprite spikeSprite;
        private Sprite crystalSprite;
        private Sprite powerUpSprite;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            LoadOrGenerateSprites();
        }

        private void LoadOrGenerateSprites()
        {
            iceTileSprite = ProceduralTextureGenerator.CreateIceTileSprite(64, 64);
            crackedIceSprite = ProceduralTextureGenerator.CreateCrackedIceSprite(64, 64);
            icicleSprite = ProceduralTextureGenerator.CreateIcicleSprite(32, 64);
            spikeSprite = ProceduralTextureGenerator.CreateHazardSpikeSprite(48, 48);
            crystalSprite = ProceduralTextureGenerator.CreateCrystalSprite(48);
            powerUpSprite = ProceduralTextureGenerator.CreatePowerUpSprite(48);
        }

        private void Start()
        {
            if (playerTransform == null && GameManager.Instance != null)
            {
                playerTransform = GameManager.Instance.playerTransform;
            }

            // Spawn initial safe runway chunks
            SpawnChunk(ChunkType.Runway);
            SpawnChunk(ChunkType.Runway);

            // Populate initial forward view
            for (int i = 0; i < chunksAhead; i++)
            {
                SpawnNextRandomChunk();
            }
        }

        private void Update()
        {
            if (playerTransform == null) return;

            // Spawn ahead
            if (playerTransform.position.x + (chunksAhead * chunkLength) > nextSpawnX)
            {
                SpawnNextRandomChunk();
            }

            // Recycle / Despawn old chunks behind player
            for (int i = activeChunks.Count - 1; i >= 0; i--)
            {
                GameObject chunk = activeChunks[i];
                if (chunk != null && chunk.transform.position.x + chunkLength < playerTransform.position.x - despawnDistanceBehind)
                {
                    activeChunks.RemoveAt(i);
                    Destroy(chunk);
                }
            }
        }

        private void SpawnNextRandomChunk()
        {
            chunkCounter++;
            ChunkType type;

            if (chunkCounter <= 2)
            {
                type = ChunkType.Runway;
            }
            else
            {
                // Weighted selection based on distance
                float r = Random.value;
                if (r < 0.20f) type = ChunkType.SlideGauntlet;
                else if (r < 0.40f) type = ChunkType.CrevasseLeap;
                else if (r < 0.60f) type = ChunkType.FragileIceSteps;
                else if (r < 0.80f) type = ChunkType.IcicleDropAlley;
                else type = ChunkType.BouncyGeyser;
            }

            SpawnChunk(type);
        }

        public void SpawnChunk(ChunkType type)
        {
            GameObject chunkObj = new GameObject($"Chunk_{chunkCounter}_{type}");
            chunkObj.transform.position = new Vector3(nextSpawnX, 0, 0);
            chunkObj.transform.parent = transform;

            switch (type)
            {
                case ChunkType.Runway:
                    BuildRunway(chunkObj);
                    break;
                case ChunkType.SlideGauntlet:
                    BuildSlideGauntlet(chunkObj);
                    break;
                case ChunkType.CrevasseLeap:
                    BuildCrevasseLeap(chunkObj);
                    break;
                case ChunkType.FragileIceSteps:
                    BuildFragileIceSteps(chunkObj);
                    break;
                case ChunkType.IcicleDropAlley:
                    BuildIcicleDropAlley(chunkObj);
                    break;
                case ChunkType.BouncyGeyser:
                    BuildBouncyGeyser(chunkObj);
                    break;
            }

            activeChunks.Add(chunkObj);
            nextSpawnX += chunkLength;
        }

        #region Procedural Chunk Builders

        private void BuildRunway(GameObject root)
        {
            // Continuous flat ice track
            CreatePlatform(root, new Vector3(chunkLength * 0.5f, -1f, 0), new Vector2(chunkLength, 2f), PlatformType.SlickIce);

            // Gentle arc of crystals
            SpawnCrystalArc(root, new Vector3(6f, 0.8f, 0), new Vector3(18f, 0.8f, 0), 5, 1.8f);

            // Chance for power-up
            if (Random.value < 0.35f)
            {
                SpawnPowerUp(root, new Vector3(chunkLength * 0.75f, 1.2f, 0), (PowerUpType)Random.Range(0, 4));
            }
        }

        private void BuildSlideGauntlet(GameObject root)
        {
            // Floor platform
            CreatePlatform(root, new Vector3(chunkLength * 0.5f, -1f, 0), new Vector2(chunkLength, 2f), PlatformType.SlickIce);

            // Low-hanging icicle ceiling requiring sliding underneath!
            float obstacleStart = 8f;
            float ceilingWidth = 10f;
            CreatePlatform(root, new Vector3(obstacleStart + ceilingWidth * 0.5f, 2.1f, 0), new Vector2(ceilingWidth, 1.5f), PlatformType.SlickIce);

            // Hanging decorative & dangerous stalactites
            for (float x = obstacleStart + 1f; x < obstacleStart + ceilingWidth - 1f; x += 1.8f)
            {
                CreateHangingIcicle(root, new Vector3(x, 1.35f, 0));
            }

            // Crystals positioned low on the ground to reward sliding
            for (float x = obstacleStart + 2f; x < obstacleStart + ceilingWidth; x += 2f)
            {
                SpawnCrystal(root, new Vector3(x, 0.4f, 0));
            }
        }

        private void BuildCrevasseLeap(GameObject root)
        {
            // First ledge
            CreatePlatform(root, new Vector3(4f, -1f, 0), new Vector2(8f, 2f), PlatformType.SlickIce);

            // Island platform in middle
            CreatePlatform(root, new Vector3(14f, 0.5f, 0), new Vector2(6f, 1.5f), PlatformType.SlickIce);

            // Landing ledge
            CreatePlatform(root, new Vector3(23f, -1f, 0), new Vector2(10f, 2f), PlatformType.SlickIce);

            // Spikes in the crevasse pit
            CreateHazardSpikes(root, new Vector3(8.5f, -3.5f, 0), 10f);

            // High arc of crystals across the gap
            SpawnCrystalArc(root, new Vector3(6.5f, 1f, 0), new Vector3(13.5f, 1.5f, 0), 4, 2.5f);
            SpawnCrystalArc(root, new Vector3(15.5f, 1.5f, 0), new Vector3(21.5f, 1f, 0), 4, 2.5f);
        }

        private void BuildFragileIceSteps(GameObject root)
        {
            // Entry platform
            CreatePlatform(root, new Vector3(3f, -1f, 0), new Vector2(6f, 2f), PlatformType.SlickIce);

            // 3 Cracking Ice Steps
            CreateFragilePlatform(root, new Vector3(9f, 0f, 0), new Vector2(3.5f, 1f));
            CreateFragilePlatform(root, new Vector3(14.5f, 1.2f, 0), new Vector2(3.5f, 1f));
            CreateFragilePlatform(root, new Vector3(20f, 0.5f, 0), new Vector2(3.5f, 1f));

            // Exit platform
            CreatePlatform(root, new Vector3(25.5f, -1f, 0), new Vector2(5f, 2f), PlatformType.SlickIce);

            // Spikes below
            CreateHazardSpikes(root, new Vector3(6.5f, -4f, 0), 18f);

            // Crystals above each fragile step
            SpawnCrystal(root, new Vector3(9f, 1.2f, 0));
            SpawnCrystal(root, new Vector3(14.5f, 2.4f, 0));
            SpawnCrystal(root, new Vector3(20f, 1.7f, 0));
        }

        private void BuildIcicleDropAlley(GameObject root)
        {
            // Ground track
            CreatePlatform(root, new Vector3(chunkLength * 0.5f, -1f, 0), new Vector2(chunkLength, 2f), PlatformType.SlickIce);

            // High overhead arch
            float archStart = 5f;
            for (int i = 0; i < 4; i++)
            {
                float x = archStart + (i * 4.5f);
                CreateFallingIcicle(root, new Vector3(x, 5.5f, 0));
                // Crystal underneath to tempt player
                SpawnCrystal(root, new Vector3(x, 0.8f, 0));
            }
        }

        private void BuildBouncyGeyser(GameObject root)
        {
            // Lower approach
            CreatePlatform(root, new Vector3(4f, -1f, 0), new Vector2(8f, 2f), PlatformType.SlickIce);

            // Bouncy Snow Geyser Launchpad
            CreatePlatform(root, new Vector3(9.5f, -0.6f, 0), new Vector2(3f, 1.2f), PlatformType.BouncySnowdrift);

            // High secret tier packed with crystals
            CreatePlatform(root, new Vector3(18f, 4.5f, 0), new Vector2(10f, 1.5f), PlatformType.SlickIce);

            // Ground exit
            CreatePlatform(root, new Vector3(22f, -1f, 0), new Vector2(12f, 2f), PlatformType.SlickIce);

            // Crystals along high bounce trajectory
            SpawnCrystalArc(root, new Vector3(10f, 1f, 0), new Vector3(16f, 5.5f, 0), 5, 3f);
            SpawnPowerUp(root, new Vector3(19f, 5.8f, 0), PowerUpType.WarmthShield);
        }

        #endregion

        #region Helper Factory Methods

        private GameObject CreatePlatform(GameObject parent, Vector3 localPos, Vector2 size, PlatformType type)
        {
            GameObject obj = new GameObject($"Platform_{type}");
            obj.transform.parent = parent.transform;
            obj.transform.localPosition = localPos;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = iceTileSprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = size;

            if (type == PlatformType.BouncySnowdrift)
            {
                sr.color = new Color(0.8f, 0.95f, 1.0f, 1.0f);
            }
            else
            {
                sr.color = Color.white;
            }

            BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
            col.size = size;

            IcePlatform ip = obj.AddComponent<IcePlatform>();
            ip.platformType = type;

            obj.layer = LayerMask.NameToLayer("Ground");
            return obj;
        }

        private GameObject CreateFragilePlatform(GameObject parent, Vector3 localPos, Vector2 size)
        {
            GameObject obj = new GameObject("FragileIceStep");
            obj.transform.parent = parent.transform;
            obj.transform.localPosition = localPos;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = crackedIceSprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = size;
            sr.color = new Color(0.9f, 0.95f, 1f, 0.9f);

            BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
            col.size = size;

            obj.AddComponent<FragileIcePlatform>();
            obj.layer = LayerMask.NameToLayer("Ground");
            return obj;
        }

        private GameObject CreateFallingIcicle(GameObject parent, Vector3 localPos)
        {
            GameObject obj = new GameObject("FallingIcicle");
            obj.transform.parent = parent.transform;
            obj.transform.localPosition = localPos;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = icicleSprite;
            sr.color = new Color(0.85f, 0.95f, 1f, 0.95f);

            PolygonCollider2D col = obj.AddComponent<PolygonCollider2D>();
            col.isTrigger = true;

            obj.AddComponent<FallingIcicle>();
            obj.tag = "Obstacle";
            return obj;
        }

        private GameObject CreateHangingIcicle(GameObject parent, Vector3 localPos)
        {
            GameObject obj = new GameObject("HangingIcicleStatic");
            obj.transform.parent = parent.transform;
            obj.transform.localPosition = localPos;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = icicleSprite;
            sr.color = new Color(0.75f, 0.9f, 1f, 0.85f);

            PolygonCollider2D col = obj.AddComponent<PolygonCollider2D>();
            col.isTrigger = true;

            HazardTrigger haz = obj.AddComponent<HazardTrigger>();
            haz.hazardName = "Ceiling Icicle";
            obj.tag = "Obstacle";
            return obj;
        }

        private GameObject CreateHazardSpikes(GameObject parent, Vector3 localPos, float width)
        {
            GameObject obj = new GameObject("SpikeHazard");
            obj.transform.parent = parent.transform;
            obj.transform.localPosition = localPos;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = spikeSprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(width, 1.5f);
            sr.color = new Color(0.7f, 0.85f, 1f, 1f);

            BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(width, 1.2f);
            col.isTrigger = true;

            HazardTrigger haz = obj.AddComponent<HazardTrigger>();
            haz.hazardName = "Glacial Spikes";
            obj.tag = "Obstacle";
            return obj;
        }

        private GameObject SpawnCrystal(GameObject parent, Vector3 localPos)
        {
            GameObject obj = new GameObject("Crystal");
            obj.transform.parent = parent.transform;
            obj.transform.localPosition = localPos;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = crystalSprite;
            sr.color = Color.white;
            sr.sortingOrder = 3;

            CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;
            col.isTrigger = true;

            obj.AddComponent<CrystalCollectible>();
            obj.tag = "Collectible";
            return obj;
        }

        private void SpawnCrystalArc(GameObject parent, Vector3 start, Vector3 end, int count, float arcHeight)
        {
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / (count - 1);
                Vector3 pos = Vector3.Lerp(start, end, t);
                pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
                SpawnCrystal(parent, pos);
            }
        }

        private GameObject SpawnPowerUp(GameObject parent, Vector3 localPos, PowerUpType type)
        {
            GameObject obj = new GameObject($"PowerUp_{type}");
            obj.transform.parent = parent.transform;
            obj.transform.localPosition = localPos;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = powerUpSprite;
            sr.color = (type == PowerUpType.WarmthShield) ? new Color(1f, 0.5f, 0.1f) : new Color(0.2f, 0.9f, 1f);
            sr.sortingOrder = 4;

            CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
            col.isTrigger = true;

            PowerUpItem item = obj.AddComponent<PowerUpItem>();
            item.powerUpType = type;
            item.duration = 8.0f;
            obj.tag = "PowerUp";
            return obj;
        }

        #endregion
    }

    /// <summary>
    /// Generic hazard trigger that deals lethal damage to player unless shielded.
    /// </summary>
    public class HazardTrigger : MonoBehaviour
    {
        public string hazardName = "Hazard";

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.HitHazard(hazardName);
            }
        }
    }
}
