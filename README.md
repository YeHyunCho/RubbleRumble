<div align="center">
  <h1>RUBBLE RUMBLE - AI를 활용한 청소 게임</h1>
  <p>🎮 캐주얼, 액션, 시뮬레이션 🎮</p>
</div>

> **시연 영상 보러가기** : https://youtu.be/PX9dC6pG-e0

<div align="center">
  <img width="1920" height="1080" alt="Image" src="https://github.com/user-attachments/assets/7018174f-2ddc-4d57-becb-7a0ce24ea987" style="border-radius: 10px;" />
</div>



## 🕹️ 프로젝트 개요

**프로젝트명** : Rubble Rumble   
**프로젝트 기간** : 2025.03 ~ 2025.06   
**프로젝트 형태** : 팀 프로젝트   
**프로젝트 소개** : 인간 플레이어와 AI가 제한된 시간 내에 더 많은 쓰레기를 수거하고 처리하며 경쟁하는 3D 청소 대전 게임으로 단순한 청소를 넘어서 여러 요소를 통해 승부가 갈리는 실시간 액션 게임입니다.   
**프로젝트 흐름도**
<img width="1024" height="625" alt="Image" src="https://github.com/user-attachments/assets/600a2b2c-1c0d-4609-b1b1-9e268d26742d" />


## 🕹️게임 플레이

### 1. 플레이어

**캐릭터 이동 및 조작**   
- 키보드의 W, A, S, D 키를 사용하여 이동
- Shift 키를 누른 상태로 이동하면 속도 증가
- 속도 증가 시 먼지가 날리는 시각적인 효과(파티클) 표시
- 캐릭터는 이동하는 방향을 자연스럽게 바라보도록 회전

**도구 교체**
- 키보드의 숫자 키를 눌러 플레이어가 도구 선택 및 교체 가능
- 1(목장갑), 2(빗자루), 3(대걸레)

**아이템 줍기 및 운반**
- 맨 손 상태로 주변의 캔, 상자, 펼친 상자를 [E]를 눌러 집음
- 집어 든 아이템은 플레이어의 오른손 위치에 고정
- 이동 중 다른 물체와 부딪히거나 물리적인 영향 X

**아이템 내려놓기(작업대)**   
- 작업대 영역에서 [Q]키를 눌러 상자를 작업대에 올리기 가능

**아이템 버리기(분리수거함)**
- 분리수거함 영역 안으로 들어가 [E]키를 눌러 쓰레기 분리수거 가능
- 버려진 아이템은 종류에 따라 정해진 점수를 획득
- 펼쳐지지 않은 상자는 버리기 불가능


### 2. AI (ML-Agents)

**역할 및 목표**
- 플레이어와 동일한 게임 환경 내에서 경쟁하는 인공지능 캐릭터
- 플레이어보다 더 높은 점수를 획득하는 것을 목표로 행동

**행동 방식**
- 게임 맵을 스스로 이동하며 목표 탐색
- 플레이어와 유사한 방식의 행동 수행

   
### 3. 쓰레기 및 다른 오브젝트

게임 월드를 구성하며 플레이어나 AI가 상호 작용할 수 있는 다양한 요소들 입니다.   
각 오브젝트는 고유한 특성과 상호작용 방식을 가집니다.

|  **먼지**  |  **물얼룩**  |  **캔**  |  **상자**  |  **펼친 상자**  |
|:----------:|:------------:|:--------:|:----------:|:---------------:|
|   빗자루   |   대걸레      |   맨손   |   맨손      |   맨손          |
| 3개 제거 시 200점 | 1개 제거 시 150점 | 1개 제거 시 100점 | 제거 안됨(작업 필요) | 1개 제거 시 220점 획득 |
|   Level 1  |   Level 3    |  Level 2 |   Level 3  |   Level 3   |


## 👩‍💻 팀원 소개

**게임 클라이언트 개발 담당**

| **이름**    | **역할**        | 
|:-----------:|:---------------:|
| 강지영      | 플레이어 이동 및 회전 시스템 구현<br>플레이어 애니메이션 제어 및 연동<br>도구 장착 위치 설정 및 조정<br>맵 전용 카메라 설정 및 시점 조정 | 
| 조예현      | 도구 사용 및 상호작용 시스템 구현<br>코드 유지보수 친화적 구조 개선<br>맵 환경 구성<br>타이틀 화면 UI 디자인 |
| 한승희(PL)  | 플레이어 상호작용 UI 구현<br>Object Pool을 이용한 Trash Spawner 개발<br>레벨 및 난이도 관리<br>전반적인 게임 흐름 제어 | 

**AI 개발 담당**
| **이름**    | **역할**        | 
|:-----------:|:---------------:|
| 강다우, 장성우    | 강화학습을 위한 환경 구성 및 관측 요소 정의<br>학습 효율을 높이기 위한 보상 설계<br>행동 파라미터 조정 및 정책 설계<br> ML-Agents 도구를 활용한 에이전트 학습 및 성능 평가| 

## 🛠️ 기술 스택
<table>
  <thead>
    <tr>
      <th>분류</th>
      <th>기술 스택</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>게임 클라이언트 개발</td>
      <td>
        <img src="https://img.shields.io/badge/Unity-222324?style=flat&logo=unity&logoColor=white">
        <img src="https://img.shields.io/badge/C%23-4D53E8?style=flat&logo=c#&logoColor=white">
        <img src="https://img.shields.io/badge/Visual Studio-5C2D91?style=flat&logo=visualstudio&logoColor=white"/>
      </td>
    </tr>
    <tr>
      <td>AI 개발</td>
      <td>
        <img src="https://img.shields.io/badge/ML Agents-222324?style=flat&logo=unity&logoColor=white">
        <img src="https://img.shields.io/badge/python-3776AB?style=flat&logo=python&logoColor=white"> 
        <img src="https://img.shields.io/badge/Visual Studio Code-007ACC?style=flat-square&logo=visualstudiocode&logoColor=white"/>
      </td>
    </tr>
    <tr>
      <td>협업</td>
      <td>
        <img src="https://img.shields.io/badge/GitHub-181717?style=flat-square&logo=GitHub&logoColor=white"/>
        <img src="https://img.shields.io/badge/Notion-000000?style=flat&logo=notion&logoColor=white"/>
        <img src="https://img.shields.io/badge/Figma-F24E1E?style=flat&logo=figma&logoColor=white"/>
      </td>
    </tr>
    
  </tbody>
</table>
