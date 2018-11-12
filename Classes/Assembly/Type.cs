using System.Collections.Generic;
using System;
namespace Compiler.Classes.Assembly
{

    public class Type
    {
        public int DataSize, InstructionSize;
        public readonly Type Base;
        public readonly string Name;
        public readonly Space Space;
        public readonly bool IsClass;
        public readonly List <FieldInfo> Fields = new List <FieldInfo>();
        public readonly List <MethodInfo> Methods = new List <MethodInfo>();
        public override bool Equals(object obj)
        {
            return base.Equals(obj) || obj.Equals(Name) || obj.Equals(FullName);
        }
        public string FullName
        {
            get { return this.Space.FullName + "." + this.Name; }
        }

        public FieldInfo GetField (string fieldName)
        {
            var type = this;
            while (true)
            {
                if (type == null) return null;
                foreach (FieldInfo field in type.Fields) if (field.Name == fieldName) return field;
                if (type.Base != null)
                {
                    type = type.Base;
                    continue;
                }
                return null;
            }
        }

        public List <MethodInfo> GetMethods (string methodName)
        {
            var lst = new List<MethodInfo>();
            var type = this;
            while (true)
            {
                if (type == null) break;
                foreach (var field in type.Methods) if ( field.Name == methodName ) lst.Add(field);
                if (type.Base != null)
                {
                    type = type.Base;
                    continue;
                }
                break;
            }
            return lst;
        }

        public MethodInfo GetMethod (string methodName)
        {
            foreach ( var field in this.Methods ) if ( field.Name == methodName ) return field;
            return null;
        }

        public Type (Space space, Type Base, string name,bool isClass)
        {
            this.Base = Base;
            this.Name = name;
            this.Space = space;
            this.DataSize = Base.DataSize;
            this.IsClass = isClass;
            space.Types.Add(this);
        }

        public Type(Assembly assembly, string space, string Base, string name, bool isClass)
        {
            this.Base = assembly.GetType(Base);
            if ( this.Base == null ) throw new Exception("TypeNotFounded");
            this.Name = name;
            this.Space = assembly[space];
            this.DataSize = this.Base.DataSize;
            this.IsClass = isClass;
            this.Space.Types.Add(this);
        }

        public Type(Space space, Type Base, string name, int dataSize, bool isClass)
        {
            this.Name = name;
            this.Space = space;
            this.Base = Base;
            space.Types.Add(this);
            this.IsClass = isClass;
            this.DataSize = dataSize;
        }

        public Type(Assembly assembly, Space space, string name, int dataSize, string baseAssembly = null, bool isClass=true)
        {
            this.Name = name;
            this.Space = space;
            if ( baseAssembly != null ) this.Base = assembly.GetType(baseAssembly);
            space.Types.Add(this);
            this.IsClass = isClass;
            this.DataSize = dataSize;
        }

        public override string ToString () { return this.FullName; }
    }
}