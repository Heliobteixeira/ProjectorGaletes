using System;

namespace ProjectorGaletes
{

    public class CoilMandrel
    {
        public int C, E, Ri, HR, F;
        public float H;
        public bool isValid
        {
            get
            {
                return (C > 0 && E > 0 && Ri > 0) ? true : false;
            }
        }

        public CoilMandrel(int C, int E, int Ri, float H, int HR, int F)
        {
            this.C = C;
            this.E = E;
            this.Ri = Ri;
            this.HR = HR;
            this.H = H;
            this.F = F;
        }


    }
}

