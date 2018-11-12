using Compiler.Classes.Assembly;

namespace Compiler.Classes.Compile
{
    using VM.Component;
    using Assembly = Compiler.Classes.Assembly.Assembly;

    public class System
    {
        public readonly Compile Compile;
        public readonly Symentic Symentic;
        public readonly Lexical Lexical;
        public readonly Parser Parser;
        public readonly TypeCalc TypeCalc;
        public readonly Assembly Assembly;
        public readonly Component MRT;
        public System(Compile compile, Symentic symentic, Lexical lexical, Parser parser, TypeCalc typeCalc,Assembly assembly,Component mrt)
        {
            Compile = compile;
            Symentic = symentic;
            Lexical = lexical;
            Parser = parser;
            this.TypeCalc = typeCalc;
            compile.System = this;
            symentic.System = this;
            lexical.System = this;
            parser.System = this;
            typeCalc.System = this;
            this.Assembly = assembly;
        }
    }
}