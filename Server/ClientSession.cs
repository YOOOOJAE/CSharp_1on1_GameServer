using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using RPGCommon; // Shared 프로젝트 참조 필요

namespace Server
{
    // 패킷 세션을 상속받아 "패킷을 조립하는 능력"을 가짐
    class ClientSession : PacketSession
    {
        public int SessionId { get; set; }

        public int WinCount { get; set; }

        public string Nickname { get; set; }
        public string UserId { get; set; }
        public GamePlayer My { get; set; }
        public TcpClient TcpClient { get; set; }
        public NetworkStream Stream { get; set; }

        public bool inLobby { get; set; } = false;
        public bool _inMatching { get; set; }

        public bool _IsReady { get; set; } = false;
        public GameRoom Room { get; set; }

        public JobType Job { get; set; } = JobType.Normal;

        // 패0킷이 완성되면 이 함수가 자동으로 호출됨
        protected override void OnPacketReceived(ushort protocolId, byte[] body)
        {
            PacketType type = (PacketType)protocolId;

            Console.WriteLine($"[Session {SessionId}] 패킷 수신: {type} (크기: {body.Length})");

            switch (type)
            {
                case PacketType.Move:
                    PacketMove movePacket = new PacketMove();
                    movePacket.Deserialize(body);
                    if (My != null && My.Room != null)
                    {
                        if(My.MoveStack > 0)
                        {
                            My.Room.HandleMove(My, movePacket);
                        }

                       /* My.X = movePacket.x;
                        My.y = movePacket.y;
                        My.Room.BroadCast(movePacket.Serialize());*/
                    }
                    break;
                case PacketType.Chat:
                    Program.HandleChatPacket(this, body);
                    break;
                case PacketType.LoginRequest:
                    Program.HandleLoginRequest(this, body);
                    break;
                case PacketType.RegisterRequest:
                    Program.HandleRegisterRequest(this, body);
                    break;
                case PacketType.RegisterResult:
                    Console.WriteLine("Debug : RegisterResult");
                    break;

                case PacketType.FastMatchingRequest:
                    Program.HandleFastMatchingRequestPacket(this, body);
                    break;
                case PacketType.FastMatchingCancel:
                    Program.HandleFastMatchingCancelPacket(this, body);
                    break;
                case PacketType.SelectJob:
                    PacketPlayerSelectJob psjPacket = new PacketPlayerSelectJob();
                    psjPacket.Deserialize(body);
                    if(psjPacket.playerId == SessionId)
                    {
                        Job = psjPacket.Job;
                        Console.WriteLine($"[Player {SessionId} : Selected {Job}]");
                    }
                    break;
                case PacketType.PlayerAttack:
                    if (My != null && My.Room != null)
                    {
                        if (!My.TryAttack()) return;
                        PacketPlayerAttack paPacket = new PacketPlayerAttack();
                        paPacket.Deserialize(body);
                        paPacket.playerId = My.PlayerId;
                        //총알 스피드 설정
                        switch(My.Job)
                        {
                            case JobType.Normal:
                                paPacket.speed = 35f;
                                break;
                            case JobType.Stamina:
                                paPacket.speed = 30f;
                                break;
                            case JobType.Speed:
                                paPacket.speed = 40f;
                                break;
                            default:
                                paPacket.speed = 10f;
                                break;
                        }

                        My.Room.BroadCast(paPacket.Serialize());

                    }
                    break;
                case PacketType.PlayerHit:
                    PacketPlayerHit phPacket = new PacketPlayerHit();
                    phPacket.Deserialize(body);
                    if (My != null && My.Room != null)
                    {
                        My.Room.HandlePlayerHit(My.PlayerId, phPacket.targetId);
                    }
                    break;
                /*case PacketType.PlayerHpUpdate: // 서버에서 해당 세션 사용 X 
                    PacketPlayerHpUpdate phuPacket = new PacketPlayerHpUpdate();
                    phuPacket.Deserialize(body);
                    if (My != null && My.Room != null)
                    {
                        My.Room.BroadCast(phuPacket.Serialize());
                    }
                    break;*/
                case PacketType.PlayerDie:
                    PacketPlayerDie pdPacket = new PacketPlayerDie();
                    pdPacket.Deserialize(body);
                    if (My != null && My.Room != null)
                    {
                        My.Room.BroadCast(pdPacket.Serialize());
                        My.Room.GameEnd(pdPacket.playerId);
                    }
                    break;
                /*case PacketType.GameEnd: // 서버에서 사용 X
                    PacketGameEnd GEPacket = new PacketGameEnd();
                    GEPacket.Deserialize(body);
                    if (Room != null)
                    {
                        Room.BroadCast(GEPacket.Serialize());
                    }
                    break;*/
                case PacketType.Ready:

                    if (My != null && My.Room != null)
                    {
                        My.Room.HandleReady(My);
                    }
                    break;
                case PacketType.Start:
                    break;
                case PacketType.LeaveGame:
                    PacketLeaveGame LGPacket = new PacketLeaveGame();
                    LGPacket.Deserialize(body);

                    int id = LGPacket.playerId;
                    if (My != null && My.Room != null)
                    {
                        My.Room.Leave(My);
                        inLobby = true;
                    }
                    break;
                case PacketType.WinCountUpdateRequest:

                    PacketWinCountUpdate wcuP = new PacketWinCountUpdate();
                    wcuP.playerId = this.SessionId;
                    wcuP.winCount = this.WinCount;

                    Send(wcuP.Serialize());
                    break;

            }
        }

        public void ResetStatus()
        {
            _IsReady = false;
            _inMatching = false;
            My = null;
            Console.WriteLine($"[Session {SessionId}] 상태 리셋 완료 (Lobby 대기 상태)");
        }

        public async Task UpdateWinCount()
        {
            await Program.HandleWinCount(this);
            Console.WriteLine("Updatewincount Message \n");
        }


        public void Send(byte[] data)
        {
            try
            {
                if (Stream.CanWrite)
                {
                    Stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Send Error] {e.Message}");
            }
        }

        public void Disconnect()
        {
            TcpClient?.Close();
            Console.WriteLine($"[Session {SessionId}] 연결 종료");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"[Session {SessionId}] OnDisconnected : {endPoint}");

            if (My != null && My.Room != null)
            {
                My.Room.DisConnectLeave(My);
                My = null;
            }
            if(_inMatching)
            {
                Lobby.Instance.HandleMatchCancel(this);
                _inMatching = false;
            }

            Disconnect();
        }
    }
}