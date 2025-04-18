using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, -5);
    
    [Header("Camera Settings")]
    public float smoothTime = 0.3f;
    public float rotationSmoothTime = 0.5f;
    
    private Vector3 velocity = Vector3.zero;
    private Vector3 currentUp = Vector3.up;
    private Quaternion targetRotation;
    private float rotationVelocity;
    
    private void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogError("CameraController: No target assigned and no player found!");
            }
        }
        
        targetRotation = transform.rotation;
    }
    
    private void LateUpdate()
    {
        if (target == null)
            return;
        
        // Calculate camera position based on the current up direction
        Vector3 relativeOffset = Quaternion.FromToRotation(Vector3.up, currentUp) * offset;
        Vector3 targetPosition = target.position + relativeOffset;
        
        // Smooth camera position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        
        // Smooth camera rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / rotationSmoothTime);
        
        // Make sure camera is looking at the target
        Vector3 lookDirection = target.position - transform.position;
        if (lookDirection.magnitude > 0.001f)
        {
            transform.forward = lookDirection.normalized;
        }
    }
    
    public void OnGravityDirectionChanged(Vector3 newUp)
    {
        currentUp = newUp;
        
        // Calculate target rotation based on the new up direction
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, newUp).normalized;
        if (forward.magnitude < 0.001f)
        {
            forward = Vector3.ProjectOnPlane(Vector3.forward, newUp).normalized;
        }
        
        targetRotation = Quaternion.LookRotation(forward, newUp);
    }
}