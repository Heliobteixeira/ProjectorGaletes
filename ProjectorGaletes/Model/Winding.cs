using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ProjectorGaletes
{
    class Winding : Dictionary<int, PancakeCoil>
    {
        //private Dictionary<string, string>[] dadosBob;


        public Winding(string strProjecto)
        {
            Dictionary<string, string>[] dadosBob = Treewscallparser.getDadosBob(strProjecto);
            parseTreewsData(dadosBob);
            Console.WriteLine("Dados do projecto {0} carregados", strProjecto);
        }

        private void parseTreewsData(Dictionary<string, string>[] dadosBob){
            PancakeCoil auxCoil;

            for(int i=0; i<dadosBob.Length; i++) {
                auxCoil = parseTreewsGal(dadosBob[i]);
                this.Add(auxCoil.Nr, auxCoil);
            }
        }

        private PancakeCoil parseTreewsGal(Dictionary<string, string> dadosGal){
            int nrGal;
            int nrEsp;
            int C_mand, E_mand, Ri_mand, HR_mand, H_mand, F_mand;
            int A, B, C, E, G, Ri, Re, nrFx, nrEspiraCruzamentos;
            int saidaAouT;
            int sentido;
            float DimRadFx;

            C_mand = convertStringToInt(dadosGal["C_MANDRIL"]);
            E_mand = convertStringToInt(dadosGal["E_MANDRIL"]);
            Ri_mand = convertStringToInt(dadosGal["RI_MANDRIL"]);
            HR_mand = convertStringToInt(dadosGal["HR_MANDRIL"]);
            H_mand = convertStringToInt(dadosGal["H_MANDRIL"]);
            F_mand = convertStringToInt(dadosGal["F_MANDRIL"]);

            CoilMandrel mandril = new CoilMandrel(C_mand, E_mand, Ri_mand, H_mand, HR_mand, F_mand);


            nrGal = convertStringToInt(dadosGal["Gal"]);
            nrEsp = convertStringToInt(dadosGal["NE"]);
            C = convertStringToInt(dadosGal["C"]);
            E = convertStringToInt(dadosGal["E"]);
            A = convertStringToInt(dadosGal["A"]);
            B = convertStringToInt(dadosGal["B"]);
            G = convertStringToInt(dadosGal["G"]);
            Re = convertStringToInt(dadosGal["RE"]);
            Ri = convertStringToInt(dadosGal["RI"]);
            DimRadFx = convertStringToFloat(dadosGal["DIMRADFX"]);
            nrFx = convertStringToInt(dadosGal["NF"]);
            saidaAouT = -1;                            //Não encontro o valor!!!
            if(dadosGal["SENTIDO"] == "D") 
            {
                sentido = 1;
            }else if(dadosGal["SENTIDO"] == "I")
            {
                sentido = -1;
            }else{
                Console.WriteLine("Unable to find sentido of coil {0}.", nrGal);
                sentido = 0;
            }

            Regex digitsOnly = new Regex(@"[^\d]");
            nrEspiraCruzamentos = convertStringToInt(digitsOnly.Replace(dadosGal["CRUZA"], ""));

            return new PancakeCoil(nrGal, mandril, C, E, A, B, G, Ri, Re, DimRadFx, nrFx, nrEsp, saidaAouT, sentido, nrEspiraCruzamentos);         

        }

        public List<int> coilsList()
        {
            return new List<int>(this.Keys);
        }

        private int convertStringToInt(string value){
            int number; 
            bool result = Int32.TryParse(value, out number);
             if (result) { return number; }
             else
             {
                //if (value == null) value = ""; 
                Console.WriteLine("Attempted conversion of '{0}' failed.",  value);
                return 0;
             }
  
        }

        private float convertStringToFloat(string value)
        {
            float number;
            bool result = float.TryParse(value.Replace(".",","), out number);
            if (result) { return number; }
            else
            {
                //if (value == null) value = ""; 
                Console.WriteLine("Attempted conversion of '{0}' failed.", value);
                return 0;
            }

        }
    }
}

