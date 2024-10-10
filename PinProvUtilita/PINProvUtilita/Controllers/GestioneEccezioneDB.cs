using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using PINProvUtilita.Models;

namespace PINProvUtilita.Controllers
{
    public class GestioneEccezioneDB
    {
        SqlConnection con = null;

        public WhiteListNumContatto RicercaEccezione(string contatto)
        {
            WhiteListNumContatto ricEccezione = new WhiteListNumContatto();
            List<WhiteListNumContatto> ricEccezioneLista = new List<WhiteListNumContatto>();
            try
            {
                IDataReader reader = null;

                string constring = ConfigurationManager.ConnectionStrings["SicurezzaPinProvisioning"].ToString();
                using (con = new SqlConnection(constring))
                {
                    using (SqlCommand cmd = new SqlCommand("spPCRUDWhiteListContatto", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        if (!String.IsNullOrEmpty(contatto))
                            cmd.Parameters.AddWithValue("@contatto ", contatto);
                        else
                            cmd.Parameters.AddWithValue("@contatto", null);

                        cmd.Parameters.AddWithValue("@tipoOperazione", 'S');

                        con.Open();
                        using (reader = cmd.ExecuteReader())
                        {
                            ricEccezioneLista = DbMapper.PopulateEntities<WhiteListNumContatto>(reader);
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

                if (ricEccezioneLista.Count > 0)
                    ricEccezione = ricEccezioneLista[0];
            }

            return ricEccezione;
        }

        public int EliminaEccezione(String contatto)
        {
            int reurncode;

            try
            {
                string constring = ConfigurationManager.ConnectionStrings["SicurezzaPinProvisioning"].ToString();
                using (con = new SqlConnection(constring))
                {
                    using (SqlCommand cmd = new SqlCommand("[spPCRUDWhiteListContatto]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@contatto", contatto);
                        cmd.Parameters.AddWithValue("@tipoOperazione", 'D');
                        cmd.Parameters.Add("@ReturnCode", SqlDbType.Int);
                        cmd.Parameters["@ReturnCode"].Direction = ParameterDirection.Output;
                        con.Open();

                        cmd.ExecuteNonQuery();
                        reurncode = (Int32)cmd.Parameters["@ReturnCode"].Value;

                        con.Close();

                        con.Close();
                        con = null;

                    }
                }
            }
            catch (Exception e)
            {

                //   string message = "Errore nella funzione SaveCertificatoDelegato SerialNum : " + SerialNum + " - codicefiscale : " + codicefiscale;
                string messageErrore = "Descrizione errore : " + e.ToString();
                LogDelegati Log = new LogDelegati();
                //  Log.SaveLogPinProvisioning(codicefiscale, operatore, (Int16)LogEvents.Errore, message, 1, messageErrore, ipclient);
                reurncode = 1;
            }
            finally
            {
                if (con != null)
                    con.Close();


            }
            return reurncode;


        }



        public int SaveEccezione(WhiteListNumContatto listEccezione)
        {
            int reurncode;

            try
            {
                string constring = ConfigurationManager.ConnectionStrings["SicurezzaPinProvisioning"].ToString();
                using (con = new SqlConnection(constring))
                {
                    using (SqlCommand cmd = new SqlCommand("[spPCRUDWhiteListContatto]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@contatto", listEccezione.Contatto);
                        cmd.Parameters.AddWithValue("@tipocontatto", listEccezione.TipoContatto);
                        cmd.Parameters.AddWithValue("@limite", listEccezione.Limite);
                        if(!String.IsNullOrEmpty(listEccezione.CodiceFiscaleSegnalazione))
                            cmd.Parameters.AddWithValue("@codiceFiscaleSegnalatore", listEccezione.CodiceFiscaleSegnalazione.ToUpper());

                        cmd.Parameters.AddWithValue("@note", listEccezione.Note);
                        cmd.Parameters.AddWithValue("@tipoOperazione", 'I');
                        cmd.Parameters.Add("@ReturnCode", SqlDbType.Int);
                        cmd.Parameters["@ReturnCode"].Direction = ParameterDirection.Output;
                        con.Open();

                        cmd.ExecuteNonQuery();
                        reurncode = (Int32)cmd.Parameters["@ReturnCode"].Value;

                        con.Close();

                        con.Close();
                        con = null;

                    }
                }
            }
            catch (Exception e)
            {

                //   string message = "Errore nella funzione SaveCertificatoDelegato SerialNum : " + SerialNum + " - codicefiscale : " + codicefiscale;
                string messageErrore = "Descrizione errore : " + e.ToString();
                LogDelegati Log = new LogDelegati();
                //  Log.SaveLogPinProvisioning(codicefiscale, operatore, (Int16)LogEvents.Errore, message, 1, messageErrore, ipclient);
                reurncode = -1;
            }
            finally
            {
                if (con != null)
                    con.Close();


            }
            return reurncode;


        }


        public int UpdateInsertEccezione(WhiteListNumContatto listEccezione, String VecchioContatto)
        {
            int reurncode;

            try
            {
                string constring = ConfigurationManager.ConnectionStrings["SicurezzaPinProvisioning"].ToString();
                using (con = new SqlConnection(constring))
                {
                    using (SqlCommand cmd = new SqlCommand("[spPCRUDWhiteListContatto]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@contatto", listEccezione.Contatto);
                        cmd.Parameters.AddWithValue("@Vecchiocontatto", VecchioContatto);
                        cmd.Parameters.AddWithValue("@tipocontatto", listEccezione.TipoContatto);
                        cmd.Parameters.AddWithValue("@limite", listEccezione.Limite);
                        if (!String.IsNullOrEmpty(listEccezione.CodiceFiscaleSegnalazione))
                            cmd.Parameters.AddWithValue("@codiceFiscaleSegnalatore", listEccezione.CodiceFiscaleSegnalazione.ToUpper());
                        cmd.Parameters.AddWithValue("@note", listEccezione.Note);
                        cmd.Parameters.AddWithValue("@tipoOperazione", 'X');
                        cmd.Parameters.Add("@ReturnCode", SqlDbType.Int);
                        cmd.Parameters["@ReturnCode"].Direction = ParameterDirection.Output;
                        con.Open();

                        cmd.ExecuteNonQuery();
                        reurncode = (Int32)cmd.Parameters["@ReturnCode"].Value;

                        con.Close();

                        con.Close();
                        con = null;

                    }
                }
            }
            catch (Exception e)
            {

                //   string message = "Errore nella funzione SaveCertificatoDelegato SerialNum : " + SerialNum + " - codicefiscale : " + codicefiscale;
                string messageErrore = "Descrizione errore : " + e.ToString();
                LogDelegati Log = new LogDelegati();
                //  Log.SaveLogPinProvisioning(codicefiscale, operatore, (Int16)LogEvents.Errore, message, 1, messageErrore, ipclient);
                reurncode = -1;
            }
            finally
            {
                if (con != null)
                    con.Close();


            }
            return reurncode;


        }

        public int UpdateEccezione(WhiteListNumContatto listEccezione, String VecchioContatto)
        {
            int reurncode;

            try
            {
                string constring = ConfigurationManager.ConnectionStrings["SicurezzaPinProvisioning"].ToString();
                using (con = new SqlConnection(constring))
                {
                    using (SqlCommand cmd = new SqlCommand("[spPCRUDWhiteListContatto]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@contatto", listEccezione.Contatto);
                        cmd.Parameters.AddWithValue("@Vecchiocontatto", VecchioContatto);
                        if (!String.IsNullOrEmpty(listEccezione.CodiceFiscaleSegnalazione))
                            cmd.Parameters.AddWithValue("@codiceFiscaleSegnalatore", listEccezione.CodiceFiscaleSegnalazione.ToUpper());
                        cmd.Parameters.AddWithValue("@note", listEccezione.Note);
                        cmd.Parameters.AddWithValue("@tipoOperazione", 'U');
                        cmd.Parameters.Add("@ReturnCode", SqlDbType.Int);
                        cmd.Parameters["@ReturnCode"].Direction = ParameterDirection.Output;
                        con.Open();

                        cmd.ExecuteNonQuery();
                        reurncode = (Int32)cmd.Parameters["@ReturnCode"].Value;

                        con.Close();

                        con.Close();
                        con = null;

                    }
                }
            }
            catch (Exception e)
            {

                //   string message = "Errore nella funzione SaveCertificatoDelegato SerialNum : " + SerialNum + " - codicefiscale : " + codicefiscale;
                string messageErrore = "Descrizione errore : " + e.ToString();
                LogDelegati Log = new LogDelegati();
                //  Log.SaveLogPinProvisioning(codicefiscale, operatore, (Int16)LogEvents.Errore, message, 1, messageErrore, ipclient);
                reurncode = -1;
            }
            finally
            {
                if (con != null)
                    con.Close();


            }
            return reurncode;


        }
    }
}