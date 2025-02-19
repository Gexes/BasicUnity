using UnityEngine;

public class DeathArea : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Ensure only the player is affected
        {
            other.transform.position = respawnPoint.position;

            // Reset player's velocity to prevent momentum carry-over
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }
        }
    }
}
