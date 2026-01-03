using UnityEngine;
using TMPro;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        
        [Header("Top Bar")]
        public TextMeshProUGUI dnaText;
        public TextMeshProUGUI diamondText;
        public TextMeshProUGUI heartText;
        public TextMeshProUGUI scoreText; 
        public TextMeshProUGUI topHighScoreText; // Added for In-Game High Score
        public TextMeshProUGUI heartTimerText;

        [Header("Test Buttons")]
        public UnityEngine.UI.Button testDnaButton;
        public UnityEngine.UI.Button testDiamondButton;

        [Header("Gameplay Controls")]
        public UnityEngine.UI.Button undoButton; // Added for Undo
        public TextMeshProUGUI undoCountText; // Added for Undo Count Display
        public UnityEngine.UI.Button restartButton; // Added for Restart

        [Header("Game Over")]
        public GameObject gameOverPanel;
        public TextMeshProUGUI finalScoreText;
        public TextMeshProUGUI highScoreText;
        public TextMeshProUGUI earnedDnaText;
        public TextMeshProUGUI totalDnaText;
        public UnityEngine.UI.Button tryAgainButton;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            if (tryAgainButton) tryAgainButton.onClick.AddListener(OnTryAgain);
            if (undoButton) undoButton.onClick.AddListener(OnUndo);
            if (restartButton) restartButton.onClick.AddListener(OnRestart);
            if (testDnaButton) testDnaButton.onClick.AddListener(() => {
                if (Systems.DNASystem.Instance) Systems.DNASystem.Instance.AddDNA(100000);
            });
            if (testDiamondButton) testDiamondButton.onClick.AddListener(() => {
                if (Systems.DiamondSystem.Instance) Systems.DiamondSystem.Instance.AddDiamonds(100);
            });
            if (gameOverPanel) gameOverPanel.SetActive(false);
        }

        public void UpdateScore(int score) 
        {
            if (scoreText) scoreText.text = $"Current Score: {score}";
            // Check High Score Live
            int best = PlayerPrefs.GetInt("HighScore", 0);
            if(score > best) UpdateHighScore(score);
        }

        public void UpdateHighScore(int best)
        {
            if (topHighScoreText) topHighScoreText.text = $"High Score: {best}";
        }

        public void ShowGameOver(int score, int highScore, int earnedDna, int totalDna)
        {
            if(gameOverPanel)
            {
                gameOverPanel.SetActive(true);
                if(finalScoreText) finalScoreText.text = $"Score: {score}";
                if(highScoreText) highScoreText.text = $"High Score: {highScore}";
                if(earnedDnaText) earnedDnaText.text = $"Earned DNA: +{earnedDna}";
                if(totalDnaText) totalDnaText.text = $"Total DNA: {totalDna}";
            }
        }

        private void OnTryAgain()
        {
             // Heart already consumed on game over, just reload scene
             UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        private void OnUndo()
        {
             if(Core.BoardManager.Instance) Core.BoardManager.Instance.Undo();
        }

        private void OnRestart()
        {
             if(Core.BoardManager.Instance) Core.BoardManager.Instance.RestartGame();
        }

        private void Start()
        {
            if (Systems.DNASystem.Instance)
            {
                UpdateDNA(Systems.DNASystem.Instance.TotalDNA);
                Systems.DNASystem.Instance.OnDNAChanged += UpdateDNA;
            }
            
            if (Systems.DiamondSystem.Instance)
            {
                UpdateDiamonds(Systems.DiamondSystem.Instance.TotalDiamonds);
                Systems.DiamondSystem.Instance.OnDiamondChanged += UpdateDiamonds;
            }
            
            if (Systems.HeartSystem.Instance)
            {
                UpdateHearts();
                Systems.HeartSystem.Instance.OnHeartChanged += UpdateHearts;
            }
            
            UpdateScore(0); // Initialize Score Text
            UpdateHighScore(PlayerPrefs.GetInt("HighScore", 0)); // Initialize High Score
        }

        private void UpdateDNA(int amount)
        {
            if (dnaText) dnaText.text = $"{amount}";
        }

        private void UpdateDiamonds(int amount)
        {
            if (diamondText) diamondText.text = $"{amount}";
        }

        private void UpdateHearts()
        {
            if (heartText && Systems.HeartSystem.Instance)
                heartText.text = $"{Systems.HeartSystem.Instance.CurrentHearts}/{Systems.HeartSystem.MAX_HEARTS}";
        }

        private void Update()
        {
            if (Systems.HeartSystem.Instance && heartTimerText)
            {
                var time = Systems.HeartSystem.Instance.GetTimeRemaining();
                if (time.TotalSeconds > 0)
                    heartTimerText.text = $"{time.Minutes:D2}:{time.Seconds:D2}";
                else
                    heartTimerText.text = "";
            }
        }
        public void UpdateUndoCount(int remaining, int total = 0)
        {
            // Hide undo UI completely when no undos available (upgrade not purchased)
            bool hasUndoUpgrade = total > 0;
            
            if (undoButton) undoButton.gameObject.SetActive(hasUndoUpgrade);
            if (undoCountText) 
            {
                undoCountText.gameObject.SetActive(hasUndoUpgrade);
                if (hasUndoUpgrade) undoCountText.text = $"{remaining}/{total}";
            }
        }
    }
}