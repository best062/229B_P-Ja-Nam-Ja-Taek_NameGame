using UnityEngine;
using UnityEngine.SceneManagement;

public class UIEndManager : MonoBehaviour
{
    public void GoToCredit()
    {
        SceneManager.LoadScene("End");
    }
}
