using UnityEngine;
using TMPro;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        
        [Header("Top Bar")]
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI scoreUnderlayText; // Underlay text for score shadow effect
        public TextMeshProUGUI topHighScoreText; // Added for In-Game High Score
        
        [Header("Resource Stack Texts")]
        public TextMeshProUGUI diamondStackText;
        public TextMeshProUGUI diamondStackUnderlayText;
        public TextMeshProUGUI dnaStackText;
        public TextMeshProUGUI dnaStackUnderlayText;
        public TextMeshProUGUI heartStackText;
        public TextMeshProUGUI heartStackUnderlayText;
        
        [Header("Profile Overlay")]
        public TextMeshProUGUI bestScoreStackText;
        public TextMeshProUGUI bestScoreStackUnderlayText;

        [Header("Test Buttons")]
        public UnityEngine.UI.Button testDnaButton;
        public UnityEngine.UI.Button testDiamondButton;

        [Header("Gameplay Controls")]
        public UnityEngine.UI.Button undoButton; // Added for Undo
        public TextMeshProUGUI undoCountText; // Added for Undo Count Display
        public TextMeshProUGUI undoCountUnderlayText; // Underlay text for undo shadow effect
        public UnityEngine.UI.Button restartButton; // Added for Restart

        [Header("Game Over")]
        public GameObject gameOverPanel;
        public TextMeshProUGUI scoreValueText;
        public TextMeshProUGUI scoreValueTextUnderlay;
        public TextMeshProUGUI bestScoreValueText;
        public TextMeshProUGUI bestScoreValueTextUnderlay;
        public TextMeshProUGUI dnaEarnedValueText;
        public TextMeshProUGUI dnaEarnedValueTextUnderlay;
        public TextMeshProUGUI totalDnaValueText;
        public TextMeshProUGUI totalDnaValueTextUnderlay;
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
            string scoreString = $"Score: {score}";
            if (scoreText) scoreText.text = scoreString;
            if (scoreUnderlayText) scoreUnderlayText.text = scoreString;
            // Check High Score Live
            int best = PlayerPrefs.GetInt("HighScore", 0);
            if(score > best) UpdateHighScore(score);
        }

        public void UpdateHighScore(int best)
        {
            string bestScoreString = $"Best: {best}";
            if (topHighScoreText) topHighScoreText.text = bestScoreString;
            if (bestScoreStackText) bestScoreStackText.text = bestScoreString;
            if (bestScoreStackUnderlayText) bestScoreStackUnderlayText.text = bestScoreString;
        }

        public void ShowGameOver(int score, int highScore, int earnedDna, int totalDna)
        {
            if(gameOverPanel)
            {
                gameOverPanel.SetActive(true);
                
                string scoreVal = score.ToString("N0");
                if(scoreValueText) scoreValueText.text = scoreVal;
                if(scoreValueTextUnderlay) scoreValueTextUnderlay.text = scoreVal;
                
                string bestVal = highScore.ToString("N0");
                if(bestScoreValueText) bestScoreValueText.text = bestVal;
                if(bestScoreValueTextUnderlay) bestScoreValueTextUnderlay.text = bestVal;
                
                string dnaVal = earnedDna.ToString("N0");
                if(dnaEarnedValueText) dnaEarnedValueText.text = dnaVal;
                if(dnaEarnedValueTextUnderlay) dnaEarnedValueTextUnderlay.text = dnaVal;
                
                string totalVal = totalDna.ToString("N0");
                if(totalDnaValueText) totalDnaValueText.text = totalVal;
                if(totalDnaValueTextUnderlay) totalDnaValueTextUnderlay.text = totalVal;
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
            string dnaValue = $"{amount}";
            if (dnaStackText) dnaStackText.text = dnaValue;
            if (dnaStackUnderlayText) dnaStackUnderlayText.text = dnaValue;
        }

        private void UpdateDiamonds(int amount)
        {
            string diamondValue = $"{amount}";
            if (diamondStackText) diamondStackText.text = diamondValue;
            if (diamondStackUnderlayText) diamondStackUnderlayText.text = diamondValue;
        }

        private void UpdateHearts()
        {
            UpdateHeartDisplay();
        }

        private void UpdateHeartDisplay()
        {
            if (Systems.HeartSystem.Instance == null) return;
            
            string heartValue;
            int currentHearts = Systems.HeartSystem.Instance.CurrentHearts;
            
            if (currentHearts > 0)
            {
                // Show heart count: "X/5"
                heartValue = $"{currentHearts}/{Systems.HeartSystem.MAX_HEARTS}";
            }
            else
            {
                // Show timer when no hearts left
                var time = Systems.HeartSystem.Instance.GetTimeRemaining();
                if (time.TotalSeconds > 0)
                    heartValue = $"{time.Minutes:D2}:{time.Seconds:D2}";
                else
                    heartValue = $"0/{Systems.HeartSystem.MAX_HEARTS}";
            }
            
            if (heartStackText) heartStackText.text = heartValue;
            if (heartStackUnderlayText) heartStackUnderlayText.text = heartValue;
        }

        private void Update()
        {
            // Update heart display every frame when hearts are depleted (for timer)
            if (Systems.HeartSystem.Instance != null && Systems.HeartSystem.Instance.CurrentHearts == 0)
            {
                UpdateHeartDisplay();
            }
        }
        public void UpdateUndoCount(int remaining, int total = 0)
        {
            // Hide undo UI completely when no undos available (upgrade not purchased)
            bool hasUndoUpgrade = total > 0;
            string undoText = $"{remaining}/{total}";
            
            if (undoButton) undoButton.gameObject.SetActive(hasUndoUpgrade);
            if (undoCountText) 
            {
                undoCountText.gameObject.SetActive(hasUndoUpgrade);
                if (hasUndoUpgrade) undoCountText.text = undoText;
            }
            if (undoCountUnderlayText)
            {
                undoCountUnderlayText.gameObject.SetActive(hasUndoUpgrade);
                if (hasUndoUpgrade) undoCountUnderlayText.text = undoText;
            }
        }
    }
}