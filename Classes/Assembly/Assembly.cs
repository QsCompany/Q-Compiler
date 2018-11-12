using System.Collections.Generic;
namespace Compiler.Classes.Assembly
{   
    public partial class Assembly
    {
        private static bool Initialized;
        public static readonly List<Assembly> AllAssembly = new List<Assembly>();
        public readonly static Space System = new Space(null, "System");        
        public static readonly Assembly GlobalAssembly = new Assembly("System");
        static Assembly() { Initialize(); }
        private static void Initialize()
        {
            if (Initialized) return;
            Initialized = true;
            
            GlobalAssembly.Spaces.Add(System);
            Type[] BasicTypes = new Type[13];
            BasicTypes[0] = new Type(GlobalAssembly, System, "object", 4);
            BasicTypes[1] = new Type(GlobalAssembly, System, "struct", 0, isClass: false);
            BasicTypes[2] = new Type(System, BasicTypes[1], "numbre", 0, false);
            BasicTypes[3] = new Type(System, BasicTypes[2], "complex", 16, false);
            BasicTypes[4] = new Type(System, BasicTypes[1], "bool", 4, false);
            BasicTypes[5] = new Type(System, BasicTypes[1], "void", 0, false);
            BasicTypes[6] = new Type(System, BasicTypes[1], "byte", 4, false);
            BasicTypes[7] = new Type(System, BasicTypes[1], "char", 4, false);
            BasicTypes[8] = new Type(System, BasicTypes[2], "short", 4, false);
            BasicTypes[9] = new Type(System, BasicTypes[2], "int", 4, false);
            BasicTypes[10] = new Type(System, BasicTypes[2], "float", 4, false);
            BasicTypes[11] = new Type(System, BasicTypes[2], "long", 8, false);
            BasicTypes[12] = new Type(System, BasicTypes[2], "double", 8, false);
            shargeProcTypes(GlobalAssembly, BasicTypes);
            GlobalAssembly.Clear();
        }        

        private static void shargeProcTypes(Assembly assembly,Type[] BasicTypes)
        {
            string[] opers = { "+", "-", "*", "/" };
            for (int k = 3; k < BasicTypes.Length; k++)
            {
                var T = BasicTypes[k];
                for (int oper = 0; oper < 4; oper++)
                {
                    for (int left = 3; left <= k; left++)
                    {
                        for (int right = 3; right <= left; right++)
                        {
                            var m = new MethodInfo();
                            var n = new MethodInfo();
                            n.Name = m.Name = opers[oper];
                            m.Params.Add(BasicTypes[left]);
                            m.Params.Add(BasicTypes[right]);
                            n.Return = m.Return = BasicTypes[left];
                            m.Params.Add(m.Return);

                            n.Params.Add(BasicTypes[right]);
                            n.Params.Add(BasicTypes[left]);
                            n.Params.Add(n.Return);

                            T.Methods.Add(n);
                            T.Methods.Add(m);
                        }
                    }
                }
            }
        }

        
    }
    public partial class Assembly:List<Assembly>
    {
        public readonly List<Space> Spaces = new List<Space>();
        public string Name;
        public Assembly(string name)
        {
            this.Name = name;
            this.Add(GlobalAssembly);
            AllAssembly.Add(this);
        }
        public Type GetType(string typeQualifier)
        {
            var c = typeQualifier.IndexOf('.');
            var typeName = typeQualifier.Substring(c + 1);
            var root = c == -1 ? new string[0] : typeQualifier.Substring(0, c).Split(new[] { '.' }, global::System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var tp in GetAllTypes(typeQualifier))
                return tp;
            return null;
        }
        private Type GetType(string[] root,string deb)
        {
            var c = GetRoot(root);
            return c == null ? null : c[deb, true];
        }
        public void Add(Assembly assembly)
        {
            if (assembly == null) return;
            if (Contain(assembly)) return;
            base.Add(assembly);
            if (!AllAssembly.Contains(assembly)) AllAssembly.Add(assembly);
        }
        public bool Contain(Assembly reference)
        {
            var e = new List<Assembly>(20);
            var b = ContainRefernce(reference, e);
            e.Clear();
            return b;
        }
        private bool ContainRefernce(Assembly assembly,List<Assembly> traited)
        {
            traited.Add(this);
            foreach (Assembly ass in this)
            {
                if (ass.Equals(assembly)) return true;
                if (!traited.Contains(ass)) ass.ContainRefernce(assembly, traited);
            }
            return false;
        }
        public List<Assembly> GetRefernces()
        {
            var ret = new List<Assembly>(20);
            GetRefernces(ret);
            return ret;
        }
        private void GetRefernces(List<Assembly> traited)
        {
            traited.Add(this);
            foreach (Assembly ass in this)
                if (!traited.Contains(ass)) ass.GetRefernces(traited);
        }

        public Assembly GetRefernce(int refernce)
        {
            if (base.Count > refernce) return base[refernce];
            return null;
        }

        public Space GetRoot(string[] root)
        {
            if(root!=null&& root.Length!=0)
                foreach (var spc in Spaces)
                {
                    if (!spc.Name.Equals(root[0])) continue;
                    var e = spc.SearchSpace(root, 1);
                    if (e != null) return e;
                }
            return null;
        }
        public Space this[string Name]
        {
            get
            {
                return GetRoot(Name.Split(new[] { '.' }, global::System.StringSplitOptions.RemoveEmptyEntries));            
            }
        }
        public List<Type> GetAllType(string typeQualifier)
        {
            var e = new List<Type>();
            GetAllType(typeQualifier, e);
            return e;
        }
        private void GetAllType(string typeQualifier, List<Type> ret)
        {
            foreach (var spc in Spaces)
                foreach (var thi in spc.GeAllTypes()) if (thi.FullName.EndsWith(typeQualifier.Trim()))
                        ret.Add(thi);
        }
        private static IEnumerable<Type>  GetAllTypes(string typeQualifier)
        {
            foreach (var item in AllAssembly)            
                foreach (var tp in item.GetAllType(typeQualifier))
                    yield return tp;            
            yield return null;            
        }
        public List<Assembly> AllReferences()
        {
            var traits = new List<Assembly>(10);
            AllReferences(traits);
            return traits;
        }
        private void AllReferences(List<Assembly> traits)
        {
            if (traits.Contains(this)) return;
            traits.Add(this);
            foreach (var ass in this)
                ass.AllReferences(traits);
        }
    }
}