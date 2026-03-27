using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    public GridManager gridManager;
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
    bool isProcessingTurn = false;
    
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
        if (gridManager.isShooting) return;
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
        
        if (isProcessingTurn) return;
        if (!gridManager.IsPlacementDone()) return;
        if (isPlayerTile) return;
        if (!gridManager.isPlayerTurn) return;
        if (!gridManager.IsPlacementDone()) return;
        if (isClicked || gridManager.isGameOver) return;
        if (isWaiting) return;
        
        isProcessingTurn = true;
        
        gridManager.ShootBullet(transform.position);
        StartCoroutine(EndTurnDelay());
        
        // 🖥️ Update UI
        gridManager.UpdateUI();
    }
    
    void OnMouseEnter()
    {
        if (isPlayerTile && gridManager.isPlacingShips)
        {
            gridManager.ShowPreview(x, y);
        }
    }
    
    public void TakeHit()
    {
        if (isClicked) return;

        isClicked = true;
        
        StartCoroutine(Bounce());

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
        
        StartCoroutine(ReturnToPosition());
    }
    
    IEnumerator ReturnToPosition()
    {
        yield return new WaitForSeconds(0.3f);
        
        // back to start position
        transform.position = startPosition;
    }
    
    IEnumerator EndTurnDelay()
    {
        isWaiting = true;
        yield return new WaitForSeconds(2f);
        isProcessingTurn = false;
        isWaiting = false;
        gridManager.EndPlayerTurn();
    }
    
    IEnumerator Bounce()
    {
        rb.isKinematic = false; // ⭐ เปิด physics ชั่วคราว

        rb.AddForce(Vector3.up * 4f, ForceMode.Impulse);

        yield return new WaitForSeconds(0.2f);
        
        transform.position = startPosition;

        rb.isKinematic = true; // ⭐ ล็อกกลับ
    }
}