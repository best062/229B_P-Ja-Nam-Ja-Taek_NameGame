using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditController : MonoBehaviour
{
    public void OnCreditEnd()
    {
        SceneManager.LoadScene("Start");
    }
}
