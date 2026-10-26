# 코드 리뷰: Rotator.cs

## 처음 코드

```csharp
public class Rotator : MonoBehaviour
{
    [Tooltip("초당 회전 각도 (도)")]
    public float rotationSpeed = 90f;

    void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
    }
}
```

## 변수 노출 방식

Claude는 `public`으로 열어뒀다. Inspector에 보이긴 하지만 다른 스크립트에서도 아무나 값을 바꿀 수 있다.

- 캡슐화 때문에 `[SerializeField] private`을 선호해 변경.

```csharp
[SerializeField] private float rotationSpeed = 90f;
```

다른 스크립트에서 읽어야 하는 경우라면 그때 public을 쓰겠지만, 그래도 필드를 그냥 열기보다는 프로퍼티로 읽기만 열어둘 것 같다.

```csharp
[SerializeField] private float rotationSpeed = 90f;
public float RotationSpeed => rotationSpeed;   // readonly
```

그래서 스크립트 4개 전부 이 방식으로 바꿨다. 필드 이름은 그대로라 씬에 저장된 값(90 등)은 날아가지 않았다.

## Time.deltaTime

`Update()`는 매 프레임 호출되는데, 프레임 수는 컴퓨터마다 다르다.

- deltaTime 없이 `Rotate(0, 90, 0)` → 프레임마다 90도. 60fps면 초당 5400도, 30fps면 2700도. 컴퓨터마다 속도가 다름.
- `90 * Time.deltaTime` → 60fps면 한 프레임에 1.5도, 30fps면 3도. 어느 쪽이든 1초에 90도.

`Time.deltaTime`은 직전 프레임에 걸린 시간(초)이라서, 곱해주면 "프레임당"이 "초당"으로 바뀐다. 이동, 회전, 타이머처럼 시간에 따라 변하는 값에는 다 곱해줘야 한다. 이 프로젝트에서는 생존시간 누적이랑 Sphere 생성 타이머에도 같은 이유로 썼다.

참고로 `Space.World`는 월드 Y축 기준으로 돌린다는 뜻. 큐브가 기울어져 있어도 바닥 기준으로 돈다.

[← 목록](DOCS.md)
