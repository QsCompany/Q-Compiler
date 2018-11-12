//أسواق الذهب

using System;
using System.Collections.Generic;
using Compiler.Classes.Assembly;
using Compiler.Help;

namespace Compiler.Classes.Compile
{
    using Type = Compiler.Classes.Assembly.Type;
    using VM.Parser;
    using VM.Component;
   

    [Serializable]
    public partial class Compile
    {
        public VariableGenerator Labels = new VariableGenerator("<lab_", ">");
        public Tree Root;
        public Pile Builder;
        private Parser parser;
        public Compile(Tree t)
        {
            Root = t;
            Compiler(Root);
        }
        public void Update(string s,Kind traitAs =Kind.Space){

            Builder = new Pile(s);
            parser = new Parser(Builder);
            Root = new Tree(Builder, null, traitAs);

        }
        public Compile(string s,Kind traitAs =Kind.Space)
        {
            Update(s, traitAs);
        }

        public bool Excecute()
        {
            var e = parser.Parse(Root, Root.Kind);
            if (e)
            {
                if (Root.Kind == Kind.Space)
                {
                    ProtoSpace(Root.Children[0].Children);
                    Compiler(Root.Children[0]);
                    foreach (var item in System.Lexical.CurrentAssembly.Spaces)
                        System.Lexical.BeginRefCall(item);
                }
                return true;
            }
            return false;
        }
        public static Tree Compiler(System sys,string s, Kind traitAs)
        {
            var e = new Compile(s, traitAs) { System = sys };
            if (e.Excecute()) return e.Root.Children[0];
            return null;
           
        }

        internal void Insert(string fn, string name, Tree p1 = null, Tree p2 = null)
        {
            if (fn == Const.nop || fn == Const.rop) return;
            var T = new Assembler(name, System.Lexical);
            T.Set(fn);
            T.LeftParam = p1;
            T.RightParam = p2;
            System.Lexical.SetInstruction(T);
        }

        internal Tree Compiler(Tree tree, string name = null)
        {
            if (tree == null || tree.Children.Count == 0) return tree;
            if (tree.Compiled.Equals(false))
                System.TypeCalc.CalcTypes(tree);
            else if (tree.Compiled.Equals(null)) throw new Exception("Uncompatible types");
            else if (tree.Compiled.Equals(null)) { }
            switch (tree.Kind)
            {
                case Kind.Numbre:
                    return tree;
                case Kind.Variable:
                    FieldInfo fl; bool b;
                    if (!System.Lexical.GetVariable(tree.Content, out fl, out b))
                        throw new Exception("Variable not Existe");
                    tree.Type = fl.Return;
                    return tree;
                case Kind.Word:
                case Kind.Term:
                case Kind.Facteur:
                case Kind.Parent:
                case Kind.Expression:
                case Kind.Unair:
                case Kind.Logic:
                    return CompileExpressoin(ref tree, name);
                case Kind.Hyratachy:
                    return CompileHyratachy(tree, name);
                case Kind.Bloc:
                    Bloc(tree.Children);
                    return tree;

                case Kind.New:
                    return New(tree.Children, name: name);
                    
                case Kind.Caller:
                    return Caller(tree.Children, name: name);
                case Kind.Array:
                    return Caller(tree.Children, Kind.Array, name);
                case Kind.Do:
                    Do(tree.Children);
                    return tree;
                case Kind.While:
                    While(tree.Children);
                    return tree;
                case Kind.For:
                    For(tree.Children);
                    return tree;

                case Kind.If:
                    If(tree.Children);
                    return tree;
                case Kind.EqAssign:
                    return EqAssigne(tree.Children);
                case Kind.TypeAssigne:
                    TypeAssigne(tree.Children);
                    return tree;
                case Kind.Goto:
                    Goto(tree.Children);
                    return tree;
                case Kind.Constructor:
                    Constructor(tree.Children);
                    return tree;
                case Kind.Function:
                    Function(tree.Children);
                    return tree;
                case Kind.Class:
                    Class(tree.Children);
                    return tree;
                case Kind.Space:
                    NameSpace(tree.Children);
                    return tree;
                case Kind.Return:
                    Insert(Const.push, null, Compiler(tree.Children[1]));
                    Insert(Const.mov, null, new Tree("eax", Kind.Register), new Tree("1", Kind.Numbre));
                    Insert(Const.jmp, Const.returnLabel, new Tree(Const.returnLabel, Kind.Label));
                    return tree;
                case Kind.Label:
                    Insert(Const.label, tree.Children[0].Content);
                    return tree;
                case Kind.Param:
                    return Compiler(tree.Children[0]);
            }
            throw new Exception("");
        }

        internal void NameSpace(IList<Tree> trees)
        {
            System.Lexical.OpenNameSpace(trees[0].Content);
            for (var i = 1; i < trees.Count; i++)
                if (trees[i].Kind == Kind.Class)
                    Class(trees[i].Children);
                else NameSpace(trees[i].Children);
            System.Lexical.CloseNameSpace();
        }

        internal void Class(IList<Tree> trees)
        {
            string Base = "object";
            if (trees[1].Content == ":") Base = trees[2].Content;
            System.Lexical.OpenClass(Base, trees[0].Content);
            //foreach (var tree in trees)
            //    if (tree.Kind == Kind.TypeAssigne)
            //        TypeAssigne(tree.Children);
            //    else if (tree.Kind == Kind.Function)
            //        ProtoMethod(tree.Children);
            foreach (var tree in trees)
                if (tree.Kind == Kind.Function)
                    Function(tree.Children);
                else if (tree.Kind == Kind.Constructor)
                    Constructor(tree.Children);
            System.Lexical.CloseClass();
        }

        internal void Function(IList<Tree> trees)
        {
            Labels.Reset();
            System.Lexical.OpenMethod(trees[1].Content, trees[0].Content, trees[2].Children);
            VM.Parser.Instruct.Parse(Const.nop).Push(System.Lexical.StreamWriter);
            this.System.Lexical.EPSInc = System.Lexical.StreamWriter.Offset;
            Insert(Const.add, null, new Tree("esp", Kind.Register), new Tree(System.Lexical.CurrentMethod.DataSize.ToString(), Kind.Numbre));
            for (var j = trees[2].Children.Count - 1; j >= 0; j--)
                Insert(Const.pop, Const.empty, trees[2].Children[j].Children[1]);
            Compiler(trees[3]);
            Insert(Const.label, Const.returnLabel, new Tree(Const.returnLabel, Kind.Label));
            Insert(Const.sub, null, new Tree("esp", Kind.Register), new Tree(System.Lexical.CurrentMethod.DataSize.ToString(), Kind.Numbre));
            VM.Parser.Instruct.Parse(Const.ret).Push(System.Lexical.StreamWriter);
            VM.Parser.Instruct.Parse(Const.rop).Push(System.Lexical.StreamWriter);
            System.Lexical.CloseFunction();
        }
        internal void Constructor(IList<Tree> trees)
        {
            Labels.Reset();
            System.Lexical.OpenMethod(trees[0].Content, trees[0].Content, trees[1].Children);
            VM.Parser.Instruct.Parse(Const.nop).Push(System.Lexical.StreamWriter);
            this.System.Lexical.EPSInc = System.Lexical.StreamWriter.Offset;
            Insert(Const.add, null, new Tree("esp", Kind.Register), new Tree(System.Lexical.CurrentMethod.DataSize.ToString(), Kind.Numbre));
            //Inset(Const.add, new Operand(VM.Bases.OperandType.Reg, "esp"), new Operand(Kind.Numbre, System.Lexical.CurrentMethod.DataSize));
            for (var j = trees[1].Children.Count - 1; j >= 0; j--)
                Insert(Const.pop, Const.empty, trees[1].Children[j].Children[1]);
            Compiler(trees[2]);
            Insert(Const.label, Const.returnLabel, new Tree(Const.returnLabel, Kind.Label));
            Insert(Const.sub, null, new Tree("esp", Kind.Register), new Tree(System.Lexical.CurrentMethod.DataSize.ToString(), Kind.Numbre));

            var a = Compile.Compiler(System, "System.new", Kind.Hyratachy); a.Type = System.Assembly.GetType("System.int");
            //Caller(new List<Tree>() { a, new Tree("()", Kind.Param) }, Kind.Caller);
            
            VM.Parser.Instruct.Parse(Const.ret).Push(System.Lexical.StreamWriter);
            VM.Parser.Instruct.Parse(Const.rop).Push(System.Lexical.StreamWriter);
            System.Lexical.CloseFunction();
            
        }

    }
    public partial class Compile
    {
        private void DefDataSpace(IList<Tree> trees)
        {
            System.Lexical.OpenNameSpace(trees[0].Content);
            for (var i = 1; i < trees.Count; i++)
                if (trees[i].Kind == Kind.Class) DefDataClass(trees[i].Children);
                else DefDataSpace(trees[i].Children);
            System.Lexical.CloseNameSpace();
        }

        private void DefDataClass(IList<Tree> trees)
        {
            string Base = "object";
            if (trees[1].Content == ":") Base = trees[2].Content;
            System.Lexical.OpenClass(Base, trees[0].Content);
            foreach (var tree in trees)
                if (tree.Kind == Kind.TypeAssigne) TypeAssigne(tree.Children);
            System.Lexical.CloseClass();
        }

        //private void CalcDataClass(IList<Tree> trees)
        //{
        //    string Base = "object";
        //    if (trees[1].Content == ":") Base = trees[2].Content;
        //    System.Lexical.OpenClass(Base, trees[0].Content);
        //    foreach (var tree in trees)
        //        if (tree.Kind == Kind.TypeAssigne) TypeAssigne(tree.Children);
        //    System.Lexical.CloseClass();
        //}

        internal void ProtoSpace(IList<Tree> trees)
        {
            System.Lexical.OpenNameSpace(trees[0].Content);
            for (var i = 1; i < trees.Count; i++)
                if (trees[i].Kind == Kind.Class) ProtoClass(trees[i].Children);
                else ProtoSpace(trees[i].Children);
            System.Lexical.CloseNameSpace();
        }

        internal void ProtoClass(IList<Tree> trees)
        {
            string Base = "System.object";
            if (trees[1].Content == ":") Base = trees[2].Content;
            System.Lexical.OpenClass(Base, trees[0].Content);
            foreach (var tree in trees)
                if (tree.Kind == Kind.TypeAssigne) TypeAssigne(tree.Children);
                else if (tree.Kind == Kind.Function) ProtoMethod(tree.Children);
                else if (tree.Kind == Kind.Constructor) ProtoConstructor(tree.Children);
            System.Lexical.CloseClass();
        }

        internal void ProtoMethod(IList<Tree> trees)
        {
            System.Lexical.OpenMethod(trees[1].Content, trees[0].Content, trees[2].Children);
            System.Lexical.CloseFunction();
        }
        internal void ProtoConstructor(IList<Tree> trees)
        {
            System.Lexical.OpenMethod(trees[0].Content, trees[0].Content, trees[1].Children);
            System.Lexical.CloseFunction();
        }
    }
}