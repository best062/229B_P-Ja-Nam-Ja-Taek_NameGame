using UnityEngine;

public class Bullet : MonoBehaviour
{
    private bool hasHit = false;
    public SkillManager skillManager;
    public SkillManager.SkillType skillType;
    public GridManager gridManager;
    public SkillManager.SkillType currentSkill;

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
                    Instantiate(gridManager.hitEffectPrefab, transform.position , Quaternion.identity);
                    t.TakeHit();
                    t.gridManager.StartCoroutine(t.gridManager.EndTurnWithDelay(1f));
                    break;

                case SkillManager.SkillType.Bomb:
                    skillManager.BombAttack(t.x, t.y);
                    skillManager.SetNormal();
                    t.gridManager.StartCoroutine(t.gridManager.EndTurnWithDelay(1.2f));
                    break;

                case SkillManager.SkillType.Scan:
                    skillManager.Scan(t.x, t.y);
                    skillManager.SetNormal();
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