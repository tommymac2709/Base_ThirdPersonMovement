using UnityEngine;

public class PauseMenuWindowController : WindowController
{
    protected override void Subscribe()
    {
        GameObject player = GameObject.FindWithTag("Player");
        player.GetComponent<InputReader>().CancelWindowEvent += ToggleWindow;
        gameObject.SetActive(false);
    }

    protected override void Unsubscribe()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            InputReader inputReader = player.GetComponent<InputReader>();
            if (inputReader != null)
            {
                inputReader.CancelWindowEvent -= ToggleWindow;
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
