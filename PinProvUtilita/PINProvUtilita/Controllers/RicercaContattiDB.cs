using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PINProvUtilita.Models;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;

namespace PINProvUtilita.Controllers
{
    public class RicercaContattiDB
    {
        SqlConnection con = null;

        public List<ListaContatti> RicercaContatti(string email, string pec, string cellulare, string certificato, string operatore, string ipclient)
        {
            List<ListaContatti> listContatti = new List<ListaContatti>();

            try
            {
                IDataReader reader = null;

                string constring = ConfigurationManager.ConnectionStrings["SicurezzaPinProvisioning"].ToString();
                using (con = new SqlConnection(constring))
                {
                    using (SqlCommand cmd = new SqlCommand("[spPGetDatiContatto]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        if (!String.IsNullOrEmpty(email))
                            cmd.Parameters.AddWithValue("@vIndirizzoEmail ", email);
                        else
                            cmd.Parameters.AddWithValue("@vIndirizzoEmail", null);

                        if (!String.IsNullOrEmpty(pec))
                            cmd.Parameters.AddWithValue("@vIndirizzoPEC", pec);
                        else
                            cmd.Parameters.AddWithValue("@vIndirizzoPEC", null);

                        if (!String.IsNullOrEmpty(cellulare))
                            cmd.Parameters.AddWithValue("@vCellulare", cellulare);
                        else
                            cmd.Parameters.AddWithValue("@vCellulare", null);

                        cmd.Parameters.AddWithValue("@vCertificato", certificato);
                        con.Open();
                        using (reader = cmd.ExecuteReader())
                        {
                            listContatti = DbMapper.PopulateEntities<ListaContatti>(reader);
                        }

                        con.Close();
                        con = null;
                    }
                }

            }
            catch (Exception e)
            {

            }
            finally
            {
                if (con != null)
                    con.Close();


            }

            return listContatti;
        }
    }
}