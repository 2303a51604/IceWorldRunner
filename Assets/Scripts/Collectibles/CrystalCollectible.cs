using UnityEngine;

namespace IceWorldRunner
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class CrystalCollectible : MonoBehaviour
    {
        [Header("Animation")]
        public float bobSpeed = 3.5f;
        public float bobHeight = 0.25f;
        public float rotateSpeed = 60f;

        [Header("Magnetism")]
        public float magnetAttractSpeed = 16f;

        private Vector3 basePosition;
        private bool isCollected = false;
        private Transform magnetTarget;

        private void Start()
        {
            basePosition = transform.position;
            GetComponent<CircleCollider2D>().isTrigger = true;
        }

        private void Update()
        {
            if (isCollected) return;

            if (magnetTarget != null)
            {
                // Rapidly fly towards player
                transform.position = Vector3.MoveTowards(transform.position, magnetTarget.position, magnetAttractSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, magnetTarget.position) < 0.6f)
                {
                    Collect();
                }
            }
            else
            {
                // Idle floating bob and rotation
                float newY = basePosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                transform.position = new Vector3(basePosition.x, newY, basePosition.z);
                transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected) return;

            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                Collect();
            }
        }

        public void AttractTo(Transform target)
        {
            magnetTarget = target;
        }

        private void Collect()
        {
            if (isCollected) return;
            isCollected = true;

            SoundManager.Instance?.PlayCrystal();
            GameManager.Instance?.AddCrystal(1);

            Destroy(gameObject);
        }
    }
}
