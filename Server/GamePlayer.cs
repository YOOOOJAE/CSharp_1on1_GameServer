using RPGCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class GamePlayer
    {
        public ClientSession Session { get; set; }
        public int PlayerId { get; set; }
        public int MyRole { get; set; }
        public JobType Job { get; set; }
        public GameRoom Room { get; set; }

        public double lastAttackTime = 0f;
        private float attackCoolTime = 1.0f;

        public int Hp { get; set; }
        public int MaxHp { get;private set; } = 3;

        public int x { get; set; }
        public int y { get; set; }

        public int MoveStack { get; private set; } = 9;
        public int MAX_STACK = 9;
        public float REGEN_TIME { get; private set; } = 2.0f;
        private float _checkTimer = 0.0f;


        public void Update(float deltaTime)
        {
            if(MoveStack >= MAX_STACK)
            {
                _checkTimer = 0;
                return;
            }
            _checkTimer += deltaTime;

            if(_checkTimer >= REGEN_TIME)
            {
                MoveStack++;
                _checkTimer -= REGEN_TIME;
                PacketMoveStack msPacket = new PacketMoveStack();
                msPacket.stack = MoveStack;
                Send(msPacket.Serialize());
            }
        }
        public bool TryAttack()
        {
            double currentTime = Environment.TickCount64 / 1000f;
            if (currentTime - lastAttackTime >= attackCoolTime)
            {
                lastAttackTime = currentTime;

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryUseStack()
        {
            if (MoveStack > 0)
            {
                MoveStack--;

                PacketMoveStack msPacket = new PacketMoveStack();
                msPacket.stack = MoveStack;
                Send(msPacket.Serialize());
                return true;
            }
            return false;
        }

        public void Send(byte[] Packet)
        {
            if (Session != null)
                Session.Send(Packet);
        }

        public void Reset()
        {
            if (Job == null) Job = JobType.Normal;
            JobData job = JobDataRepository.GetStat(Job);
            MaxHp = job.MaxHp;
            Hp = MaxHp;
            MoveStack = job.MoveStack;
            MAX_STACK = job.MoveStack;
            attackCoolTime = job.AttackSpeed;
            REGEN_TIME = job.MoveStackRegen;
        }


        public GamePlayer(ClientSession session, GameRoom room, int role, JobType job)
        {
            Session = session;
            PlayerId = session.SessionId;
            Room = room;
            MyRole = role;
            Hp = 3;
            MaxHp = 3;
            Job = job;
        }
    }
}
