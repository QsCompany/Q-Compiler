using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

namespace VM.Global
{
    public class Stream
    {
        public readonly int ShiftCapacity;
        public readonly bool Compressed;
        private int _capacity;
        private int _offset;
        private int _ip;
        public volatile byte[] Content;
        
        
        public int Capacity
        {
            [SecurityCritical]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            private set
            {
                if (value <= _capacity) return;
                
                var p = new byte[value];
                lock (p)
                {
                    lock (Content)
                    {
                        Array.Copy(Content, 0, p, 0, _capacity);
                        Content = p;
                        _capacity = value;
                    }
                }
            }
            [SecurityCritical]
            get { return _capacity; }
        }

        public int Offset
        {
            [SecurityCritical]
            get { return _offset; }
            [SecurityCritical]
            internal set
            {
                if (value >= _capacity - 100) Capacity += ShiftCapacity;
                _offset = value;
            }
        }
        public int IP
        {
            [SecurityCritical]
            get { return _ip; }
            [SecurityCritical]
            internal set
            {
                Offset += value/8;
                _ip = value%8;
            }
        }


        public void ToNextByte()
        {
            if (IP == 0) return;
            IP = 0;
            Offset++;
        }
        
        public void Reset(bool deepClean = false)
        {
            if (deepClean)
                for (var i = 0; i <= Offset; i++)
                    Content[i] = 0;
            IP = 0;
            Offset = 0;
        }
        
        public void push(byte[] bytes, int length_Bits)
        {
            Content.set(bytes, IP, length_Bits, Offset);
            IP += length_Bits;
        }

        public void push(byte[] bytes, int length_Bits, int offset, int ip = 0)
        {
            Content.set(bytes, ip, length_Bits, offset);
        }
        
        public void push(byte value, int length)
        {
            Content.setByte(value, IP, length, Offset);
            IP += length;
        }

        public Stream(ref byte[] array,int shiftCapacity,bool Compressed)
        {
            Content = array;
            ShiftCapacity = shiftCapacity;
            this.Compressed = Compressed;
        }
        
        public Stream(bool compressed = true, int shiftCapacity = 1024, int initialSize = 1024)
        {
            _offset = 0;
            _ip = 0;
            ShiftCapacity = shiftCapacity;
            Compressed = compressed;
            Content = new byte[initialSize];
            _capacity = initialSize;
        }

        public Stream(System.IO.Stream stream)
        {
            stream.Read(Content, 0, (int) stream.Length);
        }

        public void Save(System.IO.Stream stream, bool append,bool allBits)
        {
            if (!append)
                stream.Flush();
            stream.Write(Content, 0, allBits ? _capacity : _offset);
        }

        public void Load(System.IO.Stream stream, bool append)
        {
            var e = new byte[stream.Length];
            stream.Read(e, 0, e.Length);
            if (!append)
                Reset(true);
            Capacity += e.Length - _offset + 100;
            push(e, e.Length*8);
        }

        public void CopyTo(out byte[] array)
        {
            array = new byte[Offset];
            Array.Copy(Content, array, Offset);
        }
    }
}
