using RPGCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class GameRoom
    {
        List<GamePlayer> _players = new List<GamePlayer>();
        int _readyCount = 0;
        bool _isGameStarted = false;

        MapManager mManager = new MapManager();

        string mapText = File.ReadAllText("MapData_01.txt");

        object _lock = new object();

        Timer _updateTiemr;

        public GameRoom()
        {
            _updateTiemr = new Timer(Update, null, 0, 100);
            mManager.LoadMap(mapText);
        }
        public void Enter(GamePlayer Player)
        {
            lock(_players)
            {
                _players.Add(Player);
            }

            Player.Room = this;
        }

        public void BroadCast(byte[] packet)
        {
            lock(_players)
            {
                foreach(GamePlayer s in _players)
                {
                    s.Send(packet);
                }
            }
        }

        public void GameEnd(int loserId)
        {
            Console.WriteLine($"[GameRoom] Player{loserId} Dead");

            var winner = _players.Find(p => p.PlayerId != loserId);
            if (winner != null)
            {
                winner.Session.UpdateWinCount();
            }

                Task.Run(async () =>
            {
                await Task.Delay(10); // 상황 인지할 시간을 주기위해

                Console.WriteLine($"[GameRoom] GameEnd 패킷 전송");

                PacketGameEnd endPacket = new PacketGameEnd() {loserId= loserId, reason = 0 };
                BroadCast(endPacket.Serialize());

                _isGameStarted = false;
                _readyCount = 0;

                foreach (var p in _players)
                {
                    p.Session._IsReady = false;
                    p.Reset();
                }


                /*lock (_players)
                {
                    List<ClientPlayer> playersToKick = new List<ClientPlayer>(_players);

                    foreach (ClientPlayer s in playersToKick)
                    {
                        Leave(s);
                    }
                }*/

            });
        }

        public void HandlePlayerHit(int attackerId, int targetId)
        {
            lock(_players)
            {
                GamePlayer attacker = _players.Find(p => p.PlayerId == attackerId);
                GamePlayer target = _players.Find(p => p.PlayerId == targetId);

                if (attacker == null || target == null || target.Hp <= 0) return;

                /*double distance = Math.Sqrt(Math.Pow(attacker.X - target.X, 2) + Math.Pow(attacker.y - target.y, 2));
                if (distance > 20.0f) 
                {
                    Console.WriteLine($"[비정상 공격 감지] Player {attackerId}가 너무 먼 거리에서 {targetId} 공격 시도");
                    return;
                }*/
                target.Hp--;
                Console.WriteLine($"[Hit] {targetId} HP: {target.Hp}");

                PacketPlayerHpUpdate phuPacket = new PacketPlayerHpUpdate();
                phuPacket.playerId = targetId;
                phuPacket.currentHp = target.Hp;
                BroadCast(phuPacket.Serialize());

                if (target.Hp <= 0)
                {
                    PacketPlayerDie pdPacket = new PacketPlayerDie();
                    pdPacket.playerId = targetId;
                    BroadCast(pdPacket.Serialize());

                    GameEnd(targetId);
                }
            }
        }
        public void DisConnectLeave(GamePlayer Player)
        {
            lock(_players)
            {
                _players.Remove(Player);
                Player.Session.ResetStatus();

                if (_players.Count > 0)
                {
                    int loserId = Player.PlayerId;

                    PacketGameEnd endPacket = new PacketGameEnd { loserId = loserId , reason = 1};
                    BroadCast(endPacket.Serialize());
                }
                else
                {
                    Console.WriteLine("[GameRoom]방 삭제");
                    
                }
            }
        }
        public void Leave(GamePlayer Player)
        {
            PacketLeaveGame LGPacket = new PacketLeaveGame();
            LGPacket.playerId = Player.PlayerId;
            Player.Session.ResetStatus();
            if(_players.Count > 1)
            {
                lock (_players)
                {
                    foreach (GamePlayer p in _players)
                    {
                        p.Session.Send(LGPacket.Serialize());
                    }
                    _players.Remove(Player);

                    /*
                    if (_players.Count > 0)
                    {


                        Player.Send(LGPacket.Serialize());
                    }
                    else
                    {
                        Console.WriteLine("[GameRoom]방 삭제");
                        Player.Send(LGPacket.Serialize());

                    }*/
                }
            }
            else
            {
                Player.Send(LGPacket.Serialize());
            }
        }
        public void HandleReady(GamePlayer Player)
        {
            lock(_players)
            {
                if (_isGameStarted) return;

                if (Player.Session._IsReady) return;

                Player.Session._IsReady = true;
                _readyCount++;
                Console.WriteLine($"[GameRoom] Player {Player.PlayerId} Ready! ({_readyCount}/{_players.Count})");

                if (_readyCount == _players.Count && _players.Count == 2)
                {
                    StartGame();
                }

            }
        }
        public void Update(object state)
        {
            lock(_lock)
            {
                foreach (GamePlayer p in _players)
                {
                    p.Update(0.1f);
                }
            }

        }
        public void HandleMove(GamePlayer player, PacketMove packet)
        {
            if (!mManager.CanMove(packet.x, packet.y))
            {
                Console.WriteLine($"[Player{player.PlayerId}] : 벽 충돌 시도 ({packet.x}, {packet.y})");
                return;
            }

            if (!player.TryUseStack()) return;

            int dist = Math.Abs(packet.x - player.x) + Math.Abs(packet.y - player.y);

            if(dist != 1)
            {
                Console.WriteLine($"[Player{player.PlayerId}] : 거리 {dist} error");
                PacketSetPosition rollbackPacket = new PacketSetPosition();
                rollbackPacket.playerId = player.PlayerId;
                rollbackPacket.x = player.x; 
                rollbackPacket.y = player.y;
                player.Session.Send(rollbackPacket.Serialize());
                return;
            }

            player.x = packet.x;
            player.y = packet.y;

            BroadCast(packet.Serialize());
        }

        public void PlayerStartSetting(GamePlayer player)
        {
            PacketPlayerStartSetting SPPacket = new PacketPlayerStartSetting();
            if (player.MyRole == 1)
            {

                player.x = 3;
                player.y = 2;
                SPPacket.x = player.x;
                SPPacket.y = player.y;
                SPPacket.playerId = player.PlayerId;
                SPPacket.Job = player.Job;
                BroadCast(SPPacket.Serialize());
                Console.WriteLine("PlayerStarTSetting Packet Send1 ");
            }

            else if (player.MyRole == 2)
            {
                player.x = 3;
                player.y = 8;
                SPPacket.x = player.x;
                SPPacket.y = player.y;
                SPPacket.playerId = player.PlayerId;
                SPPacket.Job = player.Job;
                BroadCast(SPPacket.Serialize());
                Console.WriteLine("PlayerStarTSetting Packet Send2 ");
            }
        }
        private void StartGame()
        {
            _isGameStarted = true;
            PacketStart startPacket = new PacketStart();
            foreach(var p in _players)
            {
                PlayerStartSetting(p);
                p.Reset();
            }
            BroadCast(startPacket.Serialize());
        }
    }
}
