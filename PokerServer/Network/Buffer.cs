using System;
using System.Collections.Generic;
using System.Text;

namespace PokerServer.Network
{
    class Buffer
    {
        private byte[] _data;
        private int _index;

        public Buffer(byte[] data, int startPos = 0)
        {
            _data = data;
            _index = startPos;
        }

        public int ReadInt32()
        {
            return BitConverter.ToInt32(_data, _index);
        }
    }
}
