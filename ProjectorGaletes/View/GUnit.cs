using System;
using Gwen.Control;

namespace ProjectorGaletes
{
    public class GUnit : Base
    {
        public GUI UnitTest;

        public GUnit(Base parent) : base(parent)
        {
            
        }

        public void UnitPrint(string str)
        {
            if (UnitTest != null)
                UnitTest.PrintText(str);
        }
    }
}
