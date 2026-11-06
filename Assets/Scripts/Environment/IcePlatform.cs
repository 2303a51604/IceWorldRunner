using UnityEngine;

namespace IceWorldRunner
{
    public enum PlatformType
    {
        SlickIce,
        StandardSnow,
        BouncySnowdrift
    }

    [RequireComponent(typeof(BoxCollider2D))]
    public class IcePlatform : MonoBehaviour
    {
        [Header("Platform Config")]
        public PlatformType platformType = PlatformType.SlickIce;
        public float bounceForce = 18f;

        private BoxCollider2D boxCol;

        private void Awake()
        {
            boxCol = GetComponent<BoxCollider2D>();
            ApplyPhysicsProperties();
        }

        private void ApplyPhysicsProperties()
        {
            PhysicsMaterial2D mat = new PhysicsMaterial2D(name + "_Mat");
            switch (platformType)
            {
                case PlatformType.SlickIce:
                    mat.friction = 0.02f;
                    mat.bounciness = 0.05f;
                    tag = "Ice";
                    break;
                case PlatformType.StandardSnow:
                    mat.friction = 0.6f;
                    mat.bounciness = 0.0f;
                    tag = "Ground";
                    break;
                case PlatformType.BouncySnowdrift:
                    mat.friction = 0.2f;
                    mat.bounciness = 0.8f;
                    tag = "BouncySnow";
                    break;
            }
            boxCol.sharedMaterial = mat;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (platformType == PlatformType.BouncySnowdrift)
            {
                PlayerController player = collision.gameObject.GetComponent<PlayerController>();
                if (player != null && collision.contacts[0].normal.y < -0.5f)
                {
                    player.ApplyBouncePad(bounceForce);
                }
            }
        }
    }
}
