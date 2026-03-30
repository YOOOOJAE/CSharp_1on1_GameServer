# CSharp_1on1_GameServer
# 프로젝트 개요
C# 서버의 흐름을 공부하기 위한 프로젝트입니다. AI를 사용하였습니다. 
해당 프로젝트의 목표는 통신 원리를 이해하고, DB 처리, 서버의 검증을 통해 상호작용 하는것입니다.
---
**사용언어** : C#

**Database** : MySQL

**클라이언트** : Unity 6.2

# 구동 스크린샷
1. 접속 및 네트워크 연결
<img width="640" height="480" alt="1접속" src="https://github.com/user-attachments/assets/ed59b206-0c07-4ad3-afdf-27155e383933" />

2. 회원가입 및 로그인 (DB 연동)
<img width="640" height="480" alt="2회원가입" src="https://github.com/user-attachments/assets/f1b6d391-1127-4a41-b1a8-a4e1f013d05e" />
<img width="640" height="240" alt="3 DB에 등록" src="https://github.com/user-attachments/assets/3c6008e2-6d7e-4075-9028-b28a9e049bfb" />
<img width="640" height="480" alt="4 로그인" src="https://github.com/user-attachments/assets/354d1eb4-9e50-4024-9d1d-e87ea0a312cd" />

3. 실시간 매칭 및 플레이어 조작
<p align="left">
<img src="https://github.com/user-attachments/assets/4f1dce2a-a517-464f-ab67-387ea7ffb59d" width="45%" title="매칭 대기">
<img src="https://github.com/user-attachments/assets/9fca235e-bf5e-4ca6-b2ae-40a5bda433dd" width="45%" title="매칭 성공">
</p>
<p align="left">
<img src="https://github.com/user-attachments/assets/ab8c9e76-fbee-4904-942d-27975af50e6f" width="45%" title="인게임 동기화 1">
<img src="https://github.com/user-attachments/assets/2b589e61-3ea8-4833-8e6f-5ff34d22365f" width="45%" title="인게임 동기화 2">
</p>

4. 게임 종료 및 데이터 갱신 (승리 카운트 증가)
<p align="left">
<img src="https://github.com/user-attachments/assets/3dd224bc-df49-4a3c-b420-bcbb4135ce7f" width="45%" title="게임 결과">
<img src="https://github.com/user-attachments/assets/b6822b2c-596b-49c8-a49d-30e67e4357cd" width="45%" title="DB 업데이트 확인">
</p>

5. 실시간 채팅 시스템
<img width="100%" alt="10 채팅" src="https://github.com/user-attachments/assets/a0a95656-ce39-4b8e-8cba-6cd19420fc29" />


# Common(Core) 

**RecvBuffer**

수신 버퍼 기능을 담당하는 클래스입니다. 커서를 통해 버퍼 내 유효 데이터 범위를 제어합니다.

**PacketSession**

받은 바이트를 버퍼에 쌓아 헤더 정보를 바탕으로 패킷 단위로 잘라내어 처리하는 클래스 입니다.
RecvBuffer 활용해 수신 데이터를 관리하며 패킷 조립이 완료되면 OnPacketReceived를 호출하는 클래스입니다.

**DataType**

패킷의 타입을 정의하고 각 패킷의 직렬화와 역직렬화를 정의한 파일입니다. 인터페이스를 활용하
모든 패킷이 동일한 규격으로 동작하도록 제작하였습니다.

# Server

**Program**
서버가 실질적으로 실행하는 코드로, TCP 리스너 관리, 세션 할당, DB연동 등 핵심적인 부분이 담겨 있습니다.

**ClientSession**
PacketSession을 상속받아, 클라이언트와 1:1로 대응되는 클래스입니다.
받은 바이트와 패킷 ID에 따라 적절한 핸들러를 호출하는 컨트롤러 역할을 담당합니다.

**DatabaseManager**
MYSQL 데이터베이스와 연결하여 로그인과 회원가입 등을 관리합니다.
async/await을 사용했습니다.

**Lobby**
매칭을 지원하는 로비입니다. 큐를 이용하여 게임의 매칭을 생성하고 연결해 줍니다.

**GameRoom**
매칭에 의해 생성되면 입장한 플레이어들을 관리합니다.
인게임의 상태를 처리하는 클래스입니다.

**GamePlayer**
플레이어들의 인게임 객체로 캐릭터 고유의 상태 값을 관리합니다.

# Client 주요 모듈

**NetworkManager**
서버 연결 유지 및 씬 전환 간 세션을 관리하는 싱글톤 매니저입니다.

**ServerSession**
서버에서 전송받은 패킷 ID에 따라 적절한 핸들러를 호출하는 컨트롤러 역할입니다.

**GameManager**
서버 패킷을 기반으로 인게임의 상태와 실시간 동기화를 도와주는 클래스입니다.
