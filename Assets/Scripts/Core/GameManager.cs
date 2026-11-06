using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IceWorldRunner
{
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }

    /// <summary>
    /// Core Game Manager coordinating game loops, speed scaling, scoring, and state machine.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public const string HIGH_SCORE_KEY = "IceRunner_HighScore";
        public const string TOTAL_CRYSTALS_KEY = "IceRunner_TotalCrystals";

        [Header("State")]
        [SerializeField] private GameState currentState = GameState.Playing;
        public GameState CurrentState => currentState;

        [Header("Runner Speed Scaling")]
        public float baseRunSpeed = 9.0f;
        public float maxRunSpeed = 22.0f;
        public float speedAccelerationPer100m = 0.35f;

        [Header("Current Run Stats")]
        public float currentSpeed;
        public float distanceTraveled;
        public int crystalsCollected;
        public int currentScore;
        public int highScore;

        [Header("Player Reference")]
        public Transform playerTransform;
        private float startX;

        // Events
        public event Action<GameState> OnStateChanged;
        public event Action<int> OnScoreUpdated;
        public event Action<int> OnCrystalsUpdated;
        public event Action<float> OnDistanceUpdated;
        public event Action<PowerUpType, float> OnPowerUpTriggered;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
            currentSpeed = baseRunSpeed;
        }

        private void Start()
        {
            if (playerTransform != null)
            {
                startX = playerTransform.position.x;
            }
            SetState(GameState.Playing);
        }

        private void Update()
        {
            if (currentState == GameState.Playing)
            {
                UpdateDistanceAndSpeed();

                // Quick restart shortcut
                if (InputHelper.GetRestartDown())
                {
                    RestartGame();
                }

                // Pause toggle
                if (InputHelper.GetPauseDown())
                {
                    TogglePause();
                }
            }
            else if (currentState == GameState.GameOver)
            {
                if (InputHelper.GetRestartDown())
                {
                    RestartGame();
                }
            }
        }

        private void UpdateDistanceAndSpeed()
        {
            if (playerTransform == null) return;

            distanceTraveled = Mathf.Max(0, playerTransform.position.x - startX);
            OnDistanceUpdated?.Invoke(distanceTraveled);

            // Progressive speed scaling
            float extraSpeed = (distanceTraveled / 100f) * speedAccelerationPer100m;
            currentSpeed = Mathf.Min(baseRunSpeed + extraSpeed, maxRunSpeed);

            // Total Score = distance in meters + crystals * 25
            currentScore = Mathf.FloorToInt(distanceTraveled) + (crystalsCollected * 25);
            OnScoreUpdated?.Invoke(currentScore);

            if (currentScore > highScore)
            {
                highScore = currentScore;
                PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
            }
        }

        public void AddCrystal(int amount = 1)
        {
            crystalsCollected += amount;
            int total = PlayerPrefs.GetInt(TOTAL_CRYSTALS_KEY, 0) + amount;
            PlayerPrefs.SetInt(TOTAL_CRYSTALS_KEY, total);

            OnCrystalsUpdated?.Invoke(crystalsCollected);
        }

        public void TriggerPowerUp(PowerUpType type, float duration)
        {
            OnPowerUpTriggered?.Invoke(type, duration);
        }

        public void GameOver()
        {
            if (currentState == GameState.GameOver) return;

            SetState(GameState.GameOver);
            SoundManager.Instance?.PlayShatter();
            PlayerPrefs.Save();
        }

        public void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                SetState(GameState.Paused);
                Time.timeScale = 0f;
            }
            else if (currentState == GameState.Paused)
            {
                SetState(GameState.Playing);
                Time.timeScale = 1f;
            }
        }

        public void SetState(GameState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(currentState);
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
