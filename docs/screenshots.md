# 스크린샷

게임 자체에서 실패한 케이스는 없어서 성공 화면만 넣었다. 작업 중에 막혔던 건 [로그](prompt-log.md) 맨 아래에 적어둠.

### 1. 씬 구성

회색 Ground 위에 색 다른 Cube 5개, 가운데 Player(Capsule).

![씬 구성](images/01-scene-setup.png)

### 2. Scene 뷰

위에서 내려다본 배치. Ground 크기 10×10.

![Scene 뷰](images/02-scene-view.png)

### 3. Sphere 낙하

시작 후 3초쯤 첫 Sphere가 공중에서 떨어지는 중. 좌상단에 생존시간.

![Sphere 낙하](images/03-sphere-falling.png)

### 4. 충돌 → Game Over

Player 머리 위로 Sphere를 떨어뜨려서 일부러 맞춤. 시간이 4.6s에서 멈춤.

![Game Over](images/04-game-over-hit.png)

### 5. 에디터 전체

Console에 `Game Over` 한 줄, Hierarchy에 만든 오브젝트들, Inspector에 Cube1의 Rotator(Rotation Speed 90).
창 제목에 `MyAvoidSphereGame`이 보여서 작업 중인 프로젝트가 맞다는 것도 확인됨.

![에디터](images/05-editor-console.png)

[← 목록](DOCS.md)
