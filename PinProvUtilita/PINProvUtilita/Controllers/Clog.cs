using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace PINProvUtilita.Controllers
{
    public class Clog
    {
        SqlConnection con = null;

        public bool SaveLogPinProvisioning(string idUtente, Int16 codiceOperatore, Int16 evento, string descrizione, long tempoEsecuzione, string errorMessage, string ipClient)
        {
            string constring = ConfigurationManager.ConnectionStrings["SicurezzaClogPinProvisioning"].ToString();
            int reurncode;
            try
            {
                using (con = new SqlConnection(constring))
                {
                    using (SqlCommand cmd = new SqlCommand("[spAppendToLog]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Utente", idUtente);
                        cmd.Parameters.AddWithValue("@idClasseUtente", codiceOperatore);

                        cmd.Parameters.AddWithValue("@idevento", evento);

                        cmd.Parameters.AddWithValue("@Parametri", descrizione);

                        cmd.Parameters.AddWithValue("@ipClient", ipClient);
                        cmd.Parameters.AddWithValue("@TempoEsecuzione", tempoEsecuzione);
                        cmd.Parameters.AddWithValue("@ReturnCode", 0);  // this.Confing.Scopo == LogScope.Info ? 0 : this.Confing.Esito);
                        cmd.Parameters.AddWithValue("@DescrizioneErrore", DBNull.Value);
                        cmd.Parameters.Add("@ErrorCode", SqlDbType.Int);
                        cmd.Parameters["@ErrorCode"].Direction = ParameterDirection.Output;
                        con.Open();

                        cmd.ExecuteNonQuery();
                        reurncode = (Int32)cmd.Parameters["@ErrorCode"].Value;
                        con.Close();

                        con.Close();
                        con = null;

                        return true;
                    }
                }
            }
            catch (Exception e)
            {

                return false;
            }
            finally
            {
                if (con != null)
                    con.Close();


            }

        }
 
    }
}