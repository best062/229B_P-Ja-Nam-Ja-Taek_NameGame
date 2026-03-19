using UnityEngine;
using System.Collections.Generic;


public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public GameObject tilePrefab;
    public int width = 7;
    public int height = 7;
    public bool isGameOver = false;
    
    public Tile[,] grid;
    
    int currentShipID = 0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateGrid();
        PlaceShip(4);
        PlaceShip(3);
        PlaceShip(2);
        PlaceShip(2);
        PlaceShip(1);
        PlaceShip(1);
    }
    
    void GenerateGrid()
    {
        grid = new Tile[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(x, 0, y), Quaternion.identity);
                Tile t = tile.GetComponent<Tile>();
                t.x = x;
                t.y = y;
                grid[x, y] = t; 
            }
        }
    }

    bool CanPlaceShip(int x, int y,int lenght,bool isHorizontal)
    {
        for (int i = 0; i < lenght; i++)
        {
            int newX = x + (isHorizontal ? i : 0); //เลื่อนตำแหน่งของเรือทีละช่อง
            int newY = y + (isHorizontal ? 0 : i); //เลื่อนตำแหน่งของเรือทีละช่อง
            
            //out grid
            if (newX >= width || newY >= height)
            {
                return false;
            }
            
            //Ship collision
            if (grid[newX, newY].hasShip)
                return false;
        }
        return true;
    }
    
    void PlaceShip(int lenght)
    {
        bool placed =  false;

        while (!placed)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            
            bool isHorizontal = Random.value > 0.5f;

            if (CanPlaceShip(x,y,lenght, isHorizontal))
            {
                for (int i = 0; i < lenght; i++)
                {
                    int newX = x + (isHorizontal ? i : 0);
                    int newY = y + (isHorizontal ? 0 : i);
                    
                    grid[newX, newY].hasShip = true;
                    grid[newX, newY].shipID = currentShipID;
                    
                    //debug
                    grid[newX, newY].GetComponent<Renderer>().material.color = Color.deepSkyBlue;
                }
                currentShipID++;
                placed = true;
            }
        }
    }
    
    public int CountRemainingShips()
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
    
    public bool IsShipDestroyed(int shipID)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile t = grid[x, y];

                if (t.shipID == shipID && !t.isClicked)
                {
                    return false; // ยังไม่จม
                }
            }
        }
        return true; // จมแล้ว
    }
}