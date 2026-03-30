using System;
using System.Net;

namespace RPGCommon
{
    public abstract class PacketSession
    {
        public const int HeaderSize = 4; // Size(2) + ID(2)

        private RecvBuffer _recvBuffer = new RecvBuffer(1024 * 10);

        //소켓에서 받은 바이트를 버퍼에 넣음
        public void OnReceive(byte[] buffer, int offset, int bytesTransferred)
        {
            Array.Copy(buffer, offset, _recvBuffer.WriteSegment.Array, _recvBuffer.WriteSegment.Offset, bytesTransferred);

            _recvBuffer.OnWrite(bytesTransferred);

            while (true)
            {
                // 1. 헤더 최소 크기 확인
                if (_recvBuffer.DataSize < HeaderSize)
                    break;

                // 2. 패킷 전체 크기 파싱 (헤더의 첫 2바이트)
                ushort dataSize = BitConverter.ToUInt16(_recvBuffer.ReadSegment.Array, _recvBuffer.ReadSegment.Offset);

                // 3. 패킷 본문까지 모두 수신되었는지 확인
                if (_recvBuffer.DataSize < dataSize)
                    break;

                // 4. 사이즈 확인 후 패킷의 아이디 확인
                ushort protocolId = BitConverter.ToUInt16(_recvBuffer.ReadSegment.Array, _recvBuffer.ReadSegment.Offset + 2);

                // Body만 따로 복사해서 저장
                byte[] packetBody = new byte[dataSize - HeaderSize];
                Array.Copy(_recvBuffer.ReadSegment.Array, _recvBuffer.ReadSegment.Offset + HeaderSize, packetBody, 0, dataSize - HeaderSize);

                // 실제 처리는 상속받은 클래스에게 전달
                OnPacketReceived(protocolId, packetBody);

                //처리가 끝난 패킷만큼 커서 이동
                _recvBuffer.OnRead(dataSize);
            }

            _recvBuffer.Clean();
        }

        // 서버/클라가 각자 처리
        protected abstract void OnPacketReceived(ushort protocolId, byte[] body);
        public virtual void OnDisconnected(EndPoint endPoint)
        {

        }
    }
}