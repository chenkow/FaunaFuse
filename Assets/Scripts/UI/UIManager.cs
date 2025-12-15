using UnityEngine;
using TMPro;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        
        [Header("Top Bar")]
        public TextMeshProUGUI dnaText;
        public TextMeshProUGUI heartText;
        public TextMeshProUGUI heartTimerText;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            if (Systems.DNASystem.Instance)
            {
                UpdateDNA(Systems.DNASystem.Instance.TotalDNA);
                Systems.DNASystem.Instance.OnDNAChanged += UpdateDNA;
            }
            
            if (Systems.HeartSystem.Instance)
            {
                UpdateHearts();
                Systems.HeartSystem.Instance.OnHeartChanged += UpdateHearts;
            }
        }

        private void UpdateDNA(int amount)
        {
            if (dnaText) dnaText.text = amount.ToString();
        }

        private void UpdateHearts()
        {
            if (heartText && Systems.HeartSystem.Instance)
                heartText.text = Systems.HeartSystem.Instance.CurrentHearts.ToString();
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
    }
}