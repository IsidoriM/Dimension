using PinProvEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace UtenteDal
{
    public class UtenteContattiDal : Base
    {

        public UtenteStorico getUtenteContatti(String codiceFiscale)
        {
            UtenteStorico utente = new UtenteStorico();

            try
            {
                base.nomeSp = "spContGetContattiPersonali";

                SqlParameter parCodiceFiscale;
                parCodiceFiscale = base.CreaParametro("Utente", SqlDbType.Char, 0, true, codiceFiscale);

                using (SqlDataReader objSqlDataReader = base.ExecuteReader(base.connSicurezzaPinProvisioning, parCodiceFiscale))
                {
                    while (objSqlDataReader.Read())
                    {
                        utente.Cognome = objSqlDataReader["Cognome"].ToString();
                        utente.Nome = objSqlDataReader["Nome"].ToString();
                        utente.Utente = objSqlDataReader["Utente"].ToString();
                        utente.InfoPrivacy = objSqlDataReader["InfoPrivacy"].ToString();
                        utente.DataEmail = objSqlDataReader["DataCertificazioneEmail"].ToString();
                        utente.Email = objSqlDataReader["IndirizzoEmail"].ToString();
                        utente.DataPec = objSqlDataReader["DataCertificazionePEC"].ToString();
                        utente.Pec = objSqlDataReader["IndirizzoPEC"].ToString();
                        utente.StatoPec = objSqlDataReader["StatoVerificaPec"].ToString();
                        utente.DataCellulare = objSqlDataReader["DataCertificazioneCellulare"].ToString();
                        utente.Cellulare = objSqlDataReader["Cellulare"].ToString();
                        utente.TelefonoCasa = objSqlDataReader["TelefonoCasa"].ToString();




                    }
                }

                return utente;

            }
            catch (SqlException exSql)
            {
                dettaglioErrore.codiceErrore = -1;
                dettaglioErrore.descrizioneErrore = codiceFiscale + " - " + exSql.Message;
                //SaveLogPinProvisioning(codiceFiscale, operatore, (Int16)Evento.Errore, "spContGetContattiPersonali" + " " + dettaglioErrore.descrizioneErrore, 4000, exSql.ToString(), ipClient);

            }
            catch (Exception ex)
            {
                dettaglioErrore.codiceErrore = -2;
                dettaglioErrore.descrizioneErrore = ex.Message;
                //SaveLogPinProvisioning(codiceFiscale, operatore, (Int16)Evento.Errore, "spContGetContattiPersonali" + " " + dettaglioErrore.descrizioneErrore, 4000, ex.ToString(), ipClient);

            }


            return utente;
        }

        public List<UtenteContatti> getUtenteContattiStorico(String codiceFiscale)
        {
            
            List<UtenteContatti> utentestorico = new List<UtenteContatti>();
            try
            {
                base.nomeSp = "spContGetStoricoContattiPersonali";

                SqlParameter parCodiceFiscale;
                parCodiceFiscale = base.CreaParametro("Utente", SqlDbType.Char, 0, true, codiceFiscale);

                using (SqlDataReader objSqlDataReader = base.ExecuteReader(base.connSicurezzaPinProvisioning, parCodiceFiscale))
                {
                    while (objSqlDataReader.Read())
                    {
                        UtenteContatti utente = new UtenteContatti();

                        utente.DataStorico = objSqlDataReader["DataStorico"].ToString();
                        utente.DataUltimaModifica = objSqlDataReader["DataUltimaModifica"].ToString();
                        utente.Email = objSqlDataReader["IndirizzoEmail"].ToString();
                        utente.DataEmail = objSqlDataReader["DataCertificazioneEmail"].ToString();
                        utente.DataPec = objSqlDataReader["DataCertificazionePEC"].ToString();
                        utente.Pec = objSqlDataReader["IndirizzoPEC"].ToString();
                        utente.StatoPec = objSqlDataReader["StatoVerificaPec"].ToString();
                        utente.DataCell = objSqlDataReader["DataCertificazioneCellulare"].ToString();
                        utente.Cellulare = objSqlDataReader["Cellulare"].ToString();
                        utente.TelefonoCasa = objSqlDataReader["TelefonoCasa"].ToString();

                        utentestorico.Add(utente);

                    }
                }
                return utentestorico;
            }


            catch (SqlException SqlEx)
            {
                throw SqlEx;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
    