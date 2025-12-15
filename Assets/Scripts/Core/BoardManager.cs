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
        [SerializeField] private float tileSize = 1.1f; // Defined by SceneBuilder
        [SerializeField] private float padding = 0.02f;

        [Header("Data")]
        public AnimalSO[] allAnimals;

        private Tile[,] grid;
        private Vector3[,] slotPositions; 
        private bool isMoving = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable() => SwipeDetector.OnSwipe += Move;
        private void OnDisable() => SwipeDetector.OnSwipe -= Move;

        private void Start() 
        {
            StartCoroutine(InitRoutine());
        }
        
        private IEnumerator InitRoutine()
        {
            if (allAnimals == null || allAnimals.Length == 0)
                allAnimals = Resources.LoadAll<AnimalSO>("Data/ScriptableObjects");
            System.Array.Sort(allAnimals, (a, b) => a.level.CompareTo(b.level));
            
            yield return null;

            InitializeGrid();
            SpawnTile();
            SpawnTile();
        }

        private void InitializeGrid()
        {
            grid = new Tile[width, height];
            slotPositions = new Vector3[width, height];

            // Re-calculate math to match SceneBuilder
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
        }

        private void Move(Vector2 direction)
        {
            if (isMoving) return;
            StartCoroutine(MoveRoutine(direction));
        }

        private IEnumerator MoveRoutine(Vector2 direction)
        {
            isMoving = true;
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
            }
            isMoving = false;
        }

       private AnimalSO GetAnimalData(int level)
        {
            foreach(var anim in allAnimals) if(anim.level == level) return anim;
            return null;
        }
    }
}