using System;
using System.Collections.Generic;
using UnityEngine;

// 테트리미노 블록. 공중에서 잠깐 멈췄다가 떨어지고, 착지하면 지형이 된다.
[RequireComponent(typeof(Rigidbody))]
public class FallingBlock : MonoBehaviour
{
    enum State { Warning, Falling, Landed }

    [Tooltip("블록을 이루는 칸 (pivot 칸 기준, x/z)")]
    [SerializeField] private Vector2Int[] shapeCells;

    [Tooltip("낙하 가속도")]
    [SerializeField] private float gravity = 25f;

    [Tooltip("최대 낙하 속도")]
    [SerializeField] private float maxFallSpeed = 18f;

    Rigidbody rb;
    FieldGrid field;
    GameObject marker;
    Material markerMaterial;
    Vector2Int[] cells;
    State state;
    float landY;
    float warnTimer;
    float fallSpeed;
    int landingLevel;

    public event Action<FallingBlock> Landed;

    public bool IsDangerous => state != State.Landed;
    public Vector2Int[] Cells => cells;
    public Vector2Int[] ShapeCells => shapeCells;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    // 회전(90도 단위)을 적용한 칸 좌표. 스포너가 미리 계산할 때도 쓴다.
    public static Vector2Int[] RotateCells(Vector2Int[] shape, int rotationSteps)
    {
        Quaternion rot = Quaternion.Euler(0f, 90f * rotationSteps, 0f);
        var result = new Vector2Int[shape.Length];
        for (int i = 0; i < shape.Length; i++)
        {
            Vector3 v = rot * new Vector3(shape[i].x, 0f, shape[i].y);
            result[i] = new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.z));
        }
        return result;
    }

    public void Init(FieldGrid field, Vector2Int originCell, int rotationSteps, int landingLevel,
        float spawnY, float warnTime, Material markerMaterial)
    {
        this.field = field;
        this.landingLevel = landingLevel;
        this.markerMaterial = markerMaterial;

        Vector2Int[] rotated = RotateCells(shapeCells, rotationSteps);
        cells = new Vector2Int[rotated.Length];
        for (int i = 0; i < rotated.Length; i++)
            cells[i] = originCell + rotated[i];

        landY = landingLevel * field.CellSize + field.CellSize * 0.5f;
        transform.SetPositionAndRotation(
            field.CellToWorld(originCell) + Vector3.up * spawnY,
            Quaternion.Euler(0f, 90f * rotationSteps, 0f));
        rb.position = transform.position;
        rb.rotation = transform.rotation;

        warnTimer = warnTime;
        state = State.Warning;
        CreateMarker();
    }

    void FixedUpdate()
    {
        if (state == State.Warning)
        {
            warnTimer -= Time.fixedDeltaTime;
            if (warnTimer <= 0f)
                state = State.Falling;
            return;
        }

        if (state != State.Falling) return;

        fallSpeed = Mathf.Min(fallSpeed + gravity * Time.fixedDeltaTime, maxFallSpeed);
        Vector3 pos = rb.position;
        pos.y -= fallSpeed * Time.fixedDeltaTime;

        if (pos.y <= landY)
        {
            pos.y = landY;
            rb.MovePosition(pos);
            Land();
            return;
        }
        rb.MovePosition(pos);
    }

    void Update()
    {
        if (marker == null) return;

        // 착지가 가까울수록 빠르게 깜빡인다
        float distance = Mathf.Max(0f, transform.position.y - landY);
        float speed = Mathf.Lerp(18f, 5f, Mathf.Clamp01(distance / 10f));
        float alpha = Mathf.Lerp(0.55f, 1f, (Mathf.Sin(Time.time * speed) + 1f) * 0.5f);
        markerMaterial.color = new Color(1f, 1f, 1f, alpha);
    }

    void Land()
    {
        state = State.Landed;
        field.Occupy(cells, landingLevel);
        DestroyMarker();
        Landed?.Invoke(this);
    }

    void OnDestroy()
    {
        DestroyMarker();
    }

    void DestroyMarker()
    {
        if (marker == null) return;
        Destroy(marker.GetComponent<MeshFilter>().sharedMesh);
        Destroy(markerMaterial);
        Destroy(marker);
        marker = null;
    }

    // 착지할 칸에 반투명 그림자 + 빨간 외곽선 메시를 만든다
    void CreateMarker()
    {
        const float outline = 0.14f;
        float y = landingLevel * field.CellSize + 0.02f;
        float half = field.CellSize * 0.5f;
        var cellSet = new HashSet<Vector2Int>(cells);
        Color shadow = new Color(0f, 0f, 0f, 0.45f);
        Color red = new Color(1f, 0f, 0f, 1f);

        var verts = new List<Vector3>();
        var colors = new List<Color>();
        var tris = new List<int>();

        void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color, float lift)
        {
            int start = verts.Count;
            Vector3 up = Vector3.up * lift;
            verts.Add(a + up); verts.Add(b + up); verts.Add(c + up); verts.Add(d + up);
            for (int i = 0; i < 4; i++) colors.Add(color);
            tris.Add(start); tris.Add(start + 1); tris.Add(start + 2);
            tris.Add(start); tris.Add(start + 2); tris.Add(start + 3);
        }

        foreach (Vector2Int c in cells)
        {
            Vector3 p = field.CellToWorld(c);
            p.y = y;
            Vector3 bl = p + new Vector3(-half, 0f, -half);
            Vector3 tl = p + new Vector3(-half, 0f, half);
            Vector3 tr = p + new Vector3(half, 0f, half);
            Vector3 br = p + new Vector3(half, 0f, -half);
            AddQuad(bl, tl, tr, br, shadow, 0f);

            // 이웃 칸이 블록에 없으면 그 변이 외곽선
            if (!cellSet.Contains(c + Vector2Int.left))
                AddQuad(bl, tl, tl + Vector3.right * outline, bl + Vector3.right * outline, red, 0.005f);
            if (!cellSet.Contains(c + Vector2Int.right))
                AddQuad(br - Vector3.right * outline, tr - Vector3.right * outline, tr, br, red, 0.005f);
            if (!cellSet.Contains(c + Vector2Int.up))
                AddQuad(tl - Vector3.forward * outline, tl, tr, tr - Vector3.forward * outline, red, 0.005f);
            if (!cellSet.Contains(c + Vector2Int.down))
                AddQuad(bl, bl + Vector3.forward * outline, br + Vector3.forward * outline, br, red, 0.005f);
        }

        var mesh = new Mesh { name = "LandingMarker" };
        mesh.SetVertices(verts);
        mesh.SetColors(colors);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();

        marker = new GameObject("LandingMarker");
        marker.AddComponent<MeshFilter>().sharedMesh = mesh;
        var mr = marker.AddComponent<MeshRenderer>();
        markerMaterial = new Material(markerMaterial);
        mr.sharedMaterial = markerMaterial;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
    }
}
