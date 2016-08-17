using System;

namespace ProjectorGaletes
{

    public class PancakeCoil
    {
        public struct Cruzamentos
        {
            public float H;
            public float W;
            public float Dist;
            public int espira;
            public int count;
        }

        public int Nr;
        public int nrEsp;
        private CoilMandrel _mandrel;
        public int A, B, G, Ri, Re;
        public int C, E;
        public float dimRadFx;
        public int nrFx;
        public int saidaAouT;
        public int sentido; //1-> directa; -1-> inversa
        public Cruzamentos cruzamentos;

        public float H
        {
            get { return dimRadFx * nrFx; }
        }

        public int K
        {
            get { return C + A + B; }
        }

        public int W
        {
            get { return E + 2 * G; }
        }

        // H medio em A antes do eixo na zona de transição (incluindo enchimento)
        public float Hmed_A
        {
            get { return (float)A / nrEsp; }
        }

        // H medio em A antes do eixo (incluindo enchimento)
        public float Hmed_A1
        {
            get { return (A - mandrel.H) / nrEsp; }
        }

        // H medio em A depois do eixo (incluindo enchimento)
        public float Hmed_A2
        {
            get { return (A - mandrel.H) / nrEsp; }
        }

        // H medio em G (incluindo enchimento)
        public float Hmed_G
        {
            get { return (float)G / (nrEsp); } // ??? Nas galetes com maior numero de espiras nao preenche completamente o G ???
        }

        // H medio em B (incluindo enchimento)
        public float Hmed_B
        {
            get { return (float)B / nrEsp; }
        }

        // H medio em R (incluindo enchimento)
        public float Hmed_R
        {
            get { return ((float)Re - mandrel.Ri) / nrEsp; }
        }

        public string sentidoDesc
        {
            get { if (sentido == 1) { return "Directa"; } else { return "Inversa"; } }
        }

        public CoilMandrel mandrel
        {
            get{
                if (_mandrel.isValid)
                {
                    return _mandrel;
                }
                else
                {
                    return new CoilMandrel(this.C, this.E, this.Ri, this.H, 0, 0);
                }
            }
            
        }

        


        public PancakeCoil(int nrGal, CoilMandrel mandril, int C, int E, int A, int B, int G, int Ri, int Re, float dimRadFx, int nrFx, int nrEsp, int saidaAouT, int sentido, int nrEspiraCruzamentos)
        {
            this.Nr=nrGal;
            this._mandrel = mandril;
            this.A = A;
            this.B = B;
            this.G = G;
            this.Ri = Ri;
            this.Re = Re;
            this.dimRadFx = dimRadFx;
            this.nrFx = nrFx;
            this.nrEsp = nrEsp;
            this.saidaAouT = saidaAouT;
            this.C = C;   
            this.E = E;
            this.sentido = sentido;

            this.cruzamentos.H = Hmed_B;
            this.cruzamentos.W = 150;    // TODO: Valor hipotetico
            this.cruzamentos.Dist = 150; // TODO: Valor hipotetico
            this.cruzamentos.espira = nrEspiraCruzamentos;
            this.cruzamentos.count = nrFx; // TODO: Valor hipotetico
        }
    }
}

