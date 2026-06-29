using UnityEngine;

public class SceneHandler : MonoBehaviour
{
    public void LoadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }
}
