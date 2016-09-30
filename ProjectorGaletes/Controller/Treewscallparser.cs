using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Web;
using System.Net;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;


namespace ProjectorGaletes
{
    public static class Treewscallparser
    {

        [DllImport("treewscall32.dll", SetLastError=false, CharSet = CharSet.Ansi, EntryPoint = "WSCall")]
        private static extern string WSCall32(
            string serverport,
            string user,
            string password,
            string cmd,
            string parms);

        [DllImport("treewscall64.dll", SetLastError = false, CharSet = CharSet.Ansi, EntryPoint = "WSCall")]
        private static extern string WSCall64(
            string serverport,
            string user,
            string password,
            string cmd,
            string parms);


        private static string WSCall(string serverport, string user, string password, string cmd, string parms)
        {
            if (IntPtr.Size > 4)
            {
                return WSCall64(serverport, user, password, cmd, parms);
            }
            else
            {
                return WSCall32(serverport, user, password, cmd, parms);
            }
        }

        
        private static string getRawDadosBob2(string strProjecto)
        {
            string serverport = "treefnc1:8899";
            string user = "1520";
            string password = "thunde";
            string cmd = "WtAcessPoint";
            string func = "ENGIFOLHABOB";
            string parms = string.Format("{0}+{1}", func, strProjecto );

            //string url = string.Format("http://{0}/exec?cmd={1}&parms={2}{3}{4}", serverport, cmd, Uri.EscapeUriString(user + " "), System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password)), Uri.EscapeUriString(" " + parms));
            string url = string.Format("http://{0}/exec?cmd={1}&parms={2}+{3}+{4}", serverport, cmd, user, System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password)), parms);

            return GetResponseText(url);
        }

        private static string getRawFAKEDadosBob()
        {
            return "38;3;5;2.24;5;0.65;2;0.05;7;0.05;S;0.088;13.08;6.08;30;15*;U;19;615;615;612;615;615;612;1630;1624;1624;2854;2854;2848;979;976;976;575;575;572;2126;2126;2120;200;810;810;804;D;True;4.54;12.38;4.54;12.38;2;1;6712;150;;;;;;;;\\n39;8;5;2.24;5.6;0.65;2;0.05;5;0.05;S;0.088;12.88;6.48;14;7*;;17;800;800;797;800;800;797;1458;1452;1452;3052;3052;3046;807;804;804;747;747;744;2298;2298;2292;100;900;900;894;I;False;2.5;12.88;2.5;12.88;2;1;6747;90;2;1452;12;804;0;9;0;95\\n40;8;5;2.24;5.6;0.65;2;0.05;5;0.05;S;0.088;12.88;6.48;14;7*;;17;800;800;797;800;800;797;1458;1452;1452;3052;3052;3046;807;804;804;747;747;744;2298;2298;2292;100;900;900;894;D;True;2.5;12.88;2.5;12.88;2;1;6747;90;2;1452;12;804;0;0;0;95\\n41;8;5;2.24;5.6;0.65;2;0.05;5;0.05;S;0.088;12.88;6.48;14;7*;;17;800;800;797;800;800;797;1458;1452;1452;3052;3052;3046;807;804;804;747;747;744;2298;2298;2292;100;900;900;894;I;False;2.5;12.88;2.5;12.88;2;1;6747;90;2;1452;12;804;0;9;0;95\\n42;8;5;2.24;5.6;0.65;2;0.05;5;0.05;S;0.088;12.88;6.48;14;7*;;17;800;800;797;800;800;797;1458;1452;1452;3052;3052;3046;807;804;804;747;747;744;2298;2298;2292;100;900;900;894;D;True;2.5;12.88;2.5;12.88;2;1;6747;90;2;1452;12;804;0;0;0;95\\n";
        }

        public static string GetResponseText(string address)
        {
            var request = (HttpWebRequest)WebRequest.Create(address);
            var watch = Stopwatch.StartNew();
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                //var encoding = Encoding.GetEncoding(response.CharacterSet);
                watch.Stop();
                Console.WriteLine(String.Format("Wintree Response Time:{0} s", watch.ElapsedMilliseconds/1000));
                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream, Encoding.Default))
                    return reader.ReadToEnd();
            }
        }

        static void PrintRows(params string[] dataLines)
        {
            foreach (string line in dataLines)
            {
                Console.WriteLine(line);
            }
        }

        public static Dictionary<string, string>[] getDadosBob(string strProjecto)
        {
            Dictionary<string, string>[] result;
            string rawDataString = getRawDadosBob2(strProjecto);
            rawDataString = rawDataString.Replace("rc=0", "");
            rawDataString = rawDataString.Trim();

            if(String.IsNullOrEmpty(rawDataString))
            {
                Console.WriteLine("ERRO: O Wintree não retornou registos");

                // Caso o Wintree não esteja a retornar informação retorna informação ficticia
                #if DEBUG
                    rawDataString = getRawFAKEDadosBob();
                    Console.WriteLine("ATENÇÃO: Carregados dados ficticios");
                #else
                    return result;
                #endif
            }

            string[] strHeaderBOBST = {"Gal",
										"NF",
										"NCOBFX",
										"DIMAXICOB",
										"DIMRADCOB",
										"DIMRAIOCOB",
										"NPI",
										"EPI",
										"NPF",
										"EPF",
										"QUALPAPEL",
										"ESPPAPELADES",
										"DIMAXIFX",
										"DIMRADFX",
										"NE",
										"CRUZA",
										"AG",
										"ENCH",
										"A_max",
										"A",
										"A_min",
										"B_max",
										"B",
										"B_min",
										"C_max",
										"C",
										"C_min",
										"K_max",
										"K",
										"K_min",
										"E_max",
										"E",
										"E_min",
										"G_max",
										"G",
										"G_min",
										"W_max",
										"W",
										"W_min",
										"RI",
										"RE_max",
										"RE",
										"RE_min",
										"SENTIDO",
										"GALPAR",
										"DIMRADESI",
										"DIMAXIESI",
										"DIMRADESE",
										"DIMAXIESE",
										"CM",
										"NRFASES",
										"LM",
										"DURC",
										"NR_MANDRIL",
										"C_MANDRIL",
										"D_MANDRIL",
										"E_MANDRIL",
										"F_MANDRIL",
										"H_MANDRIL",
										"HR_MANDRIL",
										"RI_MANDRIL"};


            
            string[] auxLinhas = rawDataString.Split(new string[] { "\\n" }, StringSplitOptions.RemoveEmptyEntries);

            Console.BufferHeight += auxLinhas.Length * (strHeaderBOBST.Length+1); // Makes space available for value logging

            if (auxLinhas.Length <= 1) {
                Regex pattern = new Regex("[;,\t\r]|[\n]{2}");
                throw new System.Exception("Erro no Wintree na leitura dos dados das galetes: " + pattern.Replace(rawDataString, "")); 
            };

            result = new Dictionary<string, string>[auxLinhas.Length];

            int i = 0;
            foreach (string auxLinha in auxLinhas)
            {
                Console.WriteLine("------//-------");
                Dictionary<string, string> dadosGal = new Dictionary<string, string>();
                string[] auxCampos = auxLinha.Split(new char[] { ';' });
                if (auxCampos.Length != strHeaderBOBST.Length) { throw new System.Exception("Incoerência no número de campos!"); };

                int c = 0;
                foreach (string auxCampo in auxCampos)
                {
                    Console.WriteLine("{0,12}: {1}", strHeaderBOBST[c], auxCampo);
                    dadosGal[strHeaderBOBST[c]] = auxCampo;
                    //rowValues (i) = auxCampo;
                    c++;
                }
                
                result[i] = dadosGal;
                i++;

            }

            return result;
        }
    }
}

