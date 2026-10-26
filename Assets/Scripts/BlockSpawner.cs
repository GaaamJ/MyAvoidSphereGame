using System.Collections.Generic;
using UnityEngine;

// 블록을 어디에 떨어뜨릴지 정하고 생성한다.
// 후보 자리를 여러 개 뽑아 점수를 매기고, 가장 자연스러운 자리를 고른다.
public class BlockSpawner : MonoBehaviour
{
    [SerializeField] private FieldGrid field;
    [SerializeField] private Transform player;
    [SerializeField] private FallingBlock[] blockPrefabs;
    [SerializeField] private Material markerMaterial;

    [Header("난이도")]
    [Tooltip("시작 생성 간격 (초)")]
    [SerializeField] private float startInterval = 3f;
    [Tooltip("최소 생성 간격 (초)")]
    [SerializeField] private float minInterval = 1.5f;
    [Tooltip("최소 간격까지 걸리는 시간 (초)")]
    [SerializeField] private float rampDuration = 90f;

    [Header("낙하")]
    [Tooltip("낙하 전 공중에서 멈춰 있는 시간 (초)")]
    [SerializeField] private float warnTime = 0.8f;
    [Tooltip("착지 지점 또는 플레이어보다 얼마나 위에서 생성할지")]
    [SerializeField] private float spawnHeightAbove = 9f;

    [Header("자리 고르기")]
    [Tooltip("한 번에 비교할 후보 수")]
    [SerializeField] private int candidateCount = 12;
    [Tooltip("플레이어를 노리는 블록 비율 (0~1)")]
    [SerializeField] private float aimChance = 0.35f;

    readonly HashSet<Vector2Int> reserved = new HashSet<Vector2Int>();
    float timer;
    float elapsed;
    bool lastWasAimed;

    struct Candidate
    {
        public FallingBlock prefab;
        public Vector2Int origin;
        public int rotation;
        public Vector2Int[] cells;
        public int level;
        public float score;
    }

    void Start()
    {
        // 첫 블록은 조금 빨리
        timer = startInterval * 0.5f;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        elapsed += Time.deltaTime;
        timer += Time.deltaTime;

        float interval = Mathf.Lerp(startInterval, minInterval, elapsed / rampDuration);
        if (timer >= interval)
        {
            timer -= interval;
            Spawn();
        }
    }

    void Spawn()
    {
        Vector2Int playerCell = field.WorldToCell(player.position);
        int playerLevel = Mathf.Max(0, Mathf.RoundToInt(player.position.y - 1f));

        // 연속으로 노리지 않는다
        bool aimed = !lastWasAimed && Random.value < aimChance;

        Candidate best = default;
        bool found = false;
        for (int i = 0; i < candidateCount; i++)
        {
            if (!TryMakeCandidate(out Candidate c)) continue;
            if (aimed && !LeavesEscape(c, playerCell, playerLevel)) continue;

            c.score = Score(c, playerCell, aimed);
            if (!found || c.score < best.score)
            {
                best = c;
                found = true;
            }
        }
        if (!found) return;

        lastWasAimed = aimed;

        float spawnY = Mathf.Max(best.level, player.position.y) + spawnHeightAbove;
        // 원점에 생겼다가 옮겨지면 그 한 프레임에 플레이어와 부딪힐 수 있어서 처음부터 공중에 만든다
        Vector3 spawnPos = field.CellToWorld(best.origin) + Vector3.up * spawnY;
        Quaternion spawnRot = Quaternion.Euler(0f, 90f * best.rotation, 0f);
        FallingBlock block = Instantiate(best.prefab, spawnPos, spawnRot);
        block.Init(field, best.origin, best.rotation, best.level, spawnY, warnTime, markerMaterial);

        foreach (Vector2Int cell in best.cells)
            reserved.Add(cell);
        block.Landed += OnBlockLanded;
    }

    void OnBlockLanded(FallingBlock block)
    {
        foreach (Vector2Int cell in block.Cells)
            reserved.Remove(cell);
    }

    bool TryMakeCandidate(out Candidate c)
    {
        c = default;
        c.prefab = blockPrefabs[Random.Range(0, blockPrefabs.Length)];
        c.rotation = Random.Range(0, 4);
        c.origin = new Vector2Int(Random.Range(0, field.Size), Random.Range(0, field.Size));

        Vector2Int[] rotated = FallingBlock.RotateCells(c.prefab.ShapeCells, c.rotation);
        c.cells = new Vector2Int[rotated.Length];
        for (int i = 0; i < rotated.Length; i++)
        {
            Vector2Int cell = c.origin + rotated[i];
            // 필드 밖이거나, 다른 블록이 떨어지고 있는 칸이면 탈락
            if (!field.IsInside(cell) || reserved.Contains(cell)) return false;
            c.cells[i] = cell;
        }
        c.level = field.GetLandingLevel(c.cells);
        return true;
    }

    // 점수가 낮을수록 좋은 자리
    float Score(Candidate c, Vector2Int playerCell, bool aimed)
    {
        var cellSet = new HashSet<Vector2Int>(c.cells);

        // 주변보다 2칸 이상 높아지는 변. 많을수록 올라가기 힘든 벽
        // 밑이 빈 칸은 떨어져서 채우므로, 칸마다 (지금 높이 + 1)이 된다
        int walls = 0;
        foreach (Vector2Int cell in c.cells)
        {
            int newHeight = field.GetHeight(cell) + 1;
            foreach (Vector2Int dir in Directions)
            {
                Vector2Int n = cell + dir;
                if (!field.IsInside(n) || cellSet.Contains(n)) continue;
                if (newHeight - field.GetHeight(n) >= 2) walls++;
            }
        }

        int distance = int.MaxValue;
        foreach (Vector2Int cell in c.cells)
        {
            Vector2Int d = cell - playerCell;
            distance = Mathf.Min(distance, Mathf.Max(Mathf.Abs(d.x), Mathf.Abs(d.y)));
        }

        float score = walls * 1.5f + Random.Range(0f, 2f);
        if (aimed)
            score += distance * 4f;          // 플레이어 칸에 가까울수록 좋음
        else if (distance < 2)
            score += 8f;                     // 안 노릴 땐 플레이어 바로 옆은 피함
        return score;
    }

    // 노리는 블록이라도 피할 칸은 남긴다:
    // 플레이어 칸 상하좌우 중 블록에 안 덮이고, 1칸 이하로 오르내릴 수 있는 칸이 있어야 한다.
    bool LeavesEscape(Candidate c, Vector2Int playerCell, int playerLevel)
    {
        var cellSet = new HashSet<Vector2Int>(c.cells);
        if (!cellSet.Contains(playerCell)) return true;

        foreach (Vector2Int dir in Directions)
        {
            Vector2Int n = playerCell + dir;
            if (!field.IsInside(n) || cellSet.Contains(n) || reserved.Contains(n)) continue;
            if (Mathf.Abs(field.GetHeight(n) - playerLevel) <= 1) return true;
        }
        return false;
    }

    static readonly Vector2Int[] Directions =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };
}
