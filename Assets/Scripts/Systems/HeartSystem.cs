using UnityEngine;
using System;

namespace Systems
{
    public class HeartSystem : MonoBehaviour
    {
        public static HeartSystem Instance { get; private set; }

        public const int MAX_HEARTS = 5;
        public const float BASE_REFILL_TIME_MINUTES = 30f;
        private const string PREF_HEARTS = "Hearts_Count";
        private const string PREF_NEXT_REFILL = "Hearts_NextRefill";

        public int CurrentHearts { get; private set; }
        public DateTime NextRefillTime { get; private set; }
        
        /// <summary>
        /// Gets the current refill time in minutes, accounting for Heart Recovery upgrade.
        /// Formula: 30 - upgradeLevel (minimum 5 minutes)
        /// </summary>
        public float GetRefillTimeMinutes()
        {
            int upgradeLevel = 0;
            if (LabSystem.Instance != null)
            {
                upgradeLevel = LabSystem.Instance.GetUpgradeLevel(Core.UpgradeType.HeartRefill);
            }
            // Formula: 30 - level, minimum 5 minutes
            float refillTime = BASE_REFILL_TIME_MINUTES - upgradeLevel;
            return Mathf.Max(refillTime, 5f);
        }
        
        public event Action OnHeartChanged;

        private void Awake()
        {
            if (Instance == null) 
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else 
            {
                Destroy(gameObject);
                return;
            }
            
            LoadHearts();
        }

        private void Update()
        {
            if (CurrentHearts < MAX_HEARTS)
            {
                if (DateTime.Now >= NextRefillTime)
                {
                    AddHearts(1);
                    if (CurrentHearts < MAX_HEARTS)
                    {
                        // If we added a heart but still not full, reset timer relative to when it SHOULD have finished, 
                        // or just now for simplicity in drift prevention? 
                        // Use dynamic refill time based on upgrade level
                        NextRefillTime = NextRefillTime.AddMinutes(GetRefillTimeMinutes());
                        if (NextRefillTime < DateTime.Now) NextRefillTime = DateTime.Now.AddMinutes(GetRefillTimeMinutes()); // Catchup safety
                        SaveHearts();
                    }
                }
            }
        }

        public bool UseHeart()
        {
            if (CurrentHearts > 0)
            {
                CurrentHearts--;
                if (CurrentHearts == MAX_HEARTS - 1)
                {
                    // Was full, now 4. Start timer with dynamic refill time.
                    NextRefillTime = DateTime.Now.AddMinutes(GetRefillTimeMinutes());
                }
                SaveHearts();
                OnHeartChanged?.Invoke();
                return true;
            }
            return false;
        }

        public void AddHearts(int amount)
        {
            CurrentHearts = Mathf.Min(CurrentHearts + amount, MAX_HEARTS);
            if (CurrentHearts == MAX_HEARTS)
            {
                // Clear timer
                NextRefillTime = DateTime.Now; 
            }
            SaveHearts();
            OnHeartChanged?.Invoke();
        }

        public TimeSpan GetTimeRemaining()
        {
            if (CurrentHearts >= MAX_HEARTS) return TimeSpan.Zero;
            TimeSpan remaining = NextRefillTime - DateTime.Now;
            return remaining.TotalSeconds > 0 ? remaining : TimeSpan.Zero;
        }

        private void SaveHearts()
        {
            PlayerPrefs.SetInt(PREF_HEARTS, CurrentHearts);
            if (CurrentHearts < MAX_HEARTS)
                PlayerPrefs.SetString(PREF_NEXT_REFILL, NextRefillTime.ToBinary().ToString());
            PlayerPrefs.Save();
        }

        private void LoadHearts()
        {
            CurrentHearts = PlayerPrefs.GetInt(PREF_HEARTS, MAX_HEARTS);
            
            string timeStr = PlayerPrefs.GetString(PREF_NEXT_REFILL, "");
            if (CurrentHearts < MAX_HEARTS && !string.IsNullOrEmpty(timeStr))
            {
                long temp = Convert.ToInt64(timeStr);
                NextRefillTime = DateTime.FromBinary(temp);

                // Offline Calc - use current refill time for calculation
                float refillMinutes = GetRefillTimeMinutes();
                while (CurrentHearts < MAX_HEARTS && DateTime.Now >= NextRefillTime)
                {
                    CurrentHearts++;
                    NextRefillTime = NextRefillTime.AddMinutes(refillMinutes);
                }
                
                // If we filled up, cap it.
                if (CurrentHearts >= MAX_HEARTS)
                {
                    CurrentHearts = MAX_HEARTS;
                }
                else
                {
                     // Still needs charging, verify timer isn't in past
                     if (NextRefillTime < DateTime.Now) NextRefillTime = DateTime.Now.AddMinutes(GetRefillTimeMinutes());
                }
                SaveHearts();
            }
            OnHeartChanged?.Invoke();
        }
    }
}