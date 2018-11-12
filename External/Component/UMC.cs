using System;
using VM.Bases;
using VM.Global;
using VM.Parser;

namespace VM.Component
{
    public class UMC
    {
        public readonly Component MRT; 

        public UMC(Component MRT)
        {
            this.MRT = MRT;
        }

        public int Mem_GetValue(int i)
        {
            var i1 = GetAddress(i);
            return BitConverter.ToInt32(MRT.Cache.Cache.Content, i1);

        }

        public void Mem_SetValue(Operand o, int value)
        {
            var i1 = GetAddress(o.Value);
            var c = Bit.Coder(value);
            for (var i = 0; i < c.Length; i++)
                MRT.Cache.Cache.Content[i1 + i] = c[i];
        }

        public int GetAddress(int memoperand)
        {
            var RegOffset = new Nbit(4);
            var MemPoint = new Nbit(28);
            RegOffset.Value = memoperand >> 28;
            MemPoint.Value = memoperand;
            var i1 = (RegOffset.Value != 15 ? MRT.Registers[RegOffset.Value] : 0) + (int) MemPoint;
            return i1;
        }

        public int GetValue(Operand o)
        {
            switch (o.OperandType)
            {
                case OperandType.none:
                    return 0;
                case OperandType.Reg:
                    return MRT.Registers[o.Value];
                case OperandType.Mem:
                    return Mem_GetValue(o.Value);
                case OperandType.imm:
                    switch (o.DataType)
                    {
                        case DataType.Hex:
                            return o.Value & 0xf;
                        case DataType.Byte:
                            return o.Value & 0xff;
                        case DataType.Word:
                            return o.Value & 0xfffff;
                        case DataType.DWord:
                            return o.Value;
                        default:
                            return o.Value;
                    }
            }
            throw new Exception("Not Listed");
        }

        public void SetValue(Operand o, int value)
        {
            switch (o.OperandType)
            {
                case OperandType.none:
                    o.Value = 0;
                    break;
                case OperandType.Reg:
                    MRT.Registers[o.Value] = value;
                    break;
                case OperandType.Mem:
                    Mem_SetValue(o, value);
                    break;
                case OperandType.imm:
                    throw new Exception("Impossile to set a value to constant");
            }
        }
    }
}