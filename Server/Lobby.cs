using RPGCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Lobby
    {
        public static Lobby Instance { get; } = new Lobby();

        Queue<ClientSession> _waitingQueue = new Queue<ClientSession>();

        public void HandleMatchRequest(ClientSession session)
        {
            session._inMatching = true;
            lock (_waitingQueue)
            {
                if (_waitingQueue.Contains(session)) return;
                _waitingQueue.Enqueue(session);
            }
            CheckMatch();
            Console.WriteLine($"{session.SessionId} Matching Start");
        }

        public void HandleMatchCancel(ClientSession session)
        {
            session._inMatching = false;
            lock (_waitingQueue)
            {
                List<ClientSession> tempList = _waitingQueue.ToList();
                if(tempList.Remove(session))
                {
                    _waitingQueue = new Queue<ClientSession>(tempList);
                    Console.WriteLine($"Session{session.SessionId} cancel");
                }


            }
        }


        void CheckMatch()
        {
            if (_waitingQueue.Count >= 2)
            {
                ClientSession p1 = _waitingQueue.Dequeue();
                ClientSession p2 = _waitingQueue.Dequeue();

                p1.inLobby = false;
                p2.inLobby = false;

                if (!p1.TcpClient.Connected) { CheckMatch(); return; } // 중간에 해당 유저가 끊어지면 다시 호출
                if (!p2.TcpClient.Connected) { _waitingQueue.Enqueue(p1); return; }
                GameRoom room = new GameRoom();
                //p1과 p2에서 직업 받으면 stack수정해서 들어가게끔..
                GamePlayer player1 = new GamePlayer(p1, room, 1, p1.Job);
                GamePlayer player2 = new GamePlayer(p2, room, 2, p2.Job);

                p1.My = player1;
                p2.My = player2;
                
                room.Enter(player1);
                room.Enter(player2);
                PacketFastMatchingSuccess p1Packet = new PacketFastMatchingSuccess() { myRole = 1, enemyId = p2.SessionId };
                p1.Send(p1Packet.Serialize());


                PacketFastMatchingSuccess p2Packet = new PacketFastMatchingSuccess() { myRole = 2, enemyId = p1.SessionId };
                p2.Send(p2Packet.Serialize());
                Console.WriteLine($"Matching {p1.SessionId} , {p2.SessionId}");
            }
        }
    }
}
