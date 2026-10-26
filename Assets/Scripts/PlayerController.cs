using UnityEngine;
using UnityEngine.InputSystem;

// WASD로 플레이어를 이동시키고, Sphere에 닿으면 Game Over 처리한다.
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Tooltip("이동 속도 (m/s)")]
    public float moveSpeed = 5f;

    [Tooltip("이동 가능 범위 (Ground 중심 기준 반경, X/Z)")]
    public float boundary = 4.5f;

    Rigidbody rb;
    Vector3 moveInput;
    bool isDead;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    void Update()
    {
        moveInput = Vector3.zero;
        if (isDead) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.wKey.isPressed) moveInput.z += 1f;
        if (keyboard.sKey.isPressed) moveInput.z -= 1f;
        if (keyboard.dKey.isPressed) moveInput.x += 1f;
        if (keyboard.aKey.isPressed) moveInput.x -= 1f;
        moveInput = Vector3.ClampMagnitude(moveInput, 1f);
    }

    void FixedUpdate()
    {
        if (isDead) return;

        Vector3 next = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        next.x = Mathf.Clamp(next.x, -boundary, boundary);
        next.z = Mathf.Clamp(next.z, -boundary, boundary);
        rb.MovePosition(next);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;
        if (!collision.gameObject.CompareTag("Obstacle")) return;

        isDead = true;
        GameManager.Instance.GameOver();
    }
}
