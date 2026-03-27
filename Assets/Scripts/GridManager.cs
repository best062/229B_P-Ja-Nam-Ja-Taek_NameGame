using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;


public class GridManager : MonoBehaviour
{
    public SkillManager skillManager;
    public Tile[,] playerGrid;
    public Tile[,] enemyGrid;
    List<Vector2Int> targets = new List<Vector2Int>();
    
    [Header("Grid Settings")]
    public GameObject tilePrefab;
    public int width = 7;
    public int height = 7;
    public bool isGameOver = false;
    
    [Header("UI Settings")]
    public TMP_Text Remaining;
    public GameObject Win;
    public GameObject Lose;
    public TMP_Text turnText;
    public GameObject shipDestroyedText;
    public TMP_Text placementText;
    
    [Header("Camera Settings")]
    public Camera playerCam;
    public Camera enemyCam;
    
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    
    [Header("Effect Settings")]
    public GameObject hitEffectPrefab;
    public GameObject bombEffectPrefab;
    
    [Header("Other Settings")]
    public bool isPlayerTurn = true;
    bool isAITurnRunning = false;
    public bool isPlacingShips = true;
    public bool isShooting = false;
    
    // boat sizes
    int[] shipSizes = {4, 3, 3, 2, 2, 1, 1};
    int currentShipIndex = 0;
    public bool isHorizontal = true;
    
    //preview placement
    List<Tile> previewTiles = new List<Tile>();
    
    // current boat
    int currentShipID = 0;
    
    //int AI shot count
    int GetAIShotCount()
    {
        int rand = Random.Range(0, 100);

        if (rand < 75) return 1;
        else if (rand < 95) return 2;
        else return 3;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdatePlacementUI();
        playerGrid = new Tile[width, height];
        enemyGrid = new Tile[width, height];

        // Player ฝั่งซ้าย
        GenerateGrid(playerGrid, new Vector3(0, 0, 0), true);

        // Enemy ฝั่งขวา (ขยับไป)
        GenerateGrid(enemyGrid, new Vector3(10, 0, 0), false);
        PlaceShip(enemyGrid, 4);
        PlaceShip(enemyGrid, 3);
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
        if (currentShipIndex >= shipSizes.Length) return;

        int size = shipSizes[currentShipIndex];

        if (!CanPlaceShip(playerGrid, x, y, size, isHorizontal))
        {
            Debug.Log("วางไม่ได้!");
            return;
        }

        for (int i = 0; i < size; i++)
        {
            int nx = x + (isHorizontal ? i : 0);
            int ny = y + (isHorizontal ? 0 : i);

            Tile t = playerGrid[nx, ny];
            t.hasShip = true;
            t.shipID = currentShipID;
            t.state = Tile.TileState.Ship;
            t.GetComponent<Renderer>().material.color = Color.cyan;
        }

        currentShipID++;
        currentShipIndex++; // ⭐ สำคัญ

        UpdatePlacementUI();
        ClearPreview();
        
        if (IsPlacementDone())
        {
            isPlacingShips = false;
            isPlayerTurn = true;

            // ⭐ สลับกล้องทันที
            playerCam.gameObject.SetActive(false);
            enemyCam.gameObject.SetActive(true);

            Debug.Log("Battle Start!");
        }
    }
    
    void ClearPreview()
    {
        foreach (Tile t in previewTiles)
        {
            if (t.isClicked) continue; // ⭐ ห้ามแตะ

            switch (t.state)
            {
                case Tile.TileState.Empty:
                    t.GetComponent<Renderer>().material.color = Color.white;
                    break;

                case Tile.TileState.Ship:
                    t.GetComponent<Renderer>().material.color = Color.cyan;
                    break;
            }
        }
        previewTiles.Clear();
    }

    public void ShowPreview(int x, int y)
    {
        if (IsPlacementDone()) return;

        int size = shipSizes[currentShipIndex];

        bool canPlace = CanPlaceShip(playerGrid, x, y, size, isHorizontal);

        Color validColor = new Color(0, 1, 1, 0.5f);
        Color invalidColor = new Color(1, 0, 0, 0.5f);

        for (int i = 0; i < size; i++)
        {
            int nx = x + (isHorizontal ? i : 0);
            int ny = y + (isHorizontal ? 0 : i);

            // กันหลุด grid
            if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                continue;

            Tile t = playerGrid[nx, ny];

            Renderer r = t.GetComponent<Renderer>();

            if (t.hasShip)
            {
                // 🔴 ชนจริง
                r.material.color = new Color(1, 0, 0, 0.8f);
            }
            else
            {
                // 🔵 หรือ 🔴 ตาม valid
                r.material.color = canPlace ? validColor : invalidColor;
            }
            
            if (t.isClicked)
            {
                continue; // ⭐ สำคัญมาก
            }
            
            if (!previewTiles.Contains(t))
            {
                previewTiles.Add(t);
            }
            
        }
    }
    
    public bool IsPlacementDone()
    {
        return currentShipIndex >= shipSizes.Length;
    }
    
    void UpdatePlacementUI()
    {
        if (!IsPlacementDone())
        {
            int size = shipSizes[currentShipIndex];

            placementText.text = "Placing Ship: Size " + size +
                                 (isHorizontal ? " (Horizontal)" : " (Vertical)");
        }
        else
        {
            placementText.text = "Ready to Battle!";
        }

        placementText.color = IsPlacementDone() ? Color.green : Color.white;
    }
    
    public void ShootBullet(Vector3 targetPos)
    {
        if (isShooting) return; 
        isShooting = true;      
        
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet b = bullet.GetComponent<Bullet>();
        
        switch (skillManager.currentSkill)
        {
            case SkillManager.SkillType.Bomb:
                if (skillManager.bombCount > 0)
                {
                    skillManager.bombCount--;
                }
                break;

            case SkillManager.SkillType.Scan:
                if (skillManager.scanCount > 0)
                {
                    skillManager.scanCount--;
                }
                break;
        }

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        b.skillType = skillManager.currentSkill;
        b.skillManager = skillManager;
        b.gridManager = this;

        Vector3 start = firePoint.position;
        Vector3 end = targetPos;

        float height = 3f; 

        Vector3 gravity = Physics.gravity;

        // แยกแกน
        Vector3 displacement = end - start;
        Vector3 displacementXZ = new Vector3(displacement.x, 0, displacement.z);

        float time = Mathf.Sqrt(-2 * height / gravity.y) + Mathf.Sqrt(2 * (displacement.y - height) / gravity.y);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity.y * height);
        Vector3 velocityXZ = displacementXZ / time;

        Vector3 finalVelocity = velocityXZ + velocityY;

        rb.linearVelocity = finalVelocity; 
        skillManager.UpdateSkillUI();
    }
    
    public void UpdateUI()
    {
        int remaining = CountRemainingShips(enemyGrid); // นับฝั่ง enemy
        Remaining.text = "Remaining: " + remaining;
    }

    public void UpdateTurnUI()
    {
        if (!isPlayerTurn)
        {
            turnText.text = "Enemy Turn";
            turnText.color = Color.red;
            return;
        }

        switch (skillManager.currentSkill)
        {
            case SkillManager.SkillType.Scan:
                turnText.text = "Scan Mode";
                turnText.color = Color.black;
                break;

            case SkillManager.SkillType.Bomb:
                turnText.text = "Bomb Ready";
                turnText.color = Color.black;
                break;

            default:
                turnText.text = "Player Turn";
                turnText.color = Color.green;
                break;
        }
    }
    
    public void EndPlayerTurn()
    {
        // 🏆 Check Win
        int remaining = CountRemainingShips(enemyGrid);
        if (remaining == 0)
        {
            Debug.Log("YOU WIN!");
            isGameOver = true;
            Win.SetActive(true);
            return;
        }
        
        isPlayerTurn = false;
        playerCam.gameObject.SetActive(true);
        enemyCam.gameObject.SetActive(false);
        UpdateTurnUI();
        StartCoroutine(AITurn());
    }
    
    IEnumerator AITurn()
    {
        if (isGameOver) yield break;
        if (isAITurnRunning) yield break;

        isAITurnRunning = true;

        yield return new WaitForSeconds(1f);

        int shots = GetAIShotCount();

        for (int i = 0; i < shots; i++)
        {
            yield return new WaitForSeconds(Random.Range(0.5f , 0.9f));

            int x, y;

            // 🎯 ถ้ามี target → ยิงต่อเนื่อง
            if (targets.Count > 0)
            {
                Vector2Int t = targets[0];
                targets.RemoveAt(0);

                x = t.x;
                y = t.y;
            }
            else
            {
                x = Random.Range(0, width);
                y = Random.Range(0, height);
            }

            Tile tile = playerGrid[x, y];

            // ❌ กันยิงซ้ำ
            if (tile.isClicked)
            {
                i--;
                continue;
            }

            tile.TakeHit();

            // 🎯 ถ้ายิงโดน → เพิ่ม target รอบๆ
            if (tile.hasShip)
            {
                AddTargets(x, y);
            }

            // 🧠 เช็คแพ้ทันที
            int remaining = CountRemainingShips(playerGrid);
            if (remaining == 0)
            {
                yield return new WaitForSeconds(0.5f);

                Debug.Log("YOU LOSE!");
                isGameOver = true;
                Lose.SetActive(true);

                isAITurnRunning = false;
                yield break;
            }
        }

        yield return new WaitForSeconds(1f);
        // 🔄 จบเทิร์น AI
        isAITurnRunning = false;
        isPlayerTurn = true;

        playerCam.gameObject.SetActive(false);
        enemyCam.gameObject.SetActive(true);

        UpdateTurnUI();
    }
    
    void AddTargets(int x, int y)
    {
        Vector2Int[] dirs = new Vector2Int[]
        {
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
                Tile t = playerGrid[nx, ny];

                if (!t.isClicked)
                {
                    targets.Add(new Vector2Int(nx, ny));
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
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            isHorizontal = !isHorizontal;
            Debug.Log(isHorizontal ? "Horizontal" : "Vertical");
            UpdatePlacementUI(); // ⭐ ให้ UI เปลี่ยนด้วย
        }
        if (!isPlacingShips) return;
        UpdatePreviewByMouse();
    }
    
    void UpdatePreviewByMouse()
    {
        // ❌ ถ้าไม่โดน tile → ล้าง
        ClearPreview();
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Tile t = hit.collider.GetComponent<Tile>();

            if (t != null && t.isPlayerTile)
            {
                ShowPreview(t.x, t.y);
            }
        }
        
    }
    
    public IEnumerator EndTurnWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        EndPlayerTurn();
    }
}