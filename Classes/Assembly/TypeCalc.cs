namespace Compiler.Classes.Assembly
{
    using System;
    using global::System.Collections.Generic;
    using global::System.Globalization;
    using Compiler.Classes.Compile;
    using Compiler.Help;

    public class TypeCalc
    {
        public System System;
        
        public bool CalcTypes(IList<Tree> trees)
        {
            for (int i = 0; i < trees.Count; i++) { if (!CalcTypes(trees[i])) return false; }
            return true;
        }

        public bool CalcHeratachyType(Tree tree)
        {
            Tree f = tree.Children[0];
            FieldInfo fi; bool b;
            if (!this.System.Lexical.GetVariable(f.Content, out fi, out b)) return SetCompiled(tree,  false);

            MembreInfo lc = fi;
            var d = tree.Children.Count - 1;
            for (int i = 1; i < d; i++)
            {
                FieldInfo c = lc.Return.GetField(tree.Children[i].Content);
                if (c == null) throw new Exception("Membre not Found");
                tree.Children[i].Type = c.Return;
                lc = c;
            }
            lc = lc.Return.GetField(tree.Children[d].Content) ?? (MembreInfo)lc.Return.GetMethod(tree.Children[d].Content);
            if (lc != null) tree.Children[d].Type = lc.Return;
            else SetCompiled(tree,  false);
            tree.Type = lc.Return;
            return  SetCompiled(tree, true);
        }

        public bool CalcTypes(Tree tree)
        {
            switch (tree.Kind)
            {
                case Kind.Variable:
                    FieldInfo fi;
                    List<MethodInfo> mi;
                    bool b;
                    
                    if (!this.System.Lexical.GetVariable(tree.Content.Trim(), out fi, out b))
                        if ( (mi = this.System.Lexical.GetFunctions(tree)).Count == 0 ) return SetCompiled(tree, false);
                        else {
                            tree.Type = mi[0].Return;
                            tree.IsVariabe_Method = false;
                            return SetCompiled(tree, true);
                        }
                    tree.Type = fi.Return;
                    tree.IsVariabe_Method = true;
                    tree.Membre = fi;
                    return  SetCompiled(tree, true);
                case Kind.Hyratachy:
                    return CalcHeratachyType(tree);
                case Kind.Term:
                case Kind.Facteur:
                case Kind.Unair:
                case Kind.Expression:
                case Kind. Parent:
                    return CalcExpressionType(tree);
                case Kind.Caller:
                    return CalcCallerType(tree);
                case Kind.EqAssign:
                    return CalcAssignType(tree);
                case Kind.TypeAssigne:
                    return CalcDeclAssigneType(tree);
                case Kind.Assigne:
                    if ( tree.Count == 2 )
                        return CalcAssignType(tree);
                    return CalcDeclAssigneType(tree);
                case Kind.Numbre:
                    return CalcNumbreType(tree);
                case Kind.String:
                    tree.Type = System.Lexical.System.Assembly.GetType("string");
                    return true;
                case Kind.Array:
                    return CalcCallerType(tree);
                case Kind.Return:
                    return CalcReturnType(tree);
                case Kind.Logic:
                    if(CalcTypes(tree[0])) 
                        if ( CalcTypes(tree[2]) )
                        { tree.Type = this.System.Assembly.GetType("System.bool"); tree.Compiled = true; return true; }
                    return false;
            }
            return true;
        }

        public static bool SetCompiled(Tree tree, bool value)
        {
            tree.Compiled = value;
            return value;
        }

        private bool CalcReturnType (Tree tree)
        {
            tree[0].Kind = Kind.Variable;
            var c = CalcTypes(tree[0]);
            var z = CalcTypes(tree[1]);
            if(c && z)
            return SetCompiled(tree, tree[0].Type == tree[1].Type);
            return SetCompiled(tree, false);            
        }

        private bool CalcNumbreType (Tree tree)
        {
            double z;
            if ( !double.TryParse(tree.Content, NumberStyles.Any, CultureInfo.InvariantCulture, out z) ) return SetCompiled(tree, false);
            z = z > 0 ? z : -z;
            var e = z%1;
            tree.Type = Math.Abs(e) < 1e-9 ? System.Lexical.System.Assembly.GetType(z > int.MaxValue ? "long" : "int") : System.Lexical.System.Assembly.GetType(z > float.MaxValue ? "double" : "float");

            return SetCompiled(tree, true);
        }

        private bool CalcAssignType (Tree tree)
        {
            if (!CalcTypes(tree[1])) return SetCompiled(tree,  false);
            tree[0].Type = tree[1].Type;
            tree.Type = tree[0].Type;
            return  SetCompiled(tree, true);
        }

        private bool CalcDeclAssigneType (Tree tree)
        {
            tree[1].Type = System.Lexical.System.Assembly.GetType(tree[0].Content);
            tree.Type = tree[1].Type;
            if ( tree[1].Type == null ) return SetCompiled(tree, false);
            if ( tree.Count > 2 ) {
                if ( !CalcTypes(tree[2]) ) return SetCompiled(tree, false);

                return SetCompiled(tree, tree[1].Type.FullName == tree[2].Type.FullName);
            }
            return SetCompiled(tree, true);
        }

        private bool CalcCallerType (Tree tree)
        {
            if ( !CalcTypes(tree[0]) || !tree[0].IsVariabe_Method.Equals(false) ) return SetCompiled(tree, false);
            var tree1 = tree[1];
            int i = 0;
            for (; i < tree1.Count; i++) { if (!CalcTypes(tree1[i])) return SetCompiled(tree, false); }
            tree.Type = tree[0].Type;
            tree.Compiled = true;
            return SetCompiled(tree, true);
        }

        private bool CalcExpressionType (Tree tree)
        {
            if (tree[0].Kind == Kind.Unair || tree.Count == 2) { return SetCompiled(tree,  CalcOperatorType(tree[0], null, tree[1])); }
            return SetCompiled(tree, CalcOperatorType(tree[1], tree[0], tree[2]));
        }

        private bool CalcOperatorType (Tree @operator, Tree left, Tree right)
        {
            bool j = true;
            var Eleft = left != null;
            var Eright = right != null;
            if(!Eleft & !Eright) return  false;

            if ( Eleft ) j&=CalcTypes(left);
            if (Eright & j) j &= CalcTypes(right);
            if(!j) return false;

            var e = Eleft ? left.Type.GetMethods(@operator.Content) : null;
            var v = Eright ? right.Type.GetMethods(@operator.Content) : null;
            var d = Eleft ? e : v;
            if ( Eleft && Eright ) {
                if(left.Type!=right.Type) e.AddRange(v);
                return CalcBOperatorType(e, left, right);
            }
            return CalcUOperatorType(d, left ?? right);
        }

        private static bool CalcBOperatorType(IList<MethodInfo> e, Tree left, Tree right)
        {
            for (int i = 0; i < e.Count; i++)
                if ( e[i].Params.Count == 3 ) {
                    if ( left.Type.FullName != e[i].Params[0].FullName ) continue;
                    if ( right.Type.FullName != e[i].Params[1].FullName ) continue;
                    left.Parent.Type = e[i].Return;

                    return true;
                }
            return false;
        }

        private static bool CalcUOperatorType(IList<MethodInfo> e, Tree right)
        {
            for (int i = 0; i < e.Count; i++)
                if (e[i].Params.Count == 3)
                {
                    if (right.Type.FullName != e[i].Params[1].FullName) continue;
                    right.Parent.Type = e[i].Return;
                    return true;
                }
            return false;
        }
    }
}