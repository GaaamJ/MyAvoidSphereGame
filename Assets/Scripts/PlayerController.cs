using UnityEngine;
using UnityEngine.InputSystem;

// WASD 이동, Space 점프. 떨어지는 블록에 닿으면 Game Over.
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("이동")]
    [Tooltip("최고 이동 속도 (m/s)")]
    [SerializeField] private float moveSpeed = 6f;
    [Tooltip("출발할 때 가속도")]
    [SerializeField] private float acceleration = 60f;
    [Tooltip("키를 뗐을 때 감속도")]
    [SerializeField] private float deceleration = 80f;
    [Tooltip("공중에서 가감속 비율 (0~1)")]
    [SerializeField] private float airControl = 0.6f;

    [Header("점프")]
    [Tooltip("점프 높이. 1칸(1m)은 넘고 2칸은 못 넘게")]
    [SerializeField] private float jumpHeight = 1.3f;
    [Tooltip("떨어질 때 중력 배수. 클수록 착지가 빠르고 덜 붕 뜸")]
    [SerializeField] private float fallGravityMultiplier = 2f;
    [Tooltip("발판에서 떨어진 직후에도 점프를 받아주는 시간")]
    [SerializeField] private float coyoteTime = 0.1f;
    [Tooltip("착지 직전에 누른 점프를 기억하는 시간")]
    [SerializeField] private float jumpBufferTime = 0.15f;

    [Header("바닥 체크")]
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private float groundCheckDistance = 0.15f;

    Rigidbody rb;
    CapsuleCollider capsule;
    Vector2 moveInput;
    float lastGroundedTime = -1f;
    float lastJumpPressedTime = -1f;
    bool isGrounded;
    bool isDead;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void Update()
    {
        moveInput = Vector2.zero;
        if (isDead) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.wKey.isPressed) moveInput.y += 1f;
        if (keyboard.sKey.isPressed) moveInput.y -= 1f;
        if (keyboard.dKey.isPressed) moveInput.x += 1f;
        if (keyboard.aKey.isPressed) moveInput.x -= 1f;
        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        if (keyboard.spaceKey.wasPressedThisFrame)
            lastJumpPressedTime = Time.time;
    }

    void FixedUpdate()
    {
        if (isDead) return;

        CheckGround();
        Move();
        Jump();
        ApplyExtraGravity();
    }

    void CheckGround()
    {
        float castDistance = capsule.height * 0.5f - groundCheckRadius + groundCheckDistance;
        isGrounded = Physics.SphereCast(rb.position, groundCheckRadius, Vector3.down, out _,
            castDistance, ~0, QueryTriggerInteraction.Ignore);

        if (isGrounded)
        {
            lastGroundedTime = Time.time;
            int level = Mathf.Max(0, Mathf.RoundToInt(rb.position.y - capsule.height * 0.5f));
            GameManager.Instance.SetHeight(level);
        }
    }

    void Move()
    {
        Vector3 velocity = rb.linearVelocity;
        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        Vector3 target = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;

        float rate = moveInput.sqrMagnitude > 0.01f ? acceleration : deceleration;
        if (!isGrounded) rate *= airControl;

        horizontal = Vector3.MoveTowards(horizontal, target, rate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector3(horizontal.x, velocity.y, horizontal.z);
    }

    void Jump()
    {
        bool buffered = Time.time - lastJumpPressedTime <= jumpBufferTime;
        bool canJump = Time.time - lastGroundedTime <= coyoteTime;
        if (!buffered || !canJump) return;

        float jumpVelocity = Mathf.Sqrt(2f * -Physics.gravity.y * jumpHeight);
        Vector3 v = rb.linearVelocity;
        rb.linearVelocity = new Vector3(v.x, jumpVelocity, v.z);

        lastJumpPressedTime = -1f;
        lastGroundedTime = -1f;
    }

    void ApplyExtraGravity()
    {
        if (rb.linearVelocity.y < 0f)
            rb.AddForce(Physics.gravity * (fallGravityMultiplier - 1f), ForceMode.Acceleration);
    }

    void OnCollisionEnter(Collision collision) => CheckHit(collision);
    void OnCollisionStay(Collision collision) => CheckHit(collision);

    void CheckHit(Collision collision)
    {
        if (isDead) return;

        FallingBlock block = collision.collider.GetComponentInParent<FallingBlock>();
        if (block == null || !block.IsDangerous) return;

        isDead = true;
        rb.isKinematic = true;
        GameManager.Instance.GameOver();
    }
}
