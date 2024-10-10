using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace PINProvUtilita.Controllers
{
    public class LogDelegati
    {

        SqlConnection con = null;
        
        //// RESTITUISCE INDIRIZZO IP: IPAdress.Text = Request.ServerVariables["REMOTE_ADDR"];

        /// <summary>
        /// Salva le operazioni sul log di PinProvisioning
        /// </summary>
        /// <returns></returns>
        public bool SaveLogPinProvisioning(string idUtente, string codiceOperatore, Int16 evento, string descrizione, Int16 esito, string errorMessage, string ipClient)
        {
            string constring = ConfigurationManager.ConnectionStrings["SicurezzaPinProvisioning"].ToString();
            
            try
            {
                using (con = new SqlConnection(constring))
                {
                    using (SqlCommand cmd = new SqlCommand("[spPSaveLog]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@idutente", idUtente);
                        cmd.Parameters.AddWithValue("@codiceoperatore", codiceOperatore);
                        cmd.Parameters.AddWithValue("@idevento", evento);
                        cmd.Parameters.AddWithValue("@descrizione", descrizione);
                        cmd.Parameters.AddWithValue("@esito", esito);
                        if(errorMessage!=null)
                            cmd.Parameters.AddWithValue("@errormessage", errorMessage);
                        else
                            cmd.Parameters.AddWithValue("@errormessage", DBNull.Value);

                        if(ipClient!=null)
                            cmd.Parameters.AddWithValue("@ipclient", ipClient);
                        else
                            cmd.Parameters.AddWithValue("@ipclient", DBNull.Value);

                        con.Open();

                        cmd.ExecuteNonQuery();

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