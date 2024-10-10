using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using ElaboraOperazioniMassive.DAL;

namespace ElaboraOperazioniMassive.Operazioni
{
    public class Revoca
    {
        public int rimozione(DataRow row)
        {
            //Mi carico il servizio se è valorizzato idservizio
            //se è valorizzato idgrupposervizio nelle operazionimassiveautorizzazioni carico i servizi di quel gruppo servizi 
            //altrimenti li carico tutti
            Console.WriteLine("select della lista servizi da aggiornare su ldap" + row["codEnte"].GetType());
            SicurezzaDAL sicurezzadal = new SicurezzaDAL();
            //estrae a partire da utente e codice ente GDP i servizi che vanno revocati su Oracle/LDAP
            DataSet listaservizi = sicurezzadal.GetIdServiziUtenteEnte(row["codEnte"].ToString(), row["Utente"].ToString(), row["idGruppoServizi"].ToString(), row["idServizio"].ToString());

            if (listaservizi.Tables.Count != 0)
            {
                foreach (DataTable table in listaservizi.Tables)
                {
                    foreach (DataRow servizio in table.Rows)
                    {
                        Console.WriteLine(servizio["idServizio"].ToString());
                        Console.WriteLine(servizio["CodRuoloGDP"].ToString());
                    }
                }
            }
            int erroreservizio = 0;
            int errorecancellazione = 0;
            bool serv = false;
            bool cancellazione = false;
            bool sicurezza = false;
            var profiloGdp = new TransazioniOracle();
            if (listaservizi.Tables.Count != 0)
            {
                profiloGdp.Open();
            }
            var utenteGdp = new TransazioniOracle();
            if (listaservizi.Tables.Count != 0)
            {
                utenteGdp.Open();
            }

            // se deve essere cancellato o  gruppo servizio o idservizio
            //se cancellato gruppo servizi non essendoci altri gruppi servizi disabilito il profilo
            if (listaservizi.Tables.Count != 0)
            {
                if (row["idGruppoServizi"].ToString() != null || row["idServizio"].ToString() != null || row["codEnte"].ToString() != null)
                {
                    foreach (DataTable table in listaservizi.Tables)
                    {
                        if (table.Rows.Count > 0)
                        {
                            // aggiornamento su db Oracle
                            try
                            {
                                foreach (DataRow servizio in table.Rows)
                                {
                                    //Console.WriteLine("20201026righetabella" + table.Rows.Count.ToString());
                                    erroreservizio = profiloGdp.DeleteServizio(row["Utente"].ToString(), row["codEnte"].ToString(), servizio["CodRuoloGDP"].ToString());

                                    if (erroreservizio == -1)
                                    {
                                        serv = true;
                                    }

                                }

                                // dopo aver cancellato i servizi se non ci sono più autorizzazioni elimino utente


                                int numeroautorizzazioni = utenteGdp.ContaAutorizzazioni(row["Utente"].ToString());
                                //Console.WriteLine("20201026numeroautorizzazioni" + numeroautorizzazioni.ToString());
                                //da chiedere laura
                                if (numeroautorizzazioni == 0 && (row["RimozioneUtente"].ToString().Trim() == "1"))
                                {

                                    errorecancellazione = utenteGdp.CancellaUtente(row["Utente"].ToString());
                                    if (errorecancellazione == -1)
                                    {
                                        //Console.WriteLine("20201026erroreRimizioneUtenteGdp" + errorecancellazione.ToString());
                                        cancellazione = true;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Errore cancellazione Utente Gdp: " + ex.ToString());
                            }
                        }
                    }
                }
            }
            //Se per l'operazione è prevista la rimozione dell'utente
            // da chiedere laura 
            /*      if (row["RimozioneUtente"].ToString().Trim() == "1")
                    {
                        errorecancellazione = utenteGdp.CancellaUtente(row["Utente"].ToString());
                        if (errorecancellazione == -1)
                        {
                            cancellazione = true;
                        }
                    }
            */
            //se non ci sono stati errori
            string EsitoRevocaSicurezzaMP = "";
            //retcode 1 = Inserimento effettuato in profiloserviziorevocato
            //retcode 2 = Profiloservizio non presente per utente,codente, idservizio
            //retcode 3 = Revoca idgruppo servizi
            //retcode 4 = profilogs non esistene per utente,codente, idservizio
            //retcode 5 = effettuata sia revoca idgrupposervizi che revoca del profilo
            //retcode 6 = Revoca intero profilo per utente e codice ente
            //retcode 7 = Rrevoca del profilo e cancellazione Utente

             if (row["idGruppoServizi"].ToString() != null || row["idServizio"].ToString() != null || row["codEnte"].ToString() != null)
                {
                    if (serv == false && cancellazione == false)
                    {
                        //Aggiornamento su sql
                        //Console.WriteLine("20201026numeroautorizzazioni" + "inizio cancellazione SQL");
                        string strSQL = ConfigurationManager.ConnectionStrings["SicurezzaMP"].ToString();
                        SqlConnection conStringSQL = new SqlConnection(strSQL);
                        conStringSQL.Open();
                        //Console.WriteLine("20201026numeroautorizzazioni" + " connessione aperta");
                        SqlParameter codEnte = new SqlParameter("@vcodEnte", SqlDbType.NVarChar);
                        codEnte.Value = row["codEnte"];
                        SqlParameter Utente = new SqlParameter("@vUtente", SqlDbType.NVarChar);
                        Utente.Value = row["Utente"];
                        SqlParameter idServizio = new SqlParameter("@vidServizio", SqlDbType.Int);
                        idServizio.Value = row["idServizio"];
                        SqlParameter IPClient = new SqlParameter("@vipClient", SqlDbType.NVarChar);
                        IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                        IPAddress ipAddress = ipHostInfo.AddressList[0];
                        IPClient.Value = ipAddress.ToString();
                        SqlParameter idGS = new SqlParameter("@vGServizi", SqlDbType.Int);
                        idGS.Value = row["idGruppoServizi"];
                        SqlParameter rimozioneUtente = new SqlParameter("@vrimozioneUtente", SqlDbType.Bit);
                        rimozioneUtente.Value = row["RimozioneUtente"];
                        SqlParameter parametriPersonalizzati = new SqlParameter("@vparametri", SqlDbType.NVarChar);
                        parametriPersonalizzati.Value = row["Parametri"];
                        SqlParameter retcode = new SqlParameter("@vretCode", SqlDbType.Int);
                        retcode.Direction = ParameterDirection.Output;
                        Console.WriteLine(row["idServizio"].ToString());


                        try
                        {
                            SqlCommand cmd = new SqlCommand("spOM_Revoca", conStringSQL);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add(codEnte);
                            cmd.Parameters.Add(Utente);
                            cmd.Parameters.Add(idServizio);
                            cmd.Parameters.Add(IPClient);
                            cmd.Parameters.Add(idGS);
                            cmd.Parameters.Add(retcode);
                            cmd.Parameters.Add(parametriPersonalizzati);
                            cmd.Parameters.Add(rimozioneUtente);
                            cmd.ExecuteNonQuery();
                            EsitoRevocaSicurezzaMP = cmd.Parameters["@vretCode"].Value.ToString();

                            //Console.WriteLine("20201026connessione" + conStringSQL.ToString());
                        }

                        catch (Exception ex)
                        {
                            throw ex;
                            //Console.WriteLine("20201026erroreSql" + ex.ToString());
                        }

                        finally
                        {
                            conStringSQL.Close();
                            conStringSQL.Dispose();
                        }


                        // se aggiornamento su sicurezza non è andato a buon fine rollback 
                        if (sicurezza == false)
                        {

                            //aggiornamento su ldap
                            try
                            {

                                if (int.Parse(EsitoRevocaSicurezzaMP) == 1 || int.Parse(EsitoRevocaSicurezzaMP) == 3 || int.Parse(EsitoRevocaSicurezzaMP) == 5 || int.Parse(EsitoRevocaSicurezzaMP) == 6)
                                {
                                    foreach (DataTable t in listaservizi.Tables)
                                    {

                                        foreach (DataRow servizio in t.Rows)
                                        {
                                            var ldap = new TransazioniOracle();
                                            //Console.WriteLine("20201026ldapprimaDELETE");
                                            ldap.deleteservizioLdap(row["Utente"].ToString(), servizio["CodRuoloGDP"].ToString(), servizio["CodApplicazioneGDP"].ToString());
                                        }
                                    }

                                }

                                if (int.Parse(EsitoRevocaSicurezzaMP) == 7)
                                {
                                    var ldap = new TransazioniOracle();
                                    bool pro = ldap.DeleteUtenteLdap(row["Utente"].ToString());
                                }
                                //Console.WriteLine("20201026commit");
                                if (listaservizi.Tables.Count != 0)
                                {
                                    profiloGdp.Commit();
                                    profiloGdp.Close();
                                    utenteGdp.Commit();
                                    utenteGdp.Close();
                                }
                                return 1;
                            }
                            catch (Exception Ex)
                            {
                                Console.WriteLine("Errore cancellazione Ldap: " + Ex.ToString());
                            }
                        }
                        //se non sono andati a buon fine aggioramenti su oracle faccio rollback
                        else
                        {
                            //Console.WriteLine("20201026commitOracle");
                            if (listaservizi.Tables.Count != 0)
                            {
                                profiloGdp.Rollback();
                                profiloGdp.Close();
                                utenteGdp.Rollback();
                                utenteGdp.Close();
                                return -1;
                            }
                        }
                    }
                    else
                    {
                        if (listaservizi.Tables.Count != 0)
                        {
                            profiloGdp.Rollback();
                            profiloGdp.Close();
                            utenteGdp.Rollback();
                            utenteGdp.Close();
                            return -1;
                        }
                    }
                    //return -1;
                }

             return 1;            
            ///////////////////////////////////////////////////7
        }
    }
}
