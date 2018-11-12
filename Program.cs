using System;
using System.Security;
using System.Text;
using Compiler.Classes;
using Compiler.Classes.Compile;
using VM.Global;
using VM.Parser;
using Compiler.Classes.Assembly;
using System.Linq;
using Type = Compiler.Classes.Assembly.Type;

[module: UnverifiableCodeAttribute]
namespace Compiler
{
    using VM.Component;
using System.Collections.Generic;
    internal static class Program
    {
        public static IEnumerable<int> yld(int j)
        {
            for (int i = 0; i < j; i++)
                yield return i;
            if (j > 0)
                foreach (var item in yld(j - 1))
                    yield return item;
        }
        private static void Main ()
        {
            foreach (var i in yld(3))
            {
                Console.Write(i);               
            }
            Expl1();
        }
        private static void Expl1()
        {
            var mrt = new Component(0x6400000);
            db:
            {
                var var = @"space ac{
                        class AV{
                                System.int g;
                                constructor AV(){g=4;int y=M(2);}
                                function int M(int a){int l; if(a>0) l=a+M(a-1);else l= a; return l;}
                            }
                        }";
                var symentic = new Symentic();
                var typeCalc = new TypeCalc();
                var d = new Assembly("MV");
                var lexical = new Lexical(d,mrt.Cache.Cache);
                var builder = new Pile(var);
                var compile = new Compile(var);
                var parser = new Parser(builder);
                var system = new Classes.Compile.System(compile, symentic, lexical, parser, typeCalc, d, mrt);

                system.Lexical.StreamWriter.Reserve(8 * 6);
                if (!system.Compile.Excecute())
                    goto db;
                
                var e_=25635;
                mrt.Process.Execute(new InitialProcessData(6, e_, e_ * 3, e_ * 2, e_ * 4, 0));
                system.Assembly["ac"].Types.Clear();
            }
        }
        public static void Extartct(Assembly d)
        {
            string s = "";
            try
            {
                foreach (var type in d["ac"].Types)
                    foreach (var method in type.Methods) { ExtractMethode(method.StreamReader.Stream, method, ref s); }
            }
            catch { }
        }
        private static void ExtractMethode(StreamWriter c, MethodInfo method, ref string s)
        {
            var ee = new StreamReader(c);
            var a = new StringBuilder(method.MethodSize);
            ee.Seek(method.Offset);
            a.Append(s + "\r\n----------------" + method + "-----------------");
            var end = method.MethodSize + method.Offset;
            do
            {
                var s1 = "\r\n" + ee.Offset.ToString("D4") + " :";
                Instruct g = Instruct.Pop(ee);
                a.Append(s1 + g);
            } while (ee.Offset < end);
            a.Append("\r\n----------------" + "************************" + "-----------------");
            s = a.ToString();
        }
    }
}