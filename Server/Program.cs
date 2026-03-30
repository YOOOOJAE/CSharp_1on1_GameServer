using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MySqlX.XDevAPI;
using RPGCommon;

namespace Server
{
    class Program
    {
        const int PORT = 7777;
        static TcpListener _listener;
        static List<ClientSession> _sessions = new List<ClientSession>(); // 접속만한 유저들
        static int _sessionIdGen = 0;
        static DatabaseManager _db = new DatabaseManager();

        static Dictionary<string, ClientSession> _loginUsers = new Dictionary<string, ClientSession>(); //로그인까지한 유저들

        static void Main(string[] args)
        {
            if(_db.TestConnection())
            {
                Console.WriteLine("[DB] Database connection successful.");
            }
            else
            {
                Console.WriteLine("[DB] Error: Could not connect to Database. Check if MySQL is running.");
                return;
            }
                _listener = new TcpListener(IPAddress.Any, PORT);
            _listener.Start();
            Console.WriteLine($"[Server] Listening on {PORT}...");

            // 연결 수락 루프
            while (true)
            {
                TcpClient tcpClient = _listener.AcceptTcpClient();
                ProcessConnect(tcpClient);
            }
        }

        // 클라이언트 접속 시 호출
        static void ProcessConnect(TcpClient tcpClient)
        {
            //int sessionId = ++_sessionIdGen;
            Console.WriteLine($"[Server] Client Connected: {tcpClient.Client.RemoteEndPoint}");

            ClientSession session = new ClientSession
            {
                //SessionId = sessionId,
                TcpClient = tcpClient,
                Stream = tcpClient.GetStream()
            };

            lock (_sessions)
            {
                _sessions.Add(session);
            }

            // 로그인 사용하지 않을경우 자동으로 플레이어에게 아이디번호부여
            //PacketLogin loginPacket = new PacketLogin();
            //loginPacket.playerId = sessionId;

            //session.Send(loginPacket.Serialize());

            // 별도 스레드에서 수신 대기
            Task.Run(() => HandleClient(session));
        }
        public static async Task HandleRegisterRequest(ClientSession session, byte[] buffer)
        {
            Console.WriteLine($"[Debug] Received Bytes: {BitConverter.ToString(buffer)}");

            try
            {
                PacketRegisterRequest req = new PacketRegisterRequest();
                req.Deserialize(buffer);
                PacketRegisterResult rrp = new PacketRegisterResult();
                Console.WriteLine($"[RegisterRequest] : {req.userId}");

                var (success, message) = await _db.RegisterRequest(req.userId, req.password, req.nickname);

                if (success)
                {
                    Console.Write($"[Register] ID : {req.userId}, Nick : {req.nickname} Register Success");
                    rrp.success = 1;
                    rrp.message = message;
                }
                else
                {
                    Console.WriteLine($"[Register Failed] {message} ");
                    rrp.success = 0;
                    rrp.message = message;

                    
                }
                session.Send(rrp.Serialize());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Packet Error] {ex.Message}");
                
            }

        }

        public static async Task HandleWinCount(ClientSession session)
        {
            try
            {
                Console.WriteLine($"HandleWinCount message");
                var (s, wincount) = await _db.UpdatePlayerWinCount(session.UserId);
                if (s)
                {
                    Console.WriteLine($"[DB Success] {session.UserId} WinCount Updated.");

                    session.WinCount = wincount;

                    PacketWinCountUpdate wcuP = new PacketWinCountUpdate();
                    wcuP.winCount = wincount;
                    wcuP.playerId = session.SessionId;
                    session.Send(wcuP.Serialize());
                }
                else
                {
                    Console.WriteLine($"[DB Fail] {session.UserId} Not Found or Update Error.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB Critical Error] {ex.Message}");
            }
        }

        public static async Task HandleGetWinCount(ClientSession session)
        {
            try
            {
                var (s, wincount) = await _db.GetPlayerWinCount(session.UserId);
                if (s)
                {
                    Console.WriteLine($"[DB Success] {session.UserId} WinCount Updated.");

                    session.WinCount = wincount;

                    PacketWinCountUpdate wcuP = new PacketWinCountUpdate();
                    wcuP.winCount = wincount;
                    wcuP.playerId = session.SessionId;
                    session.Send(wcuP.Serialize());

                    Console.WriteLine($"{wcuP.winCount}");
                }
                else
                {
                    Console.WriteLine($"[DB Fail] {session.UserId} Not Found or Update Error.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB Critical Error] {ex.Message}");
            }
        }
        public static async Task HandleLoginRequest(ClientSession session, byte[] buffer)
        {
            Console.WriteLine($"[Debug] Received Bytes: {BitConverter.ToString(buffer)}");

            try
            {
                PacketLoginRequest req = new PacketLoginRequest();
                req.Deserialize(buffer);

                Console.WriteLine($"[Login Attempt] ID : {req.userId}");

                var (success, nickname, winCount) = await _db.VerifyLogin(req.userId, req.password);

                PacketLogin res = new PacketLogin();
                switch (success)
                {
                    case 1:
                        lock (_loginUsers)
                        {
                            if (_loginUsers.ContainsKey(req.userId))
                            {
                                Console.WriteLine($"[Login Denied] 이미 접속 중인 아이디: {req.userId}");
                                res.success = 4;
                            }
                            else
                            {
                                int sessionId = ++_sessionIdGen;
                                Console.WriteLine($"[Login Attempt] ID: {req.userId}, Nick :{nickname}");
                                session.SessionId = sessionId;
                                session.Nickname = nickname;
                                session.inLobby = true;
                                session.UserId = req.userId;
                                session.WinCount = winCount;
                                _loginUsers.TryAdd(req.userId, session);

                                res.playerId = session.SessionId;
                                res.nickname = session.Nickname;
                                res.success = 1;
                                res.winCount = winCount;
                            }
                        }
                        break;
                    case 2:
                        Console.WriteLine($"[Login Failed] InvaildID");
                        res.success = 2;
                        break;
                    case 3:
                        Console.WriteLine($"[Login Failed] InvaildPassword");
                        res.success = 3;
                        break;
                    default:
                        break;
                }
                session.Send(res.Serialize());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Packet Error] {ex.Message}");
            }

        }


        //

        public static void HandleFastMatchingRequestPacket(ClientSession session, byte[] buffer)
        {
            Lobby.Instance.HandleMatchRequest(session);
        }


        public static void HandleFastMatchingCancelPacket(ClientSession session, byte[] buffer)
        {
            Lobby.Instance.HandleMatchCancel(session);
        }
        public static void HandleChatPacket(ClientSession session, byte[] buffer)
        {
            PacketChat chatPacket = new PacketChat();
            chatPacket.Deserialize(buffer);
            string chat = $"{session.Nickname} [{session.SessionId}] : {chatPacket.chatMsg}";
            chatPacket.chatMsg = chat;
            Console.WriteLine($"[Chat]" + chat);

            byte[] sendData = chatPacket.Serialize();
            lock(_sessions)
            {
                foreach(ClientSession s in _sessions)
                {
                    if(s.inLobby)
                        s.Send(sendData);
                }
            }
        }
        static void HandleClient(ClientSession session )
        {
            EndPoint clientEndPoint = session.TcpClient.Client.RemoteEndPoint;
            try
            {

                while (session.TcpClient.Connected)
                {

                        byte[] buffer = new byte[1024];

                        int bytesRead = session.Stream.Read(buffer, 0, buffer.Length);

                        if (bytesRead == 0) break; // 연결 끊김

                        session.OnReceive(buffer, 0, bytesRead);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Error] Session {session.SessionId}: {e.Message}");
            }
            finally
            {
                session.OnDisconnected(clientEndPoint);
                lock (_sessions) _sessions.Remove(session);
                if (!string.IsNullOrEmpty(session.UserId))
                {
                    lock (_loginUsers)
                    {
                        _loginUsers.Remove(session.UserId);
                    }
                }
            }
        }
    }
}