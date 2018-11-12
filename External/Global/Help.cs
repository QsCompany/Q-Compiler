using System;
using System.Linq;
using System.Reflection;
using VM.Bases;
using VM.Component;

namespace VM.Global
{
    public  static partial class Help
    {
        public static bool IsSameAs<T>(this MethodInfo method,object obj=null)
        {
            try
            {
                var dt = typeof (T);
                if (obj != null)
                    method.CreateDelegate(dt, obj);
                else
                    method.CreateDelegate(dt);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        
        private static string x86Instructions()
        {
            var j = new[]
            {
                "aaa", "aad", "aam", "aad", "aas", "adc", "bsf", "bsr", "bswap", "bt", "btc", "btr", "bts", "cbw", "cdq",
                "clc",
                "cld", "cli", "cmc", "cmp", "cmps", "cmpsb", "cmpsw", "cmpsd", "cmpxchg", "cwd", "daa", "das", "dec",
                "inc",
                "enter", "hlt", "idiv",
                "imul", "in", "inc", "ins", "insb", "insw", "insd", "int", "into", "iret", "ja", "jna", "jae", "jnae",
                "jb", "je",
                "jne", "jz", "jnz", "js", "jnb", "jbe", "jnbe", "jg", "jng", "jge", "jnge", "jl", "jnl", "jle", "jnle",
                "jns", "jc"
                , "jnc", "jo", "jno", "jp", "jnp", "jpe", "jpo", "jcxz", "jecxz", "jmp", "lahf", "lds", "les", "lfs",
                "lgs", "lss",
                "lea", "leave", "lock", "lods", "lodsb", "lodsw", "lodsd", "loop", "loopd", "loope", "loopz", "loopne",
                "loopnz",
                "loopw",
                "mov", "movs", "movsb", "movsw", "movsd", "movsx", "movzx", "mul", "neg", "nop", "not", "or", "out",
                "outs", "outb"
                , "outw", "outd", "pop", "popa", "popad", "popf", "popfd", "push", "pusha", "pushad", "pushf", "pushfd",
                "pushw",
                "pushd",
                "rcl", "rcr", "rep", "ret", "retf", "retn", "rol", "ror", "sahf", "sal", "sar", "sbb", "scas", "scasb",
                "scasw",
                "scasw", "scasd", "shl", "shld", "shr", "shrd", "sts", "std", "sti", "stos", "stosb", "stosw", "stosd",
                "test",
                "wait", "xadd", "xchg", "xlat", "xlatb", "xor", "fabs", "fadd", "faddp", "fiadd", "fbldfbstp", "fchs",
                "fclex",
                "fcmov", "fcom", "fcomi", "fcos", "fdecstp", "fdiv", "fdivp", "fidiv", "fdivr", "fdivrp", "fidivr",
                "ffree",
                "ficom", "fild", "fincstp", "finit", "fist", "fisttp", "fld", "fld1", "fldL2T", "fldL2E", "fldPI",
                "fldLG2",
                "fldLN2", "fldZ", "fldCW", "fldENV", "fmul", "fmulp", "fimul", "fnop",
                "fpatan", "fprem", "fptan", "frndint", "frstor", "fsave", "fscale", "fsin", "fsincos", "fsqrt", "fst",
                "fstcw",
                "fstenv", "fstsw", "fsub", "fsubp", "fisub", "fsubr", "fsubrp", "fisubr", "ftst", "fwait", "fxam",
                "fxch", "fxstor"
                , "fxsave", "fxtract", "fy2lx", "fyl2xp1"
            };
            var outp = "";
            foreach (var s in j)
                outp += "public static void " + s + "(int v1, int v2){\r\n}\r\n";
            return outp;
        }
    }

    public static partial class Help
    {

        internal static void Import(Type t, object instant)
        {
            if (instant != null)
                _Import(t, instant, InstantBinding);
        }


        internal static void Import(Type t)
        {
            _Import(t, null, StaticBinding);

        }

        private static void _Import(IReflect t, object obj, BindingFlags staticBinding)
        {
            var dm = t.GetMethods(staticBinding);

            foreach (
                MethodInfo methodInfo1 in
                    dm.Where(
                        methodInfo =>
                            obj == null
                                ? IsSameAs<BasicInstruction>(methodInfo)
                                : IsSameAs<BasicInstruction>(methodInfo, obj)).OrderBy(methodInfo => methodInfo.Name))
            {
                BasicInstruction r;
                if (obj != null)
                    r = (BasicInstruction)methodInfo1.CreateDelegate(typeof(BasicInstruction), obj);
                else r = (BasicInstruction)methodInfo1.CreateDelegate(typeof(BasicInstruction));
                if (!UAL.BasicInstructions.Contains(r))
                {
                    UAL.BasicInstructions.Add(r);
                    UAL.NameInstructions.Add(r.Method.Name.ToLower());
                }
            }
        }

        public static void Import(string type)
        {
            var t = Type.GetType(type);
            Import(t);
        }

        public const BindingFlags StaticBinding =
            BindingFlags.CreateInstance | BindingFlags.Default | BindingFlags.Static | BindingFlags.InvokeMethod |
            BindingFlags.NonPublic | BindingFlags.Public;

        public const BindingFlags InstantBinding =
            BindingFlags.CreateInstance | BindingFlags.Default | BindingFlags.Instance | BindingFlags.InvokeMethod |
            BindingFlags.NonPublic | BindingFlags.Public;
    }
}