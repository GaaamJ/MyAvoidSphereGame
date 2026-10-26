using UnityEngine;

// 플레이어가 올라가는 만큼 카메라도 부드럽게 따라 올라간다. 가로 위치와 각도는 고정.
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Tooltip("따라가는 데 걸리는 시간. 작을수록 빠름")]
    [SerializeField] private float smoothTime = 0.4f;

    float baseY;
    float targetBaseY;
    float velocity;

    void Start()
    {
        baseY = transform.position.y;
        targetBaseY = target.position.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float wantY = baseY + Mathf.Max(0f, target.position.y - targetBaseY);
        Vector3 pos = transform.position;
        pos.y = Mathf.SmoothDamp(pos.y, wantY, ref velocity, smoothTime);
        transform.position = pos;
    }
}
