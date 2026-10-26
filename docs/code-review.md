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

Claude는 `rotationSpeed`를 `public`으로 열어 뒀습니다. Inspector에 보이긴 하지만, 다른 스크립트 어디서든 값을 바꿀 수 있게 됩니다.

저는 캡슐화를 위해 `[SerializeField] private`을 선호해서 이렇게 바꿨습니다. Inspector에는 똑같이 보이고, 밖에서는 건드릴 수 없습니다.

```csharp
[SerializeField] private float rotationSpeed = 90f;
```

다른 스크립트에서 값을 읽어야 한다면 그때는 public을 쓰겠지만, 필드를 그대로 열기보다는 읽기 전용 프로퍼티로 열 것 같습니다.

```csharp
[SerializeField] private float rotationSpeed = 90f;
public float RotationSpeed => rotationSpeed;   // 읽기만 가능
```

같은 이유로 나머지 스크립트도 모두 이 방식으로 바꿨습니다. 필드 이름은 그대로라 씬에 저장된 값(90 등)은 유지됐습니다.

## Time.deltaTime

`Update()`는 매 프레임 호출되는데, 1초에 몇 프레임이 도는지는 컴퓨터마다 다릅니다.

- deltaTime 없이 `Rotate(0, 90, 0)`을 쓰면 프레임마다 90도씩 돕니다. 60fps면 초당 5400도, 30fps면 2700도라서 컴퓨터마다 속도가 달라집니다.
- `90 * Time.deltaTime`을 쓰면 60fps에서는 한 프레임에 1.5도, 30fps에서는 3도씩 돌아서, 어느 쪽이든 1초에 90도가 됩니다.

`Time.deltaTime`은 직전 프레임에 걸린 시간(초)이라서, 곱해 주면 "프레임당" 값이 "초당" 값으로 바뀝니다. 이동, 회전, 타이머처럼 시간에 따라 변하는 값에는 모두 곱해야 합니다. 이 프로젝트에서도 생존시간과 생성 타이머에 같은 이유로 썼습니다.

참고로 `Space.World`는 월드 Y축을 기준으로 돌린다는 뜻입니다. 오브젝트가 기울어져 있어도 바닥 기준으로 돕니다.

[← 목록](DOCS.md)
