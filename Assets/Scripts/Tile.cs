using UnityEngine;

public class Tile : MonoBehaviour
{
    private GridManager gridManager;
    public int x;
    public int y;
    private Renderer rend;
    public bool hasShip = false;
    public bool isClicked = false;
    public int shipID = -1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rend = GetComponent<Renderer>();
        gridManager = FindObjectOfType<GridManager>();
    }

    // Update is called once per frame
    void OnMouseDown()
    {
        if (isClicked) return;
        isClicked = true;
        
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
        
        int remaining = gridManager.CountRemainingShips();
        Debug.Log("Remaining ships: " + remaining);
    }
}
