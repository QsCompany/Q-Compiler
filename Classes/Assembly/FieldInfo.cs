namespace Compiler.Classes.Assembly
{
    public class FieldInfo:MembreInfo
    {
        public FieldInfo()
        {
            
        }
        public override string ToString()
        {
            return Return + " " + Name;
        }
    }
}