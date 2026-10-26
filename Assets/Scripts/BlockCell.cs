using System;
using UnityEngine;

// 착지한 블록에서 떨어져 나온 한 칸. 밑이 비어 있으면 그 자리까지 떨어진다.
[RequireComponent(typeof(Rigidbody))]
public class BlockCell : MonoBehaviour, IFallingHazard
{
    // 블록 프리팹과 같은 크기 (칸 사이 틈이 살짝 보이게)
    const float VisualScale = 0.96f;

    Rigidbody rb;
    Action onSettled;
    float targetY;
    float gravity;
    float maxFallSpeed;
    float fallSpeed;
    bool falling;

    public bool IsDangerous => falling;

    public static BlockCell Create(Vector3 position, Material material, float cellSize)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "BlockCell";
        go.transform.SetPositionAndRotation(position, Quaternion.identity);
        go.transform.localScale = Vector3.one * cellSize * VisualScale;
        // 보이는 크기는 0.96이어도 충돌은 딱 1칸
        go.GetComponent<BoxCollider>().size = Vector3.one / VisualScale;
        go.GetComponent<MeshRenderer>().sharedMaterial = material;
        go.AddComponent<Rigidbody>();
        return go.AddComponent<BlockCell>();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void Drop(float targetY, float gravity, float maxFallSpeed, Action onSettled)
    {
        this.targetY = targetY;
        this.gravity = gravity;
        this.maxFallSpeed = maxFallSpeed;
        this.onSettled = onSettled;
        falling = true;
    }

    void FixedUpdate()
    {
        if (!falling) return;

        fallSpeed = Mathf.Min(fallSpeed + gravity * Time.fixedDeltaTime, maxFallSpeed);
        Vector3 pos = rb.position;
        pos.y -= fallSpeed * Time.fixedDeltaTime;

        if (pos.y <= targetY)
        {
            pos.y = targetY;
            falling = false;
            rb.MovePosition(pos);
            onSettled?.Invoke();
            return;
        }
        rb.MovePosition(pos);
    }

    // Game Over 연출
    public void Spin()
    {
        rb.interpolation = RigidbodyInterpolation.None;
        if (GetComponent<Rotator>() == null)
            gameObject.AddComponent<Rotator>();
    }
}
