# 프롬프트·결과 로그

날짜: 2026-10-26 / Unity 6.3 (6000.3.26f1), URP

## 1. 게임 만들기

**프롬프트**

> 공 피하기 게임 프로토타입. (과제 사양 표 붙여넣음)
> 플레이어는 WASD 이동. 애매한 부분은 질문하고 확인받을 것.

**Claude가 한 일**

- 프로젝트부터 확인함. URP 프로젝트이고 Input System이 New 전용이라 `Keyboard.current`로 키 입력을 받기로 함.
- 애매한 부분 4개를 먼저 물어봄. 내가 고른 답:
  - Cube 위치 → 에디터에서 한 번만 랜덤 배치
  - Game Over 후 → Sphere 생성 중지, 떨어진 Sphere는 8초 뒤 삭제
  - 플레이어 이동 범위 → Ground(10×10) 밖으로 못 나가게
  - 시간 표시 → `Time: 12.3s`
- 스크립트 4개 작성: `Rotator`, `PlayerController`, `SphereSpawner`, `GameManager`
- MCP로 씬 구성: Ground, Cube 5개 + 머티리얼, Player, 카메라/조명, UI, `Obstacle` 태그

**확인 결과**

- [x] Sphere가 3초마다 생성되고 중력으로 떨어짐 (3초, 6초 시점에 생성 확인)
- [x] Sphere에 맞으면 Console에 `Game Over` 한 번 출력
- [x] Game Over 후 시간 멈춤 (게임 시간 9.1초일 때 생존시간 4.58초 그대로)
- [x] Game Over 후 새 Sphere 안 생김
- [x] Cube1이 Y축으로 회전, Inspector 값 90
- [x] WASD 이동은 직접 눌러봐야 함 (MCP로는 키 입력 테스트 못 함)

## 2. GitHub 올리기

**프롬프트**

> 이 프로젝트 github에 올리게 repo 만들어주셈

**Claude가 한 일**

- 공개 범위, 이름 물어봄 → Public, `MyAvoidSphereGame`
- Unity용 `.gitignore` 작성 (Library, Temp, Logs 등 제외)
- 첫 커밋 후 `gh repo create`로 생성, push

**확인 결과**

- [x] https://github.com/GaaamJ/MyAvoidSphereGame 에 올라감 (파일 170개)

## 3. 문서 작성 + 코드 수정

**프롬프트**

> docs 폴더 만들고 문서 생성. 사람이 쓴 것처럼, 짧고 읽기 쉽게.
> public 썼던데 난 캡슐화 때문에 [SerializeField]를 선호함.

**Claude가 한 일**

- 스크립트 4개의 Inspector용 `public` 필드를 `[SerializeField] private`으로 변경
- 바꾼 뒤 Inspector 값이 그대로인지 확인 (90, 5, 4.5, 3, 머티리얼/텍스트 참조 모두 유지)
- 스크린샷 찍고 이 문서들 작성

## 중간에 막혔던 것

| 상황 | 원인 | 해결 |
|---|---|---|
| TMP 기본 리소스 import 실패 | 클래스가 있는 어셈블리 이름을 잘못 씀 | 어셈블리 검색해서 다시 호출 |
| MCP 코드 실행 컴파일 에러 | `Object`가 `object`랑 헷갈림 | `GameManager.Instance`로 바꿈 |
| Play 중 시간이 거의 안 감 | Unity 창이 비활성이라 프레임이 느림 | 조금 기다렸다가 다시 확인 |
| `Rotator.cs`가 `public`으로 돌아감 | 내가 실수로 변경 사항을 discard함 | 다시 수정. Play 중 스크립트가 바뀌어서 Play도 같이 꺼졌음 |

[← 목록](DOCS.md)
