using System;
using System.Collections.Generic;
using VM.Global;

namespace Compiler.Classes.Assembly
{
    public class MethodInfo : MembreInfo
    {
        public readonly Dictionary<int, string> JumpInstruction = new Dictionary<int, string>();
        public readonly Dictionary<string, int> LabelsInstruction = new Dictionary<string, int>();
        public int DataSize, MethodSize;
        public List<Type> Params = new List<Type>();
        private readonly StreamReader _streamReader;

        public StreamReader StreamReader
        {
            get
            {
                _streamReader.Seek(Offset);
                return _streamReader;
            }
        }

        public MethodInfo(StreamWriter stream)
        {
            _streamReader = new StreamReader(stream);
        }
        internal MethodInfo(){}

        public override string ToString()
        {
            var c = Name + "(";
            bool str = true;
            foreach (var param in Params)
            {
                c += (str ? "" : ",") + param;
                str = false;
            }
            return c + ")";
        }

        [NonSerialized] public List<FieldInfo> LocalVariables = new List<FieldInfo>();
        [NonSerialized] public List<Label<MethodInfo>> Calls = new List<Label<MethodInfo>>();
        public bool IsConstruct;
    }
}