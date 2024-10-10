using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

namespace TestMenuEnteMvc.Class
{
    public class ControllerBase
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
                //throw new PinProvisioningException(
                //    @"La stringa di connessione """ + configName + @""" non presente nel file di configurazione.",
                //    30101,
                //    (int)LogEvents.Errore,
                //    null);
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
                List<TestMenuEnteMvc.Class.Ruolo> queryResult;

                using (DbCommand command = database.GetStoredProcCommand(Query))
                {
                    database.AddInParameter(command, "@CodiceOperatore", DbType.String, iamRole);

                    using (IDataReader reader = database.ExecuteReader(command))
                    {
                        queryResult = TestMenuEnteMvc.Class.DbMapper.PopulateEntities<TestMenuEnteMvc.Class.Ruolo>(reader);
                    }
                }

                return queryResult.Count > 0 ? queryResult[0].Descrizione : string.Empty;
            }
            catch (Exception ex)
            {
                throw;
            }
            //catch (Exception ex)
            //{
            //    throw new PinProvisioningControllerException(
            //        "[PinProvisioningControllerBase] " + ex.Message,
            //        database.ConnectionString,
            //        Query,
            //        191,
            //        (int)LogEvents.Errore,
            //        ex.InnerException);
            //}
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
                string[] operatorRules = this.LoadRuoliOperatore().Split('|');

                // verifica se l'operatore ha il ruolo di "Operatore GAI" o "Operatore PIN".
                if (operatorRules.Contains(TestMenuEnteMvc.Class.Ruolo.OperatoreGai, StringComparer.OrdinalIgnoreCase))
                {
                    operatorRules = new[] { TestMenuEnteMvc.Class.Ruolo.OperatoreGai };
                }
                else if (operatorRules.Contains(TestMenuEnteMvc.Class.Ruolo.OperatoreAssegnazionePin, StringComparer.OrdinalIgnoreCase))
                {
                    operatorRules = new[] { TestMenuEnteMvc.Class.Ruolo.OperatoreAssegnazionePin };
                }

                // prende tutti i ruoli abilitati per PinProvisioning
                IEnumerable<TestMenuEnteMvc.Class.Ruolo> activeRoles = this.LoadRuoliAttivi();

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
            catch (Exception ex)
            {
                throw;
            }
            //catch (Exception ex)
            //{
            //    throw new PinProvisioningControllerException(
            //        "[PinProvisioningControllerBase] " + ex.Message,
            //        194,
            //        (int)LogEvents.Errore,
            //        ex.InnerException);
            //}
        }

        /// <summary>
        /// Carica una lista dei ruoli attivi disponibili per l'applicazione.
        /// </summary>
        /// <returns>Una lista dei ruoli disponibili.</returns>
        public virtual IEnumerable<TestMenuEnteMvc.Class.Ruolo> LoadRuoliAttivi()
        {
            SqlDatabase database = this.GetDatabase<SqlDatabase>("SicurezzaPinProvisioning");
            const string Query = "spPGetRuoliAttivi";
            using (IDataReader reader = database.ExecuteReader(CommandType.StoredProcedure, Query))
            try
            {
               
                {
                    return TestMenuEnteMvc.Class.DbMapper.PopulateEntities<TestMenuEnteMvc.Class.Ruolo>(reader);
                }
            }
            //catch (PinProvisioningException)
            //{
            //    throw;
            //}
            catch (Exception ex)
            {
                //throw new PinProvisioningControllerException(
                //    "[PinProvisioningControllerBase] " + ex.Message,
                //    database.ConnectionString,
                //    Query,
                //    192,
                //    (int)LogEvents.Errore,
                //    ex.InnerException);
                return TestMenuEnteMvc.Class.DbMapper.PopulateEntities<TestMenuEnteMvc.Class.Ruolo>(reader);
                //return false;
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
        public virtual string LoadRuoliOperatore()
        {
            return ProfilazioneIam.Instance.LoadRuoli(this.LoadRuoliAttiviString());
        }

        /// <summary>
        /// Contiene un set di metodi per gestire il ciclo di vita di una connessione ad un database oracle.
        /// </summary>
        internal sealed class OracleFactory
        {
            /// <summary>
            /// Istanza del factory.
            /// </summary>
            public static readonly OracleFactory Instance = new OracleFactory();

        }
    }
}