using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, -5);
    
    [Header("Camera Settings")]
    public float positionSmoothTime = 0.3f;
    public float rotationSmoothTime = 0.5f;
    
    // Smoothing variables
    private Vector3 _positionVelocity = Vector3.zero;
    private Vector3 _smoothedPlayerForward;
    private Vector3 _smoothedPlayerUp;
    private Vector3 _forwardVelocity;
    private Vector3 _upVelocity;
    private float _orientationSmoothTime = 0.2f;
    
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
                return;
            }
        }
        
        _smoothedPlayerForward = target.forward;
        _smoothedPlayerUp = target.up;
    }
    
    private void LateUpdate()
    {
        if (target == null)
            return;
        
        // Smooth player orientation changes
        _smoothedPlayerForward = Vector3.SmoothDamp(_smoothedPlayerForward, target.forward, 
                                                   ref _forwardVelocity, _orientationSmoothTime);
        _smoothedPlayerUp = Vector3.SmoothDamp(_smoothedPlayerUp, target.up, 
                                              ref _upVelocity, _orientationSmoothTime);
        
        // Create rotation based on smoothed directions
        Quaternion smoothedRotation = Quaternion.LookRotation(_smoothedPlayerForward, _smoothedPlayerUp);
        
        // Position camera relative to player with proper orientation
        Vector3 targetPosition = target.position + smoothedRotation * offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, 
                                              ref _positionVelocity, positionSmoothTime);
        
        // Point camera at target
        Vector3 lookDirection = target.position - transform.position;
        if (lookDirection.magnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, _smoothedPlayerUp);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 
                                                Time.deltaTime / rotationSmoothTime);
        }
    }
}