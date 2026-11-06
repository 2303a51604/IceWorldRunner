using System.Collections;
using UnityEngine;

namespace IceWorldRunner
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float jumpForce = 15.0f;
        public float doubleJumpForce = 13.0f;
        public float slideBoostForce = 3.5f;
        public float gravityScale = 3.2f;
        public float fallMultiplier = 1.6f;

        [Header("Ice Physics")]
        public float iceFriction = 0.05f;
        public float normalFriction = 0.5f;
        public bool hasIceCrampons = false;

        [Header("Slide Settings")]
        public float slideDuration = 0.75f;
        private bool isSliding = false;
        private float slideTimer = 0f;

        [Header("State Flags")]
        public bool isGrounded { get; private set; }
        public bool isOnIce { get; private set; }
        public bool canDoubleJump { get; private set; }
        public bool isAlive { get; private set; } = true;
        public bool hasWarmthShield { get; private set; } = false;

        [Header("Visuals & Sprites")]
        public SpriteRenderer spriteRenderer;
        public Sprite standingSprite;
        public Sprite slidingSprite;

        // Internal References
        private Rigidbody2D rb;
        private BoxCollider2D boxCollider;
        private Vector2 originalColliderSize;
        private Vector2 originalColliderOffset;

        // Timers for tight platformer feel
        private float coyoteTime = 0.15f;
        private float coyoteTimer = 0f;
        private float jumpBufferTime = 0.15f;
        private float jumpBufferTimer = 0f;

        // Player effects helper
        private PlayerEffects playerEffects;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();
            playerEffects = GetComponent<PlayerEffects>();

            originalColliderSize = boxCollider.size;
            originalColliderOffset = boxCollider.offset;

            rb.gravityScale = gravityScale;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;
        }

        private void Update()
        {
            if (!isAlive) return;
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

            CheckGrounded();
            HandleInput();
            HandleSlide();
            CheckAbyssDeath();
        }

        private void FixedUpdate()
        {
            if (!isAlive) return;
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

            ApplyMovementPhysics();
            ApplyBetterJumpingPhysics();
        }

        private void CheckGrounded()
        {
            Bounds b = boxCollider.bounds;
            Vector2 footCheckPos = new Vector2(b.center.x, b.min.y);
            Vector2 boxSize = new Vector2(b.size.x * 0.85f, 0.22f);

            // Check non-trigger colliders below feet (excluding player's own collider)
            Collider2D[] hits = Physics2D.OverlapBoxAll(footCheckPos, boxSize, 0f);
            Collider2D groundCol = null;
            foreach (var h in hits)
            {
                if (h != null && h != boxCollider && !h.isTrigger)
                {
                    groundCol = h;
                    break;
                }
            }

            bool wasGrounded = isGrounded;
            isGrounded = groundCol != null;

            if (isGrounded)
            {
                coyoteTimer = coyoteTime;
                canDoubleJump = true;

                isOnIce = groundCol.CompareTag("Ice") || groundCol.GetComponent<IcePlatform>() != null || groundCol.name.Contains("Platform");

                if (!wasGrounded)
                {
                    playerEffects?.PlayLandDust();
                }
            }
            else
            {
                coyoteTimer -= Time.deltaTime;
                isOnIce = false;
            }
        }

        private void HandleInput()
        {
            // Jump Buffer via InputHelper (works with both New Input System & Legacy Space/W/Up)
            if (InputHelper.GetJumpDown())
            {
                jumpBufferTimer = jumpBufferTime;
            }
            else
            {
                jumpBufferTimer -= Time.deltaTime;
            }

            // Execute Jump
            if (jumpBufferTimer > 0f)
            {
                if (coyoteTimer > 0f)
                {
                    ExecuteJump(jumpForce);
                    jumpBufferTimer = 0f;
                    coyoteTimer = 0f;
                    SoundManager.Instance?.PlayJump();
                    playerEffects?.PlayJumpDust();
                }
                else if (canDoubleJump)
                {
                    ExecuteJump(doubleJumpForce);
                    canDoubleJump = false;
                    jumpBufferTimer = 0f;
                    SoundManager.Instance?.PlayDoubleJump();
                    playerEffects?.PlayDoubleJumpBurst();
                }
            }

            // Slide trigger (Down arrow / S / Left Ctrl / Gamepad B)
            if (InputHelper.GetSlideDown() && !isSliding)
            {
                StartSlide();
            }
        }

        private void ExecuteJump(float force)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
            if (isSliding)
            {
                StopSlide();
            }
        }

        private void ApplyMovementPhysics()
        {
            float targetSpeed = GameManager.Instance != null ? GameManager.Instance.currentSpeed : 10f;

            // Extra forward momentum boost while sliding on icy slopes
            if (isSliding && isOnIce)
            {
                targetSpeed += slideBoostForce;
            }

            // Apply ice friction vs grip
            float currentVx = rb.linearVelocity.x;
            float accelerationRate = (isOnIce && !hasIceCrampons) ? 8f : 30f;

            // Horizontal steering adjustment
            float inputX = InputHelper.GetHorizontal();
            if (inputX > 0.1f) targetSpeed *= 1.2f;
            else if (inputX < -0.1f) targetSpeed *= 0.8f;

            float newVx = Mathf.MoveTowards(currentVx, targetSpeed, accelerationRate * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(newVx, rb.linearVelocity.y);
        }

        private void ApplyBetterJumpingPhysics()
        {
            // Fall faster when falling down for snappy platforming
            if (rb.linearVelocity.y < 0)
            {
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
            }
            // Low jump when releasing jump button early
            else if (rb.linearVelocity.y > 0 && !InputHelper.GetJumpHeld())
            {
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier * 0.75f) * Time.fixedDeltaTime;
            }
        }

        private void StartSlide()
        {
            isSliding = true;
            slideTimer = slideDuration;

            // Reduce collider to half height so player can duck under low overhangs
            boxCollider.size = new Vector2(originalColliderSize.x * 1.3f, originalColliderSize.y * 0.45f);
            boxCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - (originalColliderSize.y * 0.25f));

            if (slidingSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = slidingSprite;
            }

            SoundManager.Instance?.StartSlide();
            playerEffects?.SetSlideEffect(true);
        }

        private void HandleSlide()
        {
            if (!isSliding) return;

            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0f)
            {
                StopSlide();
            }
        }

        public void StopSlide()
        {
            isSliding = false;
            boxCollider.size = originalColliderSize;
            boxCollider.offset = originalColliderOffset;

            if (standingSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = standingSprite;
            }

            SoundManager.Instance?.StopSlide();
            playerEffects?.SetSlideEffect(false);
        }

        public void ApplyBouncePad(float bounceForce)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
            canDoubleJump = true;
            SoundManager.Instance?.PlayBounce();
            playerEffects?.PlayDoubleJumpBurst();
            CameraController.Instance?.Shake(0.15f, 0.15f);
        }

        public void EnableWarmthShield(float duration)
        {
            StartCoroutine(WarmthShieldRoutine(duration));
        }

        private IEnumerator WarmthShieldRoutine(float duration)
        {
            hasWarmthShield = true;
            playerEffects?.SetShieldGlow(true);
            yield return new WaitForSeconds(duration);
            hasWarmthShield = false;
            playerEffects?.SetShieldGlow(false);
        }

        public void EnableIceCrampons(float duration)
        {
            StartCoroutine(IceCramponsRoutine(duration));
        }

        private IEnumerator IceCramponsRoutine(float duration)
        {
            hasIceCrampons = true;
            yield return new WaitForSeconds(duration);
            hasIceCrampons = false;
        }

        public void HitHazard(string hazardName = "Hazard")
        {
            if (!isAlive) return;

            if (hasWarmthShield)
            {
                hasWarmthShield = false;
                playerEffects?.SetShieldGlow(false);
                playerEffects?.PlayShieldBreakBurst();
                CameraController.Instance?.Shake(0.2f, 0.3f);
                SoundManager.Instance?.PlayShatter();
                return;
            }

            Die();
        }

        public void Die()
        {
            if (!isAlive) return;
            isAlive = false;

            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            boxCollider.enabled = false;

            if (spriteRenderer != null) spriteRenderer.enabled = false;

            playerEffects?.PlayDeathShatter();
            CameraController.Instance?.Shake(0.35f, 0.45f);
            GameManager.Instance?.GameOver();
        }

        private void CheckAbyssDeath()
        {
            if (transform.position.y < -9.0f)
            {
                Die();
            }
        }
    }
}
