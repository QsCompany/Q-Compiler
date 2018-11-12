using System;
using VM.Bases;

namespace VM.Global
{
    public partial class StreamReader
    {
        public Stream Stream;
        private int _ip;

        public int Offset
        {
            get; internal set; 
            
        }

        public bool IsConnected
        {
            get { return !(Offset >= Stream.Content.Length - 4); }
        }

        public int IP
        {
            get
            {
                return _ip;
            }

            set
            {
                Offset += value / 8;
                _ip = value % 8;
            }
        }

        public bool Seek(int offset)
        {
            if (offset >= Stream.Capacity) return false;
            Offset = offset;
            return true;
        }

        public static implicit operator Stream(StreamReader s)
        {
            return s.Stream;
        }

    }
    public partial class StreamReader
    {
        public StreamReader(Stream stream)
        {
            Stream = stream;
        }

        public int read(int length_bits)
        {
            var r = read(length_bits, Offset, IP);
            IP += length_bits;
            return r;
        }

        public int read(int length_bits, int offset,int ip=0)
        {
            if (length_bits > (int)AsmDataType.DWord)
                throw new OverflowException("the data cannot be greater than " + (int)DataType.DWord);
            if (offset + (ip + length_bits) / 8 >= Stream.Content.Length - 1)
                throw new OverflowException("stream data has disconnected " + (int)DataType.DWord);
            return Bit.Decode(Stream.Content.get(ip, length_bits, offset));
        }
        public void reset()
        {
            Offset = 0;
            IP = 0;
        }

        public void shift()
        {
            if (_ip == 0) return;
            _ip = 0;
            Offset++;
        }
    }
}