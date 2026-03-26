using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    private GridManager gridManager;
    private Renderer rend;
    private Rigidbody rb;
    private Vector3 startPosition;
    public SkillManager skillManager;
    
    [Header("Settings")]
    public int x;
    public int y;
    public bool isPlayerTile;
    public bool hasShip = false;
    public bool isClicked = false;
    public int shipID = -1;
    public Color defaultColor = Color.white;
    bool isWaiting = false;
    
    [Header("Physics")]
    public float mass = 1f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rend = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
        skillManager = FindFirstObjectByType<SkillManager>();

        startPosition = transform.position;
        gridManager = FindFirstObjectByType(typeof(GridManager)) as GridManager;
        if (!isPlayerTile)
        {
            rend.material.color = Color.gray;
        }
    }
    
    public enum TileState
    {
        Empty,
        Ship,
        Hit,
        Miss
    }
    public TileState state = TileState.Empty;
    
    public void ResetColor()
    {
        rend.material.color = defaultColor;
    }

    // Update is called once per frame
    void OnMouseDown()
    {
        if (gridManager.isPlacingShips && isPlayerTile)
        {
            gridManager.PlaceShipManual(x, y);
            return;
        }
        
        if (!gridManager.IsPlacementDone())
        {
            Debug.Log("วางเรือให้ครบก่อน!");
            return;
        }
        
        if (!gridManager.IsPlacementDone()) return;
        if (isPlayerTile) return;
        if (!gridManager.isPlayerTurn) return;
        if (!gridManager.IsPlacementDone()) return;
        if (isClicked || gridManager.isGameOver) return;

        skillManager.UseSkill(x, y);
        
        // 🖥️ Update UI
        gridManager.UpdateUI();

        int remaining = gridManager.CountRemainingShips(gridManager.enemyGrid);

        // 🏆 Check Win
        if (remaining == 0)
        {
            Debug.Log("YOU WIN!");
            gridManager.isGameOver = true;
            gridManager.Win.SetActive(true);
            return;
        }
        
        StartCoroutine(EndTurnDelay());
        if (isWaiting) return;
    }
    
    void OnMouseEnter()
    {
        if (isPlayerTile && gridManager.isPlacingShips)
        {
            gridManager.ShowPreview(x, y);
        }
    }
    
    IEnumerator ReturnToPosition()
    {
        yield return new WaitForSeconds(0.4f);

        // stop position
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // back to start position
        transform.position = startPosition;
    }
    
    public void TakeHit()
    {
        if (isClicked) return;

        isClicked = true;

        if (hasShip)
        {
            state = TileState.Hit;
            rend.material.color = Color.green;
        }
        else
        {
            state = TileState.Miss;
            rend.material.color = Color.red;
        }

        // physics
        if (rb != null)
        {
            rb.AddForce(Vector3.up * mass, ForceMode.Impulse);
        }

        StartCoroutine(ReturnToPosition());
    }
    
    IEnumerator EndTurnDelay()
    {
        isWaiting = true;
        yield return new WaitForSeconds(2f);
        isWaiting = false;
        gridManager.EndPlayerTurn();
    }
}
