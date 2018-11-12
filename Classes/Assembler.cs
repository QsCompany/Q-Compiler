using System;
using System.Collections.Generic;
using Compiler.Classes.Assembly;
using Type = Compiler.Classes.Assembly.Type;
using VM.Parser;
using VM.Component;
namespace Compiler.Classes
{
    [Serializable]
    public class Assembler
    {
        public Assembler(Lexical lexical)
        {
            Lexical = lexical;
        }

        private readonly Lexical Lexical;
        public Type Type;

        public string Name { get; set; }
        public Tree LeftParam { get; set; }
        public Tree RightParam { get; set; }
        public byte Function { get; set; }
        public string Fn { get; set; }

        public static int SetFunc(string func,out string Fn)
        {
            byte Function = (byte)Descripter.COperators.IndexOf(func);// (byte)IndexOf(Const.Funcs, func);
            if (Function > 100)
            {
                switch (func)
                {
                    case Const.space:
                        Function = 254;
                        break;
                    case Const.@class:
                        Function = 253;
                        break;
                    case Const.proc:
                        Function = 252;
                        break;

                    case Const.endSpace:
                        Function = 255;
                        break;
                    case Const.endClass:
                        Function = 251;
                        break;
                    case Const.endProc:
                        Function = 250;
                        break;
                    case "eq":
                        Function = 0;
                        break;
                }
                Fn = func;
            }
            else Fn = Descripter.COperators[Function].Value;// Const.Funcs[Function];
            return Function;
        }
        public void Set(string func)
        {
            Function = (byte)Descripter.COperators.IndexOf(func);// (byte)IndexOf(Const.Funcs, func);
            if (Function > 100)
            {
                switch (func)
                {
                    case Const.space:
                        Function = 254;
                        break;
                    case Const.@class:
                        Function = 253;
                        break;
                    case Const.proc:
                        Function = 252;
                        break;

                    case Const.endSpace:
                        Function = 255;
                        break;
                    case Const.endClass:
                        Function = 251;
                        break;
                    case Const.endProc:
                        Function = 250;
                        break;
                    case "eq":
                        Function = 0;
                        break;
                }
                Fn = func;
            }
            else Fn = Descripter.COperators[Function].Value;// Const.Funcs[Function];
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (LeftParam != null ? LeftParam.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (RightParam != null ? RightParam.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Function.GetHashCode();
                hashCode = (hashCode*397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Fn != null ? Fn.GetHashCode() : 0);
                return hashCode;
            }
        }
        public override string ToString()
        {
            var s = Fn ;
            if (LeftParam != null)
            {
                s += " " + Lexical.GetVariableOffset(LeftParam);
                if (RightParam != null)
                    s += "," + Lexical.GetVariableOffset(RightParam);
            }
            return s;
        }
        public Instruct ToInstruct()
        {

            var e=UAL.NameInstructions.IndexOf(Fn);
            return new Instruct()
            {
                Function = e == -1 ? Function : (byte)e,
                Desdestination = LeftParam != null ? Lexical.GetVariableOffset(LeftParam) : default(Operand),
                Source = RightParam != null ? Lexical.GetVariableOffset(RightParam) : default(Operand),
            };
        }
        public bool Equals(Assembler other)
        {
            return Equals(LeftParam, other.LeftParam) && Equals(RightParam, other.RightParam) && Function == other.Function && string.Equals(Name, other.Name) && string.Equals(Fn, other.Fn);
        }
        public static int IndexOf<T>(IEnumerable<T> lst, T value)
        {
            int i = 0;
            foreach (var l in lst)
                if (l.Equals(value)) return i;
                else i++;
            return -1;
        }
        public Assembler(string name, Lexical lexical): this(lexical)
        {
            Name = name;
        }
        public static string ToString(IEnumerable<Assembler> assemblers)
        {
            var see = "";
            foreach (var assembler in assemblers)
                see += "\r\n" + assembler;
            return see;
        }
    }
}