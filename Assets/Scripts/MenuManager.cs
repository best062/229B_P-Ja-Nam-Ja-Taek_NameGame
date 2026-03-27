using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Main"); // ⭐ ชื่อ scene เกมคุณ
    }

    public void ExitGame()
    {
        Application.Quit();

        Debug.Log("Exit Game"); // ไว้ดูใน Editor
    }
}
