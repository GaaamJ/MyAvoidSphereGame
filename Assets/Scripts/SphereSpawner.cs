using UnityEngine;

// 일정 간격으로 공중의 랜덤 위치에 Sphere를 생성한다.
public class SphereSpawner : MonoBehaviour
{
    [Tooltip("생성 간격 (초)")]
    public float spawnInterval = 3f;

    [Tooltip("생성 범위 (Ground 중심 기준 반경, X/Z)")]
    public float spawnRange = 4.5f;

    [Tooltip("생성 높이")]
    public float spawnHeight = 10f;

    [Tooltip("Sphere 크기")]
    public float sphereScale = 1f;

    [Tooltip("생성 후 자동 삭제까지 걸리는 시간 (초)")]
    public float sphereLifetime = 8f;

    public Material sphereMaterial;

    float timer;

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer -= spawnInterval;
            SpawnSphere();
        }
    }

    void SpawnSphere()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Sphere";
        sphere.tag = "Obstacle";
        sphere.transform.position = new Vector3(
            Random.Range(-spawnRange, spawnRange),
            spawnHeight,
            Random.Range(-spawnRange, spawnRange));
        sphere.transform.localScale = Vector3.one * sphereScale;

        if (sphereMaterial != null)
            sphere.GetComponent<Renderer>().sharedMaterial = sphereMaterial;

        sphere.AddComponent<Rigidbody>();
        Destroy(sphere, sphereLifetime);
    }
}
