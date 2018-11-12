using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Classes.Assembly
{
    public partial class AssemblySearche
    {
        Lexical Lexical;
        Assembly Assembly;
        private Space Space { get { return Lexical.CurrentSpace; } }
        private Type Type { get { return Lexical.CurrentType; } }
        private MethodInfo Method { get { return Lexical.CurrentMethod; } }
        public AssemblySearche(Lexical lexical)
        {
            Lexical = lexical;
            Assembly = lexical.CurrentAssembly;
        }

    }
    public partial class AssemblySearche
    {
        public static Space SearchSpace(Space @this,string[] name)
        {
            int i = 0;
            while (@this != null && i < name.Length)
                @this = @this[name[i]];
            return @this;
        }        
        private static Type SearchType(Space @this,string[] root, string nameType)
        {
            var space = SearchSpace(@this, root);
            if (space == null) return null;
            return space[nameType, true];
        }
        private static Space SearchSpace(Assembly @this, string[] root)
        {
            foreach (var item in @this.Spaces)
                if (item.Name.Equals(root[0]))
                    return SearchSpace(item, PushDown(root));
            return null;
        }
        private static Type SearchType(Assembly @this, string[] root,string nameType)
        {
            var space = SearchSpace(@this, root);
            if (space == null) return null;
            return space[nameType, true];
        }

        private static Space DeepSearchSpace(Assembly @this, string[] root)
        {
            return DeepSearchSpace(@this, root, new List<Assembly>());
        }
        private static Space DeepSearchSpace(Assembly @this, string[] root, List<Assembly> traits)
        {
            if (traits.Contains(@this)) return null;
            traits.Add(@this);
            Space ret = SearchSpace(@this, root);
            if (ret != null) return ret;
            foreach (var ass in @this)
            {
                ret = DeepSearchSpace(ass, root, traits);
                if (ret != null) return ret;
            }
            return null;
        }
        private static Type DeepSearchType(Assembly @this, string[] root, string nameType)
        {
            var space = DeepSearchSpace(@this, root);
            if (space == null) return null;
            return space[nameType, true];
        }

        private static string[] PushDown(string[] root)
        {
            if (root.Length < 0) return new string[0];
            var e = new string[root.Length - 1];
            Array.Copy(root, 1, e, 0, e.Length);
            return e;
        }
    }
    public class AssemblySmartSearch
    {
        public static IEnumerable<Type> SearcheUD(Space space, string root = "")
        {
            foreach (var spc in space.Types)
                yield return spc;
            foreach (var spc in space)
                foreach (var item in SearcheUD(spc))
                    yield return item;
        }
        public static IEnumerable<Type> SearcheDU(Space sun, string root = "")
        {
            foreach (var spc in sun.Types)
                yield return spc;
            foreach (var spc in sun.Parent)
                if (root.Length <= spc.FullName.Length && spc.FullName.Contains(root))
                    if (spc == sun) continue;
                    else
                        foreach (var item in SearcheUD(spc))
                            yield return item;
            foreach (var src in SearcheDU(sun.Parent))
                yield return src;
        }


        public static Type DUSearchType(Space @this, string name)
        {
            foreach (var tp in SearcheDU(@this))            
                if (tp.FullName.EndsWith(name)) return tp;
            return null;
        }
        public static Type UDSearchType(Space @this, string name)
        {
            foreach (var tp in SearcheUD(@this))
                if (tp.FullName.EndsWith(name)) return tp;
            return null;
        }

        public static IEnumerable<Space> DUSearchSpace(Space @this, string name)
        {
            foreach (var spc in @this)
                DUSearchSpace(spc, name);
            if (@this.FullName.EndsWith(name)) yield return @this;
        }

        public static IEnumerable<Space> DUSearchSpace(Assembly @this, string name)
        {
            var lthis = @this.AllReferences();
            foreach (var _this in lthis)
                foreach (var spc in _this.Spaces)
                    foreach (var sp in DUSearchSpace(spc, name))
                        yield return sp;
        }
    }
    public interface ISearchable
    {
        Assembly From { get; set; }
        Type GetType(string name);
        Space GetSpace(string root);

        IEnumerable<Type> GetTypes(string name);
        //IEnumerable<Space> GetSpace(string root);
    }

    class SmartSearch:ISearchable
    {
        public Assembly From { get; set; }

        public Type GetType(string name)
        {
        //    foreach (var ass in From.AllReferences())
            return null;
                
        }

        public Space GetSpace(string root)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Type> GetTypes(string name)
        {
            throw new NotImplementedException();
        }

        //IEnumerable<Space> ISearchable.GetSpace(string root)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
