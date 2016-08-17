using System;

namespace ProjectorGaletes
{

    public class CoilMandrel
    {
        public int C, E, Ri, HR, F;
        public float H;
        public bool isValid;

        public CoilMandrel(int C, int E, int Ri, float H, int HR, int F)
        {
            this.C = C;
            this.E = E;
            this.Ri = Ri;
            this.HR = HR;
            this.H = H;
            this.F = F;

            if (C > 0 && E > 0 && Ri > 0)
            {
                isValid = true;
            }
            else
            {
                isValid = false;
            }
        }


    }
}

