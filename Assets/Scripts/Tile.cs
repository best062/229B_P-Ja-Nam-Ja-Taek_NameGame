using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    private GridManager gridManager;
    private Renderer rend;
    private Rigidbody rb;
    private Vector3 startPosition;
    
    [Header("Settings")]
    public int x;
    public int y;
    public bool isPlayerTile;
    public bool hasShip = false;
    public bool isClicked = false;
    public int shipID = -1;
    
    [Header("Physics")]
    public float mass = 1f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rend = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();

        startPosition = transform.position;
        gridManager = FindFirstObjectByType(typeof(GridManager)) as GridManager;
        if (!isPlayerTile)
        {
            rend.material.color = Color.gray;
        }
    }

    // Update is called once per frame
    void OnMouseDown()
    {
        if (gridManager.isPlacingShips && isPlayerTile)
        {
            gridManager.PlaceShipManual(x, y);
            return;
        }
        if (isPlayerTile) return;
        if (!gridManager.isPlayerTurn) return;
        if (isClicked || gridManager.isGameOver) return;

        TakeHit();
        
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

        gridManager.EndPlayerTurn();
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
            rend.material.color = Color.green;
        }
        else
        {
            rend.material.color = Color.red;
        }

        // physics
        if (rb != null)
        {
            rb.AddForce(Vector3.up * mass, ForceMode.Impulse);
        }

        StartCoroutine(ReturnToPosition());
    }
}
