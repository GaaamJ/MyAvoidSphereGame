using System.Collections.Generic;
using UnityEngine;

// 블록이 착지할 때의 '쿵' 연출: 칸 가장자리 먼지 + 카메라 흔들림.
public class LandingEffects : MonoBehaviour
{
    public static LandingEffects Instance { get; private set; }

    [SerializeField] private ParticleSystem dust;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private Transform player;

    [Header("카메라 흔들림")]
    [Tooltip("플레이어 바로 옆에 떨어졌을 때 흔들림 세기")]
    [SerializeField] private float nearShake = 0.25f;
    [Tooltip("멀리 떨어졌을 때 흔들림 세기")]
    [SerializeField] private float farShake = 0.05f;
    [Tooltip("이 거리 이상이면 farShake")]
    [SerializeField] private float shakeRange = 6f;

    [Header("먼지")]
    [Tooltip("칸 하나당 먼지 개수")]
    [SerializeField] private int dustPerCell = 8;
    [Tooltip("먼지가 퍼지는 속도")]
    [SerializeField] private float dustSpeed = 2.5f;
    [Tooltip("칸 중심에서 먼지가 생기는 거리. 0.5보다 커야 블록 옆면에 안 가려짐")]
    [SerializeField] private float dustRadius = 0.6f;

    void Awake()
    {
        Instance = this;
    }

    // cellBottoms: 착지한 칸들의 바닥 중심. strength: 1이면 블록, 작으면 쪼개진 칸
    public void PlayLanding(IList<Vector3> cellBottoms, float strength)
    {
        if (cellBottoms.Count == 0) return;

        float nearest = float.MaxValue;
        foreach (Vector3 bottom in cellBottoms)
        {
            EmitDust(bottom, strength);
            if (player != null)
            {
                Vector3 d = bottom - player.position;
                d.y = 0f;
                nearest = Mathf.Min(nearest, d.magnitude);
            }
        }

        if (cameraFollow != null)
        {
            float shake = Mathf.Lerp(nearShake, farShake, Mathf.Clamp01(nearest / shakeRange));
            cameraFollow.Shake(shake * strength);
        }
    }

    void EmitDust(Vector3 bottom, float strength)
    {
        if (dust == null) return;

        int count = Mathf.Max(1, Mathf.RoundToInt(dustPerCell * strength));
        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            var emit = new ParticleSystem.EmitParams
            {
                position = bottom + dir * dustRadius + Vector3.up * 0.1f,
                velocity = dir * dustSpeed * Random.Range(0.5f, 1f) + Vector3.up * Random.Range(0.5f, 1.2f),
                applyShapeToPosition = false
            };
            dust.Emit(emit, 1);
        }
    }
}
