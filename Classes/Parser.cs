using System;
using System.Collections.Generic;
using Compiler.Help;

namespace Compiler.Classes
{
    public class KeyValuePair<K, V>
    {
        public K Key;
        public V Value;
        public void Add(K key, V value)
        {
            this.Key = key;
            this.Value = value;
        }
        public KeyValuePair(K key, V value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
    public class BiDictionary<K, V> : List<KeyValuePair<K, V>>
    {

        public int IndexOf(K key)
        {
            for (int i = 0; i < Count; i++)
            {
                var d = this[i];
                if (d.Key.Equals(key)) return i;
            }
            return -1;
        }
        public int IndexOf(V value,bool isValue=true)
        {
            for (int i = 0; i < Count; i++)
            {
                var d = this[i];
                if (d.Value.Equals(value)) return i;
            }
            return -1;
        }
        public KeyValuePair<K, V> get(K key)
        {
            var i = IndexOf(key);
            return i == -1 ? null : base[i];
        }
        public KeyValuePair<K, V> get(V value, bool isValue = true)
        {
            var i = IndexOf(value, true);
            return i == -1 ? null : base[i];
        }

        public K this[V val, bool isValue = true]
        {
            get
            {
                return get(val).Key;
            }
            set
            {
                get(val).Key = value;
            }
        }

        public V this[K key]
        {
            get
            {
                return get(key).Value;
            }
            set
            {
                get(key).Value = value;
            }
        }


        public void Add(K key, V value)
        {
            base.Add(new KeyValuePair<K, V>(key, value));
        }
        public bool Contain(K key) { return get(key) != null; }
        public bool Contain(V value, bool isValue = true) { return get(value) != null; }
    }

    public class Descripter
    {
        public static readonly BiDictionary<string, string> COperators = new BiDictionary<string, string>       {                            
                            {"<=",Const.leq},{">=",Const.gte},{"++",Const.inc},
                            {"!",Const.not},{"<<",Const.shl},{">>",Const.shr},
                            {"+",Const.add},{"-",Const.sub},{"*",Const.mul},{"/",Const.div},{"%",Const.rst},{"\\",Const.idiv},{"^",Const.pow},
                            {"&",Const.and},{"==",Const.eq},{"#",Const.neq},{"<",Const.inf},{">",Const.sup},{"|",Const.or},
            };
        //public static readonly List<string> Operators = new List<string> { "+-", "*/%\\", "^", "~!#<>|" };
        public readonly static BiDictionary<int, BiDictionary<string, string>> Operators;
        static Descripter()
        {
            Operators = new BiDictionary<int, BiDictionary<string, string>>()
            {
                {0,new BiDictionary<string,string> { COperators[0+6],COperators[1+6]}},
                {1,new BiDictionary<string,string>(){COperators[2+6],COperators[3+6],COperators[4+6],COperators[5+6]}},
                {2,new BiDictionary<string,string>(){COperators[6+6]}},
                {3,new BiDictionary<string,string>(){COperators[7+6],COperators[8+6],COperators[9+6],COperators[10+6],COperators[11+6],COperators[12+6]}}
            };
        }
    }
   
    [Serializable] public partial class Parser
    {
        public Trace Trace = new Trace(Kind.Null);
        public const string Unair = "!+-";
                

        private const string Eps = " \0\n\t\r\v";
        public static readonly string[] FUnair = {Const.ne, Const.upl, Const.usu};
        public static string GetUnair(char @char, bool eq = false)
        {            
            for (var i = 0; i < Unair.Length; i++) if (Unair[i] == @char) return FUnair[i];
            return "(" + @char + ")";
        }

        public Pile Pile;

        public readonly Trace Errors = new Trace(0, int.MaxValue, Kind.Space);

        private Tree _tts;

        private Tree Ts
        {
            get
            {
                _tts.Children.Clear();
                return _tts;
            }
            set { _tts = value; }
        }

        public bool Space (Tree parent)
        {
            Pile.Save(Kind.Space);
            var T = new Tree(ref Pile, parent, Kind.Space);
            bool Mark, end;
            bool vB = end = (Mark = KeyWord(Ts, Resource.space)) && Heritachy(T) && KeyWord(Ts, "{");
            while (vB && (Class(T) || Space(T))) ;
            if ( end ) end = KeyWord(Ts, "}");
            return T.Set(end, Mark);
        }


        public bool Class (Tree parent)
        {
            Pile.Save(Kind.Class);
            bool i = false;
            var vB = KeyWord(Ts, "class") || (i = KeyWord(Ts, "struct"));
            var T = new Tree(ref Pile, parent, i ? Kind.Struct : Kind.Class);
            if (vB) vB = Variable(T) && (!KeyWord(T, ":") || Heritachy(T)) & KeyWord(Ts, "{");
            while (vB && (Function(T) || TypeAssigne(T, true) || Constructor(T))) ;
            return T.Set(vB && KeyWord(Ts, "}"));
        }
        public bool Constructor(Tree parent)
        {
            Pile.Save(Kind.Constructor);
            var T = new Tree(ref Pile, parent, Kind.Constructor);
            return T.Set(KeyWord(Ts, "constructor") && Variable(T) && DeclaredParams(T) && (Bloc(T))) && ESpace;
        }

        public bool Function(Tree parent)
        {
            Pile.Save(Kind.Function);
            var T = new Tree(ref Pile, parent, Kind.Function);
            return T.Set(KeyWord(Ts, "function") && Heritachy(T) && Variable(T) && DeclaredParams(T) && (Bloc(T))) && ESpace;
        }

        public bool Bloc (Tree parent)
        {
            Pile.Save(Kind.Bloc);
            var T = new Tree(ref Pile, parent, Kind.Bloc);
            if ( ESpace & !KeyWord(Ts, "{") ) return Pile.Leave(false);
            while ( Instruction(T) ) { }
            return T.Set(KeyWord(Ts, "}"));
        }

        public bool If (Tree parent)
        {
            Pile.Save(Kind.If);
            var T = new Tree(ref Pile, parent, Kind.If);
            return T.Set(Ifs(T) && KeyWord(Ts, "else") && Instruction(T));
        }
        
        public bool For (Tree parent)
        {
            Pile.Save(Kind.For);
            var T = new Tree(ref Pile, parent, Kind.For);
            return T.Set(KeyWord(Ts, "for") && KeyWord(Ts, "(") && (Instruction(T, false) | true) && KeyWord(Ts, ";") && Expression(T) && KeyWord(Ts, ";") && (Instruction(T, false) | true) && KeyWord(Ts, ")") && Instruction(T));
        }

        public bool While (Tree parent)
        {
            Pile.Save(Kind.While);
            var T = new Tree(ref Pile, parent, Kind.While);
            return T.Set(KeyWord(Ts, "while") && KeyWord(_tts, "(") && Expression(T) && KeyWord(_tts, ")") && Instruction(T));
        }

        public bool Do (Tree parent)
        {
            Pile.Save(Kind.Do);
            var T = new Tree(ref Pile, parent, Kind.Do);
            return T.Set(KeyWord(Ts, "do") && Instruction(T) && KeyWord(Ts, "while") && KeyWord(Ts, "(") && Expression(T) && KeyWord(Ts, ")"));
        }

        public bool Instruction (Tree parent, bool endWithComa = true)
        {
            Pile.Save(Kind.Instruction);
            if ( Boucle(parent) ) return Pile.Leave(true);
            var vB = Caller(parent) || Goto(parent) || (Label(parent) ? ((endWithComa = false) | true) : false) || Return(parent) || Assigne(parent) || Expression(parent);
            return Pile.Leave(vB & endWithComa ? KeyWord(Ts, ";") : vB);
        }

        public bool Assigne (Tree parent)
        {
            return EqAssigne(parent) || TypeAssigne(parent);
        }

        public bool Parent (Tree parent)
        {
            Pile.Save(Kind.Parent);
            return Pile.Leave(KeyWord(Ts, ")") && Expression(parent) && KeyWord(Ts, ")"));
        }

        public bool Caller(Tree parent, bool cA = true)
        {
            return _Caller(parent, cA) || New(parent);
        }
        private bool _Caller (Tree parent, bool cA = true)
        {
            Pile.Save(Kind.Caller);
            var T = new Tree(ref Pile, parent, cA ? Kind.Caller : Kind.Array);
            return T.Set(Heritachy(T) && Parametre(T, cA));
        }

        private bool New(Tree parent)
        {
            Pile.Save(Kind.Caller);
            var T = new Tree(ref Pile, parent, Kind.Caller);
            return T.Set(KeyWord(Ts, "new") && Heritachy(T) && Parametre(T));
        }


        

        public bool Goto (Tree parent)
        {
            Pile.Save(Kind.Goto);
            var T = new Tree(ref Pile, parent, Kind.Goto);
            return !KeyWord(T, "goto") ? Pile.Leave(false) : T.Set(Variable(T));
        }

        public bool Return(Tree parent)
        {
            Pile.Save(Kind.Return);
            var T = new Tree(ref Pile, parent, Kind.Return);
            return T.Set(KeyWord(T, "return") && (Expression(T) | true));
        }

        public bool Label (Tree parent)
        {
            Pile.Save(Kind.Label);
            var T = new Tree(ref Pile, parent, Kind.Label);
            return T.Set(Variable(T) && KeyWord(Ts, ":"));
        }

        private bool Ifs (Tree parent)
        {
            Pile.Save();
            return Pile.Leave(KeyWord(Ts, "if") && KeyWord(Ts, "(") && Expression(parent) && KeyWord(Ts, ")") && Instruction(parent));
        }

        public bool Boucle (Tree parent)
        {
            Pile.Save();
            return Pile.Leave(If(parent) || For(parent) || While(parent) || Do(parent) || Bloc(parent));
        }

        public bool Heritachy(Tree parent)
        {
            Pile.Save(Kind.Hyratachy);
            var T = new Tree(ref Pile, parent, Kind.Hyratachy);
            bool vB1 = Variable(T);
            while (vB1 && KeyWord(Ts, ".") && Variable(T)) ;
            if (T.Children.Count == 1)
            {
                T = T.Children[0];
                T.Parent = parent;
                parent.Children.Add(T);
                return Pile.Leave(vB1);
            }
            return T.Set(vB1);
        }

        public bool ComplexHeritachy (Tree parent)
        {
            Pile.Save(Kind.Word);
            var T = new Tree(ref Pile, parent, Kind.Hyratachy);
            bool vB1 = SWord(T);
            while (vB1 && (KeyWord(Ts, ".") ? SWord(T) : false)) ;
            if (T.Children.Count == 1)
            {
                T = T.Children[0];
                T.Parent = parent;
                return T.Set(vB1);
            } return T.Set(vB1);
        }

        private bool SWord(Tree parent)
        {
            Pile.Save();
            bool vB = Caller(parent);
            if (!vB) vB = Caller(parent, true);
            if (!vB) vB = Caller(parent, false);
            if (!vB) vB = Heritachy(parent);
            if (!vB) vB = Numbre(parent);
            if (!vB) vB = String(parent);
            if (!vB) vB = Parent(parent);
            //if (!vB ) parent.Children.RemoveAt(parent.Children.Count - 1);
            return Pile.Leave(vB);
        }

        private Tree Reform (Tree tree)
        {
            if ( tree.Children.Count <= 4 ) return tree;
            var lt = tree.Children[0];
            var last = tree.Children.Count - 1;
            for (int i = tree.Children.Count - 1; i >= 2; i -= 2) {
                var t = new Tree(ref Pile, null, Kind.Parent);
                var d = tree.Children[last - i + 2];
                Add(t, lt, tree.Children[last - i + 1], d);
                t.Start = lt.Start;
                t.End = d.End;
                lt = t;
            }

            lt.Parent = tree.Parent;
            lt.Kind = tree.Kind;
            return lt;
        }
        private static void Add (Tree parent, params Tree[] child)
        {
            foreach ( var tree in child ) {
                parent.Children.Add(tree);
                tree.Parent = parent;
            }
        }

        private static Kind GetKind (int i)
        {
            switch (i - 1)
            {
                case 0:
                    return Kind.Term;
                case 1:
                    return Kind.Facteur;
                case 2:
                    return Kind.Power;
                case 3:
                    return Kind.Logic;
                case 4:
                    return Kind.Word;
            }
            return Kind.Expression;
        }

        public bool Expression (Tree parent, int i = 0)
        {
            Pile.Save(Kind.Expression);
            var T = new Tree(ref Pile, parent, GetKind(i));
            bool vB;
            var vB1 = i < 4 ? Expression(T, ++i) : ComplexHeritachy(T);
            if ( vB1 )
                do 
                {
                    vB = Operator(T, Descripter.Operators[i - 1]); //Contain(Operators[i - 1].ToCharArray(), T);
                    if ( vB ) vB = Expression(T, i);
                } while ( vB );
            T = Reform(T);
            if ( T.Children.Count == 1 ) {
                T = T.Children[0];
                T.Parent = parent;
            }
            return T.Set(vB1);
        }

        private bool Operator(Tree parent, List<KeyValuePair<string, string>> list)
        {
            Pile.Save();
            var T = new Tree(ref Pile, parent, Kind.Operator);
            if (Pile.Open)
                foreach (var opr in list)
                    if (KeyWord(Ts, opr.Key))
                        return T.Set();
            return Pile.Leave(false);
        }

        public bool TypeAssigne (Tree parent,bool endWithComa=false)
        {
            Pile.Save(Kind.TypeAssigne);
            var T = new Tree(ref Pile, parent, Kind.TypeAssigne);
            return T.Set(Heritachy(T) && Variable(T) && (KeyWord(Ts, "=") ? Expression(T) : true) && (endWithComa ? KeyWord(Ts, ";") : true));
        }

        public bool EqAssigne(Tree parent)
        {
            Pile.Save(Kind.EqAssign);
            var T = new Tree(ref Pile, parent, Kind.EqAssign);
            return T.Set(Heritachy(T) && KeyWord(Ts, "=") && Expression(T));
        }

        public bool DeclaredParam (Tree parent)
        {
            Pile.Save(Kind.DeclareParam);
            var T = new Tree(ref Pile, parent, Kind.DeclareParam);
            return T.Set(Heritachy(T) && Variable(T));
        }

        public bool DeclaredParams (Tree parent,bool pB=true)
        {
            Pile.Save(Kind.DeclareParams);
            var T = new Tree(ref Pile, parent, Kind.DeclareParams);
            bool end = false;
            var vB = (end = KeyWord(Ts, pB ? "(" : "[")) && DeclaredParam(T);
            while (vB && KeyWord(Ts, ",") && DeclaredParam(T)) ;
            return T.Set(end && KeyWord(Ts, pB ? ")" : "]"));
        }

        public bool Parametre(Tree parent, bool pB = true)
        {
            Pile.Save(Kind.Param);
            var T = new Tree(ref Pile, parent, Kind.Param);
            var end = false;
            var vB = (end = KeyWord(Ts, pB ? "(" : "[")) && Expression(T);
            while (vB && KeyWord(Ts, ",") && Expression(T)) ;
            return T.Set(end && KeyWord(Ts, pB ? ")" : "]"));
        }

        public bool Contain(IList<char> list, Tree parent)
        {
            Pile.Save();
            var T = new Tree(ref Pile, parent, Kind.Operator);
            return Pile.Open && list.Contains(Pile.Current) ? Pile.Next() | T.Set() : Pile.Leave(false);
        }


    }

    public partial class Parser
    {
        public Compile.System System;

        public Parser (Pile pile)
        {
            Pile = pile;
            Ts = new Tree(ref Pile, null, Kind.Null);
            _tts.Children.Add(_tts);
        }

        public void GetPMun ()
        {
            Pile.Save();
            var vB = false;
            while ( Pile.Open ) {
                if (!Descripter.Operators[0].Contain(Pile.Current.ToString())) break;
                vB = true;
                Pile.Next();
            }
            Pile.Leave(vB);
        }

        public bool ESpace
        {
            get
            {
                while ( Pile.Open ) {
                    if ( !Contain(Eps, Pile.Current) ) break;
                    Pile.Next();
                }
                return true;
            }
        }

        public bool GetWhiteSpace ()
        {
            Pile.Save();
            var start = true;
            if ( Pile.Close ) return Pile.Leave(true);

            do {
                if ( !Contain(Eps, Pile.Current) ) return Pile.Leave(!start);
                start = false;
            } while ( Pile.Next() );
            return Pile.Leave(true);
        }

        public bool Chiffre (bool Int = false)
        {
            Pile.Save();
            GetPMun();
            bool dot = false, start = true;
            while ( Pile.Open ) {
                if ( !char.IsDigit(Pile.Current) )
                    if ( Pile.Current == '.' & !Int )
                        if ( dot ) return Pile.Leave(false);
                        else dot = true;
                    else break;
                start = false;
                Pile.Next();
            }
            return Pile.Leave(!start);
        }

        public bool Numbre (Tree parent)
        {
            var i = ESpace;
            Pile.Save();
            var T = new Tree(ref Pile, parent, Kind.Numbre);
            var n1 = Chiffre();
            if ( n1 ) if ( KeyWord(Ts, "e") || KeyWord(Ts, "E") ) n1 = Chiffre();
            return T.Set(n1) && ESpace;
        }

        public bool String (Tree parent)
        {
            var i = ESpace;
            Pile.Save();
            if (Pile.Close) return Pile.Leave(false);
            var sC = Pile.Current;
            var T = new Tree(ref Pile, parent, Kind.String);
            if ( KeyWord(T, "\"") | KeyWord(T, "\'") ) {
                while ( Pile.Open ) {
                    if ( Pile.Current == sC ) {
                        Pile.Next();
                        return T.Set();
                    }
                    Pile.Next();
                }
            }
            return Pile.Leave(false);
        }

        public bool Variable (Tree parent)
        {
            var start = ESpace;
            Pile.Save(Kind.Variable);            
            var T = new Tree(ref Pile, parent, Kind.Variable);
            while ( Pile.Open ) {
                if ( char.IsDigit(Pile.Current) ) { if ( start ) return Pile.Leave(false); }
                else if ( char.IsLetter(Pile.Current) ) { }
                else break;
                start = false;
                Pile.Next();
            }
            return T.Set(!start) & ESpace;
        }
        public bool KeyWord (Tree parent, string keyWord)
        {
            Pile.Save(Kind.KeyWord);
            var T = new Tree(ref Pile, parent, Kind.KeyWord);
            foreach ( var item in keyWord ) {
                if ( Pile.Open )
                    if ( item == Pile.Current ) {
                        Pile.Next();
                        continue;
                    }

                return Pile.Leave(false);
            }
            return T.Set() & ESpace;
        }

        public static bool Contain (string chainChars, char charact)
        {
            for (var i = 0; i < chainChars.Length; i++) if ( chainChars[i] == charact ) return true;
            return false;
        }
        public bool Parse(Tree trees, Kind kind)
        {
            switch (kind)
            {
                case Kind.Numbre:
                    return Numbre(trees);
                case Kind.Variable:
                    return Variable(trees);
                case Kind.String:
                    return String(trees);
                case Kind.Expression:
                    return Expression(trees);
                case Kind.Return:
                    return Return(trees);
                case Kind.Caller:
                    return Caller(trees);
                case Kind.Assigne:
                    return Assigne(trees);
                case Kind.Hyratachy:
                    return Heritachy(trees);
                case Kind.For:
                    return For(trees);
                case Kind.If:
                    return Ifs(trees);
                case Kind.ElseIf:
                    return If(trees);
                case Kind.While:
                    return While(trees);
                case Kind.Do:
                    return Do(trees);
                case Kind.Bloc:
                    return Bloc(trees);
                case Kind.Instruction:
                    return Instruction(trees);
                case Kind.Parent:
                    return Parent(trees);
                case Kind.Ifs:
                    return If(trees);
                case Kind.Param:
                    return Parametre(trees);
                case Kind.TypeAssigne:
                    return TypeAssigne(trees);
                case Kind.EqAssign:
                    return EqAssigne(trees);
                case Kind.Space:
                    return Space(trees);
                case Kind.Class:
                    return Class(trees);
                case Kind.Const:
                    return Numbre(trees) || String(trees);
                case Kind.DeclareParams:
                    return DeclaredParams(trees);
                case Kind.Function:
                    return Function(trees);
                case Kind.DeclareParam:
                    return DeclaredParam(trees);                
                case Kind.Word:
                    return SWord(trees);
                case Kind.Array:
                    return Caller(trees, false);
                case Kind.Goto:
                    return Goto(trees);
                case Kind.Label:
                    return Label(trees);                    
                case Kind.Constructor:
                    return Constructor(trees);
                case Kind.New:
                    return New(trees);
                case Kind.Struct:
                    return Class(trees);
                default:
                    break;
            }
            return false;
        }
    }
}