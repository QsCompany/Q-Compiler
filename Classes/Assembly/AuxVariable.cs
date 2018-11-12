namespace Compiler.Classes.Assembly
{
    public class AuxVariable
    {
        public FieldInfo Field;
        public bool IsInLive;

        public AuxVariable (FieldInfo field, bool isInLive)
        {
            this.IsInLive = isInLive;
            this.Field = field;
        }
    }
}