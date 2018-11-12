using System;
using Compiler.Classes.Assembly;

namespace Compiler.Classes.Compile
{
    using Type = Compiler.Classes.Assembly.Type;

    public partial class Compile
    {
        private Tree CompileHyratachy(Tree tree, string name)
        {
            var f = Compiler(tree.Children[0]);
            FieldInfo fi; bool b;
            
            if (!System.Lexical.GetVariable(f.Content, out fi, out b))
                throw new Exception("Variable not Found");
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
            else throw new Exception("Membre not Found");
            if (name != null)
            {
                if (!System.Lexical.GetVariable(name, out fi, out b))
                    throw new Exception("Membre not Found");
                if (fi.Return.FullName != lc.Return.FullName)
                    throw new Exception("Uncompatible Types");
            }
            tree.Type = lc.Return;
            return tree;
        }
    }
}
