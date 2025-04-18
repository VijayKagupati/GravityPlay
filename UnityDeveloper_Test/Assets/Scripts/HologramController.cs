using UnityEngine;

public class HologramController : MonoBehaviour
{
    [Header("Preview Settings")]
    public Transform player;
    public GameObject hologramPrefab;
    public float previewDistance = 3f;
    
    private GameObject _hologramInstance;
    private Vector3 _currentPreviewDirection;
    private bool _showingPreview = false;
    
    private void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        
        if (hologramPrefab == null)
        {
            // Debug.LogError("No hologram prefab assigned. Please assign a prefab that looks like the player.");
            return;
        }
        
        // Create and initialize the hologram instance
        _hologramInstance = Instantiate(hologramPrefab);
        _hologramInstance.name = "GravityPreviewHologram";
        
        DisableComponents(_hologramInstance);
        _hologramInstance.SetActive(false);
    }
    
    private void Update()
    {
        if (player == null || _hologramInstance == null)
            return;
        
        Vector3 previewDirection = Vector3.zero;
        bool keyPressed = false;
        
        // Get player's local directions for orientation
        Vector3 localForward = player.forward;
        Vector3 localRight = player.right;
        Vector3 localUp = player.up;
        
        // Check for direction keys to show preview
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            previewDirection = localForward;
            keyPressed = true;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            previewDirection = -localForward;
            keyPressed = true;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            previewDirection = -localRight;
            keyPressed = true;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            previewDirection = localRight;
            keyPressed = true;
        }
        else if (Input.GetKeyDown(KeyCode.PageUp))
        {
            previewDirection = -localUp;
            keyPressed = true;
        }
        else if (Input.GetKeyDown(KeyCode.PageDown))
        {
            previewDirection = localUp;
            keyPressed = true;
        }
        
        if (keyPressed)
        {
            ShowPreview(previewDirection);
        }
        
        if (_showingPreview)
        {
            UpdateHologramPosition();
        }
        
        // Hide preview when Enter is pressed (gravity direction is confirmed)
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && _showingPreview)
        {
            HidePreview();
        }
    }
    
    private void ShowPreview(Vector3 direction)
    {
        _currentPreviewDirection = direction.normalized;
        _showingPreview = true;
        _hologramInstance.SetActive(true);
        UpdateHologramPosition();
    }
    
    private void UpdateHologramPosition()
    {
        // Position the hologram at a distance in the preview direction
        Vector3 previewPosition = player.position + _currentPreviewDirection * previewDistance;
        
        // Calculate new orientation based on the gravity direction
        Vector3 newUp = -_currentPreviewDirection;
        Vector3 newForward = Vector3.ProjectOnPlane(player.forward, newUp).normalized;
        if (newForward.magnitude < 0.001f)
        {
            // Fallback if forward is too close to gravity direction
            newForward = Vector3.ProjectOnPlane(Vector3.forward, newUp).normalized;
        }
        
        // Create rotation that matches how player would look with new gravity
        Quaternion targetRotation = Quaternion.FromToRotation(player.up, newUp) * player.rotation;
        
        _hologramInstance.transform.position = previewPosition;
        _hologramInstance.transform.rotation = targetRotation;
    }
    
    private void HidePreview()
    {
        _showingPreview = false;
        _hologramInstance.SetActive(false);
    }
    
    private void DisableComponents(GameObject obj)
    {
        // Make hologram non-physical by disabling physics components
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        
        // Disable all colliders to prevent interaction
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }
    
    private void OnDestroy()
    {
        if (_hologramInstance != null)
        {
            Destroy(_hologramInstance);
        }
    }
}