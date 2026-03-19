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
    }

    // Update is called once per frame
    void OnMouseDown()
    {
        if (isClicked || gridManager.isGameOver) return;

        isClicked = true;

        // color hit / miss
        if (hasShip)
        {
            rend.material.color = Color.green;

            if (gridManager.IsShipDestroyed(shipID))
            {
                Debug.Log("Ship Destroyed!");
            }
        }
        else
        {
            rend.material.color = Color.red;
        }

        // bounce
        if (rb != null)
        {
            rb.AddForce(Vector3.up * mass, ForceMode.Impulse);
        }

        // return position
        StartCoroutine(ReturnToPosition());

        // check win
        int remaining = gridManager.CountRemainingShips();

        if (remaining == 0)
        {
            Debug.Log("YOU WIN!");
            gridManager.isGameOver = true;
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
}
