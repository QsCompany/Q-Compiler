namespace Compiler.Classes.Assembly
{
    using System;
    using Compiler.Classes.Compile;
    using VM.Bases;
    using VM.Component;
    using VM.Global;
    using VM.Parser;
    using Kind = Compiler.Help.Kind;
    using global::System.Collections.Generic;

    [Flags]
    internal enum Scop
    {
        Space = 0,
        Class = 1,
        Method = 2,
    }
    public partial class Lexical
    {
        public static readonly List <string> AssemblyList = new List <string>();
        public static readonly List <Consts> ConstsList = new List <Consts>();

        private MethodInfo GetInfoInvokation (Assembler invoker, out Type Class)
        {
            Class = this.CurrentType;
            return this.CurrentMethod;
        }

        public void Call (MethodInfo t)
        {
            if ( IsFromCurrentAssembly(t) ) {
                /*  Value: shifset of method from Current address of instruction  (><)0
                 * call 0xXXXXXX                todo: Becarful 0xXXXXXX Is the Offset
                */
                var instruct = new Instruct();
                instruct.Function = (byte) UAL.NameInstructions.IndexOf(Const.call);
                this.CurrentMethod.Calls.Add(new Label <MethodInfo> {index = this.StreamWriter.Offset, Value = t});
                instruct.Desdestination.OperandType = OperandType.imm;
                instruct.Desdestination.DataType = DataType.Word;
                instruct.Desdestination.Value = t.Offset - this.StreamWriter.Offset;
                instruct.Push(this.StreamWriter);
            }
            else {
                #region

                /*  <loc_Assembly> contain the address of assembly by afect it by the variable assembly
                 *  Offset:Point   shifset of method in assembly >=0
                 *  
                 *  eax=<loc_Assembly>+Offset
                 *  Value= eax
                 *  
                 * mov eax,[assI]
                 * add eax,offset
                 * call eax
                 */

                #endregion

                Instruct.Parse("mov eax,[esi+" + AssemblyList.IndexOf(t.Return.Space.FullName) + "]").Push(this.StreamWriter);
                this.CurrentMethod.Calls.Add(new Label <MethodInfo> {index = this.StreamWriter.Offset, Value = t});
                Instruct.Parse("add eax,0x" + t.Offset.ToString("x4")).Push(this.StreamWriter);
                Instruct.Parse("call eax").Push(this.StreamWriter);
            }
        }

        private void CallManager (Assembler t, ref Instruct instruct)
        {
            instruct.Desdestination.OperandType = OperandType.Mem;
            Type Class;
            MethodInfo mi = GetInfoInvokation(t, out Class);
            this.CurrentMethod.Calls.Add(new Label <MethodInfo> {index = this.StreamWriter.Offset, Value = mi});
            if ( IsFromCurrentAssembly(mi) ) {
                /*  Value: shifset of method from Current address of instruction  (><)0
                 * call 0xXXXXXX                todo: Becarful 0xXXXXXX Is the Offset
                */
                instruct.Desdestination.OperandType = OperandType.imm;
                instruct.Desdestination.DataType = DataType.Word;
            }
            else {
                #region

                /*  <loc_Assembly> contain the address of assembly by afect it by the variable assembly
                 *  Offset:Point   shifset of method in assembly >=0
                 *  
                 *  eax=<loc_Assembly>+Offset
                 *  Value= eax
                 *  
                 * mov eax,[assI]
                 * add eax,offset
                 * call eax
                 */

                #endregion

                Instruct.Parse("mov eax,[esi+" + AssemblyList.IndexOf(Class.Space.FullName) + "]").Push(this.StreamWriter);
                Instruct.Parse("add eax,0x0000");
                Instruct.Parse("call eax");
                instruct.Desdestination.OperandType = OperandType.Reg;
                instruct.Desdestination.Value = 0; /*eax*/
            }
        }

        private static bool IsFromCurrentAssembly (MethodInfo funcName) { return true; }
    }
    public partial class Lexical
    {
        public readonly Dictionary <int, string> JumpInstruction = new Dictionary <int, string>();
        public readonly Dictionary <string, int> LabelsInstruction = new Dictionary <string, int>();
        public readonly Dictionary<Tree, int> newClasses = new Dictionary<Tree, int>();
        public UAL ual = new UAL(null);
        
        public int EPSInc;
        internal readonly StreamWriter StreamWriter;
        private readonly StreamReader StreamReader;
        public readonly Assembly CurrentAssembly;
        public Space CurrentSpace;
        public MethodInfo CurrentMethod;
        private Scop GlobalScope;
        internal Type CurrentType;

        #region OpenClose Scopes

        private bool FunctionPrototypeExist(string Name, IList<Tree> parametres, out MethodInfo methodInfo)
        {
            foreach (MethodInfo method in this.CurrentType.Methods)
                if (method.Name.Equals(Name))
                {
                    bool b = true;
                    if (parametres.Count != method.Params.Count) continue;
                    for (int j = 0; j < parametres.Count; j++)
                    {
                        string s = parametres[j].Children[0].Content;
                        Type param = method.Params[j ];
                        if (s.Equals(param.FullName) || s.Equals(param.Name)) continue;
                        b = false;
                        break;
                    }
                    if (b)
                    {
                        methodInfo = method;
                        return true;
                    }
                }
            methodInfo = null;
            return false;
        }

        private readonly List <string> NameSpaceTags = new List <string>();


        #endregion

        public Lexical (Assembly assembly,StreamWriter stream)
        {
            if ( stream == null ) stream = new StreamWriter(false, 1024, 2048);
            this.StreamWriter = stream;
            this.StreamReader = new StreamReader(this.StreamWriter);
            this.CurrentAssembly = assembly;
        }
    }
    public partial class Lexical
    {
        public void OpenClass (string baseName, string className)
        {
            this.GlobalScope = Scop.Class;
            this.CurrentType = CurrentSpace[className, true] ?? new Type(CurrentSpace, System.Assembly.GetType(baseName) ?? System.Assembly.GetType("System.object"), className, true);
        }

        public void CloseClass()
        {
            this.CurrentType = null;
            this.GlobalScope = Scop.Space;
        }

        public void OpenNameSpace (string Name)
        {
            this.GlobalScope = Scop.Space;

            if (CurrentSpace == null)
            {
                CurrentSpace = CurrentAssembly[Name];
                if (CurrentSpace == null)
                    CurrentAssembly.Spaces.Add(CurrentSpace = new Space(null, Name));
            }
            else CurrentSpace = CurrentSpace.Add(Name);

            this.NameSpaceTags.Add(Name);
        }

        public void CloseNameSpace()
        {
            Space s = CurrentSpace.GoBack(this.NameSpaceTags[this.NameSpaceTags.Count - 1]);
            //if (s == null) throw new Exception("NameSpaceCloseTagsError");
            CurrentSpace = s;
            this.NameSpaceTags.RemoveAt(this.NameSpaceTags.Count - 1);
            this.GlobalScope = Scop.Space;
        }

        public void OpenMethod(string Name, string returnType, IList<Tree> parametres)
        {
            this.GlobalScope = Scop.Method;
            this.LabelsInstruction.Clear();
            this.JumpInstruction.Clear();
            this.AuxVariables.Clear();
            if (FunctionPrototypeExist(Name, parametres, out this.CurrentMethod))
            {
                this.CurrentMethod.Offset = this.StreamWriter.Offset;
                return;
            }
            this.CurrentMethod = new MethodInfo(this.StreamWriter);

            #region Ajouter Les Variable Local

            foreach (Tree tree in parametres) 
                SetVariable(tree.Children[0].Content, tree.Children[1].Content);

            #endregion

            #region Make Prototype

            for (int i = 0; i < this.CurrentMethod.LocalVariables.Count; i++) 
                this.CurrentMethod.Params.Add(this.CurrentMethod.LocalVariables[i].Return);
            this.CurrentMethod.Return = this.System.Assembly.GetType(returnType);
            this.CurrentMethod.Name = Name;
            this.CurrentMethod.IsConstruct = (Name == returnType);
            this.CurrentMethod.Offset = this.StreamWriter.Offset;
            this.CurrentType.Methods.Add(this.CurrentMethod);

            #endregion
        }

        public void CloseFunction()
        {
            this.GlobalScope = Scop.Class;
            Instruct c;
            if (this.LabelsInstruction.Count != 0 || this.JumpInstruction.Count != 0)
            {
                int loff = this.StreamWriter.Offset;
                foreach (var i in this.JumpInstruction)
                {
                    this.StreamReader.Seek(i.Key);
                    this.StreamWriter.Seek(i.Key);
                    int add = this.LabelsInstruction[i.Value];
                    c = Instruct.Pop(this.StreamReader);
                    c.Desdestination.Value = add - i.Key - c.NCLength();
                    c.Push(this.StreamWriter);
                }
                this.StreamWriter.Seek(EPSInc);
                this.StreamReader.Seek(EPSInc);
                c = Instruct.Pop(this.StreamReader);
                c.Source.Value = CurrentMethod.DataSize;
                c.Push(this.StreamWriter);

                this.StreamWriter.Seek(loff);
                JumpInstruction.Clear();
                LabelsInstruction.Clear();

            }

            this.CurrentMethod.MethodSize = this.StreamWriter.Offset - this.CurrentMethod.Offset;
        }
    }
    public partial class Lexical
    {
        public System System;
        public List <AuxVariable> AuxVariables = new List <AuxVariable>();
        public void DisactiveVariable (FieldInfo field) { foreach ( AuxVariable auxVariable in this.AuxVariables ) if ( auxVariable.Field.Equals(field) ) auxVariable.IsInLive = false; }
        public void DisactiveVariable (string fieldName) { foreach ( AuxVariable auxVariable in this.AuxVariables ) if ( auxVariable.Field.Name.Equals(fieldName) ) auxVariable.IsInLive = false; }

        public FieldInfo GetNewVaiable (string type, string name = null)
        {
            foreach ( AuxVariable auxVariable in this.AuxVariables )
                if ( !auxVariable.IsInLive && auxVariable.Field.Return.FullName == type ) {
                    auxVariable.IsInLive = true;
                    return auxVariable.Field;
                }
            FieldInfo e = SetVariable(type, name ?? "<var" + this.AuxVariables.Count + ">");
            this.AuxVariables.Add(new AuxVariable(e, true));
            return e;
        }

        public FieldInfo GetNewVaiable (Type type, string name = null)
        {
            foreach ( AuxVariable auxVariable in this.AuxVariables )
                if ( !auxVariable.IsInLive && auxVariable.Field.Return.FullName == type.FullName ) {
                    auxVariable.IsInLive = true;
                    return auxVariable.Field;
                }
            FieldInfo e = SetVariable(type, name ?? "<var" + this.AuxVariables.Count + ">");
            this.AuxVariables.Add(new AuxVariable(e, true));
            return e;
        }

        public bool GetVariable (string name, out FieldInfo fieldInfo, out bool isGloBal)
        {
            isGloBal = true;
            if ( name == null ) {
                fieldInfo = null;
                return isGloBal = false;
            }
            bool Glob = name.StartsWith("this.");
            if ( Glob ) name = name.Substring(5);
            else
                foreach ( FieldInfo variable in this.CurrentMethod.LocalVariables )
                    if ( variable.Name.Equals(name) ) {
                        isGloBal = false;
                        fieldInfo = variable;
                        return true;
                    }
            fieldInfo = this.CurrentType.GetField(name);
            if ( fieldInfo == null ) {
                foreach ( Consts cnst in ConstsList ) if ( cnst.Name == name ) return true;
                return isGloBal = false;
            }
            return true;
        }
        
        public Operand GetVariableOffset (Tree val)
        {
            FieldInfo var;
            bool isGlob;
            switch ( val.Kind ) {
                case Kind.Numbre:
                    return Operand.Parse(val.Content);
                case Kind.Label:
                    int i;
                    if (LabelsInstruction.TryGetValue(val.Content, out i)) return new Operand() { OperandType = OperandType.imm, DataType = DataType.Word, Value = StreamWriter.Offset - i };
                    return new Operand() { OperandType = OperandType.imm, DataType = DataType.Word, };
                case Kind.Return:
                    break;
                case Kind.Variable:
                    if ( !GetVariable(val.Content, out var, out isGlob) ) throw new Exception("Not fo");
                    int r = var.Offset;
                    goto h;
                case Kind.Hyratachy:
                    if ( !GetVariable(val.Children[0].Content, out var, out isGlob) ) throw new Exception("Not fo");
                    r = var.Offset;
                    for (i = 1; i < val.Children.Count; i++) {
                        FieldInfo c = var.Return.GetField(val.Children[i].Content);
                        if ( c == null ) throw new Exception("Membre not Found");
                        val.Children[i].Type = c.Return;
                        r += c.Offset;
                        var = c;
                    }
                    h:
                    if ( isGlob ) return Operand.Parse("[ebp+0x" + r.ToString("x7") + "]");
                    return Operand.Parse("[esp+0x" + r.ToString("x7") + "]");
                case Kind.Const:
                    break;
                case Kind.Register:
                    return Operand.Parse(val.Content);
            }
            throw new Exception("Variable expected");
        }

        public List<MethodInfo> GetStaticFunctions(string method)
        {
            var d=method.LastIndexOf('.');
            var meth = method.Substring(d + 1);
            List<MethodInfo> rets = new List<MethodInfo>();
            MethodInfo m;
            foreach (var type in System.Assembly.GetAllType(method.Substring(0, d)))
            {
                m=type.GetMethod(method);
                if (m == null) rets.Add(m);
            }
            return rets;
        }
        
        public List <MethodInfo> GetFunctions (Tree val)
        {
            if ( val.Kind == Kind.Variable ) {
                List <MethodInfo> c = this.CurrentType.GetMethods(val.Content);
                return c;
            }
            if ( val.Kind == Kind.Hyratachy ) {
                FieldInfo var;
                bool isGlob;
                if (!GetVariable(val.Children[0].Content, out var, out isGlob))
                    return GetStaticFunctions(val.Content);
                int i;
                for (i = 1; i < val.Children.Count - 1; i++) {
                    FieldInfo c = var.Return.GetField(val.Children[i].Content);
                    if ( c == null ) throw new Exception("Membre not Found");
                    val.Children[i].Type = c.Return;
                    var = c;
                }
                List <MethodInfo> j;
                if ( (j = var.Return.GetMethods(val[i].Content)).Count == 0 ) throw new Exception("Method is not exist");
                return j;
            }
            throw new Exception("Function expected");
        }

        public MethodInfo GetMethod(Tree val, IList<Tree> Params)
        {
            List<MethodInfo> lst = GetFunctions(val);
            foreach (MethodInfo method in lst)
            {
                int i = 0;
                if (method.Params.Count != Params.Count) continue;
                for (; i < Params.Count; i++)
                    if (Params[i].Type != method.Params[i])
                    {
                        i = -1;
                        break;
                    }
                if (i == -1) continue;
                return method;
            }
            return null;
        }

        public void BeginRefCall (Space space)
        {
            foreach (var type in space.Types)
                foreach ( var method in type.Methods ) {
                    foreach ( var call in method.Calls ) {
                        StreamWriter.Save();
                        StreamReader.Save();
                        StreamReader.Seek(call.index);
                        var e = Instruct.Pop(StreamReader);
                        e.Desdestination.Value = call.Value.Offset - call.index - e.NCLength();
                        StreamWriter.Seek(call.index);
                        e.Push(StreamWriter);
                        StreamWriter.Restore();
                        StreamReader.Restore();
                    }
                }
            foreach ( var spa in space ) BeginRefCall(spa);
        }

        public MethodInfo GetMethod(string Name, params string[] paramTypes)
        {
            foreach (MethodInfo method in this.CurrentType.Methods)
            {
                if (method.Name != Name) continue;
                if (method.Params.Count != paramTypes.Length) continue;
                int i = 0;
                foreach (string type in paramTypes)
                    if (method.Params[i++].FullName != type)
                    {
                        i = -1;
                        break;
                    }
                if (i == method.Params.Count) return method;
            }
            return null;
        }

        public FieldInfo SetVariable(Type v, string varName)
        {
            var f = new FieldInfo { Return = v, Name = varName };
            if ( this.GlobalScope == Scop.Class )
            {
                f.Offset = this.CurrentType.DataSize;
                this.CurrentType.Fields.Add(f);
                if (f.Return.Equals(this.CurrentType)) throw new Exception("'" + varName + "': member names cannot be the same as their enclosing type");
                this.CurrentType.DataSize += f.Return.DataSize;
            }
            else
            {
                f.Offset = this.CurrentMethod.DataSize;
                this.CurrentMethod.LocalVariables.Add(f);
                this.CurrentMethod.DataSize += f.Return.DataSize;
            }
            return f;
        }

        public FieldInfo SetVariable(string type, string varName)
        {
            Type Return = this.System.Assembly.GetType(type);
            if (Return == null) throw new Exception("UnExpectedType");
            return SetVariable(Return, varName);
        }

        public void SetInstruction (string fn, Tree left, Tree right)
        {
            var T = new Assembler(fn, this.System.Lexical) {LeftParam = left, RightParam = right};
            if ( this.CurrentMethod == null ) return;
            if ( fn == Const.label ) {
                this.LabelsInstruction.Add(left.Content, this.StreamWriter.Offset);
                return;
            }
            Instruct instruct = Instruct.Parse(T.ToString());
            if ( T.Fn == Const.call ) CallManager(T, ref instruct);
            if ( Assembler.IndexOf(Const.JMPS, instruct.Function) != -1 )
                if ( T.LeftParam.Kind == Kind.Label || T.LeftParam.Kind == Kind.Variable ) {
                    instruct.Desdestination.OperandType = OperandType.imm;
                    instruct.Desdestination.DataType = DataType.Word;
                    this.JumpInstruction.Add(this.StreamWriter.Offset, T.LeftParam.Content);
                }
            if ( instruct.Desdestination.Equals(instruct.Source) ) { }
            instruct.Push(this.StreamWriter);
        }
        public void SetInstruction(Assembler T)
        {
            Instruct instruct;
            if (this.CurrentMethod == null) return;
            if (T.Fn == Const.label)
            {
                this.LabelsInstruction.Add(T.LeftParam.Content, this.StreamWriter.Offset);
                return;
            }

            instruct = T.ToInstruct();
            var tinstruct = Instruct.Parse(T.ToString());
            if (T.Fn == Const.call) CallManager(T, ref instruct);
            if (Assembler.IndexOf(Const.JMPS, instruct.Function) != -1)
                if (T.LeftParam.Kind == Kind.Label || T.LeftParam.Kind == Kind.Variable)
                {
                    instruct.Desdestination.OperandType = OperandType.imm;
                    instruct.Desdestination.DataType = DataType.Word;
                    this.JumpInstruction.Add(this.StreamWriter.Offset, T.LeftParam.Content);
                }
            instruct.Push(this.StreamWriter);
        }

        
    }
}
