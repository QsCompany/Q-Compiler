//using System.Collections.Generic;
//using Compiler.Bases;

//namespace Compiler.Global
//{
//    public struct Instruction
//    {
//        internal string Inst;
//        internal string Param1;
//        internal string Param2;

//        public Instruction(string inst, string param1 = null, string param2 = null)
//        {
//            Inst = inst;
//            Param1 = param1;
//            Param2 = param2;
//        }

//        public override string ToString()
//        {
//            return (string.Concat(Inst, " ", Param1) + (!string.IsNullOrEmpty(Param2) ? " ," + Param2 : ""));
//        }
//    }
//    public class Program : List<Instruction>
//    {
//        public readonly Arbre<Structs> Head = new Arbre<Structs>(null, new Structs());

//        public override string ToString()
//        {
//            var s = "";
//            foreach (var inst in this)
//                s += string.Format("{0} {1}\n", inst.Inst,
//                    (string.IsNullOrEmpty(inst.Param1)
//                        ? ""
//                        : (inst.Param1 + (string.IsNullOrEmpty(inst.Param2) ? "" : "," + inst.Param2))));
//            return s;
//        }

//        public byte[] GetBytes()
//        {
//            return null;
//        }

//        public void Index(Instruction s)
//        {
//            switch (s.Inst)
//            {
//                case "proc":
//                case "space":
//                case "class":
//                    var v = new Structs { Type = s.Inst, Name = s.Param1, pointer = Count };
//                    Head.Open(v);
//                    break;

//                case "end space":
//                case "end class":
//                case "end proc":
//                    Head.Close();
//                    break;
//            }
//        }
//    }
//    public class Arbre<T> : List<Arbre<T>>
//    {
//        public string NodeName;

//        public Arbre<T> Parent;
//        public Arbre<T> _current;

//        public Arbre(Arbre<T> parent, T value)
//        {
//            Parent = parent;
//            Value = value;
//        }

//        public Arbre<T> Current
//        {
//            get { return _current ?? this; }
//            set { _current = value; }
//        }

//        public T Value { get; set; }

//        public void Close()
//        {
//            Current = Current.Parent;
//        }

//        public void Open(T value)
//        {
//            var v = new Arbre<T>(Current, value);
//            Current.Add(v);
//            Current = v;
//        }

//        public override string ToString()
//        {
//            return Value.ToString();
//        }
//    }
//}