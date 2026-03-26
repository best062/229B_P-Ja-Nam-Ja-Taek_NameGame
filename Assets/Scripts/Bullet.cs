using UnityEngine;

public class Bullet : MonoBehaviour
{
    private bool hasHit = false;

    void OnCollisionEnter(Collision col)
    {
        if (hasHit) return;
        hasHit = true;

        Tile t = col.gameObject.GetComponent<Tile>();

        if (t != null)
        {
            t.TakeHit(); // ⭐ ใช้ระบบเดิม
        }

        Destroy(gameObject, 0.1f);
    }
}
