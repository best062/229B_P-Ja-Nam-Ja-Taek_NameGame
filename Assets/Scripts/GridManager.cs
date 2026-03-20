using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;


public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public GameObject tilePrefab;
    public int width = 7;
    public int height = 7;
    public bool isGameOver = false;
    public TMP_Text Remaining;
    public GameObject Win;
    public TMP_Text turnText;
    public GameObject shipDestroyedText;
    
    public Tile[,] playerGrid;
    public Tile[,] enemyGrid;
    public bool isPlayerTurn = true;
    List<Vector2Int> targets = new List<Vector2Int>();
    public bool isPlacingShips = true;
    
    int currentShipID = 0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerGrid = new Tile[width, height];
        enemyGrid = new Tile[width, height];

        // Player ฝั่งซ้าย
        GenerateGrid(playerGrid, new Vector3(0, 0, 0), true);

        // Enemy ฝั่งขวา (ขยับไป)
        GenerateGrid(enemyGrid, new Vector3(10, 0, 0), false);
        PlaceShip(enemyGrid, 4);
        PlaceShip(enemyGrid, 3);
        PlaceShip(enemyGrid, 2);
        PlaceShip(enemyGrid, 2);
        PlaceShip(enemyGrid, 1);
        PlaceShip(enemyGrid, 1);
        
        UpdateTurnUI();
    }
    
    void GenerateGrid(Tile[,] grid, Vector3 offset, bool isPlayer)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(x, 0, y) + offset, Quaternion.identity);

                Tile t = tile.GetComponent<Tile>();
                t.x = x;
                t.y = y;
                t.isPlayerTile = isPlayer; 

                grid[x, y] = t;
            }
        }
    }

    bool CanPlaceShip(Tile[,] grid, int x, int y, int length, bool isHorizontal)
    {
        for (int i = 0; i < length; i++)
        {
            int newX = x + (isHorizontal ? i : 0);
            int newY = y + (isHorizontal ? 0 : i);

            //out grid
            if (newX >= width || newY >= height)
                return false;

            if (grid[newX, newY].hasShip)
                return false;
        }
        return true;
    }
    
    void PlaceShip(Tile[,] grid, int length)
    {
        bool placed = false;

        while (!placed)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            bool isHorizontal = Random.value > 0.5f;

            if (CanPlaceShip(grid, x, y, length, isHorizontal))
            {
                for (int i = 0; i < length; i++)
                {
                    int newX = x + (isHorizontal ? i : 0);
                    int newY = y + (isHorizontal ? 0 : i);

                    grid[newX, newY].hasShip = true;
                    grid[newX, newY].shipID = currentShipID;

                    // debug
                    if (grid == playerGrid)
                    {
                        grid[newX, newY].GetComponent<Renderer>().material.color = Color.cyan;
                    }
                }

                currentShipID++;
                placed = true;
            }
        }
    }
    
    public int CountRemainingShips(Tile[,] grid)
    {
        HashSet<int> aliveShips = new HashSet<int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile t = grid[x, y];

                if (t.hasShip && !t.isClicked)
                {
                    aliveShips.Add(t.shipID);
                }
            }
        }

        return aliveShips.Count;
    }
    
    public bool IsShipDestroyed(Tile[,] grid, int shipID)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile t = grid[x, y];

                if (t.shipID == shipID && !t.isClicked)
                {
                    return false;
                }
            }
        }

        return true;
    }
    
    public void PlaceShipManual(int x, int y)
    {
        Tile tile = playerGrid[x, y];

        if (tile.hasShip) return;

        tile.hasShip = true;
        tile.shipID = currentShipID;

        tile.GetComponent<Renderer>().material.color = Color.cyan;

        currentShipID++;
    }
    
    public void UpdateUI()
    {
        int remaining = CountRemainingShips(enemyGrid); // นับฝั่ง enemy
        Remaining.text = "Remaining: " + remaining;
    }
    
    void UpdateTurnUI()
    {
        if (isPlayerTurn)
        {
            turnText.text = "Player Turn";
        }
        else
        {
            turnText.text = "AI Turn";
        }
        
        turnText.color = isPlayerTurn ? Color.green : Color.red;
    }
    
    public void EndPlayerTurn()
    {
        isPlayerTurn = false;
        UpdateTurnUI();
        StartCoroutine(AITurn());
    }
    
    IEnumerator AITurn()
    {
        if (isGameOver) yield break;
        yield return new WaitForSeconds(1f);

        int x, y;
        
        if (targets.Count > 0)
        {
            Vector2Int t = targets[0];
            targets.RemoveAt(0);
        
            x = t.x;
            y = t.y;
        }
        else
        {
            do
            {
                x = Random.Range(0, width);
                y = Random.Range(0, height);
            }
            while (playerGrid[x, y].isClicked);
        }
        
        Tile tile = playerGrid[x, y];
        tile.TakeHit();
        if (tile.hasShip)
        {
            AddTargets(x, y);
        }

        isPlayerTurn = true;
        UpdateTurnUI();
    }
    
    void AddTargets(int x, int y)
    {
        Vector2Int[] dirs = {
            new Vector2Int(1,0),
            new Vector2Int(-1,0),
            new Vector2Int(0,1),
            new Vector2Int(0,-1)
        };

        foreach (var d in dirs)
        {
            int nx = x + d.x;
            int ny = y + d.y;

            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
            {
                if (!playerGrid[nx, ny].isClicked)
                {
                    Vector2Int newTarget = new Vector2Int(nx, ny);

                    if (!targets.Contains(newTarget))
                    {
                        targets.Add(newTarget);
                    }
                }
            }
        }
    }
    
    public void ShowShipDestroyed()
    {
        StartCoroutine(ShowShipDestroyedRoutine());
    }

    IEnumerator ShowShipDestroyedRoutine()
    {
        shipDestroyedText.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        shipDestroyedText.SetActive(false);
    }
}