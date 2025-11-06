using UnityEngine;

public class PickupMana : MonoBehaviour
{
    // Amount of XP this pickup grants
    [SerializeField] private float manaAmount = 50f;

    //If we hit the player, try to get the playerprogression module
    void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<MistInteractive.ThirdPerson.Player.PlayerStateMachine>();
        if (player != null)
        {
            var manaModule = player.GetModule<MistInteractive.ThirdPerson.Stats.PlayerManaModule>();

            // Add XP to the player's progression module
            manaModule?.Restore(manaAmount);
            // Destroy the pickup after granting XP
            Destroy(gameObject);

        }
    }
}
