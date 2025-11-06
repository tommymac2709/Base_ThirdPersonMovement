using UnityEngine;

public class PickupXP : MonoBehaviour
{
    // Amount of XP this pickup grants
    [SerializeField] private float xpAmount = 10f;

    //If we hit the player, try to get the playerprogression module
    void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<MistInteractive.ThirdPerson.Player.PlayerStateMachine>();
        if (player != null)
        {
            var progressionModule = player.GetModule<MistInteractive.ThirdPerson.Stats.PlayerProgressionModule>();
           
            // Add XP to the player's progression module
            progressionModule?.GainExperience(xpAmount);
            // Destroy the pickup after granting XP
            Destroy(gameObject);
            
        }
    }
}
