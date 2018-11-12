using System;
using System.IO;
using System.Text;
using Compiler.Help;

namespace Compiler.Classes
{
    [Serializable]
    public static class Debugger
    {        
        public static bool IsExpr(Tree T)
        {
            return T.Kind != Kind.Numbre && T.Kind != Kind.String && T.Kind != Kind.Const && T.Kind != Kind.Variable;
        }

        public static int GetErro(Trace trace, ref int end)
        {
            var e = trace.Start;
            var s = trace.End;
            foreach (var tr in trace)
            {
                var erro = GetErro(tr, ref end);
                e = e > erro ? e : erro;
                end = s > end ? s : end;
            }
            return e;
        }
    }
}