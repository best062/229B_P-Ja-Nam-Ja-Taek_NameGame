using UnityEngine;
using System.Collections;

public class SkillManager : MonoBehaviour
{
    public GridManager gridManager;

    public enum SkillType
    {
        Normal,
        Bomb,
        Scan
    }

    public SkillType currentSkill = SkillType.Normal;

    public int bombCount = 2;
    public int scanCount = 3;

    
    // SET SKILL
    public void SetNormal()
    {
        currentSkill = SkillType.Normal;
    }

    public void SetBomb()
    {
        if (bombCount <= 0) return;

        currentSkill = SkillType.Bomb;
    }

    public void SetScan()
    {
        if (scanCount <= 0) return;

        currentSkill = SkillType.Scan;
    }

    
    // USE SKILL
    public void UseSkill(int x, int y)
    {
        switch (currentSkill)
        {
            case SkillType.Normal:
                gridManager.enemyGrid[x, y].TakeHit();
                gridManager.StartCoroutine(EndTurnDelay());
                break;

            case SkillType.Bomb:
                BombAttack(x, y);
                bombCount--;
                currentSkill = SkillType.Normal;
                gridManager.StartCoroutine(EndTurnDelay());
                break;

            case SkillType.Scan:
                Scan(x, y);
                scanCount--;
                currentSkill = SkillType.Normal;
                break;
        }
    }

    
    // BOMB
    void BombAttack(int x, int y)
    {
        int[,] dirs = new int[,]
        {
            {0,0},
            {1,0},
            {-1,0},
            {0,1},
            {0,-1}
        };

        for (int i = 0; i < dirs.GetLength(0); i++)
        {
            int nx = x + dirs[i, 0];
            int ny = y + dirs[i, 1];

            if (nx >= 0 && nx < gridManager.width &&
                ny >= 0 && ny < gridManager.height)
            {
                gridManager.enemyGrid[nx, ny].TakeHit();
            }
        }
    }

    
    // SCAN
    void Scan(int x, int y)
    {
        Tile t = gridManager.enemyGrid[x, y];

        if (t.hasShip)
        {
            t.GetComponent<Renderer>().material.color = Color.yellow;
        }
        else
        {
            t.GetComponent<Renderer>().material.color = Color.black;
        }

        gridManager.StartCoroutine(ResetScanColor(t));
    }

    IEnumerator ResetScanColor(Tile t)
    {
        yield return new WaitForSeconds(1f);

        if (!t.isClicked)
        {
            t.ResetColor();
        }
    }

    
    // DELAY
    IEnumerator EndTurnDelay()
    {
        yield return new WaitForSeconds(2f);
        gridManager.EndPlayerTurn();
    }
}