using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, -5);
    
    [Header("Camera Settings")]
    public float smoothTime = 0.3f;
    
    private Vector3 velocity = Vector3.zero;
    
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
    }
    
    private void LateUpdate()
    {
        if (target == null)
            return;
        
        Vector3 targetPosition = target.position;
        Vector3 desiredPosition = targetPosition + offset;
        
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        
        transform.LookAt(targetPosition);
    }
}