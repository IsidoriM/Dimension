using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Oracle.DataAccess;
using ElaboraOperazioniMassive.Entities;

namespace ElaboraOperazioniMassive.DAL
{
   public class controllerBase
    {
        
        /// <summary>
        /// Ottiene un'istanza del DB a cui connettersi.
        /// <example>
        ///     <![CDATA[this.GetDatabase<SqlDatabase>(myConnConfigName);]]>
        ///     <![CDATA[this.GetDatabase<OracleDatabase>(myConnConfigName);]]>
        /// </example>
        /// </summary>
        /// <typeparam name="T">Typo di database da creare.</typeparam>
        /// <param name="configName">Nome con il quale la stringa di connessione è stata inserita nella sezione ConnectionStrings del file di configurazione.</param>
        /// <returns>Un'istanza del <see cref="Database"/> voluto</returns>
        /// <exception cref="PinProvisioningException">Stringa di connessione non presente nel file di configurazione.;30101;null</exception>
        
            public virtual T GetDatabase<T>(string configName) where T : Database
            {
                var connString = ConfigurationManager.ConnectionStrings[configName];

                if (connString == null)
                {
                    throw new ElaboraOperazioniException(
                        @"La stringa di connessione """ + configName + @""" non presente nel file di configurazione.",
                        30101,
                        (int)LogEvents.Errore,
                        null);
                }

                return (T)Activator.CreateInstance(typeof(T), connString.ConnectionString);
            }

            /// <summary>
            /// Carica la descrione del ruolo passato come argomento.
            /// </summary>
            /// <param name="iamRole">Ruolo di interesse.</param>
            /// <returns>
            /// Descrizione del ruolo (Amministratore, Operatore ...),
            /// una stringa vuota se il ruolo non è stato trovato
            /// </returns>




            public virtual string LoadDescrizioneRuolo(string iamRole)
            {
                SqlDatabase database = this.GetDatabase<SqlDatabase>("SicurezzaPinProvisioning");
                const string Query = "spGetRuolo";

                try
                {
                    List<Ruolo> queryResult;

                    using (DbCommand command = database.GetStoredProcCommand(Query))
                    {
                        database.AddInParameter(command, "@CodiceOperatore", DbType.String, iamRole);

                        using (IDataReader reader = database.ExecuteReader(command))
                        {
                            queryResult = DbMapper.PopulateEntities<Ruolo>(reader);
                        }
                    }

                    return queryResult.Count > 0 ? queryResult[0].Descrizione : string.Empty;
                }
                catch (ElaboraOperazioniException)
                {
                    throw;
                }
                catch (Exception ex)
                {


                    throw new ElaboraOperazioniControllerException(
                        "[ElaboraDecessiControllerBase] " + ex.Message,
                        database.ConnectionString,
                        Query,
                        191,
                        (int)LogEvents.Errore,
                        ex.InnerException);
                }
            }

            /// <summary>
            /// Trova e restituisce un ruolo valido ed attivo per l'operatore corrente.
            /// </summary>
            /// <returns>Il ruolo dell'operatore se lo trova; una stringa vuota se non lo trova.</returns>
            [Obsolete]
            public virtual string LoadEnabledUserRole()
            {
                try
                {
                    // prendo tutti i ruoli posseduti dall'operatore
                    ////string[] operatorRules = ProfilazioneIam.Instance.LoadRuoli().Split('|');
                    // modifica di flavio assegno il ruolo = 2

                    //string[] operatorRules = this.LoadRuoliOperatore().Split('|');
                    string[] operatorRules = null;
                    // Fine modifica
                    // verifica se l'operatore ha il ruolo di "Operatore GAI" o "Operatore PIN".
                    if (operatorRules.Contains(Ruolo.OperatoreGai, StringComparer.OrdinalIgnoreCase))
                    {
                        operatorRules = new[] { Ruolo.OperatoreGai };
                    }
                    else if (operatorRules.Contains(Ruolo.OperatoreAssegnazionePin, StringComparer.OrdinalIgnoreCase))
                    {
                        operatorRules = new[] { Ruolo.OperatoreAssegnazionePin };
                    }

                    // prende tutti i ruoli abilitati per PinProvisioning
                    IEnumerable<Ruolo> activeRoles = this.LoadRuoliAttivi();

                    foreach (string operatorRule in from operatorRule in operatorRules
                                                    from activeRule in activeRoles
                                                    where
                                                        operatorRule.Equals(
                                                            activeRule.Codice,
                                                            StringComparison.OrdinalIgnoreCase)
                                                    select operatorRule)
                    {
                        return operatorRule;
                    }

                    return string.Empty;
                }
                catch (ElaboraOperazioniException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new ElaboraOperazioniControllerException(
                        "[ElaboraDecessiControllerBase] " + ex.Message,
                        194,
                        (int)LogEvents.Errore,
                        ex.InnerException);
                }
            }

            /// <summary>
            /// Carica una lista dei ruoli attivi disponibili per l'applicazione.
            /// </summary>
            /// <returns>Una lista dei ruoli disponibili.</returns>
            public virtual IEnumerable<Ruolo> LoadRuoliAttivi()
            {
                SqlDatabase database = this.GetDatabase<SqlDatabase>("SicurezzaPinProvisioning");
                const string Query = "spPGetRuoliAttivi";

                try
                {
                    using (IDataReader reader = database.ExecuteReader(CommandType.StoredProcedure, Query))
                    {
                        return DbMapper.PopulateEntities<Ruolo>(reader);
                    }
                }
                catch (ElaboraOperazioniException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new ElaboraOperazioniControllerException(
                        "[ElaboraDecessiControllerBase] " + ex.Message,
                        database.ConnectionString,
                        Query,
                        192,
                        (int)LogEvents.Errore,
                        ex.InnerException);
                }
            }

            /// <summary>
            /// Carica una lista dei ruoli attivi disponibili per l'applicazione codificati come stringa
            /// in cui i singoli ruoli sono separati da pipe.
            /// </summary>
            /// <returns>Una stringa con i ruoli attivi separati da pipe.</returns>
            public virtual string LoadRuoliAttiviString()
            {
                var ruoliAttivi = this.LoadRuoliAttivi();
                StringBuilder ruoliAttiviBuilder = new StringBuilder();

                foreach (var ruolo in ruoliAttivi)
                {
                    ruoliAttiviBuilder.Append(ruolo.Codice + '|');
                }

                if (ruoliAttiviBuilder.Length > 1)
                {
                    ruoliAttiviBuilder.Remove(ruoliAttiviBuilder.Length - 1, 1);
                }

                return ruoliAttiviBuilder.ToString();
            }

            /// <summary>
            /// Carica i ruoli dell'operatore. I ruoli sono restituiti in un'unica
            /// stringa separati da pipe.
            /// I ruoli ottenuti sono filtrati solo su quelli realmente utilizzati da PassiProvisioning.
            /// </summary>
            /// <returns>Una stringa con i ruoli dell'operatore separati da pipe.</returns>

            // da controllare se serve


            //public virtual string LoadRuoliOperatore()
            //{
            //    return ProfilazioneIam.Instance.LoadRuoli(this.LoadRuoliAttiviString());
            //}
            ///


            /// <summary>
            /// Carica un'istanza di <see cref="Ruolo"/> dell'operatore corrente
            /// utilizzando un codice operatore valido ed attivo per l'operatore corrente.
            /// Questo metodo andrebbe usato solo se il codice dell'operatore non è stato trovato nel
            /// cookie (In genere accade quando si accede da una pagina direttamente).
            /// </summary>
            /// <returns>
            /// Un'istanza con il ruolo dell'operatore se è stato trovato; altrimenti <c>null</c>.
            /// </returns>
            /// <exception cref="System.InvalidOperationException">[Controller Base: 111]  + ex.Message</exception>


            /// <summary>
            /// Contiene un set di metodi per gestire il ciclo di vita di una connessione ad un database oracle.
            /// </summary>
            internal sealed class OracleFactory
            {
                /// <summary>
                /// Istanza del factory.
                /// </summary>
                public static readonly OracleFactory Instance = new OracleFactory();

                /// <summary>
                /// Creates the command.
                /// </summary>
                /// <param name="connection">The connection.</param>
                /// <param name="type">The type.</param>
                /// <returns></returns>
                public DbCommand CreateCommand(DbConnection connection, CommandType type)
                {
                    DbCommand command = new Oracle.DataAccess.Client.OracleCommand { CommandType = type, BindByName = true };

                    command.Connection = connection;

                    return command;
                }

                /// <summary>
                /// Creates the command.
                /// </summary>
                /// <param name="connection">The connection.</param>
                /// <param name="type">The type.</param>
                /// <param name="text">The text.</param>
                /// <returns></returns>
                public DbCommand CreateCommand(DbConnection connection, CommandType type, string text)
                {
                    DbCommand command = new Oracle.DataAccess.Client.OracleCommand { CommandType = type, CommandText = text, BindByName = true };

                    command.Connection = connection;

                    return command;
                }

                /// <summary>
                /// Instanzia una nuova connessione verso un Database.
                /// </summary>
                /// <param name="value">value è una stringa che a seconda del valore assunto dal parametro
                /// <c>useConfigFile</c> è considerata come stringa di connessione o come chiave del file
                /// di configurazione in cui si trova la stringa di connsessione.</param>
                /// <param name="useConfigFile">Se <c>true</c> la stringa di connessione sarà cercata 
                /// nel file di configurazione.</param>
                /// <returns>Un'oggetto che rappresenta una connessione verso un database.</returns>
                public DbConnection CreateConnection(string value, bool useConfigFile)
                {
                    if (!useConfigFile)
                    {
                        return this.CreateConnection(value);
                    }

                    var connString = ConfigurationManager.ConnectionStrings[value];

                    if (connString == null)
                    {
                        throw new ElaboraOperazioniException(
                            @"La stringa di connessione """ + value + @""" non presente nel file di configurazione.",
                            30101,
                            (int)LogEvents.Errore,
                            null);
                    }

                    return this.CreateConnection(connString.ConnectionString);
                }

                /// <summary>
                /// Crea una nuova connessione verso un Database per la connessione è utilizzata la stringa
                /// di connessione indicata come parametro.
                /// </summary>
                /// <param name="connectionString">La stringa di connessione.</param>
                /// <returns>Un'oggetto che rappresenta una connessione verso un database.</returns>
                public DbConnection CreateConnection(string connectionString)
                {
                    return new Oracle.DataAccess.Client.OracleConnection (connectionString);
                }

                /// <summary>
                /// Crea un nuovo parametro di input.
                /// </summary>
                /// <param name="parameterName">Nome del parametro.</param>
                /// <param name="type">Tipo di dato del parametro.</param>
                /// <param name="value">Valore del parametro.</param>
                /// <returns>Un oggetto che rappresenta il parametro.</returns>
                public DbParameter CreateInParameter(string parameterName, DbType type, object value)
                {
                    return this.CreateParameter(parameterName, type, ParameterDirection.Input, value);
                }

                /// <summary>
                /// Crea un nuovo parametro di output.
                /// </summary>
                /// <param name="parameterName">Nome del parametro.</param>
                /// <param name="type">Tipo di dato del parametro.</param>
                /// <returns>Un oggetto che rappresenta il parametro.</returns>
                public DbParameter CreateOutParameter(string parameterName, DbType type)
                {
                    return this.CreateParameter(parameterName, type, ParameterDirection.Output);
                }

                /// <summary>
                /// Crea un nuovo parametro.
                /// </summary>
                /// <param name="parameterName">Nome del parametro.</param>
                /// <param name="type">Tipo di dato del parametro.</param>
                /// <param name="direction">Direzione del parametro.</param>
                /// <returns>Un oggetto che rappresenta il parametro.</returns>
                public DbParameter CreateParameter(string parameterName, DbType type, ParameterDirection direction)
                {
                    return new Oracle.DataAccess.Client.OracleParameter { ParameterName = parameterName, DbType = type, Direction = direction };
                }

                /// <summary>
                /// Crea un nuovo parametro.
                /// </summary>
                /// <param name="parameterName">Nome del parametro.</param>
                /// <param name="type">Tipo di dato del parametro.</param>
                /// <param name="direction">Direzione del parametro.</param>
                /// <param name="value">Valore del parametro.</param>
                /// <returns>Un oggetto che rappresenta il parametro.</returns>
                public DbParameter CreateParameter(
                    string parameterName,
                    DbType type,
                    ParameterDirection direction,
                    object value)
                {
                    return new Oracle.DataAccess.Client.OracleParameter (parameterName, value) { DbType = type, Direction = direction };
                }
            }
        }
    }

