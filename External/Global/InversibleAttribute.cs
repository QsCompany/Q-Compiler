using System;

namespace Compiler.Global
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class InversibleAttribute : Attribute
    {
        readonly string _methodeName;
        private readonly Type _target;
        private readonly int _argument;

        public InversibleAttribute(Type targe, string methodeName, int argument = 0)
        {
            _methodeName = methodeName;
            _argument = argument;
            _target = targe;
        }

        public string MethodeName
        {
            get { return _methodeName; }
        }

        public Type Target
        {
            get { return _target; }
        }

        public int Argument
        {
            get { return _argument; }
        }

        public Delegate method;
    }
}