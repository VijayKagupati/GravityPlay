using UnityEngine;

public class CollectibleCube : MonoBehaviour
{
    [Header("References")]
    public AudioSource audioSource;
    public AudioClip collectSound;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }
    
    private void Collect()
    {
        // Play collection sound
        if (audioSource != null && collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }
        
        // Notify GameManager that a cube was collected
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CubeCollected();
        }
        
        // Hide the cube
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        
        // Destroy after sound finishes playing
        Destroy(gameObject, collectSound != null ? collectSound.length : 0f);
    }
}