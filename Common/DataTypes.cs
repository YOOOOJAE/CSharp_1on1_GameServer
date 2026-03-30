using System;
using System.Text;

namespace RPGCommon
{
    public enum PacketType
    {
        None = 0,
        Login = 1,
        Move = 2,
        Attack = 3,
        MonsterSpawn = 4,
        SetPosition = 5,
        LoginRequest = 6,
        LoginFail = 7,
        RegisterRequest = 8,
        RegisterResult = 9,
        Chat = 10,
        Start = 11,
        Ready = 12,
        FastMatchingRequest = 20,
        FastMatchingCancel = 21,
        FastMatchingSuccess = 22,
        PlayerHit = 90,
        PlayerAttack = 91,
        PlayerHpUpdate = 92,
        PlayerDie = 93,
        GameEnd = 100,
        GameDisconnectEnd = 101,
        LeaveGame = 105,
        WinCountUpdate = 200,
        WinCountUpdateRequest = 201,
        SelectJob = 900,
        PlayerStartSetting = 999,
        MoveStack = 1000


    }

    public interface IPacket
    {
        PacketType Type { get; }
        byte[] Serialize();
        void Deserialize(byte[] data);
    }

    public struct PacketMove :IPacket
    {
        public PacketType Type => PacketType.Move;
        public int playerId;
        public int x; public int y;

        public byte[] Serialize()
        {
            byte[] body = new byte[12]; // ID(4) + X(4) + Y(4)
            Array.Copy(BitConverter.GetBytes(playerId), 0, body, 0, 4);
            Array.Copy(BitConverter.GetBytes(x), 0, body, 4, 4);
            Array.Copy(BitConverter.GetBytes(y), 0, body, 8, 4);

            byte[] packet = new byte[body.Length + 4];
            Array.Copy(BitConverter.GetBytes((ushort)packet.Length), 0, packet, 0, 2);
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, 2, 2);
            Array.Copy(body, 0, packet, 4, body.Length);

            return packet;
        }

        public void Deserialize(byte[] data)
        {
            playerId = BitConverter.ToInt32(data, 0);
            x = BitConverter.ToInt32(data, 4);
            y = BitConverter.ToInt32(data, 8);
        }
    }
    public struct PacketSetPosition : IPacket
    {
        public PacketType Type => PacketType.SetPosition;
        public int playerId;
        public int x; public int y;

        public byte[] Serialize()
        {
            byte[] body = new byte[12]; 
            Array.Copy(BitConverter.GetBytes(playerId), 0, body, 0, 4);
            Array.Copy(BitConverter.GetBytes(x), 0, body, 4, 4);
            Array.Copy(BitConverter.GetBytes(y), 0, body, 8, 4);

            byte[] packet = new byte[body.Length + 4];
            Array.Copy(BitConverter.GetBytes((ushort)packet.Length), 0, packet, 0, 2);
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, 2, 2);
            Array.Copy(body, 0, packet, 4, body.Length);

            return packet;
        }

        public void Deserialize(byte[] data)
        {
            playerId = BitConverter.ToInt32(data, 0);
            x = BitConverter.ToInt32(data, 4);
            y = BitConverter.ToInt32(data, 8);
        }
    }

    public struct PacketPlayerSelectJob : IPacket
    {
        public PacketType Type => PacketType.SelectJob;
        public int playerId;
        public JobType Job;

        public byte[] Serialize()
        {
            byte[] body = new byte[14];
            Array.Copy(BitConverter.GetBytes(playerId), 0, body, 0, 4);
            Array.Copy(BitConverter.GetBytes((ushort)Job), 0, body, 4, 2);

            byte[] packet = new byte[body.Length + 4];
            Array.Copy(BitConverter.GetBytes((ushort)packet.Length), 0, packet, 0, 2);
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, 2, 2);
            Array.Copy(body, 0, packet, 4, body.Length);

            return packet;
        }

        public void Deserialize(byte[] data)
        {
            playerId = BitConverter.ToInt32(data, 0);
            ushort JobValue = BitConverter.ToUInt16(data, 4);
            Job = (JobType)JobValue;
        }
    }

    public struct PacketPlayerStartSetting : IPacket
    {
        public PacketType Type => PacketType.PlayerStartSetting;
        public int playerId;
        public int x; public int y;
        public JobType Job;

        public byte[] Serialize()
        {
            byte[] body = new byte[14];
            Array.Copy(BitConverter.GetBytes(playerId), 0, body, 0, 4);
            Array.Copy(BitConverter.GetBytes(x), 0, body, 4, 4);
            Array.Copy(BitConverter.GetBytes(y), 0, body, 8, 4);
            Array.Copy(BitConverter.GetBytes((ushort)Job), 0, body, 12, 2);

            byte[] packet = new byte[body.Length + 4];
            Array.Copy(BitConverter.GetBytes((ushort)packet.Length), 0, packet, 0, 2);
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, 2, 2);
            Array.Copy(body, 0, packet, 4, body.Length);

            return packet;
        }

        public void Deserialize(byte[] data)
        {
            playerId = BitConverter.ToInt32(data, 0);
            x = BitConverter.ToInt32(data, 4);
            y = BitConverter.ToInt32(data, 8);
            ushort JobValue = BitConverter.ToUInt16(data, 12);
            Job = (JobType)JobValue;
        }
    }

    public struct PacketLoginRequest : IPacket
    {
        public PacketType Type => PacketType.LoginRequest;
        public string userId;
        public string password;

        public byte[] Serialize()
        {
            byte[] idBytes = Encoding.UTF8.GetBytes(userId ?? "");
            byte[] pwBytes = Encoding.UTF8.GetBytes(password ?? "");
            ushort idLen = (ushort)idBytes.Length;
            ushort pwLen = (ushort)pwBytes.Length;


            int packetSize = 4 + 2 + idLen + 2 + pwLen;
            byte[] packet = new byte[packetSize];

            //offset 넣어서 코드 보기 편하게..
            int offset = 0;

            Array.Copy(BitConverter.GetBytes((ushort)packetSize), 0, packet, offset, 2); offset += 2;
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, offset, 2); offset += 2;

            Array.Copy(BitConverter.GetBytes(idLen), 0, packet, offset, 2); offset += 2;
            Array.Copy(idBytes, 0, packet, offset, idLen); offset += idLen;


            Array.Copy(BitConverter.GetBytes(pwLen), 0, packet, offset, 2); offset += 2;
            Array.Copy(pwBytes, 0, packet, offset, pwLen);

            return packet;
        }

        public void Deserialize(byte[] data)
        {
            int offset = 0;

            ushort idLen = BitConverter.ToUInt16(data, offset); offset += 2;
            userId = Encoding.UTF8.GetString(data, offset, idLen); offset += idLen;

            ushort pwLen = BitConverter.ToUInt16(data, offset); offset += 2;
            password = Encoding.UTF8.GetString(data, offset, pwLen);
        }
    }
    public struct PacketLogin : IPacket
    {
        public PacketType Type => PacketType.Login;
        public int playerId;
        public int success; // 1 = success , 2 = invaildID, 3 = invaildPassword, 4 = AlreadyLogin
        public int winCount;
        public string nickname;

        public byte[] Serialize()
        {
            byte[] nnBytes = Encoding.UTF8.GetBytes(nickname ?? "");
            ushort nnLen = (ushort)nnBytes.Length;

            int packetSize = 4 + 4 + 4 + 4 + 2 + nnBytes.Length; // Header 4 + playerid 4+ success 4 + wincount 4 + nicklen 2 + nick
            byte[] packet = new byte[packetSize];

            int offset = 0;

            Array.Copy(BitConverter.GetBytes((ushort)packetSize), 0, packet, offset, 2); offset += 2;
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, offset, 2); offset += 2;

            Array.Copy(BitConverter.GetBytes(success), 0, packet, offset, 4); offset += 4;
            Array.Copy(BitConverter.GetBytes(winCount), 0, packet, offset, 4); offset += 4;
            Array.Copy(BitConverter.GetBytes(playerId), 0, packet, offset, 4); offset += 4;
            Array.Copy(BitConverter.GetBytes(nnLen), 0, packet, offset, 2); offset += 2;
            Array.Copy(nnBytes, 0, packet, offset, nnBytes.Length);

            return packet;
        }

        public void Deserialize(byte[] data)
        {
            int offset = 0;
            success = BitConverter.ToInt32(data, offset); offset += 4;
            winCount = BitConverter.ToInt32(data, offset); offset += 4;
            playerId = BitConverter.ToInt32(data, offset); offset += 4;
            ushort nickLen = BitConverter.ToUInt16(data, offset); offset += 2;
            nickname = Encoding.UTF8.GetString(data, offset, nickLen);
        }
    }
    public struct PacketRegisterRequest : IPacket
    {
        public PacketType Type => PacketType.RegisterRequest;
        public string userId;
        public string password;
        public string nickname;

        public byte[] Serialize()
        {
            byte[] idBytes = Encoding.UTF8.GetBytes(userId ?? "");
            byte[] pwBytes = Encoding.UTF8.GetBytes(password ?? "");
            byte[] nnBytes = Encoding.UTF8.GetBytes(nickname ?? "");
            ushort idLen = (ushort)idBytes.Length;
            ushort pwLen = (ushort)pwBytes.Length;
            ushort nnLen = (ushort)nnBytes.Length;

            int packetSize = 4 + 2 + idLen + 2 + pwLen + 2 + nnLen; // header 4 + idlen 2 + id + pwlen 2+ pw + nickLen 2+ nickname
            byte[] packet = new byte[packetSize];


            int offset = 0;

            Array.Copy(BitConverter.GetBytes((ushort)packetSize), 0, packet, offset, 2); offset += 2;
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, offset, 2); offset += 2;

            Array.Copy(BitConverter.GetBytes(idLen), 0, packet, offset, 2); offset += 2;
            Array.Copy(idBytes, 0, packet, offset, idLen); offset += idLen;

            Array.Copy(BitConverter.GetBytes(pwLen), 0, packet, offset, 2); offset += 2;
            Array.Copy(pwBytes, 0, packet, offset, pwLen); offset += pwLen;

            Array.Copy(BitConverter.GetBytes(nnLen), 0, packet, offset, 2); offset += 2;
            Array.Copy(nnBytes, 0, packet, offset, nnLen);

            return packet;
        }

        public void Deserialize(byte[] data)
        {
            int offset = 0;

            ushort idLen = BitConverter.ToUInt16(data, offset); offset += 2;
            userId = Encoding.UTF8.GetString(data, offset, idLen); offset += idLen;

            ushort pwLen = BitConverter.ToUInt16(data, offset); offset += 2;
            password = Encoding.UTF8.GetString(data, offset, pwLen); offset += pwLen;

            ushort nnLen = BitConverter.ToUInt16(data, offset); offset += 2;
            nickname = Encoding.UTF8.GetString(data, offset, nnLen);
        }
    }

    public struct PacketRegisterResult : IPacket
    {
        public PacketType Type => PacketType.RegisterResult;
        public int success; // 0 실패 1 성공
        public string message;

        public byte[] Serialize()
        {
            byte[] msgBytes = Encoding.UTF8.GetBytes(message ?? "");
            ushort msgLen = (ushort)msgBytes.Length;

            int packetSize = 4 + 4  + 2 + msgBytes.Length; // header 4 + success 4 + msgLen 2 + message
            byte[] packet = new byte[packetSize];

            int offset = 0;

            Array.Copy(BitConverter.GetBytes((ushort)packetSize), 0, packet, offset, 2); offset += 2;
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, offset, 2); offset += 2;

            Array.Copy(BitConverter.GetBytes(success), 0, packet, offset, 4); offset += 4;
            Array.Copy(BitConverter.GetBytes(msgLen), 0, packet, offset, 2); offset += 2;
            Array.Copy(msgBytes, 0, packet, offset, msgBytes.Length);

            return packet;
        }

        public void Deserialize(byte[] data)
        {
            int offset = 0;
            success = BitConverter.ToInt32(data, offset); offset += 4;
            ushort messageLen = BitConverter.ToUInt16(data, offset); offset += 2;
            message = Encoding.UTF8.GetString(data, offset, messageLen);
        }
    }
    public struct PacketChat : IPacket
    {
        public PacketType Type => PacketType.Chat;
        public int playerId;
        public string nickname;
        public string chatMsg;
        public byte[] Serialize()
        {
            byte[] body = Encoding.UTF8.GetBytes(chatMsg);
            ushort stringLen = (ushort)body.Length;
            byte[] nickbody = Encoding.UTF8.GetBytes(nickname ?? "");
            ushort nickbodyLen = (ushort)nickbody.Length;

            int headerSize = 4;
            int bodySize = 4 + 2 + stringLen + 2 + nickbodyLen; // ID 4 + msgLen 2 + msg + nicklen 2 + nick
            byte[] packet = new byte[headerSize + bodySize];

            int offset = 0;

            Array.Copy(BitConverter.GetBytes((ushort)packet.Length), 0, packet, offset, 2); offset += 2;
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, offset, 2); offset += 2;
            Array.Copy(BitConverter.GetBytes(playerId), 0, packet, offset, 4); offset += 4;
            Array.Copy(BitConverter.GetBytes(stringLen), 0, packet, offset, 2); offset += 2;
            Array.Copy(body, 0, packet, offset, body.Length); offset += body.Length;
            Array.Copy(BitConverter.GetBytes(nickbodyLen), 0, packet, offset, 2); offset += 2;
            Array.Copy(nickbody, 0, packet, offset, nickbody.Length);


            return packet;
        }

        public void Deserialize(byte[] data)
        {
            int offset = 0;
            playerId = BitConverter.ToInt32(data, 0); offset += 4;
            ushort stringLen = BitConverter.ToUInt16(data, offset); offset += 2;
            chatMsg = Encoding.UTF8.GetString(data, offset, stringLen); offset += stringLen;
            ushort nickbodyLen = BitConverter.ToUInt16(data, offset); offset += 2;
            nickname = Encoding.UTF8.GetString(data, offset, nickbodyLen);
        }
    }

    public struct PacketStart : IPacket
    {
        public PacketType Type => PacketType.Start;

        public int playerId;
        public byte[] Serialize()
        {

            byte[] packet = new byte[8];

            Array.Copy(BitConverter.GetBytes((ushort)packet.Length), 0, packet, 0, 2);
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, 2, 2);

            Array.Copy(BitConverter.GetBytes(playerId), 0, packet, 4, 4);

            return packet;
        }

        public void Deserialize(byte[] data)
        {
            playerId = BitConverter.ToInt32(data, 0);
        }
    }
    public struct PacketReady : IPacket
    {
        public PacketType Type => PacketType.Ready;

        public int playerId;
        public byte[] Serialize()
        {

            byte[] packet = new byte[8];

            Array.Copy(BitConverter.GetBytes((ushort)packet.Length), 0, packet, 0, 2);
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, 2, 2);

            Array.Copy(BitConverter.GetBytes(playerId), 0, packet, 4, 4);

            return packet;
        }

        public void Deserialize(byte[] data)
        {
            playerId = BitConverter.ToInt32(data, 0);
        }
    }
    public struct PacketFastMatchingRequest : IPacket
    {
        public PacketType Type => PacketType.FastMatchingRequest;

        public byte[] Serialize()
        {

            byte[] packet = new byte[4];

            Array.Copy(BitConverter.GetBytes((ushort)packet.Length), 0, packet, 0, 2);
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, 2, 2);

            return packet;
        }

        public void Deserialize(byte[] data)
        {
        }
    }
    public struct PacketFastMatchingCancel : IPacket
    {
        public PacketType Type => PacketType.FastMatchingCancel;

        public byte[] Serialize()
        {

            byte[] packet = new byte[4];

            Array.Copy(BitConverter.GetBytes((ushort)packet.Length), 0, packet, 0, 2);
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, 2, 2);

            return packet;
        }

        public void Deserialize(byte[] data)
        {
        }
    }
    public struct PacketFastMatchingSuccess : IPacket
    {
        public PacketType Type => PacketType.FastMatchingSuccess;

        public int myRole;
        public int enemyId;
        public byte[] Serialize()
        {

            byte[] packet = new byte[12];

            Array.Copy(BitConverter.GetBytes((ushort)packet.Length), 0, packet, 0, 2);
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, 2, 2);

            Array.Copy(BitConverter.GetBytes(myRole), 0, packet, 4, 4);
            Array.Copy(BitConverter.GetBytes(enemyId), 0, packet, 8, 4);

            return packet;
        }

        public void Deserialize(byte[] data)
        {
            myRole = BitConverter.ToInt32(data, 0);
            enemyId = BitConverter.ToInt32(data, 4);
        }
    }

    public struct PacketPlayerAttack : IPacket
    {
        public PacketType Type => PacketType.PlayerAttack;

        public int playerId;
        public float angle;
        public float speed;
        public byte[] Serialize()
        {

            byte[] packet = new byte[16];

            Array.Copy(BitConverter.GetBytes((ushort)packet.Length), 0, packet, 0, 2);
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, 2, 2);

            Array.Copy(BitConverter.GetBytes(playerId), 0, packet, 4, 4);
            Array.Copy(BitConverter.GetBytes(angle), 0, packet, 8, 4);
            Array.Copy(BitConverter.GetBytes(speed), 0, packet, 12, 4);

            return packet;
        }

        public void Deserialize(byte[] data)
        {
            playerId = BitConverter.ToInt32(data, 0);
            angle = BitConverter.ToSingle(data, 4);
            speed = BitConverter.ToSingle(data, 8);
        }
    }

    public struct PacketPlayerHit : IPacket
    {
        public PacketType Type => PacketType.PlayerHit;

        public int attackerId;
        public int targetId;
        public byte[] Serialize()
        {

            byte[] packet = new byte[12];

            Array.Copy(BitConverter.GetBytes((ushort)packet.Length), 0, packet, 0, 2);
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, 2, 2);

            Array.Copy(BitConverter.GetBytes(attackerId), 0, packet, 4, 4);
            Array.Copy(BitConverter.GetBytes(targetId), 0, packet, 8, 4);

            return packet;
        }

        public void Deserialize(byte[] data)
        {
            attackerId = BitConverter.ToInt32(data, 0);
            targetId = BitConverter.ToInt32(data, 4);
        }
    }

    public struct PacketPlayerHpUpdate : IPacket
    {
        public PacketType Type => PacketType.PlayerHpUpdate;

        public int playerId;
        public int currentHp;
        public byte[] Serialize()
        {

            byte[] packet = new byte[12];

            Array.Copy(BitConverter.GetBytes((ushort)packet.Length), 0, packet, 0, 2);
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, 2, 2);

            Array.Copy(BitConverter.GetBytes(playerId), 0, packet, 4, 4);
            Array.Copy(BitConverter.GetBytes(currentHp), 0, packet, 8, 4);

            return packet;
        }

        public void Deserialize(byte[] data)
        {
            playerId = BitConverter.ToInt32(data, 0);
            currentHp = BitConverter.ToInt32(data, 4);
        }
    }

    public struct PacketPlayerDie : IPacket
    {
        public PacketType Type => PacketType.PlayerDie;

        public int playerId;
        public byte[] Serialize()
        {

            byte[] packet = new byte[8];

            Array.Copy(BitConverter.GetBytes((ushort)packet.Length), 0, packet, 0, 2);
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, 2, 2);

            Array.Copy(BitConverter.GetBytes(playerId), 0, packet, 4, 4);

            return packet;
        }

        public void Deserialize(byte[] data)
        {
            playerId = BitConverter.ToInt32(data, 0);
        }
    }
    public struct PacketGameEnd: IPacket
    {
        public PacketType Type => PacketType.GameEnd;

        public int loserId;
        public int reason;
        public byte[] Serialize()
        {

            byte[] packet = new byte[8];

            Array.Copy(BitConverter.GetBytes((ushort)packet.Length), 0, packet, 0, 2);
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, 2, 2);

            Array.Copy(BitConverter.GetBytes(loserId), 0, packet, 4, 4);

            return packet;
        }

        public void Deserialize(byte[] data)
        {
            loserId = BitConverter.ToInt32(data, 0);
        }
    }
    public struct PacketMoveStack : IPacket
    {
        public PacketType Type => PacketType.MoveStack;

        public int stack;
        public byte[] Serialize()
        {

            byte[] packet = new byte[12];

            Array.Copy(BitConverter.GetBytes((ushort)packet.Length), 0, packet, 0, 2);
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, 2, 2);

            Array.Copy(BitConverter.GetBytes(stack), 0, packet, 4, 4);

            return packet;
        }

        public void Deserialize(byte[] data)
        {
            stack = BitConverter.ToInt32(data, 0);
        }
    }
    public struct PacketLeaveGame : IPacket
    {
        public PacketType Type => PacketType.LeaveGame;

        public int playerId;

        public byte[] Serialize()
        {

            byte[] packet = new byte[8];

            Array.Copy(BitConverter.GetBytes((ushort)packet.Length), 0, packet, 0, 2);
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, 2, 2);
            Array.Copy(BitConverter.GetBytes(playerId), 0, packet, 4, 4);

            return packet;
        }

        public void Deserialize(byte[] data)
        {
            playerId = BitConverter.ToInt32(data, 0);
        }
    }
    public struct PacketWinCountUpdate : IPacket
    {
        public PacketType Type => PacketType.WinCountUpdate;

        public int playerId;
        public int winCount;

        public byte[] Serialize()
        {

            byte[] packet = new byte[12];

            Array.Copy(BitConverter.GetBytes((ushort)packet.Length), 0, packet, 0, 2);
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, 2, 2);
            Array.Copy(BitConverter.GetBytes(playerId), 0, packet, 4, 4);
            Array.Copy(BitConverter.GetBytes(winCount), 0, packet, 8, 4);

            return packet;
        }

        public void Deserialize(byte[] data)
        {
            playerId = BitConverter.ToInt32(data, 0);
            winCount = BitConverter.ToInt32(data, 4);
        }
    }

    public struct PacketWinCountUpdateRequest : IPacket
    {
        public PacketType Type => PacketType.WinCountUpdateRequest;

        public byte[] Serialize()
        {

            byte[] packet = new byte[4];

            Array.Copy(BitConverter.GetBytes((ushort)packet.Length), 0, packet, 0, 2);
            Array.Copy(BitConverter.GetBytes((ushort)Type), 0, packet, 2, 2);


            return packet;
        }

        public void Deserialize(byte[] data)
        {

        }
    }
}
