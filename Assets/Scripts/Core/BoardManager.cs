using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameInput;

namespace Core
{
    public class BoardManager : MonoBehaviour
    {
        public static BoardManager Instance { get; private set; }

        [Header("Configuration")]
        public int width = 4;
        public int height = 4;
        [SerializeField] private Tile tilePrefab;
        [SerializeField] private Transform boardContainer; 
        [SerializeField] private Transform tileLayer;
        [SerializeField] private Transform slotsParent; // Reference to Slots container
        [SerializeField] private float tileSize = 1.1f; // Defined by SceneBuilder
        [SerializeField] private float padding = 0.02f;

        [Header("Data")]
        public AnimalSO[] allAnimals;

        private Tile[,] grid;
        private Vector3[,] slotPositions; 
        public bool IsInputActive { get; set; } = true;
        private bool isMoving = false;
        private int currentScore = 0;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable() => SwipeDetector.OnSwipe += Move;
        private void OnDisable() => SwipeDetector.OnSwipe -= Move;

        private void OnDestroy() 
        {
            if (Systems.LabSystem.Instance) Systems.LabSystem.Instance.OnUpgradePurchased -= OnUpgradePurchased;
        }

        private void Start() 
        {
            if (Systems.LabSystem.Instance) Systems.LabSystem.Instance.OnUpgradePurchased += OnUpgradePurchased;
            StartCoroutine(InitRoutine());
        }

        private void OnUpgradePurchased(Core.UpgradeType type)
        {
            Debug.LogWarning($"BoardManager: OnUpgradePurchased Received. Type: {type}");
            if (type == UpgradeType.Undo)
            {
                UndosRemaining++; // Give free undo on purchase
                int max = GetMaxUndos();
                Debug.LogWarning($"BoardManager: Undo Purchased. UndosRemaining: {UndosRemaining}, MaxUndos: {max}");
                if (UI.UIManager.Instance) UI.UIManager.Instance.UpdateUndoCount(UndosRemaining, max);
            }
        }
        
        private IEnumerator InitRoutine()
        {
            if (allAnimals == null || allAnimals.Length == 0)
                allAnimals = Resources.LoadAll<AnimalSO>("Data/ScriptableObjects");
            System.Array.Sort(allAnimals, (a, b) => a.level.CompareTo(b.level));
            
            // Wait 2 frames to ensure LabSystem has loaded upgrade data
            yield return null;
            yield return null;

            InitializeGrid();
            UndosRemaining = GetMaxUndos(); // Set initial with upgraded value
            
            // Update UI immediately
            if(UI.UIManager.Instance) UI.UIManager.Instance.UpdateUndoCount(UndosRemaining, GetMaxUndos());
            
            SpawnTile();
            SpawnTile();
        }

        private void InitializeGrid()
        {
            grid = new Tile[width, height];
            slotPositions = new Vector3[width, height];

            // Read actual slot positions from scene GameObjects
            if (slotsParent != null)
            {
                foreach (Transform slot in slotsParent)
                {
                    // Parse slot name: Slot_X_Y
                    string[] parts = slot.name.Split('_');
                    if (parts.Length >= 3 && int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
                    {
                        if (x >= 0 && x < width && y >= 0 && y < height)
                        {
                            slotPositions[x, y] = slot.localPosition;
                        }
                    }
                }
            }
            else
            {
                // Fallback: calculate positions mathematically
                float calculatedTotalWidth = (width * tileSize) + ((width - 1) * padding);
                float calculatedTotalHeight = (height * tileSize) + ((height - 1) * padding);
                Vector3 origin = new Vector3(-calculatedTotalWidth / 2 + tileSize / 2, -calculatedTotalHeight / 2 + tileSize / 2, 0);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        slotPositions[x, y] = origin + new Vector3(x * (tileSize + padding), y * (tileSize + padding), 0);
                    }
                }
            }
            
            if(tileLayer) { foreach(Transform t in tileLayer) Destroy(t.gameObject); }
        }
        
        private void SpawnTile()
        {
            List<Vector2Int> emptySlots = new List<Vector2Int>();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (grid[x, y] == null) emptySlots.Add(new Vector2Int(x, y));

            if (emptySlots.Count > 0)
            {
                Vector2Int pos = emptySlots[Random.Range(0, emptySlots.Count)];
                CreateTileAt(pos.x, pos.y, allAnimals[0]); 
            }
        }

        private void CreateTileAt(int x, int y, AnimalSO data)
        {
            Tile t = Instantiate(tilePrefab, tileLayer);
            t.transform.localPosition = slotPositions[x, y];
            
            // MATCH SCALE: Set Tile Scale to match the Background Slots (1.1)
            t.transform.localScale = Vector3.one * tileSize;
            
            t.Init(x, y, data);
            grid[x, y] = t;
            
            // Unlock Start Animals (Level 1)
            if(Systems.CollectionSystem.Instance) 
                Systems.CollectionSystem.Instance.UnlockAnimal(data.level);
        }

        private void Move(Vector2 direction)
        {
            if (!IsInputActive) return;
            if (isMoving) return;
            StartCoroutine(MoveRoutine(direction));
        }

        private IEnumerator MoveRoutine(Vector2 direction)
        {
            isMoving = true;
            SaveState(); // Save before any changes

            bool anyMoved = false;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if(grid[x,y] != null) grid[x, y].MergedThisTurn = false;

            int startX = (direction.x == 1) ? width - 1 : 0;
            int endX = (direction.x == 1) ? -1 : width;
            int stepX = (direction.x == 1) ? -1 : 1;

            int startY = (direction.y == 1) ? height - 1 : 0;
            int endY = (direction.y == 1) ? -1 : height;
            int stepY = (direction.y == 1) ? -1 : 1;

            for (int x = startX; x != endX; x += stepX)
            {
                for (int y = startY; y != endY; y += stepY)
                {
                    Tile t = grid[x, y];
                    if (t == null) continue;

                    int farX = x; int farY = y;
                    int nextX = x + (int)direction.x;
                    int nextY = y + (int)direction.y;
                    
                    Tile tileToMergeWith = null;

                    while (nextX >= 0 && nextX < width && nextY >= 0 && nextY < height)
                    {
                        Tile obstacle = grid[nextX, nextY];
                        if (obstacle != null)
                        {
                            if (obstacle.Level == t.Level && !obstacle.MergedThisTurn) tileToMergeWith = obstacle;
                            break; 
                        }
                        farX = nextX; farY = nextY;
                        nextX += (int)direction.x; nextY += (int)direction.y;
                    }

                    if (tileToMergeWith != null)
                    {
                        grid[x, y] = null; 
                        t.MoveTo(slotPositions[tileToMergeWith.x, tileToMergeWith.y]);
                        Destroy(t.gameObject, 0.15f);
                        
                        AnimalSO nextData = GetAnimalData(tileToMergeWith.Level + 1);
                        if(nextData != null)
                        {
                            tileToMergeWith.Upgrade(nextData);
                            tileToMergeWith.MergedThisTurn = true;
                            
                            // Score Calculation: Level of merged animal (Level 1 = 1 point, Level 25 = 25 points)
                            int points = nextData.level; 
                            currentScore += points;
                            if(UI.UIManager.Instance) UI.UIManager.Instance.UpdateScore(currentScore);
                            
                            // UNLOCK IN COLLECTION
                            if(Systems.CollectionSystem.Instance) 
                                Systems.CollectionSystem.Instance.UnlockAnimal(nextData.level);
                            
                            // CARD REWARD CHANCE
                            if(Systems.CardRewardSystem.Instance)
                                Systems.CardRewardSystem.Instance.TryRewardCard(nextData.level);
                                
                            // --- XP & VAMPIRE SYSTEM HOOK ---
                            Debug.Log($"Merge complete! Checking PerkManager...");
                            if(Managers.PerkManager.Instance != null)
                            {
                                // XP Formula: 10 * 1.5^(Rank-1)
                                float xp = 10f * Mathf.Pow(1.5f, tileToMergeWith.Level - 1); 
                                Debug.Log($"Adding {xp} XP to PerkManager");
                                Managers.PerkManager.Instance.AddXP(xp);
                                
                                // Trigger 'Joker Slime' check? (Every X merges)
                                // Not implemented yet, but good place for it.
                            }
                            else
                            {
                                Debug.LogError("PerkManager.Instance is NULL!");
                            }
                            // --------------------------------
                        }
                        anyMoved = true;
                    }
                    else
                    {
                        if (farX != x || farY != y)
                        {
                            grid[x, y] = null;
                            grid[farX, farY] = t;
                            t.SetCoords(farX, farY);
                            t.MoveTo(slotPositions[farX, farY]);
                            anyMoved = true;
                        }
                    }
                }
            }

            if (anyMoved)
            {
                 yield return new WaitForSeconds(0.16f);
                 SpawnTile();
                 if (CheckGameOver())
                 {
                     HandleGameOver();
                 }
                 
                 // Trigger Vacuum Check
                 TriggerVacuumCheck();
            }
            else
            {
                // No move happened, discard state
                if(history.Count > 0) history.Pop();
            }
            isMoving = false;
        }

       private bool CheckGameOver()
       {
            // 1. Check for empty slots
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (grid[x, y] == null) return false;

            // 2. Check for possible merges
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Tile current = grid[x, y];
                    // Check Right
                    if (x < width - 1)
                    {
                        Tile right = grid[x + 1, y];
                        if (right != null && right.Level == current.Level) return false;
                    }
                    // Check Down
                    if (y < height - 1)
                    {
                        Tile down = grid[x, y + 1];
                        if (down != null && down.Level == current.Level) return false;
                    }
                }
            }
            return true;
       }

        public void RestartGame()
        {
             if(isMoving) return;

             // Use 1 heart on restart
             if(Systems.HeartSystem.Instance)
             {
                 if(!Systems.HeartSystem.Instance.UseHeart())
                 {
                     Debug.Log("Not enough hearts to restart!");
                     return; // Don't restart if no hearts
                 }
             }

             // Clear Grid
             foreach(var t in grid) if(t != null) Destroy(t.gameObject);
             System.Array.Clear(grid, 0, grid.Length);
             
             // Clear History
             history.Clear();
             UndosRemaining = GetMaxUndos(); // Correctly reset to max
             
             if(UI.UIManager.Instance) UI.UIManager.Instance.UpdateUndoCount(UndosRemaining, GetMaxUndos());
             
             // Reset Score
             currentScore = 0;
             if(UI.UIManager.Instance) UI.UIManager.Instance.UpdateScore(currentScore);
             
             // Start Game
             SpawnTile();
             SpawnTile();
             
             Debug.Log("Game Restarted (1 Heart Used)");
        }

       private void HandleGameOver()
       {
            Debug.Log("Game Over!");
            
            // Use 1 heart on game over
            if(Systems.HeartSystem.Instance)
            {
                Systems.HeartSystem.Instance.UseHeart();
            }
            
            // Calculate base DNA
            int baseDNA = currentScore / 10;
            
            // Apply DNA Multiplier upgrade bonus (+1% per level)
            float multiplier = 1f;
            if (Systems.LabSystem.Instance != null)
            {
                int dnaUpgradeLevel = Systems.LabSystem.Instance.GetUpgradeLevel(UpgradeType.ExtraDNA);
                multiplier = 1f + (dnaUpgradeLevel * 0.01f); // +1% per level
            }
            int earnedDNA = Mathf.RoundToInt(baseDNA * multiplier);
            
            // Persistence
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            if (currentScore > highScore)
            {
                highScore = currentScore;
                PlayerPrefs.SetInt("HighScore", highScore);
            }

             // Update DNA System (Delegate Logic)
             int totalDNA = 0;
             if(Systems.DNASystem.Instance) 
             {
                 Systems.DNASystem.Instance.AddDNA(earnedDNA); 
                 totalDNA = Systems.DNASystem.Instance.TotalDNA;
             }
             else 
             {
                 // Fallback if system missing (shouldn't happen)
                 totalDNA = PlayerPrefs.GetInt("TotalDNA", 0) + earnedDNA;
                 PlayerPrefs.SetInt("TotalDNA", totalDNA);
                 PlayerPrefs.Save();
             }

            if (UI.UIManager.Instance)
            {
                UI.UIManager.Instance.ShowGameOver(currentScore, highScore, earnedDNA, totalDNA);
            }
       }

        // --- UNDO SYSTEM ---
        [System.Serializable]
        public struct BoardState
        {
            public int score;
            public int[,] gridLevels; // 0 if empty, level otherwise
        }
        
        private Stack<BoardState> history = new Stack<BoardState>();
        public int UndosRemaining { get; private set; } = 3; // Default

        public void SaveState()
        {
            BoardState state = new BoardState();
            state.score = currentScore;
            state.gridLevels = new int[width, height];
            
            for(int x=0; x<width; x++)
            {
                for(int y=0; y<height; y++)
                {
                    if(grid[x,y] != null) state.gridLevels[x,y] = grid[x,y].Level;
                    else state.gridLevels[x,y] = 0;
                }
            }
            
            history.Push(state);
            // Optional: Limit stack size?
        }

        public void Undo()
        {
            if (isMoving || history.Count == 0 || UndosRemaining <= 0) return;
            
            BoardState previous = history.Pop();
            UndosRemaining--;
            if(UI.UIManager.Instance) UI.UIManager.Instance.UpdateUndoCount(UndosRemaining, GetMaxUndos());
            RestoreState(previous);
        }

        private void RestoreState(BoardState state)
        {
            // Update Score
            currentScore = state.score;
            if(UI.UIManager.Instance) UI.UIManager.Instance.UpdateScore(currentScore);
            
            // Clear current tiles
            foreach(var t in grid) if(t != null) Destroy(t.gameObject);
            System.Array.Clear(grid, 0, grid.Length);
            
            // Re-spawn
            for(int x=0; x<width; x++)
            {
                for(int y=0; y<height; y++)
                {
                    int level = state.gridLevels[x,y];
                    if(level > 0)
                    {
                        AnimalSO data = GetAnimalData(level);
                        CreateTileAt(x, y, data);
                    }
                }
            }
        }
        
        public void AddUndos(int amount)
        {
             UndosRemaining += amount;
        }

        private int GetMaxUndos()
        {
            int bonus = 0;
            if (Systems.LabSystem.Instance != null)
            {
                // Fallback: Use Level directly as bonus (+1 per level)
                // This bypasses unconfigured ScriptableObject data
                bonus = Systems.LabSystem.Instance.GetUpgradeLevel(UpgradeType.Undo);
                Debug.Log($"BoardManager: Max Undos Calc: 0 Base + {bonus} Bonus (Level {bonus})");
            }
            return bonus; // No default undos, must purchase upgrade
        }
        
        // --- END UNDO ---

        // --- VAMPIRE SKILLS IMPLEMENTATION ---
        
        public void ShuffleBoard()
        {
            if (isMoving) return;
            SaveState();
            
            List<AnimalSO> animals = new List<AnimalSO>();
            // Collect all animals
            for(int x=0; x<width; x++)
            {
                for(int y=0; y<height; y++)
                {
                    if(grid[x,y] != null) animals.Add(grid[x,y].Data);
                }
            }
            
            // Clear grid
            foreach(var t in grid) if(t!=null) Destroy(t.gameObject);
            System.Array.Clear(grid, 0, grid.Length);
            
            // Shuffle list
            for (int i = 0; i < animals.Count; i++)
            {
                AnimalSO temp = animals[i];
                int randomIndex = Random.Range(i, animals.Count);
                animals[i] = animals[randomIndex];
                animals[randomIndex] = temp;
            }
            
            // Repopulate
            int index = 0;
            // Iterate grid randomly or sequentially? Sequentially is fine since list is shuffled.
            for(int x=0; x<width; x++)
            {
                 for(int y=0; y<height; y++)
                 {
                     if(index < animals.Count)
                     {
                         CreateTileAt(x, y, animals[index]);
                         index++;
                     }
                 }
            }
            Debug.Log("Board Shuffled!");
        }

        private int movesSinceVacuum = 0;
        public void TriggerVacuumCheck()
        {
            movesSinceVacuum++;
            // Check Perk Logic
            if(Managers.PerkManager.Instance != null)
            {
                // Is Vacuum Active? Vacuum is "Black Hole" in design.
                // Assuming we look for "Vacuum" or "Black Hole" string
                float vacuumLevel = Managers.PerkManager.Instance.GetFlatStatBonus("Vacuum"); // Or specialized method
                // We stored 'Cooldown' in PerkRuntimeData or we can infer from Level.
                // Let's assume Level 1 = 10 moves. Level 5 = 5 moves.
                
                // For simplicity, let's say Threshold = 11 - Level. (Min 2).
                if(vacuumLevel > 0)
                {
                    int threshold = 12 - (int)vacuumLevel * 2; 
                    if(threshold < 3) threshold = 3;
                    
                    if(movesSinceVacuum >= threshold)
                    {
                        VacuumLowest();
                        movesSinceVacuum = 0;
                    }
                }
            }
        }

        private void VacuumLowest()
        {
            // Find lowest rank animal
            Tile lowest = null;
            int minRank = 999;
            
            foreach(var t in grid)
            {
                if(t != null)
                {
                    if(t.Level < minRank)
                    {
                        minRank = t.Level;
                        lowest = t;
                    }
                }
            }
            
            if(lowest != null && minRank == 1) // Only Vacuum Rank 1 as per design? Or "Lowest"? User said "Rank 1 yutar".
            {
                // Only swallow Rank 1
                grid[lowest.x, lowest.y] = null;
                Vector3 pos = lowest.transform.position;
                Destroy(lowest.gameObject);
                
                // Spawn Effect?
                // Add XP?
                // Logic: Vacuum gives XP.
                if(Managers.PerkManager.Instance) Managers.PerkManager.Instance.AddXP(10); // Fixed XP for Rank 1
                
                Debug.Log("Vacuum activated! Swallowed Rank 1.");
            }
        }


        // --- ACTIVE SKILLS API ---
        public void WipeRow(int yIndex)
        {
            SaveState();
            for(int x=0; x<width; x++)
            {
                if(grid[x,yIndex] != null)
                {
                    Destroy(grid[x,yIndex].gameObject);
                    grid[x,yIndex] = null;
                }
            }
            SpawnTile(); // Refill
        }

        public void WipeColumn(int xIndex)
        {
            SaveState();
            for(int y=0; y<height; y++)
            {
                if(grid[xIndex,y] != null)
                {
                    Destroy(grid[xIndex,y].gameObject);
                    grid[xIndex,y] = null;
                }
            }
            SpawnTile();
        }

        public void RemoveTileAt(int x, int y)
        {
            if(grid[x,y] != null)
            {
                SaveState();
                Destroy(grid[x,y].gameObject);
                grid[x,y] = null;
                SpawnTile();
            }
        }
        
        public void SwapTiles(int x1, int y1, int x2, int y2)
        {
            SaveState();
            Tile t1 = grid[x1,y1];
            Tile t2 = grid[x2,y2];
            
            // Logic to swap transform positions and grid array refs
            if(t1) 
            {
                t1.SetCoords(x2, y2);
                t1.MoveTo(slotPositions[x2,y2]); // Instant or anim?
            }
            if(t2) 
            {
                t2.SetCoords(x1, y1);
                t2.MoveTo(slotPositions[x1,y1]);
            }
            
            grid[x1,y1] = t2;
            grid[x2,y2] = t1;
        }

       private AnimalSO GetAnimalData(int level)
        {
            foreach(var anim in allAnimals) if(anim.level == level) return anim;
            return null;
        }
    }
}