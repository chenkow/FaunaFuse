using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class NavigationManager : MonoBehaviour
    {
        public static NavigationManager Instance { get; private set; }
        
        private int currentButtonIndex = 2; // Default: Play (middle)

        [Header("Views")]
        public GameObject gameplayView;
        public GameObject collectionView;
        public GameObject labView;
        public GameObject shopView;
        public GameObject leaderboardView;

        [Header("Buttons")]
        public Button playBtn;
        public Button collectionBtn;
        public Button labBtn;
        public Button shopBtn;
        public Button leaderboardBtn;

        [Header("Nav Button Controllers")]
        public NavButtonController playNavBtn;
        public NavButtonController collectionNavBtn;
        public NavButtonController labNavBtn;
        public NavButtonController shopNavBtn;
        public NavButtonController leaderboardNavBtn;

        [Header("World Objects")]
        public GameObject mainBoard;
        public GameObject scoreBackground; // Score UI (only visible in Play)
        public GameObject undoContainer; // Undo UI (only visible in Play)

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            if(playBtn) playBtn.onClick.AddListener(() => ShowView(gameplayView));
            if(collectionBtn) collectionBtn.onClick.AddListener(() => ShowView(collectionView));
            if(labBtn) labBtn.onClick.AddListener(() => ShowView(labView));
            if(shopBtn) shopBtn.onClick.AddListener(() => ShowView(shopView));
            if(leaderboardBtn) leaderboardBtn.onClick.AddListener(() => ShowView(leaderboardView));
            
            // Default show gameplay
            if(gameplayView) ShowView(gameplayView);
        }

        public void ShowView(GameObject viewToShow)
        {
            if(gameplayView) gameplayView.SetActive(false);
            if(collectionView) collectionView.SetActive(false);
            if(labView) labView.SetActive(false);
            if(shopView) shopView.SetActive(false);
            if(leaderboardView) leaderboardView.SetActive(false);
            
            if(viewToShow) viewToShow.SetActive(true);

            // Special handling for Board
            if(mainBoard) mainBoard.SetActive(viewToShow == gameplayView);
            
            // Score Background only visible in Play
            if(scoreBackground) scoreBackground.SetActive(viewToShow == gameplayView);
            
            // Undo Container only visible in Play
            if(undoContainer) undoContainer.SetActive(viewToShow == gameplayView);
            
            // Calculate button indices and direction
            int newIndex = GetButtonIndex(viewToShow);
            int direction = newIndex - currentButtonIndex;
            
            // Update navigation button visuals with slide direction
            playNavBtn?.SetActive(viewToShow == gameplayView, direction);
            collectionNavBtn?.SetActive(viewToShow == collectionView, direction);
            labNavBtn?.SetActive(viewToShow == labView, direction);
            shopNavBtn?.SetActive(viewToShow == shopView, direction);
            leaderboardNavBtn?.SetActive(viewToShow == leaderboardView, direction);
            
            currentButtonIndex = newIndex;

            // Toggle Input
            if(Core.BoardManager.Instance) 
            {
                Core.BoardManager.Instance.IsInputActive = (viewToShow == gameplayView);
            }
        }
        
        private int GetButtonIndex(GameObject view)
        {
            // Button order: Leaderboard(0) - Collection(1) - Play(2) - Lab(3) - Shop(4)
            if (view == leaderboardView) return 0;
            if (view == collectionView) return 1;
            if (view == gameplayView) return 2;
            if (view == labView) return 3;
            if (view == shopView) return 4;
            return 2; // Default to Play
        }
    }
}