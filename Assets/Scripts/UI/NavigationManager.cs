using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class NavigationManager : MonoBehaviour
    {
        public static NavigationManager Instance { get; private set; }

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

        private void Awake()
        {
             if (Instance == null) Instance = this;
        }

        private void Start()
        {
            if(playBtn) playBtn.onClick.AddListener(()=> ShowView(gameplayView));
            if(collectionBtn) collectionBtn.onClick.AddListener(()=> ShowView(collectionView));
            if(labBtn) labBtn.onClick.AddListener(()=> ShowView(labView));
            if(shopBtn) shopBtn.onClick.AddListener(()=> ShowView(shopView));
            if(leaderboardBtn) leaderboardBtn.onClick.AddListener(()=> ShowView(leaderboardView));
            
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
        }
    }
}