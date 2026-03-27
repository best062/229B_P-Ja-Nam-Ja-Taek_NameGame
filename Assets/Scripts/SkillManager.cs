using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class SkillManager : MonoBehaviour
{
    public GridManager gridManager;
    
    [Header("Settings")]
    public TMP_Text bombText;
    public TMP_Text scanText;
    public Button bombButton;
    public Button scanButton;
    public Button normalButton;

    
    public enum SkillType
    {
        Normal,
        Bomb,
        Scan
    }

    public SkillType currentSkill = SkillType.Normal;

    public int bombCount = 2;
    public int scanCount = 3;

    
    void Start()
    {
        UpdateSkillUI();
    }
    
    // SET SKILL
    public void SetNormal()
    {
        currentSkill = SkillType.Normal;
        UpdateSkillUI();
    }
    public void SetBomb()
    {
        if (bombCount <= 0) return;

        currentSkill = SkillType.Bomb;
        UpdateSkillUI();
    }
    public void SetScan()
    {
        if (scanCount <= 0) return;

        currentSkill = SkillType.Scan;
        UpdateSkillUI();
    }
    
    // USE SKILL
    public void UseSkill(int x, int y)
    {
        if (!gridManager.isPlayerTurn) return;
        
        switch (currentSkill)
        {
            case SkillType.Normal:
                gridManager.enemyGrid[x, y].TakeHit();
                break;
            
            case SkillType.Bomb:
                BombAttack(x, y);
                bombCount--;
                currentSkill = SkillType.Normal;
                UpdateSkillUI();
                break;
            
            case SkillType.Scan:
                Scan(x, y);
                scanCount--;
                currentSkill = SkillType.Normal;
                UpdateSkillUI();
                break;
        }
    } 
    
    // BOMB
    public void BombAttack(int x, int y)
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
    public void Scan(int x, int y)
    {
        Tile t = gridManager.enemyGrid[x, y];

        if (t.hasShip)
        {
            t.GetComponent<Renderer>().material.color = Color.yellow;
        }
        else
        {
            t.GetComponent<Renderer>().material.color = Color.red;
        }

        gridManager.StartCoroutine(ResetScanColor(t));
    }

    IEnumerator ResetScanColor(Tile t)
    {
        yield return new WaitForSeconds(1.5f);

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
    
    //Update UI
    public void UpdateSkillUI()
    {
        // 🔢 จำนวน
        bombText.text = "Skill: Bomb (" + bombCount + ")";
        scanText.text = "Skill: Scan (" + scanCount + ")";

        // 🔒 ปิดปุ่มเมื่อหมด
        bombButton.interactable = bombCount > 0;
        scanButton.interactable = scanCount > 0;

        // 🎨 highlight ปุ่ม
        normalButton.image.color = currentSkill == SkillType.Normal ? Color.green : Color.white;
        bombButton.image.color   = currentSkill == SkillType.Bomb   ? Color.green : Color.white;
        scanButton.image.color   = currentSkill == SkillType.Scan   ? Color.green : Color.white;
        
        //ใช้หมด
        if (bombCount <= 0)
        {
            bombButton.image.color = Color.gray;
        }

        if (scanCount <= 0)
        {
            scanButton.image.color = Color.gray;
        }
    }
}