using UnityEngine;

public class PickupHealth : MonoBehaviour
{
    // Amount of XP this pickup grants
    [SerializeField] private float healthAmount = 50f;

    //If we hit the player, try to get the playerprogression module
    void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<MistInteractive.ThirdPerson.Player.PlayerStateMachine>();
        if (player != null)
        {
            var healthModule = player.GetModule<MistInteractive.ThirdPerson.Stats.PlayerHealthModule>();

            // Add XP to the player's progression module
            healthModule?.Heal(healthAmount);
            // Destroy the pickup after granting XP
            Destroy(gameObject);

        }
    }
}
