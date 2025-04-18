using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float jumpForce = 5f;
    public LayerMask groundLayers;
    public float groundCheckDistance = 0.2f;
    
    [Header("Fall Detection")]
    public float maxFallTime = 5f;  // Maximum time player can fall before game over
    
    [Header("References")]
    public Transform modelTransform;
    
    private Rigidbody _rb;
    private Animator _animator;
    private bool _isGrounded;
    private Vector3 _moveDirection;
    private float _timeSinceLastGrounded = 0f;
    
    // Gravity control
    private Vector3 _gravityDirection = Vector3.down;
    private Vector3 _selectedGravityDirection = Vector3.down;
    
    // Animation parameter names
    private string _animParamSpeed = "Speed";
    private string _animParamGrounded = "Grounded";
    private string _animTriggerJump = "Jump";
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
        
        if (modelTransform == null && transform.childCount > 0)
            modelTransform = transform.GetChild(0);
            
        if (_animator == null)
        {
            // Debug.LogWarning("No Animator component found on player or its children. Animations won't work.");
        }
        
        // Custom gravity implementation
        Physics.gravity = Vector3.zero;
        _rb.useGravity = false;
    }
    
    private void Update()
    {
        CheckGrounded();
        CheckFallDuration();
        
        // Only process input if gameplay is active
        if (IsGameplayActive())
        {
            HandleMovementInput();
            HandleGravityInput();
            
            if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
            {
                Jump();
            }
        }
        else
        {
            // Prevent movement when gameplay is inactive
            _moveDirection = Vector3.zero;
        }
        
        UpdateAnimator();
    }
    
    private void CheckFallDuration()
    {
        // Trigger game over if player falls for too long
        if (IsGameplayActive() && !_isGrounded)
        {
            if (_timeSinceLastGrounded > maxFallTime)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GameOver("You fell into the void!");
                }
            }
        }
    }
    
    private void UpdateAnimator()
    {
        if (_animator != null)
        {
            _animator.SetFloat(_animParamSpeed, _moveDirection.magnitude);
            _animator.SetBool(_animParamGrounded, _isGrounded);
        }
    }
    
    private void FixedUpdate()
    {
        if (IsGameplayActive())
        {
            Move();
            ApplyGravity();
        }
        else
        {
            // Stop all physics movement when gameplay is inactive
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }
    
    private bool IsGameplayActive()
    {
        return GameManager.Instance != null && GameManager.Instance.IsGameplayActive;
    }
    
    private void CheckGrounded()
    { 
        RaycastHit hit;
        Vector3 rayStart = transform.position + transform.up * 0.1f;
        
        if (Physics.Raycast(rayStart, -transform.up, out hit, groundCheckDistance + 0.1f, groundLayers))
        {
            _isGrounded = true;
            _timeSinceLastGrounded = 0;
        }
        else
        {
            _isGrounded = false;
            _timeSinceLastGrounded += Time.deltaTime;
        }
    }
    
    private void HandleMovementInput()
    {
        // Using direct key input for WASD movement
        float horizontal = 0f;
        float vertical = 0f;
        
        if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontal += 1f;
        if (Input.GetKey(KeyCode.W)) vertical += 1f;
        if (Input.GetKey(KeyCode.S)) vertical -= 1f;
        
        // Convert camera-relative input to world-space direction based on current gravity orientation
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        
        cameraForward = Vector3.ProjectOnPlane(cameraForward, -_gravityDirection).normalized;
        cameraRight = Vector3.ProjectOnPlane(cameraRight, -_gravityDirection).normalized;
        
        _moveDirection = cameraForward * vertical + cameraRight * horizontal;
        
        if (_moveDirection.magnitude > 1f)
        {
            _moveDirection.Normalize();
        }
    }
    
    private void HandleGravityInput()
    {
        // Get local orientation vectors
        Vector3 localForward = transform.forward;
        Vector3 localRight = transform.right;
        Vector3 localUp = transform.up;
        
        // Select gravity direction based on arrow keys
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _selectedGravityDirection = localForward;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _selectedGravityDirection = -localForward;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _selectedGravityDirection = -localRight;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _selectedGravityDirection = localRight;
        }
        else if (Input.GetKeyDown(KeyCode.PageUp))
        {
            _selectedGravityDirection = -localUp;
        }
        else if (Input.GetKeyDown(KeyCode.PageDown))
        {
            _selectedGravityDirection = localUp;
        }
        
        // Apply gravity change when Enter is pressed
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            _gravityDirection = _selectedGravityDirection.normalized;
            ChangeOrientation(-_gravityDirection);
        }
    }
    
    private void ApplyGravity()
    {
        if (!_isGrounded)
        {
            // Apply standard gravity acceleration in the current gravity direction
            _rb.AddForce(_gravityDirection * 9.81f, ForceMode.Acceleration);
        }
    }
    
    private void Move()
    {
        Vector3 targetVelocity = _moveDirection * moveSpeed;
        
        // Preserve velocity in gravity direction to maintain jump/fall physics
        float gravityVelocity = Vector3.Dot(_rb.velocity, _gravityDirection);
        Vector3 gravityComponent = _gravityDirection * gravityVelocity;
        
        _rb.velocity = targetVelocity + gravityComponent;
        
        // Rotate model to face movement direction
        if (_moveDirection.magnitude > 0.1f && modelTransform != null)
        {
            Vector3 forward = Vector3.ProjectOnPlane(_moveDirection, -_gravityDirection).normalized;
            if (forward.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(forward, -_gravityDirection);
                modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
    
    private void Jump()
    {
        _rb.AddForce(-_gravityDirection * jumpForce, ForceMode.Impulse);
        
        if (_animator != null)
        {
            _animator.SetTrigger(_animTriggerJump);
        }
    }
    
    public void ChangeOrientation(Vector3 newUp)
    {
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, newUp) * transform.rotation;
        transform.rotation = targetRotation;
    }
}