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
    }
    
    private void Update()
    {
        CheckGrounded();
        HandleMovementInput();
        
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
    }
    
    private void CheckGrounded()
    { 
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        
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
        
        cameraForward.y = 0;
        cameraRight.y = 0;
        
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        moveDirection = cameraForward * vertical + cameraRight * horizontal;
        
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }
    }
    
    private void Move()
    {
        Vector3 targetVelocity = moveDirection * moveSpeed;
        
        targetVelocity.y = rb.velocity.y;
        
        rb.velocity = targetVelocity;
        
        if (moveDirection.magnitude > 0.1f && modelTransform != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    private void Jump()
    {
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        
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