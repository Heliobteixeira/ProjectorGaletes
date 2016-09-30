using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace ProjectorGaletes
{
    class Winding : Dictionary<int, PancakeCoil>
    {
        //private Dictionary<string, string>[] dadosBob;

        private string dbConnectionString = ConfigurationManager.ConnectionStrings["DBConString"].ConnectionString;
        private string _projecto;
        public string projecto
        {
            get { return _projecto; }
        }
        private bool _loaded = false;
        public bool isLoaded
        {
            get { return _loaded; }
            set { _loaded = value; }
        }

        public Winding(string strProjecto, bool cached = true)
        {
            if (cached)
            {
                Console.WriteLine("Fetching SQL data...");
                DataTable dt = loadDBCachedData(strProjecto);
                parseSQLData(dt);
                isLoaded=true; // TODO: Function to check if is loaded
            }
            else
            {
                Console.WriteLine("Fetching Wintree data...");
                Dictionary<string, string>[] dadosBob = Treewscallparser.getDadosBob(strProjecto);
                parseTreewsData(dadosBob);
                isLoaded=true; // TODO: Function to check if is loaded
            }
            
            _projecto = strProjecto;
            Console.WriteLine("Dados do projecto {0} carregados", strProjecto);
        }

        private void parseTreewsData(Dictionary<string, string>[] dadosBob){
            this.Clear(); // Cleans current list of coils
            PancakeCoil auxCoil;

            for(int i=0; i<dadosBob.Length; i++) {
                auxCoil = parseTreewsGal(dadosBob[i]);
                this.Add(auxCoil.Nr, auxCoil);
            }
        }

        private void parseSQLData(DataTable dt)
        {
            this.Clear(); // Cleans current list of coils
            PancakeCoil auxCoil;

            foreach (DataRow dr in dt.Rows)
            {
                auxCoil = parseSQLGal(dr);
                this.Add(auxCoil.Nr, auxCoil);
            }
        }

        private PancakeCoil parseTreewsGal(Dictionary<string, string> dadosGal){
            int nrGal;
            int nrEsp;
            int C_mand, E_mand, Ri_mand, HR_mand, F_mand;
            float H_mand;
            int A, B, C, E, G, Ri, Re, nrFx;
            int[] nrEspiraCruzamentos;
            int saidaAouT;
            int sentido;
            float DimRadFx;

            C_mand = convertStringToInt(dadosGal["C_MANDRIL"]);
            E_mand = convertStringToInt(dadosGal["E_MANDRIL"]);
            Ri_mand = convertStringToInt(dadosGal["RI_MANDRIL"]);
            HR_mand = convertStringToInt(dadosGal["HR_MANDRIL"]);
            H_mand = convertStringToFloat(dadosGal["H_MANDRIL"]);
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


            nrEspiraCruzamentos = parseEspirasCruzamentos(dadosGal["CRUZA"]);

            return new PancakeCoil(nrGal, mandril, C, E, A, B, G, Ri, Re, DimRadFx, nrFx, nrEsp, saidaAouT, sentido, nrEspiraCruzamentos);         

        }

        private PancakeCoil parseSQLGal(DataRow dadosGal)
        {
            int nrGal;
            int nrEsp;
            int C_mand, E_mand, Ri_mand, HR_mand, F_mand;
            float H_mand; 
            int A, B, C, E, G, Ri, Re, nrFx;
            int[] nrEspiraCruzamentos;
            int saidaAouT;
            int sentido;
            float DimRadFx;

            C_mand = (int)dadosGal["mandril_C"];
            E_mand = (int)dadosGal["mandril_E"];
            Ri_mand = (int)dadosGal["mandril_Ri"];
            HR_mand = (int)dadosGal["mandril_Hr"];
            H_mand = Convert.ToSingle(dadosGal["mandril_H"]);
            F_mand = (int)dadosGal["mandril_F"];

            CoilMandrel mandril = new CoilMandrel(C_mand, E_mand, Ri_mand, H_mand, HR_mand, F_mand);
            
            nrGal = (int)dadosGal["galete_Nr"];
            nrEsp = (int)dadosGal["galete_NE"];
            C = (int)dadosGal["galete_C"];
            E = (int)dadosGal["galete_E"];
            A = (int)dadosGal["galete_A"];
            B = (int)dadosGal["galete_B"];
            G = (int)dadosGal["galete_G"];
            Re = (int)dadosGal["galete_Re"];
            Ri = (int)dadosGal["galete_Ri"];
            DimRadFx = Convert.ToSingle(dadosGal["galete_DimRadFx"]);
            nrFx = (int)dadosGal["galete_NF"];
            saidaAouT = (int)dadosGal["galete_SaidaAouT"];
            sentido = (int)dadosGal["galete_Sentido"];

            nrEspiraCruzamentos = parseEspirasCruzamentos((string)dadosGal["galete_Cruza"]);  // TODO: Corrigir

            return new PancakeCoil(nrGal, mandril, C, E, A, B, G, Ri, Re, DimRadFx, nrFx, nrEsp, saidaAouT, sentido, nrEspiraCruzamentos);

        }

        private int[] parseEspirasCruzamentos(string rawWTStringCruzamentos)
        {
            string[] arrayCruzamentos;
            string[] splitChars = {"*", ";"};
            arrayCruzamentos = rawWTStringCruzamentos.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);  // '*' -> Wintree     ';' -> DB
            return Array.ConvertAll(arrayCruzamentos, s => Int32.Parse(s));
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
            bool result = float.TryParse(value.Replace(".", ","), out number);
            if (result) { return number; }
            else
            {
                //if (value == null) value = ""; 
                Console.WriteLine("Attempted conversion of '{0}' failed.", value);
                return 0;
            }

        }

        public void cacheWindingToDB()
        {
            string storedProcedure = GeneralConstants.storProcName_insertCoil;
            SQLManager sqlManager = new SQLManager(ConfigurationManager.ConnectionStrings["DBConString"].ConnectionString);
            foreach (PancakeCoil coil in this.Values)
            {
                SqlParameter[] parametrosSQL = 
                {
                    new SqlParameter("@Projecto", _projecto),
                    new SqlParameter("@Mandril_C", coil.mandrel.C),
                    new SqlParameter("@Mandril_E", coil.mandrel.E),
                    new SqlParameter("@Mandril_Ri", coil.mandrel.Ri),
                    new SqlParameter("@Mandril_Hr", coil.mandrel.HR),
                    new SqlParameter("@Mandril_H", coil.mandrel.H),
                    new SqlParameter("@Mandril_F", coil.mandrel.F),
                    new SqlParameter("@Galete_Nr", coil.Nr),
                    new SqlParameter("@Galete_NE", coil.nrEsp),
                    new SqlParameter("@Galete_C", coil.C),
                    new SqlParameter("@Galete_E", coil.E),
                    new SqlParameter("@Galete_A", coil.A),
                    new SqlParameter("@Galete_B", coil.B),
                    new SqlParameter("@Galete_G", coil.G),
                    new SqlParameter("@Galete_Re", coil.Re),
                    new SqlParameter("@Galete_Ri", coil.Ri),
                    new SqlParameter("@Galete_DimRadFx", coil.dimRadFx),
                    new SqlParameter("@Galete_NF", coil.nrFx),
                    new SqlParameter("@Galete_Sentido", coil.sentido),
                    new SqlParameter("@Galete_Cruza", coil.cruzamentos.csvString),
                    new SqlParameter("@Galete_SaidaAouT", coil.saidaAouT),
                };

                sqlManager.executeStoredProcedure(storedProcedure, ref parametrosSQL);

            }

            sqlManager.Disconnect();

            return;
        }

        private DataTable loadDBCachedData(string project)
        {
            string storedProcedure = GeneralConstants.storProcName_getWindingData;
            SQLManager sqlManager = new SQLManager(ConfigurationManager.ConnectionStrings["DBConString"].ConnectionString);

            SqlParameter[] parametrosSQL = { SQLManager.newSqlParameter("@Projecto", project, SqlDbType.NChar) };
            DataTable dt = sqlManager.executeStoredProcedure(storedProcedure, ref parametrosSQL);
            sqlManager.Disconnect();
            return dt;

        }

        public static DateTime? getDBLastUpdate(string project)
        {
            // Returns the last cached date/time of a specified project or null if it wasn't cached at all

            string storedProcedure = GeneralConstants.storProcName_getProjectCacheDate;
            SQLManager sqlManager = new SQLManager(ConfigurationManager.ConnectionStrings["DBConString"].ConnectionString);
            DateTime? cachedDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue; // Date.MinValue is < than SQL minimum admissible date

            SqlParameter datePrmtr = SQLManager.newSqlParameter("@cachedDate", cachedDate, SqlDbType.DateTime, ParameterDirection.Output);
            /*
            SqlParameter datePrmtr = new SqlParameter("@cachedDate", cachedDate);
            datePrmtr.SqlDbType= SqlDbType.DateTime;
            datePrmtr.Direction = ParameterDirection.Output;
            */
            SqlParameter[] parametrosSQL =
            {
                new SqlParameter("@Projecto", project),
                datePrmtr 
            };

            sqlManager.executeStoredProcedure(storedProcedure, ref parametrosSQL);


            if (!(datePrmtr.Value is DBNull))
            {
                cachedDate = (DateTime)datePrmtr.Value;
            }
            else 
            {
                cachedDate = null;
            }

            sqlManager.Disconnect();

            return cachedDate;
        }

        public static int? getDaysSinceLastCached(string project)
        {
            // Returns number of days since last caches. If it wasn't cached returns -1
            DateTime? date = getDBLastUpdate(project);
            if(date.HasValue)
            {
                int days = (DateTime.Now - date.Value).Days;
                return days;
            }
            else 
            {
                return null;
            }
            
        }

    }
}

