# StackClimb

떨어지는 테트리미노 블록을 피하면서, 쌓인 블록을 밟고 최대한 높이 올라가는 Unity 프로토타입입니다.
Claude Code와 Unity MCP를 이용해 AI와 함께 만들었고, 처음에는 공 피하기 게임이었습니다.

**[브라우저에서 플레이하기](https://gaaamj.github.io/StackClimb/)** (PC 크롬·엣지 권장, 게임 화면을 한 번 클릭한 뒤 조작)

![플레이 화면](docs/images/09-climbed-height.png)

## 조작

- WASD 이동, Space 점프 (한 번에 한 칸)
- Game Over 후 R로 재시작

## 규칙

- 블록은 10×10 칸에 맞춰 떨어지고, 떨어진 자리에 그대로 쌓입니다.
- 떨어질 자리는 빨간 외곽선으로 미리 보여 줍니다.
- 떨어지는 중인 블록에 닿으면 Game Over입니다.
- 좌상단 `Height`가 점수입니다 (지금 서 있는 높이).

## 실행

Unity 6000.3.26f1 (URP)로 열고 `Assets/Scenes/SampleScene`을 실행합니다.

## 문서

작업 과정, 스크린샷, 코드 리뷰는 [docs/DOCS.md](docs/DOCS.md)에 있습니다.
