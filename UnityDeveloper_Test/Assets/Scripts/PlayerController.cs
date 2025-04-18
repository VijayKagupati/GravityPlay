using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float jumpForce = 5f;
    public LayerMask groundLayers;
    public float groundCheckDistance = 0.2f;
    
    [Header("References")]
    public Transform modelTransform;
    private Rigidbody rb;
    private Animator animator;
    private bool isGrounded;
    private Vector3 moveDirection;
    private float timeSinceLastGrounded = 0f;
    
    // Gravity variables
    private Vector3 gravityDirection = Vector3.down;
    private Vector3 selectedGravityDirection = Vector3.down;
    
    private const string ANIM_PARAM_SPEED = "Speed";
    private const string ANIM_PARAM_GROUNDED = "Grounded";
    private const string ANIM_TRIGGER_JUMP = "Jump";
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        
        if (modelTransform == null && transform.childCount > 0)
            modelTransform = transform.GetChild(0);
            
        if (animator == null)
        {
            Debug.LogWarning("No Animator component found on player or its children. Animations won't work.");
        }
        
        // Disable Unity's default gravity so we can control it
        Physics.gravity = Vector3.zero;
        rb.useGravity = false;
    }
    
    private void Update()
    {
        CheckGrounded();
        HandleMovementInput();
        HandleGravityInput();
        
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
        
        UpdateAnimator();
    }
    
    private void UpdateAnimator()
    {
        if (animator != null)
        {
            animator.SetFloat(ANIM_PARAM_SPEED, moveDirection.magnitude);
            
            animator.SetBool(ANIM_PARAM_GROUNDED, isGrounded);
        }
    }
    
    private void FixedUpdate()
    {
        Move();
        ApplyGravity();
    }
    
    private void CheckGrounded()
    { 
        RaycastHit hit;
        Vector3 rayStart = transform.position + transform.up * 0.1f;
        
        if (Physics.Raycast(rayStart, -transform.up, out hit, groundCheckDistance + 0.1f, groundLayers))
        {
            isGrounded = true;
            timeSinceLastGrounded = 0;
        }
        else
        {
            isGrounded = false;
            timeSinceLastGrounded += Time.deltaTime;
        }
    }
    
    private void HandleMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");  
        
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        
        // Project camera directions onto the plane perpendicular to gravity
        cameraForward = Vector3.ProjectOnPlane(cameraForward, -gravityDirection).normalized;
        cameraRight = Vector3.ProjectOnPlane(cameraRight, -gravityDirection).normalized;
        
        moveDirection = cameraForward * vertical + cameraRight * horizontal;
        
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }
    }
    
    private void HandleGravityInput()
    {
        // Local directional vectors relative to the player's current orientation
        Vector3 localForward = transform.forward;
        Vector3 localRight = transform.right;
        Vector3 localUp = transform.up;
        
        // Select gravity direction with arrow keys based on character's local orientation
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedGravityDirection = -localForward; // Down in forward direction
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedGravityDirection = localForward; // Down in backward direction
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedGravityDirection = localRight; // Down in right direction
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedGravityDirection = -localRight; // Down in left direction
        }
        else if (Input.GetKeyDown(KeyCode.PageUp))
        {
            selectedGravityDirection = -localUp; // Down in up direction
        }
        else if (Input.GetKeyDown(KeyCode.PageDown))
        {
            selectedGravityDirection = localUp; // Down in down direction
        }
        
        // Apply the new gravity direction ONLY when Enter is pressed
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            gravityDirection = selectedGravityDirection.normalized;
            ChangeOrientation(-gravityDirection);
            
            // Notify camera about gravity change
            CameraController cameraController = Camera.main.GetComponent<CameraController>();
            if (cameraController != null)
            {
                cameraController.OnGravityDirectionChanged(-gravityDirection);
            }
        }
    }
    
    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            // Apply gravity force
            rb.AddForce(gravityDirection * 9.81f, ForceMode.Acceleration);
        }
    }
    
    private void Move()
    {
        Vector3 targetVelocity = moveDirection * moveSpeed;
        
        // Instead of just y, preserve velocity in the direction of gravity
        float gravityVelocity = Vector3.Dot(rb.velocity, gravityDirection);
        Vector3 gravityComponent = gravityDirection * gravityVelocity;
        
        // Set velocity with preserved gravity component
        rb.velocity = targetVelocity + gravityComponent;
        
        if (moveDirection.magnitude > 0.1f && modelTransform != null)
        {
            // Create a rotation that accounts for the current up direction
            Vector3 forward = Vector3.ProjectOnPlane(moveDirection, -gravityDirection).normalized;
            if (forward.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(forward, -gravityDirection);
                modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
    
    private void Jump()
    {
        rb.AddForce(-gravityDirection * jumpForce, ForceMode.Impulse);
        
        if (animator != null)
        {
            animator.SetTrigger(ANIM_TRIGGER_JUMP);
        }
    }
    
    public void ChangeOrientation(Vector3 newUp)
    {
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, newUp) * transform.rotation;
        transform.rotation = targetRotation;
    }
}