using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using PINProvUtilita.Models;

namespace PINProvUtilita.Controllers
{
    public class FunctionDB
    {
        SqlConnection con = null;

        public int SaveCertificatoDelegato(string SerialNum, string codicefiscale,string operatore,string ipclient)
        {
            int reurncode;

            try
            {
                string constring = ConfigurationManager.ConnectionStrings["SicurezzaPinProvisioning"].ToString();
                using (con = new SqlConnection(constring))
                {
                    using (SqlCommand cmd = new SqlCommand("[spECreateEntratelDelegato]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SerialNum", SerialNum);
                        cmd.Parameters.AddWithValue("@CodiceFiscale", codicefiscale.ToUpper());
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

                string message = "Errore nella funzione SaveCertificatoDelegato SerialNum : " + SerialNum + " - codicefiscale : " + codicefiscale;
                string messageErrore = "Descrizione errore : " + e.ToString();
                LogDelegati Log = new LogDelegati();
                Log.SaveLogPinProvisioning(codicefiscale, operatore, (Int16)LogEvents.Errore, message, 1, messageErrore, ipclient);
                reurncode = 1;
            }
            finally
            {
                if (con != null)
                    con.Close();


            }
            return reurncode;


        }

        public bool DeleteCertificatoDelegato(string SerialNum, string codicefiscale, string operatore, string ipclient)
        {
            bool save = false;

            try
            {
                string constring = ConfigurationManager.ConnectionStrings["SicurezzaPinProvisioning"].ToString();

                using (con = new SqlConnection(constring))
                {
                    using (SqlCommand cmd = new SqlCommand("[spEDeleteEntratelDelegato]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SerialNum", SerialNum);
                        cmd.Parameters.AddWithValue("@CodiceFiscale", codicefiscale);


                        con.Open();

                        cmd.ExecuteNonQuery();

                        con.Close();
                        con = null;
                        save = true;
                    }
                }
            }
            catch (Exception e)
            {
                string message = "Errore nella funzione DeleteCertificatoDelegato SerialNum : " + SerialNum + " - codicefiscale : " + codicefiscale;
                string messageErrore = "Descrizione errore : " + e.ToString();
                LogDelegati Log = new LogDelegati();
                Log.SaveLogPinProvisioning(codicefiscale, operatore, (Int16)LogEvents.Errore, message, 1, messageErrore, ipclient);
                save = false;
                
            }
            finally
            {
                if (con != null)
                    con.Close();


            }
            return save;


        }




        public List<Lista_Delegati> RicercaDelegaCF(string codicefiscale, string operatore, string ipclient)
        {
            List<Lista_Delegati> listaGetDelegati = new List<Lista_Delegati>();
            try
            {
                IDataReader reader = null;

                string constring = ConfigurationManager.ConnectionStrings["SicurezzaPinProvisioning"].ToString();

                using (con = new SqlConnection(constring))
                {
                    using (SqlCommand cmd = new SqlCommand("[spGetCertificatiPerDelega]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@cf", codicefiscale);


                        con.Open();
                        using (reader = cmd.ExecuteReader())
                        {
                            listaGetDelegati = DbMapper.PopulateEntities<Lista_Delegati>(reader);
                        }

                        con.Close();
                    }
                }



            }
            catch (Exception e)
            {

                //string message = "Errore nella funzione RicercaDelegaCF codicefiscale : " + codicefiscale;
                //string messageErrore = "Descrizione errore : " + e.ToString();
                //LogDelegati Log = new LogDelegati();
                //Log.SaveLogPinProvisioning(codicefiscale, operatore, (Int16)LogEvents.Errore, message, 1, messageErrore, ipclient);
                throw e;

            }
            finally
            {
                if (con != null)
                    con.Close();


            }

            return listaGetDelegati;
        }



        public List<Certificati> RicercaDelegaPIva(string codicefiscale,string operatore,string ipclient)
        {
            List<Certificati> listaGetCodiceFiscali = new List<Certificati>();
            try
            {
                IDataReader reader = null;

                string constring = ConfigurationManager.ConnectionStrings["SicurezzaPinProvisioning"].ToString();
                using (con = new SqlConnection(constring))
                {
                    using (SqlCommand cmd = new SqlCommand("[spEgetRicercaDelegatoPIva]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PartitaIva", codicefiscale);


                        con.Open();
                        using (reader = cmd.ExecuteReader())
                        {
                            listaGetCodiceFiscali = DbMapper.PopulateEntities<Certificati>(reader);
                        }

                        con.Close();
                        con = null;
                    }
                }



            }
            catch (Exception e)
            {
                //string message = "Errore nella funzione RicercaDelegaPIva codicefiscale : " + codicefiscale;
                //string messageErrore = "Descrizione errore : " + e.ToString();
                //LogDelegati Log = new LogDelegati();
                //Log.SaveLogPinProvisioning(codicefiscale, operatore, (Int16)LogEvents.Errore, message, 1, messageErrore, ipclient);
                throw e;

            }
            finally
            {
                if (con != null)
                    con.Close();


            }

            return listaGetCodiceFiscali;
        }




        public List<Certificati> RicercaEntratel(string codicefiscale,string operatore,string ipclient)
        {
            List<Certificati> listaGetDatiGrid = new List<Certificati>();
            try
            {
                IDataReader reader = null;

                string constring = ConfigurationManager.ConnectionStrings["SicurezzaPinProvisioning"].ToString();
                using (con = new SqlConnection(constring))
                {
                    using (SqlCommand cmd = new SqlCommand("[spGetCertificati]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@cf ", codicefiscale);


                        con.Open();
                        using (reader = cmd.ExecuteReader())
                        {
                            listaGetDatiGrid = DbMapper.PopulateEntities<Certificati>(reader);
                        }

                        con.Close();
                        con = null;
                    }
                }



            }
            catch (Exception e)
            {
                string message = "Errore nella funzione RicercaEntratel  codicefiscale : " + codicefiscale;
                string messageErrore = "Descrizione errore : " + e.ToString();
                LogDelegati Log = new LogDelegati();
                Log.SaveLogPinProvisioning(codicefiscale, operatore, (Int16)LogEvents.Errore, message, 1, messageErrore, ipclient);
 

            }
            finally
            {
                if (con != null)
                    con.Close();


            }

            return listaGetDatiGrid;
        }
    }
}