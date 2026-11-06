using System.Collections;
using UnityEngine;

namespace IceWorldRunner
{
    public enum PowerUpType
    {
        WarmthShield, // Protects from 1 hit + melts falling icicles
        IceCrampons,  // 100% grip on ice platforms, no slippage
        CrystalMagnet,// Pulls all crystals within range
        SpeedSurge    // Temporary invulnerable high speed burst
    }

    [RequireComponent(typeof(CircleCollider2D))]
    public class PowerUpItem : MonoBehaviour
    {
        [Header("Power-Up Configuration")]
        public PowerUpType powerUpType = PowerUpType.WarmthShield;
        public float duration = 8.0f;

        [Header("Visual Bob")]
        public float bobSpeed = 4.0f;
        public float bobHeight = 0.3f;

        private Vector3 startPos;
        private bool isPickedUp = false;

        private void Start()
        {
            startPos = transform.position;
            GetComponent<CircleCollider2D>().isTrigger = true;
        }

        private void Update()
        {
            if (isPickedUp) return;

            float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(startPos.x, startPos.y + yOffset, startPos.z);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isPickedUp) return;

            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                ApplyPowerUp(player);
            }
        }

        private void ApplyPowerUp(PlayerController player)
        {
            isPickedUp = true;
            SoundManager.Instance?.PlayPowerUp();
            GameManager.Instance?.TriggerPowerUp(powerUpType, duration);

            switch (powerUpType)
            {
                case PowerUpType.WarmthShield:
                    player.EnableWarmthShield(duration);
                    break;
                case PowerUpType.IceCrampons:
                    player.EnableIceCrampons(duration);
                    break;
                case PowerUpType.CrystalMagnet:
                    player.StartCoroutine(MagnetRoutine(player.transform, duration));
                    break;
                case PowerUpType.SpeedSurge:
                    player.EnableWarmthShield(duration);
                    break;
            }

            Destroy(gameObject);
        }

        private static IEnumerator MagnetRoutine(Transform playerTrans, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (playerTrans == null) yield break;

                Collider2D[] hits = Physics2D.OverlapCircleAll(playerTrans.position, 12f);
                foreach (var hit in hits)
                {
                    CrystalCollectible crystal = hit.GetComponent<CrystalCollectible>();
                    if (crystal != null)
                    {
                        crystal.AttractTo(playerTrans);
                    }
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }
}
