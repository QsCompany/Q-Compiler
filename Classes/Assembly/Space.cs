namespace Compiler.Classes.Assembly
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
using System.Collections.ObjectModel;

     public partial class Space : IEnumerable <Space>, IEnumerable <Type>
    {
        public readonly string Name;
        public readonly Space Parent;
        internal readonly List<Space> Children = new List<Space>();
        public readonly List<Type> Types = new List<Type>();
        public readonly static ReadOnlyCollection<Space> AllSpaces;
        private readonly static List<Space> _AllSpaces = new List<Space>();
        static Space()
        {
            AllSpaces = new ReadOnlyCollection<Space>(_AllSpaces);
        }
        public Space (Space parent, string name)
        {
            this.Name = name;
            this.Parent = parent;
            if ( parent != null ) this.Parent.Children.Add(this);
            _AllSpaces.Add(this);
        }

               public string FullName
        {
            get
            {
                Space d = this;
                string s = d.Name;
                while ( d.Parent != null ) {
                    s = d.Parent.Name + "." + s;
                    d = d.Parent;
                }
                return s;
            }
        }

        public Space Root
        {
            get
            {
                Space root = this;
                while ( root.Parent != null ) root = root.Parent;
                return root;
            }
        }

        #region IEnumerable<Space> Members

        public IEnumerator <Space> GetEnumerator () { return this.Children.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator () { return GetEnumerator(); }

        #endregion

        #region IEnumerable<Type> Members

        IEnumerator <Type> IEnumerable <Type>.GetEnumerator () { return this.Types.GetEnumerator(); }

        #endregion

        public Space Add (string heritachyName)
        {
            string[] names = heritachyName.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
            Space This = this;
            foreach ( string name in names ) {
                Space space = This[name] ?? new Space(This, name);
                This = space;
            }
            return This;
        }

        public Space GoBack (string heritachyName)
        {
            string[] d = heritachyName.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
            Space This = this;
            if ( !This.FullName.EndsWith(heritachyName) ) return null;
            for (int i = d.Length - 1; i >= 0; i--) This = This.Parent;
            return This;
        }

        public static void GetAllTypes (Space space, List <List <Type>> r)
        {
            r.Add(space.Types);
            foreach ( Space sp in space.Children ) GetAllTypes(sp, r);
        }

        public IEnumerable <Type> GeAllTypes ()
        {
            var a = new List <List <Type>>();
            GetAllTypes(Root, a);
            foreach ( var types in a ) foreach ( Type type in types ) yield return type;
        }
        public override string ToString()
        {
            return FullName;
        }

        internal Space SearchSpace(string[] root, int p)
        {
            var e = string.Join(".", root);
            foreach (var item in _AllSpaces)
                if (item.FullName.EndsWith(e)) return item;
            return null;
        }
    }

     public partial class Space
     {
         public Space this[string name]
         {
             get
             {
                 if (name == "::") return this.Parent;
                 if (string.IsNullOrWhiteSpace(name)) return this;
                 foreach (Space space in this.Children)
                     if (space.Name == name)
                         return space;
                 return null;
             }
         }
      
         public Type this[string typeName,bool AsType=true]
         {
             get
             {
                 foreach (var type in this.Types)
                     if (type.Name.Trim() == typeName.Trim()) return type;
                 return null;
             }
         }
     }
}
