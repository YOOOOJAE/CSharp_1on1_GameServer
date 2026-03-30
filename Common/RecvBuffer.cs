using System;

namespace RPGCommon
{
    public class RecvBuffer
    {
        ArraySegment<byte> _buffer;
        int _readPos;
        int _writePos;

        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        public int DataSize => _writePos - _readPos;
        public int FreeSize => _buffer.Count - _writePos;

        // 데이터를 읽어들일 범위
        public ArraySegment<byte> ReadSegment => new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize);

        //데이터를 받을 수 있는 남은 범위
        public ArraySegment<byte> WriteSegment => new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize);

        public void OnRead(int numOfBytes)
        {
            _readPos += numOfBytes;
        }

        public void OnWrite(int numOfBytes)
        {
            _writePos += numOfBytes;
        }

        public void Clean()
        {
            int dataSize = DataSize;
            if (dataSize == 0)
            {
                // 남은 데이터가 없으면 커서 원위치
                _readPos = _writePos = 0;
            }
            else
            {
                // 남은 데이터가 있으면 앞으로 당김
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }
    }
}