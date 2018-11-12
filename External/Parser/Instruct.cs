using System;
using System.Security;
using VM.Bases;
using VM.Component;
using VM.Global;

namespace VM.Parser
{
    [SecuritySafeCritical]
    [SecurityCritical]
    public partial class Instruct
    {
        public Operand Desdestination;
        public byte Function;
        public Operand Source;

        public int NCLength()
        {
            return Length%8 == 0 ? Length/8 : Length/8 + 1;
        }
        public int Length
        {
            get
            {
                return (Desdestination.OperandType != OperandType.none
                    ? Source.Length
                    : 0) + 8 + Desdestination.Length;
            }

        }
        public override string ToString()
        {
            return UAL.NameInstructions[Function] +
                   (Desdestination.OperandType != OperandType.none
                       ? (" " + Desdestination + (Source.OperandType != OperandType.none ? "," + Source : ""))
                       : "");
        }

        public static Instruct Parse(string s)
        {
            var c = s.Split(new[] {' ', ','}, StringSplitOptions.RemoveEmptyEntries);
            var i = new Instruct();
            i.Function = (byte) UAL.NameInstructions.IndexOf(c[0]);
            if (c.Length > 1)
                i.Desdestination = Operand.Parse(c[1]);
            if (c.Length > 2)
                i.Source = Operand.Parse(c[2]);
            return i;
        }
    }

    public partial class Instruct
    {
        public bool Equals(Instruct obj)
        {
            if (Function == obj.Function)
                if (Desdestination.Equals(obj.Desdestination))
                    if (Source.Equals(obj.Source)) return true;
            return false;
        }

        public void Push(Stream stream)
        {
            stream.push(Function, (int) AsmDataType.Byte);
            Desdestination.Push(stream);
            if (Desdestination.OperandType != OperandType.none)
                Source.Push(stream);
            if (!stream.Compressed)
                stream.ToNextByte();
        }
        
        public static Instruct Pop(StreamReader stream)
        {
            var i = new Instruct {Function = (byte) stream.read((int) AsmDataType.Byte)};
            i.Desdestination = Operand.Pop(stream);
            i.Source = i.Desdestination.OperandType != OperandType.none ? Operand.Pop(stream) : new Operand();
            if (!stream.Stream.Compressed)
                stream.shift();
            return i;
        }

        private static Stream Decompress(Stream stream)
        {
            if (stream.Compressed)
            {
                var Stream = new Stream(false, stream.Capacity + 400);
                var s = new StreamReader(stream);
                for (var i = 0; i < stream.Offset; i++)
                    Pop(s).Push(Stream);
                return Stream;
            }
            return stream;
        }
    }
}