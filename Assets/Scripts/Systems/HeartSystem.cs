using UnityEngine;
using System;

namespace Systems
{
    public class HeartSystem : MonoBehaviour
    {
        public static HeartSystem Instance { get; private set; }

        public const int MAX_HEARTS = 5;
        public const float REFILL_TIME_MINUTES = 30f;
        private const string PREF_HEARTS = "Hearts_Count";
        private const string PREF_NEXT_REFILL = "Hearts_NextRefill";

        public int CurrentHearts { get; private set; }
        public DateTime NextRefillTime { get; private set; }
        
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
                        // Ideally: NextRefillTime += 30mins
                        // But if offline for days, correct validation needed.
                        // For loop in Update: just add 30 mins to the *previous* target.
                        NextRefillTime = NextRefillTime.AddMinutes(REFILL_TIME_MINUTES);
                        if (NextRefillTime < DateTime.Now) NextRefillTime = DateTime.Now.AddMinutes(REFILL_TIME_MINUTES); // Catchup safety
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
                    // Was full, now 4. Start timer.
                    NextRefillTime = DateTime.Now.AddMinutes(REFILL_TIME_MINUTES);
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

                // Offline Calc
                while (CurrentHearts < MAX_HEARTS && DateTime.Now >= NextRefillTime)
                {
                    CurrentHearts++;
                    NextRefillTime = NextRefillTime.AddMinutes(REFILL_TIME_MINUTES);
                }
                
                // If we filled up, cap it.
                if (CurrentHearts >= MAX_HEARTS)
                {
                    CurrentHearts = MAX_HEARTS;
                }
                else
                {
                     // Still needs charging, verify timer isn't in past
                     if (NextRefillTime < DateTime.Now) NextRefillTime = DateTime.Now.AddMinutes(REFILL_TIME_MINUTES);
                }
                SaveHearts();
            }
            OnHeartChanged?.Invoke();
        }
    }
}