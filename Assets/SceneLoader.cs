using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Load the next scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("DemoSceneTwo");
        }
    }
}
