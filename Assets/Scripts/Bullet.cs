using UnityEngine;

public class Bullet : MonoBehaviour
{
    private bool hasHit = false;
    public SkillManager skillManager;
    public SkillManager.SkillType skillType;

    void OnCollisionEnter(Collision col)
    {
        if (hasHit) return;
        hasHit = true;
        
        Tile t = col.gameObject.GetComponent<Tile>();

        if (t != null)
        {
            switch (skillType)
            {
                case SkillManager.SkillType.Normal:
                    t.TakeHit();
                    break;

                case SkillManager.SkillType.Bomb:
                    skillManager.BombAttack(t.x, t.y); // ⭐ ใช้ของเดิม
                    break;

                case SkillManager.SkillType.Scan:
                    skillManager.Scan(t.x, t.y);
                    break;
            }
            t.gridManager.isShooting = false;
        }

        Destroy(gameObject); // ⭐ ต้องมี
    }
    
    void Update()
    {
        Debug.DrawRay(transform.position, Vector3.down * 2f, Color.red);
    }
}