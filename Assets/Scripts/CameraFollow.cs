using UnityEngine;

// 플레이어가 올라가는 만큼 카메라도 부드럽게 따라 올라간다. 가로 위치와 각도는 고정.
// 블록이 착지하면 잠깐 흔들린다.
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Tooltip("따라가는 데 걸리는 시간. 작을수록 빠름")]
    [SerializeField] private float smoothTime = 0.4f;

    [Tooltip("흔들림이 사라지는 데 걸리는 시간 (초)")]
    [SerializeField] private float shakeDuration = 0.18f;

    Vector3 basePosition;
    float startY;
    float targetStartY;
    float velocity;
    float shakeTimer;
    float shakeStrength;

    void Start()
    {
        basePosition = transform.position;
        startY = basePosition.y;
        targetStartY = target.position.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 흔들림과 상관없이 따라가는 기준 위치는 따로 계산한다
        float wantY = startY + Mathf.Max(0f, target.position.y - targetStartY);
        basePosition.y = Mathf.SmoothDamp(basePosition.y, wantY, ref velocity, smoothTime);

        Vector3 offset = Vector3.zero;
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            float fade = Mathf.Clamp01(shakeTimer / shakeDuration);
            offset = Random.insideUnitSphere * shakeStrength * fade;
        }
        transform.position = basePosition + offset;
    }

    // 이미 흔들리는 중이면 더 센 쪽을 남긴다
    public void Shake(float strength)
    {
        float current = shakeTimer > 0f ? shakeStrength * (shakeTimer / shakeDuration) : 0f;
        shakeStrength = Mathf.Max(strength, current);
        shakeTimer = shakeDuration;
    }
}
