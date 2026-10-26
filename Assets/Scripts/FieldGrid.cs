using UnityEngine;

// 10x10 필드를 1x1 칸으로 나누고, 칸마다 쌓인 블록 높이를 기록한다.
public class FieldGrid : MonoBehaviour
{
    [Tooltip("한 변의 칸 수")]
    [SerializeField] private int size = 10;

    [Tooltip("한 칸의 크기")]
    [SerializeField] private float cellSize = 1f;

    int[,] heights;

    public int Size => size;
    public float CellSize => cellSize;

    void Awake()
    {
        heights = new int[size, size];
    }

    public bool IsInside(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < size && cell.y >= 0 && cell.y < size;
    }

    public int GetHeight(Vector2Int cell)
    {
        return IsInside(cell) ? heights[cell.x, cell.y] : 0;
    }

    // 블록이 이 칸들 위에 떨어지면 몇 층에 멈추는지 (가장 높은 칸 기준)
    public int GetLandingLevel(Vector2Int[] cells)
    {
        int level = 0;
        foreach (Vector2Int c in cells)
            level = Mathf.Max(level, GetHeight(c));
        return level;
    }

    // 블록 한 칸이 level 층에 놓였다. 빈칸은 채워지므로 칸마다 높이는 쌓인 개수와 같다.
    public void OccupyCell(Vector2Int cell, int level)
    {
        if (IsInside(cell))
            heights[cell.x, cell.y] = Mathf.Max(heights[cell.x, cell.y], level + 1);
    }

    // 칸 중심의 월드 좌표 (y는 0)
    public Vector3 CellToWorld(Vector2Int cell)
    {
        float origin = -size * cellSize * 0.5f + cellSize * 0.5f;
        return transform.position + new Vector3(origin + cell.x * cellSize, 0f, origin + cell.y * cellSize);
    }

    public Vector2Int WorldToCell(Vector3 world)
    {
        Vector3 local = world - transform.position;
        float half = size * cellSize * 0.5f;
        return new Vector2Int(
            Mathf.FloorToInt((local.x + half) / cellSize),
            Mathf.FloorToInt((local.z + half) / cellSize));
    }
}
