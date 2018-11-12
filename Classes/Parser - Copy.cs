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
        public KeyValuePair<K, V> get(K key)
        {
            for (int i = 0; i < Count; i++)
            {
                var d = this[i];
                if (d.Key.Equals(key)) return d;
            }
            return null;
        }
        public KeyValuePair<K, V> get(V value, bool isValue = true)
        {
            for (int i = 0; i < Count; i++)
            {
                var d = this[i];
                if (d.Value.Equals(value)) return d;
            }
            return null;
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
            this.Add(key, value);
        }
        public bool Contain(K key) { return this[key] != null; }
        public bool Contain(V value, bool isValue = true) { return this[value] != null; }
    }
    public class Descripter
    {
        public static readonly BiDictionary<string, string> COperators = new BiDictionary<string, string>
        {
            {"+",Const.add},{"-",Const.sub},{"*",Const.mul},{"/",Const.div},{"%",Const.rst},{"\\",Const.idiv},{"^",Const.pow},
            {"&",Const.and},{"==",Const.eq},{"#",Const.neq},{"<",Const.inf},{">",Const.sup},{"|",Const.or},
            {"<=",Const.leq},{">=",Const.gte},{"++",Const.inc}
        };
        //public static readonly List<string> Operators = new List<string> { "+-", "*/%\\", "^", "~!#<>|" };
        static Descripter()
        {
            var Operators = new List<List<KeyValuePair<string, string>>>()
            {
                new List<KeyValuePair<string,string>>(){COperators[0],COperators[1]},
                new List<KeyValuePair<string,string>>(){COperators[2],COperators[3],COperators[4],COperators[5]},
                new List<KeyValuePair<string,string>>(){COperators[6]},
                new List<KeyValuePair<string,string>>(){COperators[7],COperators[8],COperators[9],COperators[10],COperators[11],COperators[12]},
            };
        }
    }
   
    [Serializable] public partial class Parser
    {
        public Trace Trace = new Trace(Kind.Null);
        public const string Unair = "!+-";
                

        private const string Eps = " \0\n\t\r\v";
        public static readonly string[] FUnair = {Const.ne, Const.upl, Const.usu};

        public static string GetOperant(char @char, bool eq = false)
        {
            if (!eq)
            {
                if (@char == '<') return "leq";
                if (@char == '>') return "gth";
                if (@char == '=') return "eq";
            }
            else
            {

                if (@char == '<') return "leq";
                if (@char == '>') return "gte";
                if (@char == '=') return "eq";
            }
            return "(" + @char + ")";
        }

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
            bool Mark, vB = Mark = KeyWord(Ts, Resource.space);
            if ( vB ) vB = Heratachy(T);
            if ( vB ) vB = KeyWord(Ts, "{");
            var end = vB;
            while ( vB ) if ( !(vB = Class(T)) ) vB = Space(T);
            if ( end ) end = KeyWord(Ts, "}");
            return T.Set(end, Mark);
        }


        public bool Class (Tree parent)
        {
            Pile.Save(Kind.Class);
            var T = new Tree(ref Pile, parent, Kind.Class);
            var vB = KeyWord(Ts, "class");
            if ( vB ) vB = Variable(T);
            if ( vB ) vB = (!KeyWord(T, ":") || Heratachy(T)) & KeyWord(Ts, "{");
            var end = vB;
            while ( vB ) {
                vB = Function(T);
                if ( !vB ) vB = TypeAssigne(T) & KeyWord(Ts, ";");
            }
            if ( end ) end = KeyWord(Ts, "}");
            return T.Set(end);
        }

        public bool Function (Tree parent)
        {
            Pile.Save(Kind.Function);
            var T = new Tree(ref Pile, parent, Kind.Function);
            var vB = false;
            if ( KeyWord(Ts, "function") ) {
                vB = Heratachy(T);
                if ( vB ) Variable(T);
                if ( vB ) vB = DeclaredParams(T);
                if ( vB ) vB = Bloc(T) & ESpace;
            }
            return T.Set(vB);
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
            var isif = Ifs(T);
            if ( isif ) if ( KeyWord(Ts, "else") ) isif = Instruction(T);
            return T.Set(isif);
        }

        public bool For (Tree parent)
        {
            Pile.Save(Kind.For);
            var T = new Tree(ref Pile, parent, Kind.For);
            var vB = false;
            if ( KeyWord(Ts, "for") )
                if (KeyWord(Ts, "(") & Instruction(T) & Expression(T) & KeyWord(Ts, ";") & Instruction(T, false) & KeyWord(Ts, ")"))
                    if ( Instruction(T) ) vB = true;
            return T.Set(vB);
        }

        public bool While (Tree parent)
        {
            Pile.Save(Kind.While);
            var T = new Tree(ref Pile, parent, Kind.While);
            var vB = false;
            if ( KeyWord(Ts, "while") ) if ( KeyWord(_tts, "(") & Expression(T) & KeyWord(_tts, ")") ) if ( Instruction(T) ) vB = true;
            return T.Set(vB);
        }

        public bool Do (Tree parent)
        {
            Pile.Save(Kind.Do);
            var T = new Tree(ref Pile, parent, Kind.Do);
            var vB = false;
            if ( KeyWord(Ts, "do") )
                if ( Instruction(T) & KeyWord(Ts, "while") &
                     Parametre(T) ) vB = true;
            return T.Set(vB);
        }

        public bool Instruction (Tree parent, bool wC = true)
        {
            Pile.Save(Kind.Instruction);
            if ( Boucle(parent) ) return Pile.Leave(true);
            var vB = Caller(parent);
            if ( !vB ) vB = Goto(parent);
            if ( !vB ) {
                vB = Label(parent);
                if ( vB ) wC = false;
            }
            if ( !vB ) vB = Return(parent);
            if ( !vB ) vB = Assigne(parent);
            if ( !vB ) vB = Expression(parent);
            if ( wC & vB ) vB = KeyWord(Ts, ";");
            return Pile.Leave(vB);
        }

        public bool Assigne (Tree parent)
        {
            var vB = EqAssigne(parent);
            if ( !vB ) vB = TypeAssigne(parent);
            return vB;
        }

        public bool Parent (Tree parent)
        {
            Pile.Save(Kind.Parent);
            if ( !KeyWord(Ts, "(") ) return Pile.Leave(false);
            Expression(parent);
            return Pile.Leave(KeyWord(Ts, ")"));
        }

        public bool Caller (Tree parent, bool cA = true)
        {
            Pile.Save(Kind.Caller);
            var T = new Tree(ref Pile, parent, cA ? Kind.Caller : Kind.Array);
            var vB = Heratachy(T);
            if ( vB ) vB = Parametre(T, cA);
            return T.Set(vB);
        }

        public bool Heratachy (Tree parent)
        {
            Pile.Save(Kind.Hyratachy);
            var T = new Tree(ref Pile, parent, Kind.Hyratachy);
            bool vB1 = Variable(T), vB;
            if ( vB1 )
                do {
                    vB = KeyWord(Ts, ".");
                    if ( vB ) vB = Variable(T);
                } while ( vB );
            if ( T.Children.Count == 1 ) {
                T = T.Children[0];
                T.Parent = parent;
                parent.Children.Add(T);
                return Pile.Leave(vB1);
            }
            return T.Set(vB1);
        }

        public bool Goto (Tree parent)
        {
            Pile.Save(Kind.Goto);
            var T = new Tree(ref Pile, parent, Kind.Goto);
            return !KeyWord(T, "goto") ? Pile.Leave(false) : T.Set(Variable(T));
        }

        public bool Return (Tree parent)
        {
            Pile.Save(Kind.Return);
            var T = new Tree(ref Pile, parent, Kind.Return);
            GetWhiteSpace();
            return !KeyWord(T, "return") ? Pile.Leave(false) : T.Set((GetWhiteSpace() || true) && Expression(T));
        }

        public bool Label (Tree parent)
        {
            Pile.Save(Kind.Label);
            var T = new Tree(ref Pile, parent, Kind.Label);
            var vB = Variable(T);
            if ( vB ) vB = KeyWord(Ts, ":");
            return T.Set(vB);
        }

        private bool Ifs (Tree parent)
        {
            Pile.Save();
            var vB = KeyWord(Ts, "if");
            if ( vB ) vB = KeyWord(Ts, "(");
            if ( vB ) vB = Expression(parent);
            if ( vB ) vB = KeyWord(Ts, ")");
            if ( vB ) vB = Instruction(parent);
            return Pile.Leave(vB);
        }

        public bool Boucle (Tree parent)
        {
            Pile.Save();
            var vB = If(parent);
            if ( !vB ) vB = For(parent);
            if ( !vB ) vB = While(parent);
            if ( !vB ) vB = Do(parent);
            if ( !vB ) vB = Bloc(parent);
            return Pile.Leave(vB);
        }

        public bool Word (Tree parent)
        {
            Pile.Save(Kind.Word);
            var T = new Tree(ref Pile, parent, Kind.Hyratachy);
            bool vB1 = SWord(T), vB;
            if ( vB1 )
                do {
                    vB = KeyWord(Ts, ".");
                    if ( vB ) vB = SWord(T);
                } while ( vB );
            if ( T.Children.Count != 1 ) return T.Set(vB1);
            T = T.Children[0];
            T.Parent = parent;
            return T.Set(vB1);
        }

        private bool SWord(Tree parent)
        {
            Pile.Save();
            bool vB = Caller(parent);
            if (!vB) vB = Caller(parent, false);
            if (!vB) vB = Heratachy(parent);
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

        private Tree Reform (ref Tree tree)
        {
            if ( tree.Children.Count <= 4 ) return tree;
            var last = tree.Children.Count - 1;
            var lt = tree.Children[last];
            for (int i = tree.Children.Count - 1; i >= 2; i -= 2) {
                var t = new Tree(ref Pile, null, Kind.Parent);
                var d = tree.Children[i - 2];
                Add(t, d, tree.Children[i - 1], lt);
                t.Start = d.Start;
                t.End = lt.End;
                lt = t;
            }
            lt.Parent = tree.Parent;
            lt.Kind = tree.Kind;
            return lt;
        }

        private void Add (Tree parent, params Tree[] child)
        {
            foreach ( var tree in child ) {
                parent.Children.Add(tree);
                tree.Parent = parent;
            }
        }

        private Kind GetKind (int i)
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
        public bool Operator(Tree parent,string pre)
        {
            Pile.Save();
            var T = new Tree(ref Pile, parent, Kind.Operator);
            if (Pile.Open)
                //foreach (var pre in Operators)
                    foreach (var opr in pre)
                    {
                        if (opr == Pile.Current)
                        {
                            Pile.Next();
                            if (Pile.Current == '=') Pile.Next();
                            return T.Set();
                        }
                    }
            return Pile.Leave(false);
        }

        public bool Expression (Tree parent, int i = 0)
        {
            Pile.Save(Kind.Expression);
            var T = new Tree(ref Pile, parent, GetKind(i));
            bool vB;
            var vB1 = i < 4 ? Expression(T, ++i) : Word(T);
            if ( vB1 )
                do {
                    vB = Operator(T, Descripter. Operators[i - 1]); //Contain(Operators[i - 1].ToCharArray(), T);
                    if ( vB ) vB = Expression(T, i);
                } while ( vB );
            T = Reform(T);
            if ( T.Children.Count == 1 ) {
                T = T.Children[0];
                T.Parent = parent;
            }
            return T.Set(vB1);
        }

        public bool TypeAssigne (Tree parent)
        {
            Pile.Save(Kind.TypeAssigne);
            var T = new Tree(ref Pile, parent, Kind.TypeAssigne);
            var vB = false;
            if ( Heratachy(T) )
                if ( Variable(T) ) {
                    vB = true;
                    if ( KeyWord(Ts, "=") ) vB = Expression(T);
                }

            return T.Set(vB);
        }

        public bool EqAssigne (Tree parent)
        {
            Pile.Save(Kind.EqAssign);
            var T = new Tree(ref Pile, parent, Kind.EqAssign);
            var vB = Heratachy(T);
            if ( vB ) vB = KeyWord(Ts, "=");
            if ( vB ) vB = Expression(T);
            return T.Set(vB);
        }

        public bool DeclaredParam (Tree parent)
        {
            Pile.Save(Kind.DeclareParam);
            var T = new Tree(ref Pile, parent, Kind.DeclareParam);
            var vB = false;
            if ( Heratachy(T) ) if ( Variable(T) ) vB = true;
            return T.Set(vB);
        }

        public bool DeclaredParams (Tree parent)
        {
            Pile.Save(Kind.DeclareParams);
            var T = new Tree(ref Pile, parent, Kind.DeclareParams);
            if ( !KeyWord(Ts, "(") ) return Pile.Leave(false);
            var vB = DeclaredParam(T);
            while ( vB ) {
                vB = KeyWord(Ts, ",");
                if ( vB ) vB = DeclaredParam(T);
            }
            return T.Set(KeyWord(Ts, ")"));
        }

        public bool Parametre (Tree parent, bool pB = true)
        {
            Pile.Save(Kind.Param);
            string o = pB ? "(" : "[", c = pB ? ")" : "]";
            var T = new Tree(ref Pile, parent, Kind.Param);
            if ( !KeyWord(Ts, o) ) return Pile.Leave(false);
            var vB = Expression(T);
            while ( vB ) {
                vB = KeyWord(Ts, ",");
                vB &= Expression(T);
            }
            return T.Set(KeyWord(Ts, c));
        }

        public bool Contain (IList <char> list, Tree parent)
        {
            Pile.Save();
            var T = new Tree(ref Pile, parent, Kind.Operator);
            if ( Pile.Open )
                if ( list.Contains(Pile.Current) ) {
                    Pile.Next();
                    return T.Set();
                }
            return Pile.Leave(false);
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
                if (!Contain(Descripter.Operators[0], Pile.Current)) break;
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
            Pile.Save();
            //var un = Unaire(parent);
            var T = new Tree(ref Pile, parent, Kind.Numbre);
            var n1 = ESpace & Chiffre();
            if ( n1 ) if ( KeyWord(Ts, "e") || KeyWord(Ts, "E") ) n1 = Chiffre();
            return T.Set(n1) & ESpace;
        }

        public bool String (Tree parent)
        {
            Pile.Save();
            if ( Pile.Close ) return Pile.Leave(false);
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
            Pile.Save(Kind.Variable);
            var start = ESpace;
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

        public bool Constant (Tree parent)
        {
            Pile.Save();
            var vB = String(parent);
            if ( !vB ) vB = Numbre(parent);

            return Pile.Leave(vB);
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

        public bool Operator ()
        {
            Pile.Save();
            if ( !Pile.Open ) return Pile.Leave(false);
            var vB = Descripter.COperators.Contain(Pile.Current.ToString());
            if ( vB ) Pile.Next();
            return Pile.Leave(vB);
        }

        public static bool Contain (string chainChars, char charact)
        {
            for (var i = 0; i < chainChars.Length; i++) if ( chainChars[i] == charact ) return true;
            return false;
        }

        public bool Unaire (Tree parent)
        {
            Pile.Save();
            var T = new Tree(ref Pile, parent, Kind.Unair);
            var vB = false;
            if ( Pile.Open ) {
                if ( Contain(Unair, Pile.Current) ) {
                    vB = true;
                    Pile.Next();
                }
            }
            return T.Set(vB);
        }
    }
}