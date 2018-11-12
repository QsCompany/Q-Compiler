using VM.Parser;

namespace VM.Component
{
    public class Process
    {
        public Component MRT;

        public Process(Component mrt)
        {
            MRT = mrt;
        }

        public void Execute(int instructionsPointer,int dataPointer,int stackPointer)
        {
            MRT.Registers["cs"] = instructionsPointer;
            MRT.Registers["sp"] = dataPointer;
            MRT.Registers["ss"] = stackPointer;
            Instruct b;
            do
            {
                int c = MRT.Registers[12];
                MRT.Cache.Stream.Seek(instructionsPointer + c);
                b = Instruct.Pop(MRT.Cache.Stream);
                c += b.NCLength();
                MRT.Registers[12] = c;
                UAL.BasicInstructions[b.Function](b.Desdestination, b.Source);
            } while (b.Function != 0);
        }
    }
}