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

    [Header("찌그러짐")]
    [Tooltip("보이는 캡슐 (콜라이더는 그대로 두고 이것만 늘이고 줄인다)")]
    [SerializeField] private Transform visual;
    [Tooltip("점프할 때 모양 (가로, 세로)")]
    [SerializeField] private Vector2 jumpStretch = new Vector2(0.8f, 1.25f);
    [Tooltip("착지할 때 모양 (가로, 세로)")]
    [SerializeField] private Vector2 landSquash = new Vector2(1.25f, 0.75f);
    [Tooltip("이 속도보다 빠르게 떨어지다 착지하면 찌그러짐")]
    [SerializeField] private float squashMinFallSpeed = 2f;
    [Tooltip("원래 모양으로 돌아오는 빠르기")]
    [SerializeField] private float squashRecover = 12f;

    Rigidbody rb;
    CapsuleCollider capsule;
    Vector2 moveInput;
    Vector3 visualScale = Vector3.one;
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
        UpdateVisual();

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

        bool wasGrounded = isGrounded;
        float fallSpeed = -rb.linearVelocity.y;
        CheckGround();
        if (isGrounded && !wasGrounded && fallSpeed > squashMinFallSpeed)
            SetVisualShape(landSquash);

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
        SetVisualShape(jumpStretch);

        lastJumpPressedTime = -1f;
        lastGroundedTime = -1f;
    }

    void SetVisualShape(Vector2 shape)
    {
        visualScale = new Vector3(shape.x, shape.y, shape.x);
    }

    // 모양을 원래대로 되돌리면서, 발바닥이 바닥에 붙어 있도록 위치를 보정한다
    void UpdateVisual()
    {
        if (visual == null) return;

        visualScale = Vector3.Lerp(visualScale, Vector3.one, 1f - Mathf.Exp(-squashRecover * Time.deltaTime));
        visual.localScale = visualScale;
        float halfHeight = capsule.height * 0.5f;
        visual.localPosition = new Vector3(0f, -(1f - visualScale.y) * halfHeight, 0f);
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

        IFallingHazard hazard = collision.collider.GetComponentInParent<IFallingHazard>();
        if (hazard == null || !hazard.IsDangerous) return;

        isDead = true;
        rb.isKinematic = true;
        GameManager.Instance.GameOver();
    }
}
