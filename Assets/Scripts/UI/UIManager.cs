using System.Collections;
using UnityEngine;

namespace IceWorldRunner
{
    /// <summary>
    /// Comprehensive frost-themed UI system for Ice World Runner.
    /// Manages HUD stats, active power-up timers, game over screen, and instructions.
    /// 100% self-contained with no external UI package dependencies.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private float currentDistance;
        private int currentScore;
        private int currentCrystals;
        private string activePowerUpName = "";
        private float powerUpTimeRemaining = 0f;
        private bool isNewRecord = false;

        private GUIStyle titleStyle;
        private GUIStyle hudStyle;
        private GUIStyle alertStyle;
        private GUIStyle cardStyle;
        private GUIStyle buttonStyle;
        private Texture2D cardBackground;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreUpdated += UpdateScore;
                GameManager.Instance.OnDistanceUpdated += UpdateDistance;
                GameManager.Instance.OnCrystalsUpdated += UpdateCrystals;
                GameManager.Instance.OnStateChanged += HandleStateChanged;
                GameManager.Instance.OnPowerUpTriggered += HandlePowerUp;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreUpdated -= UpdateScore;
                GameManager.Instance.OnDistanceUpdated -= UpdateDistance;
                GameManager.Instance.OnCrystalsUpdated -= UpdateCrystals;
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
                GameManager.Instance.OnPowerUpTriggered -= HandlePowerUp;
            }
        }

        private void Update()
        {
            if (powerUpTimeRemaining > 0f)
            {
                powerUpTimeRemaining -= Time.deltaTime;
                if (powerUpTimeRemaining <= 0f)
                {
                    activePowerUpName = "";
                }
            }
        }

        private void UpdateScore(int score)
        {
            currentScore = score;
        }

        private void UpdateDistance(float distance)
        {
            currentDistance = distance;
        }

        private void UpdateCrystals(int crystals)
        {
            currentCrystals = crystals;
        }

        private void HandlePowerUp(PowerUpType type, float duration)
        {
            activePowerUpName = type.ToString();
            powerUpTimeRemaining = duration;
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                isNewRecord = currentScore >= GameManager.Instance.highScore && currentScore > 0;
            }
        }

        private void InitGUIStyles()
        {
            if (titleStyle == null)
            {
                cardBackground = new Texture2D(1, 1);
                cardBackground.SetPixel(0, 0, new Color(0.08f, 0.16f, 0.28f, 0.90f));
                cardBackground.Apply();

                titleStyle = new GUIStyle();
                titleStyle.fontSize = 28;
                titleStyle.fontStyle = FontStyle.Bold;
                titleStyle.normal.textColor = new Color(0.9f, 0.96f, 1f);
                titleStyle.alignment = TextAnchor.MiddleCenter;

                hudStyle = new GUIStyle();
                hudStyle.fontSize = 20;
                hudStyle.fontStyle = FontStyle.Bold;
                hudStyle.normal.textColor = new Color(0.85f, 0.95f, 1f);

                alertStyle = new GUIStyle();
                alertStyle.fontSize = 22;
                alertStyle.fontStyle = FontStyle.Bold;
                alertStyle.normal.textColor = new Color(1f, 0.8f, 0.2f);
                alertStyle.alignment = TextAnchor.MiddleCenter;

                cardStyle = new GUIStyle();
                cardStyle.normal.background = cardBackground;
                cardStyle.padding = new RectOffset(20, 20, 20, 20);

                buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.fontSize = 18;
                buttonStyle.fontStyle = FontStyle.Bold;
                buttonStyle.normal.textColor = Color.white;
            }
        }

        private void OnGUI()
        {
            InitGUIStyles();

            GameState state = GameManager.Instance != null ? GameManager.Instance.CurrentState : GameState.Playing;

            if (state == GameState.Playing || state == GameState.Paused)
            {
                DrawHUD();
                DrawControlsReminder();
            }

            if (state == GameState.GameOver)
            {
                DrawGameOverScreen();
            }
            else if (state == GameState.Paused)
            {
                DrawPauseScreen();
            }
        }

        private void DrawHUD()
        {
            // Top Left HUD Card
            GUILayout.BeginArea(new Rect(25, 20, 320, 130), cardStyle);
            GUILayout.Label("❄ ICE WORLD RUNNER", hudStyle);
            GUILayout.Label("DISTANCE:  " + Mathf.FloorToInt(currentDistance) + " m", hudStyle);
            GUILayout.Label("CRYSTALS:  " + currentCrystals, hudStyle);
            GUILayout.Label("SCORE:     " + currentScore, hudStyle);
            GUILayout.EndArea();

            // Active Power-up alert
            if (powerUpTimeRemaining > 0f)
            {
                GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 150, 20, 300, 60), cardStyle);
                GUILayout.Label("⚡ " + activePowerUpName + " (" + powerUpTimeRemaining.ToString("F1") + "s)", alertStyle);
                GUILayout.EndArea();
            }
        }

        private void DrawControlsReminder()
        {
            if (currentDistance < 80f)
            {
                GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 240, Screen.height - 80, 480, 50), cardStyle);
                GUIStyle tip = new GUIStyle(hudStyle);
                tip.fontSize = 15;
                tip.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label("SPACE: Jump / Double Jump  |  S / CTRL: Slide  |  A/D: Steer", tip);
                GUILayout.EndArea();
            }
        }

        private void DrawGameOverScreen()
        {
            float width = 420;
            float height = 340;
            Rect centerRect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);

            GUILayout.BeginArea(centerRect, cardStyle);
            GUILayout.Space(10);
            GUILayout.Label("FROZEN OVER!", titleStyle);
            GUILayout.Space(10);

            if (isNewRecord)
            {
                GUILayout.Label("★ NEW BEST RECORD! ★", alertStyle);
            }
            else
            {
                int best = GameManager.Instance != null ? GameManager.Instance.highScore : 0;
                GUILayout.Label("BEST SCORE: " + best, alertStyle);
            }

            GUILayout.Space(10);
            GUILayout.Label("Distance Reached: " + Mathf.FloorToInt(currentDistance) + " m", hudStyle);
            GUILayout.Label("Crystals Gathered: " + currentCrystals, hudStyle);
            GUILayout.Label("Total Score: " + currentScore, hudStyle);

            GUILayout.Space(25);
            if (GUILayout.Button("PLAY AGAIN (Press R)", buttonStyle, GUILayout.Height(45)))
            {
                GameManager.Instance?.RestartGame();
            }
            GUILayout.EndArea();
        }

        private void DrawPauseScreen()
        {
            float width = 340;
            float height = 220;
            Rect centerRect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);

            GUILayout.BeginArea(centerRect, cardStyle);
            GUILayout.Space(10);
            GUILayout.Label("GAME PAUSED", titleStyle);
            GUILayout.Space(20);

            if (GUILayout.Button("RESUME (Esc)", buttonStyle, GUILayout.Height(40)))
            {
                GameManager.Instance?.TogglePause();
            }
            GUILayout.Space(10);
            if (GUILayout.Button("RESTART RUN", buttonStyle, GUILayout.Height(40)))
            {
                GameManager.Instance?.RestartGame();
            }
            GUILayout.EndArea();
        }
    }
}
