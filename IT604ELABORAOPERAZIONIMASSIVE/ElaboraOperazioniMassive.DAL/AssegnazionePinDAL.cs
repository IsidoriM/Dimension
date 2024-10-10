using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Net;
using System.IO;
using System.Text;
using System.Configuration;


namespace ElaboraOperazioniMassive.DAL
{
    public class AssegnazionePinDAL
    {
        public string strAssPin = ConfigurationManager.ConnectionStrings["AssegnazionePin"].ToString();
        public SqlConnection conAssPin;
        public SqlCommand commAssPin;

        public void InserisciLog(string CodiceFiscale, int IdClasseUtente)
        {
            try
            {
                // Inserimento tabella di LOG
                conAssPin = new SqlConnection(strAssPin);
                commAssPin = new SqlCommand();
                commAssPin.Connection = conAssPin;
                commAssPin.CommandText = "spRichiestaPIN_Gestione_AddLog";
                commAssPin.CommandType = CommandType.StoredProcedure;
                conAssPin.Open();

                SqlParameter CodFisc = new SqlParameter("@CodiceFiscale", SqlDbType.NVarChar);
                CodFisc.Value = CodiceFiscale;
                SqlParameter Utente = new SqlParameter("@Utente", SqlDbType.NVarChar);
                Utente.Value = "PINONLINE.ElaboraDecessi";
                SqlParameter Esito = new SqlParameter("@Esito", SqlDbType.Int);
                Esito.Value = 0;
                SqlParameter DataRichiesta = new SqlParameter("@DataRichiesta", SqlDbType.DateTime);
                DataRichiesta.Value = System.DateTime.Now;
                SqlParameter IdEvento = new SqlParameter("@IdEvento", SqlDbType.Int);
                IdEvento.Value = 15;
                SqlParameter IdClasseUt = new SqlParameter("@IdClasseUtente", SqlDbType.Int);
                IdClasseUt.Value = IdClasseUtente;
                SqlParameter IPClient = new SqlParameter("@IPClient", SqlDbType.NVarChar);
                IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPClient.Value = ipAddress.ToString();
                SqlParameter RetCode = new SqlParameter("@RetCode", SqlDbType.Int);
                RetCode.Direction = ParameterDirection.Output;

                commAssPin.Parameters.Clear();

                commAssPin.Parameters.Add(CodFisc);
                commAssPin.Parameters.Add(Utente);
                commAssPin.Parameters.Add(Esito);
                commAssPin.Parameters.Add(DataRichiesta);
                commAssPin.Parameters.Add(IdEvento);
                commAssPin.Parameters.Add(IdClasseUt);
                commAssPin.Parameters.Add(IPClient);
                commAssPin.Parameters.Add(RetCode);
                commAssPin.ExecuteNonQuery();

            }
            catch (SqlException SqlEx)
            {
                throw SqlEx;
            }
            catch (Exception ex)
            {
                throw ex;

            }
            finally
            {
                conAssPin.Close();
                conAssPin.Dispose();
            }
        }
        public void insertLogDeceduti(string CodiceFiscale, int IdClasseUtente, string StatoRevoca, string MessRevoca)
        {
            try
            {
                conAssPin = new SqlConnection(strAssPin);
                commAssPin = new SqlCommand();
                commAssPin.Connection = conAssPin;
                commAssPin.CommandText = "spAddPinOnline_LogBatchDeceduti";
                commAssPin.CommandType = CommandType.StoredProcedure;
                conAssPin.Open();

                SqlParameter CodFisc = new SqlParameter("@CodiceFiscale", SqlDbType.NVarChar);
                CodFisc.Value = CodiceFiscale;
                SqlParameter Stato = new SqlParameter("@Stato", SqlDbType.NVarChar);
                Stato.Value = StatoRevoca;
                SqlParameter messaggio = new SqlParameter("@Messaggio", SqlDbType.NVarChar);
                messaggio.Value = MessRevoca;
                commAssPin.Parameters.Clear();
                commAssPin.Parameters.Add(CodFisc);
                commAssPin.Parameters.Add(Stato);
                commAssPin.Parameters.Add(messaggio);
                commAssPin.ExecuteNonQuery();

            }
            catch (SqlException SQLEx)
            {
                throw SQLEx;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            finally
            {
                conAssPin.Close();
                conAssPin.Dispose();
            }
        }

        //public string GetDataUltimaElaborazione()
        //{
        //    string data = DateTime.Now.AddDays(1).ToString(); ;
        //    try
        //    {
        //        // Inserimento tabella di LOG
        //        conAssPin = new SqlConnection(strAssPin);
        //        commAssPin = new SqlCommand();
        //        commAssPin.Connection = conAssPin;
        //        commAssPin.CommandText = "select top 1 CAST(DataOra AS date) from PinOnline_LogBatchDeceduti where Messaggio like 'Fine Elaborazione%' order by IDLogBatch desc";
        //        commAssPin.CommandType = CommandType.Text;
        //        conAssPin.Open();

        //        data = commAssPin.ExecuteScalar().ToString();
        //    }
        //    catch (SqlException SQLEx)
        //    {
        //        throw SQLEx;
        //    }

        //    catch (Exception ex)
        //    {
        //        data = DateTime.Now.AddDays(1).ToString();
        //        //throw ex;
        //    }
        //    finally
        //    {
        //        conAssPin.Close();
        //        conAssPin.Dispose();
        //    }
        //    return ConvertiData(data);
        //}
        //public string ConvertiData(string Data)
        //{
        //    // converte data in formato Formato AAAA-MM-GG
        //    string ritorno;
        //    try
        //    {
        //        int i = int.Parse(Data.Substring(6, 4));
        //        ritorno = String.Format("{0}-{1}-{2}", Data.Substring(6, 4), Data.Substring(3, 2), Data.Substring(0, 2));

        //    }
        //    catch
        //    {
        //        ritorno = String.Format("{0}-{1}-{2}", Data.Substring(0, 4), Data.Substring(5, 2), Data.Substring(8, 2));

        //    }
        //    return ritorno;
        //}
        public List<string> GetElencoCodiciEsclusi()
        {
            List<string> elencoCodiciEsclusi = new List<string>();
            try
            {
                conAssPin = new SqlConnection(strAssPin);
                commAssPin = new SqlCommand();
                commAssPin.Connection = conAssPin;
                commAssPin.CommandText = "spGetPinOnline_EsclusioneDecessi";
                commAssPin.CommandType = CommandType.StoredProcedure;
                conAssPin.Open();

                using (SqlDataReader reader = commAssPin.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        elencoCodiciEsclusi.Add(reader["CodiceFiscale"].ToString());
                    }
                }
            }
            catch (SqlException SQLEx)
            {
                throw SQLEx;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return elencoCodiciEsclusi;
        }
    }
}


