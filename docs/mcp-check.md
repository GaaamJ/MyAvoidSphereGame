# MCP 작업 확인

"연결됨" 표시만 보고 성공으로 판단하지 않고, 실제로 이 프로젝트(당시 이름 `MyAvoidSphereGame`)가 바뀌었는지 따로 확인했습니다.

## 사용한 도구

| MCP 도구 | 용도 | 조회/변경 |
|---|---|---|
| `editor/state` 리소스 | 연결된 에디터가 어떤 프로젝트인지, 컴파일 중인지 확인 | 조회 |
| `manage_scene` (get_hierarchy) | 씬 구성 확인 | 조회 |
| `refresh_unity` | 스크립트 작성 후 컴파일 | 변경 |
| `read_console` | 컴파일 에러, `Game Over` 로그 확인 | 조회 |
| `manage_editor` | `Obstacle` 태그 추가, Play/Stop | 변경 |
| `execute_code` | 오브젝트·머티리얼·UI 생성, 씬 저장, 상태값 확인 | 둘 다 |
| `manage_camera` (screenshot) | Game/Scene 뷰 캡처 | 조회 |

스크립트 파일은 MCP가 아니라 Claude Code가 직접 작성했고, MCP로는 Unity에 컴파일만 요청했습니다.

## 반영 확인

1. 연결된 에디터가 맞는 프로젝트인지
   - `editor/state` 결과가 `MyAvoidSphereGame@65040508649d3008`, 씬은 `Assets/Scenes/SampleScene.unity`였습니다.
   - [에디터 스크린샷](images/05-editor-console.png)의 창 제목도 `MyAvoidSphereGame`입니다.
2. 씬 파일에 저장됐는지
   - `SampleScene.unity`를 텍스트로 열어 `Ground`, `Cube1~5`, `Player`, `GameManager`, `SphereSpawner`, `SurvivalTimeText`, `rotationSpeed: 90`이 들어 있는 것을 확인했습니다.
3. 스크립트가 컴파일됐는지
   - `read_console`에 에러가 없었습니다.
   - `[SerializeField]`로 바꾼 뒤 리플렉션으로 확인하니, `rotationSpeed`는 private이고 값은 90 그대로였습니다.
4. 실제로 동작하는지
   - Play 후 Console에 `Game Over`가 찍혔습니다([스크린샷](images/05-editor-console.png)).
   - 생존시간, Sphere 개수, Cube 회전 각도를 Play 중에 값으로 읽어 확인했습니다([로그](prompt-log.md)).
5. Git에 들어갔는지
   - GitHub repo에 `Assets/Scripts`, `Assets/Scenes`, `Assets/Materials`가 올라간 것을 확인했습니다.

[← 목록](DOCS.md)
