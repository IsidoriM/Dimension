using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Transactions;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using ElaboraOperazioniMassive.Entities;



namespace ElaboraOperazioniMassive.DAL
{
    public class TransazioniOracle : controllerBase
    {
        // TODO: va predisposta una configurazione tramite web.config delle costanti che riguardano LDAP.
        // modifica momentanea  flavio da capire cosa mettere come cod fiscale operatore essendo un batch

        //private readonly string codFisOperatore = ProfilazioneIam.Instance.LoadCodiceFiscaleOperatore();
        private readonly string codFisOperatore = "codFisOperatore";
        // fine modifica
        private readonly string container = ConfigurationManager.AppSettings["container"];

        private readonly string host = ConfigurationManager.AppSettings["host"];

        private readonly string password = @ConfigurationManager.AppSettings["password"];

        private readonly string port = ConfigurationManager.AppSettings["port"];

        private readonly string username = @ConfigurationManager.AppSettings["username"];

        /// <summary>
        /// Riferimento al Command per Oracle.
        /// </summary>
        private DbCommand oraCommand;

        /// <summary>
        /// Riferimento al Command per SqlServer.
        /// </summary>
        private DbCommand sqlCommand;
        /// <summary>
        /// Inizializza la classe <see cref="AziendaGdp"/>.
        /// Questo costruttore di default apre automaticamente le connessioni verso il DB (SicurezzaMP e Oracle).
        /// Il ciclo di vita delle connessioni è gestito internamente e lo stato può essere cambiato tramite i metodi
        /// <see cref="Open"/>, <see cref="Close"/> e <see cref="Commit"/>.
        /// </summary>
        /// 
        /// Inizializza la classe <see cref="AziendaGdp"/>.
        /// Questo costruttore di default apre automaticamente le connessioni verso il DB (SicurezzaMP e Oracle).
        /// Il ciclo di vita delle connessioni è gestito internamente e lo stato può essere cambiato tramite i metodi
        /// <see cref="Open"/>, <see cref="Close"/> e <see cref="Commit"/>.
        /// </summary>
        public void Rollback()
        {
            if (this.oraCommand.Transaction != null)
            {
                this.oraCommand.Transaction.Rollback();
            }
        }
        public void Close()
        {
            // Gestione connessione verso oracle
            if (this.oraCommand != null)
            {
                if (this.oraCommand.Transaction != null)
                {
                    this.oraCommand.Transaction.Rollback();
                }

                if (this.oraCommand.Connection.State != ConnectionState.Closed)
                {
                    this.oraCommand.Connection.Close();
                }

                this.oraCommand.Dispose();
                this.oraCommand = null;
            }

        }
        public void Commit()
        {
            // Rende definitive le modifiche apportate al DB. una volta invocate le transazioni diventano null.
            this.oraCommand.Transaction.Commit();
            //this.sqlCommand.Transaction.Commit();
        }
        /// <summary>
        /// Inizializza la classe <see cref="Apri"/>.
        /// Questo costruttore di default apre automaticamente le connessioni verso il DB (SicurezzaMP e Oracle).
        /// Il ciclo di vita delle connessioni è gestito internamente e lo stato può essere cambiato tramite i metodi
        /// <see cref="Open"/>, <see cref="Close"/> e <see cref="Commit"/>.
        /// </summary>
        public void Apri()
        {
            this.Open();
        }
        public void Open()
        {
            // Apertura connessioni Oracle.
            if (this.oraCommand == null)
            {
                var conn = OracleFactory.Instance.CreateConnection("AziendaGDP", true);

                conn.Open();

                this.oraCommand = OracleFactory.Instance.CreateCommand(conn, CommandType.Text);
            }

            if (this.oraCommand.Transaction == null)
            {
                this.oraCommand.Transaction = this.oraCommand.Connection.BeginTransaction();
            }

            //if (this.sqlCommand == null)
            //{
            //    var sqlConn =
            //        new SqlConnection(
            //            ConfigurationManager.ConnectionStrings["SicurezzaPinProvisioning"].ConnectionString);

            //    sqlConn.Open();

            //    this.sqlCommand = new SqlCommand
            //    {
            //        Connection = sqlConn,
            //        CommandType = CommandType.StoredProcedure,
            //    };
            //}

            //if (this.sqlCommand.Transaction == null)
            //{
            //    this.sqlCommand.Transaction = this.sqlCommand.Connection.BeginTransaction();
            //}
        }
        private int GetExistingUserPrimaryKey(string idUtente, bool checkDate)
        {
            this.oraCommand.Parameters.Clear();
            this.oraCommand.CommandText = @"SELECT IMUTE_SEQ_UTENTE_PK AS PK
                                                FROM SIDBA.TB_IMUTE_UTENTE_CL
                                                WHERE IMUTE_USERID_UTENTE=:userID";

            if (checkDate)
            {
                this.oraCommand.CommandText += @" AND IMUTE_DATA_FINE=TO_DATE('31-12-9999','dd-mm-yyyy')";
            }

            this.oraCommand.Parameters.Add(
                OracleFactory.Instance.CreateInParameter(":userID", DbType.String, idUtente));

            using (var reader = this.oraCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    // Contiene l'id dell'utente se già presente
                    return Convert.ToInt32(reader["PK"]);
                }
            }

            return -1;
        }
        private int GetAziendaPrimaryKey(string codEnte)
        {
            this.oraCommand.Parameters.Clear();
            //modifica del 25052016 Flavio Giovannini elimino il controllo sulla data
            //                this.oraCommand.CommandText = @"SELECT IMUNI_SEQ_UNITA_PK AS PK
            //                                                FROM SIDBA.TB_IMUNI_UNITCOMP_CL
            //                                                WHERE IMUNI_COD_UNITA=:codEnte
            //                                                        --AND IMUNI_IMTUN_COD_TIPOUNITA IN (4, 6)
            //                                                        AND IMUNI_IMTUN_COD_TIPOUNITA IN (4)
            //                                                        AND IMUNI_DATA_FINE = TO_DATE('31/12/9999', 'DD/MM/YYYY')";

            this.oraCommand.CommandText = @"SELECT IMUNI_SEQ_UNITA_PK AS PK
                                                FROM SIDBA.TB_IMUNI_UNITCOMP_CL
                                                WHERE IMUNI_COD_UNITA=:codEnte
                                                        --AND IMUNI_IMTUN_COD_TIPOUNITA IN (4, 6)
                                                        AND IMUNI_IMTUN_COD_TIPOUNITA IN (4)";


            this.oraCommand.Parameters.Add(
                OracleFactory.Instance.CreateInParameter(":codEnte", DbType.String, codEnte));

            using (var reader = this.oraCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    return Convert.ToInt32(reader["PK"]);
                }
            }

            return -1;
        }
        private int GetRuolo(string codgdpservizio)
        {
            this.oraCommand.Parameters.Clear();

            this.oraCommand.CommandText = @"SELECT IMRUO_COD_RUOLO_PK as PK FROM SIDBA.TB_IMRUO_RUOLO_CT WHERE IMRUO_COD_RUOLO_UTENTE=:ruolo";

            this.oraCommand.Parameters.Add(
                OracleFactory.Instance.CreateInParameter(":ruolo", DbType.String, codgdpservizio));

            using (var reader = this.oraCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    return Convert.ToInt32(reader["PK"]);
                }
            }

            return -1;

        }
        public int DeleteServizio(string idUtente, string codEnte, string codservizioGDP)
        {
            //cancellazione del singolo gruppo servizio
            int utentePk = this.GetExistingUserPrimaryKey(idUtente, true);
            int aziendaPk = this.GetAziendaPrimaryKey(codEnte);
            int ruoloPk = this.GetRuolo(codservizioGDP);
            int ritorno = -1;

            if ((aziendaPk != -1) && (utentePk != -1))
            {


                try
                {
                    this.oraCommand.Parameters.Clear();
                    this.oraCommand.CommandText = @"UPDATE SIDBA.TB_IMAIN_ABILITAZIONEI_CL
                                          SET IMAIN_DATA_FINE=CURRENT_DATE,
                                          IMAIN_DATA_AGGIORN=CURRENT_DATE,
                                          IMAIN_COD_UTENTE='Automatico'
                                          WHERE IMAIN_IMRUO_COD_RUOLO_PK=:ruoloPK
                                          AND IMAIN_IMUTE_SEQ_UTENTE_PK=:utentePK
                                          AND IMAIN_IMUNI_SEQ_UNITA_PK =:aziendaPk
                                          AND IMAIN_DATA_FINE=TO_DATE('31-12-9999','dd-mm-yyyy')";
                    this.oraCommand.Parameters.Add(
                           OracleFactory.Instance.CreateInParameter(":utentePk", DbType.Int32, utentePk));
                    this.oraCommand.Parameters.Add(
                        OracleFactory.Instance.CreateInParameter(":aziendaPk", DbType.Int32, aziendaPk));

                    this.oraCommand.Parameters.Add(
                        OracleFactory.Instance.CreateInParameter(":ruoloPk", DbType.Int32, ruoloPk));

                    this.oraCommand.ExecuteNonQuery();

                    ritorno = 1;

                }

                catch
                {
                    ritorno = -1;
                }


            }


            return ritorno;


        }
        public int ContaAutorizzazioni(String idUtente)
        {
            int utentePk = this.GetExistingUserPrimaryKey(idUtente, true);
            ElaboraOperazioniMassive.DAL.AssegnazionePinDAL log = new ElaboraOperazioniMassive.DAL.AssegnazionePinDAL();

            this.oraCommand.Parameters.Clear();
            this.oraCommand.CommandText = @"SELECT COUNT(*) AS COUNT FROM SIDBA.TB_IMAIN_ABILITAZIONEI_CL
                           WHERE IMAIN_IMUTE_SEQ_UTENTE_PK=:utentePK
                           AND IMAIN_DATA_FINE = TO_DATE('31-12-9999','dd-mm-yyyy')";

            this.oraCommand.Parameters.Add(
                OracleFactory.Instance.CreateInParameter(":utentePK", DbType.Int32, utentePk));


            using (var reader = this.oraCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    return Convert.ToInt32(reader["COUNT"]);
                }
            }

            return -1;

        }
        public int CancellaUtente(string idUtente)
        {

            int utentePk = this.GetExistingUserPrimaryKey(idUtente, true);
            // ElaboraOperazioniMassive.DAL.AssegnazionePinDAL log = new ElaboraOperazioniMassive.DAL.AssegnazionePinDAL();
            try
            {
                //Console.WriteLine("20201026cancellaoracle");

                this.DeleteAllGruppiServizi(utentePk);
                this.DeleteUtenteTipo(utentePk);
                this.oraCommand.Parameters.Clear();
                this.oraCommand.CommandText = @"UPDATE SIDBA.TB_IMUTE_UTENTE_CL
                                                SET IMUTE_DATA_AGGIORN=CURRENT_DATE,
                                                    IMUTE_DATA_FINE=CURRENT_DATE,
                                                    IMUTE_COD_UTENTE='Automatico'
                                                WHERE IMUTE_SEQ_UTENTE_PK=:utentePK
                                                      AND IMUTE_DATA_FINE = TO_DATE('31-12-9999','dd-mm-yyyy')";

                this.oraCommand.Parameters.Add(
                    OracleFactory.Instance.CreateInParameter(":utentePK", DbType.Int32, utentePk));


                this.oraCommand.ExecuteNonQuery();
                return 1;
            }
            catch
            {


                return -1;

            }
        }
        private int DeleteUtenteTipo(int utentePk)
        {
            DateTime? dataInizio = null;
            try
            {
                this.oraCommand.Parameters.Clear();
                this.oraCommand.CommandText =
                    @"SELECT IMUTU_DATA_INIZ_PK AS DataInizio FROM SIDBA.TB_IMUTU_UTENTETIPO_CL
                      WHERE --IMUTU_IMTUT_COD_TIPOUTENTE_PK=3 AND 
                            IMUTU_IMUTE_SEQ_UTENTE_PK=:utentePK 
                            AND IMUTU_DATA_FINE=TO_DATE('31-12-9999','dd-mm-yyyy')";

                this.oraCommand.Parameters.Add(
                    OracleFactory.Instance.CreateInParameter(":utentePK", DbType.Int32, utentePk));

                using (var reader = this.oraCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        dataInizio = Convert.ToDateTime(reader["DataInizio"]);
                    }
                }

                if (dataInizio == null)
                {

                    return -1;
                }

                this.oraCommand.Parameters.Clear();
                this.oraCommand.CommandText = @"UPDATE SIDBA.TB_IMUTU_UTENTETIPO_CL
                                                SET IMUTU_DATA_FINE=CURRENT_DATE,
		                                            IMUTU_DATA_AGGIORN=CURRENT_DATE,
		                                            IMUTU_COD_UTENTE=:operatoreCF
                                                WHERE --IMUTU_IMTUT_COD_TIPOUTENTE_PK=3 AND 
                                                        IMUTU_IMUTE_SEQ_UTENTE_PK=:utentePK 
                                                        AND IMUTU_DATA_FINE=TO_DATE('31-12-9999','dd-mm-yyyy')";

                this.oraCommand.Parameters.Add(
                    OracleFactory.Instance.CreateInParameter(":utentePK", DbType.Int32, utentePk));
                this.oraCommand.Parameters.Add(
                    OracleFactory.Instance.CreateInParameter(":operatoreCF", DbType.String, this.codFisOperatore));

                this.oraCommand.ExecuteNonQuery();
                return 0;
            }
            catch
            {
                return -1;
            }
        }
        private int DeleteAllGruppiServizi(int utentePk)
        {


            try
            {
                this.oraCommand.Parameters.Clear();

                this.oraCommand.CommandText = @"UPDATE SIDBA.TB_IMAIN_ABILITAZIONEI_CL S
                                                SET S.IMAIN_DATA_FINE = CURRENT_DATE,
                                                    S.IMAIN_COD_UTENTE = 'Automatico',
                                                    S.IMAIN_DATA_AGGIORN = CURRENT_DATE
                                                WHERE S.IMAIN_IMUTE_SEQ_UTENTE_PK = :utentePk
                                                      AND S.IMAIN_DATA_FINE = TO_DATE('31/12/9999', 'DD/MM/YYYY')";


                this.oraCommand.Parameters.Add(
                    OracleFactory.Instance.CreateInParameter(":utentePk", DbType.Int32, utentePk));


                this.oraCommand.ExecuteNonQuery();
                return 0;

            }
            catch
            {
                return -1;
            }

        }
        public bool DeleteUtenteLdap(string idUtente)
        {
            try
            {
                string PathInizio = "LDAP://" + this.host + ":" + this.port + "/";
                string PathFine = ",ou=UsersSSO,dc=inpdap,dc=it";
                var ldap = new LdapAdmin(this.host, this.port, this.container, this.username, this.password);
                var ldapUsers = ldap.Find(idUtente, LdapAdmin.PathScope.Users, LdapAdmin.PropertyType.UserId, idUtente);
                if (ldapUsers.Count > 0)
                {
                    //DirectoryEntry user = new DirectoryEntry(PathInizio + "uid=" + idUtente + PathFine);
                    ldap.RootDirectory.Path = PathInizio + "uid=" + idUtente + PathFine;
                    DirectoryEntry user = ldap.RootDirectory;
                    DirectoryEntry ou = user.Parent;
                    ou.Children.Remove(user);
                    ou.CommitChanges();
                }
                return true;
            }

            catch (System.DirectoryServices.DirectoryServicesCOMException E)
            {
                throw E;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void deleteservizioLdap(string idutente, string ruoloGDP, string applicazionegdp)
        {
            try
            {
                var ldap = new LdapAdmin(this.host, this.port, this.container, this.username, this.password);
                ldap.deleteprofiloLdap(idutente, ruoloGDP, applicazionegdp);
            }
            catch (System.DirectoryServices.DirectoryServicesCOMException E)
            {
                //Console.WriteLine("20201026dettaglioerroreLdap" + E.ToString());
                throw E;
            }
            catch(Exception ex)
            {
                //Console.WriteLine("20201026dettaglioerroreLdap" + ex.ToString());
                throw ex;
            }
        
        }

        public sealed class LdapAdmin
        {
            private readonly Dictionary<PropertyType, string> properties;

            public LdapAdmin(string host, string port, string container, string username, string password)
            {
                string path = @"LDAP://" + host + ":" + port + "/" + container;

                this.Host = host;
                this.Port = port;
                this.Container = container;
                this.AccountUser = username;
                this.AccountPassword = password;

                this.RootDirectory = new DirectoryEntry(path, username, password, AuthenticationTypes.None);
                // Predispongo un dizionario che contiene l'elenco degli attributi LDAP gestiti.
                //this.properties = new Dictionary<PropertyType, string>
                //                      {
                //                          { PropertyType.Cn, "cn" }, 
                //                          { PropertyType.ObjectClass, "objectClass" }, 
                //                          { PropertyType.SsoAuth, "INPDAPSSOAuth" }, 
                //                          { PropertyType.GroupMembersInter, "inpdapGroupMembersInter" }, 
                //                          { PropertyType.DisabledFlag, "INPDAPSSODisabledFlag"}, 
                //                          { PropertyType.UserType, "INPDAPUserType" }, 
                //                          ////{ PropertyType.Email, "mail" }, 
                //                          { PropertyType.Email, "postOfficeBox"},
                //                          { PropertyType.Surname, "sn" }, 
                //                          { PropertyType.UserId, "uid" }, 
                //                          { PropertyType.Name, "givenname" },
                //                          { PropertyType.uniquemember,"uniquemember"}
                //                      };
                if (this.ScriptInternet)
                    this.properties = new Dictionary<PropertyType, string>
                                      {
                                          { PropertyType.Cn, "cn" }, 
                                          { PropertyType.ObjectClass, "objectClass" }, 
                                          { PropertyType.GroupMembers, "inpdapGroupMembersInter" }, 
                                          { PropertyType.DisabledFlag, "inpdapSSODisabledFlag"}, 
                                          { PropertyType.UserType, "inpdapUserType" }, 
                                          { PropertyType.Email, "postOfficeBox"},
                                          { PropertyType.Surname, "sn" }, 
                                          { PropertyType.UserId, "uid" }, 
                                          { PropertyType.Name, "givenname" },
                                          { PropertyType.uniquemember,"uniquemember"},
                                          { PropertyType.organization,"o"}
                                          
                                      };
                else
                    this.properties = new Dictionary<PropertyType, string>
                                      {
                                          { PropertyType.Cn, "cn" }, 
                                          { PropertyType.ObjectClass, "objectClass" }, 
                                          { PropertyType.GroupMembers, "inpdapSSOAuth" }, 
                                          { PropertyType.DisabledFlag, "inpdapSSODisabledFlag"}, 
                                          { PropertyType.UserType, "inpdapUserType" }, 
                                          { PropertyType.Email, "postOfficeBox"},
                                          { PropertyType.Surname, "sn" }, 
                                          { PropertyType.UserId, "uid" }, 
                                          { PropertyType.Name, "givenname" },
                                          { PropertyType.uniquemember,"uniquemember"},
                                          { PropertyType.organization,"o"}
                                          
                                      };
            }

            public enum PathScope
            {
                /// <summary>
                /// Limita la ricerca al ramo GroupsSSO.
                /// </summary>
                Groups,

                /// <summary>
                /// Limita la ricerca al ramo UsersSSO.
                /// </summary>
                Users,

                /// <summary>
                /// Estende la ricerca a tutti i rami.
                /// </summary>
                All,
                /// <summary>
                /// limita la ricerca ad un singolo user
                /// </summary>
                User,
                /// <summary>
                /// limita la ricerca ad un singolo gruppo
                /// </summary>
                Group,
            }

            public enum PropertyType
            {
                /// <summary>
                /// Proprietà specifica del protocollo.
                /// </summary>
                Cn,

                /// <summary>
                /// Proprietà specifica del protocollo.
                /// </summary>
                ObjectClass,

                /// <summary>
                /// Attributo multivalore – lista dei ruoli GDP relativi alle autorizzazioni.
                /// Contiene il codice a due caratteri delle applicazioni intranet a cui l'utente è abilitato puntualmente.
                /// </summary>
                //SsoAuth,

                /// <summary>
                /// Attributo multivalore – lista dei ruoli GDP relativi alle autorizzazioni.
                /// Contiene il codice a due caratteri delle applicazioni internet a cui l'utente è abilitato puntualmente.
                /// </summary>
                //GroupMembersInter,

                /// <summary>
                /// Attributo multivalore – lista dei ruoli GDP relativi alle autorizzazioni.
                /// Contiene il codice a due caratteri delle applicazioni internet o intranet a cui l'utente è abilitato puntualmente.
                /// </summary>
                GroupMembers,

                /// <summary>
                /// Attributo valorizzato da Siteminder:
                /// '0' abilitato; '1' disabilitato; '16777216' cambio password obbligatorio.
                /// </summary>
                DisabledFlag,

                /// <summary>
                /// attributo multivalore contenente il codice a due lettere delle tipologie utente.
                /// </summary>
                UserType,

                /// <summary>
                /// Indirizzo email (da PIN Prov).
                /// </summary>
                Email,

                /// <summary>
                /// Nome
                /// </summary>
                Name,

                /// <summary>
                /// Cognome (da Anagrafe se esiste altrimenti da PIN).
                /// </summary>
                Surname,

                /// <summary>
                /// Codice fiscale (da Anagrafe se esiste altrimenti da PIN).
                /// </summary>
                UserId,

                /// <summary>
                /// Attributo multivalore – lista dei ruoli GDP e applicazioni GDP relativi alle autorizzazioni.
                /// Contiene il codice a due caratteri delle applicazioni internet a cui l'utente è abilitato puntualmente. 
                /// </summary>
                Groupuniquemember,

                /// <summary>
                /// campo da aggiungere su groupSSO. 
                /// </summary>
                uniquemember,

                /// <summary>
                /// campo da aggiungere su UserSSO. 
                /// </summary>
                organization
            }
            public bool ScriptInternet
            {
                get
                {
                    return bool.Parse(ConfigurationManager.AppSettings["InsertInternet"]);
                }
            }
            /// <summary>
            /// O
            /// </summary>
            /// <value>
            /// TEST: IMEAPPL
            /// </value>
            public string AccountPassword { get; set; }

            /// <summary>
            /// Ottiene o imposta l'username.
            /// </summary>
            /// <value>
            /// TEST: uid=pin,ou=UsersAPP,DC=INPDAP,DC=IT
            /// </value>
            public string AccountUser { get; set; }

            /// <summary>
            /// Otiiene o imposta la base usata come radice per tutte le ricerche.
            /// </summary>
            /// <value>
            /// TEST: DC=INPDAP,DC=IT
            /// </value>
            public string Container { get; set; }

            /// <summary>
            /// Ottiene o imposta l'indirizzo dell'host a cui connettersi.
            /// </summary>
            /// <value>
            /// TEST: 10.192.192.54
            /// </value>
            public string Host { get; set; }

            /// <summary>
            /// Otiiene o imposta in numero della porta.
            /// </summary>
            /// <value>
            /// TEST: 3890
            /// </value>
            public string Port { get; set; }

            /// <summary>
            /// Ottiene un dizionario tramite il quale è possibile risalire alla chiave di un attributo di un ramo.
            /// </summary>
            public Dictionary<PropertyType, string> Properties
            {
                get
                {
                    return this.properties;
                }
            }

            /// <summary>
            /// Ottiene il riferimento al DirectoryEntry che è usato come punto di partenza per posizionarsi sui rami.
            /// </summary>
            public DirectoryEntry RootDirectory { get; private set; }
            /// <summary>
            /// Esegue una ricerca. La ricerca può avvenire sui rami UsersSSO o GroupsSSO e viene configurata tramite i 
            /// parametri passati.
            /// Di default se non è passato nessun valore a <c>propertiesToLoad</c> si tenta di caricarle tutte.
            /// Questa versione esegue una ricerca andando ad estrarre tutti gli elementi che hanno una corrispondenza
            /// con gli attributi presenti in <c>filters</c>.
            /// </summary>
            /// <param name="scope">Indica il ramo su cui eseguire la ricerca (UsersSSO o GroupsSSO o tutto).</param>
            /// <param name="filters">Elenco degli attributi usati per eseguire la ricerca. Saranno estratti tutti gli elementi che contengono i valori passati</param>
            /// <param name="propertiesToLoad">Elenco degli attributi da estrarre. Se è nullo si prova a caricarli tutti.</param>
            /// <returns>Il risultato della ricerca.</returns>
            //public SearchResultCollection Find(
            //    PathScope scope,
            //    Dictionary<PropertyType, string> filters,
            //    params string[] propertiesToLoad)
            //{
            //    string filter = filters.Keys.Aggregate(
            //        string.Empty,
            //        (current, attribute) =>
            //        current + ("(" + this.Properties[attribute] + "=" + filters[attribute] + ")"));

            //    return this.Find(string.Empty, scope, "(&" + filter + ")", propertiesToLoad);
            //}
            /// <summary>
            /// Esegue una ricerca. La ricerca può avvenire sui rami UsersSSO o GroupsSSO e viene configurata tramite i 
            /// parametri passati.
            /// Di default se non è passato nessun valore a <c>propertiesToLoad</c> si tenta di caricarle tutte.
            /// Questa è la versione più generica e flessibile per la ricerca dato che consente di definire un filtro custom.
            /// Le regole per il filtro sono le seguenti:
            /// WildCard (*): E' possibile usare il carattere * per eseguire un confronto sul contenuto e non di uguaglianza
            /// AND (&): (&(objectClass=person)(objectClass=user)) --> objectClass=person AND objectClass=user
            /// OR (|): (|(objectClass=person)(objectClass=top)(objectClass=user)) --> objectClass=person OR objectClass=top OR objectClass=user
            /// NOT (!): (&(objectClass=group)(|(objectClass=top)(!(objectClass=person)))) --> objectClass=group AND (objectClass=top OR NOT(objectClass=person))
            /// </summary>
            /// <param name="scope">Indica il ramo su cui eseguire la ricerca (UsersSSO o GroupsSSO o tutto).</param>
            /// <param name="filter">Un filtro di ricerca espresso secondo le regole sopra esposte.</param>
            /// <param name="propertiesToLoad">Elenco degli attributi da estrarre. Se è nullo si prova a caricarli tutti.</param>
            /// <returns>Il risultato della ricerca.</returns>
            public SearchResultCollection Find(string IdUtenteGruppo, PathScope scope, string filter, params string[] propertiesToLoad)
            {
                string pathAddin;
                var props = propertiesToLoad ?? this.Properties.Keys.Select(key => this.Properties[key]).ToArray();
                var searcher = new DirectorySearcher(
                    this.RootDirectory,
                    filter,
                    props,
                    SearchScope.Subtree);

                switch (scope)
                {
                    case PathScope.Groups:
                        pathAddin = "ou=GroupsSSO,";
                        break;
                    case PathScope.Users:
                        pathAddin = "ou=UsersSSO,";
                        break;
                    case PathScope.User:
                        pathAddin = "uid=" + IdUtenteGruppo + ",ou=UsersSSO,";
                        break;
                    case PathScope.Group:
                        string[] Par = IdUtenteGruppo.Split('|');
                        //cn=utenteEnteVD,ou=VD,ou=roles,ou=GroupsSSO
                        pathAddin = "cn=" + Par[0] + ",ou=" + Par[1] + ",ou=roles,ou=GroupsSSO,";
                        break;
                    default:
                        pathAddin = string.Empty;
                        break;
                }
                // Esempio di Path valido: LDAP://128.128.0.10:389/objectclass=person,ou=users,dc=domain,dc=domain2
                this.RootDirectory.Path = "LDAP://" + this.Host + ":" + this.Port + "/" + pathAddin + this.Container;
                //var p = this.RootDirectory.SchemaClassName;
                return searcher.FindAll();
            }
            /// <summary>
            /// Esegue una ricerca. La ricerca può avvenire sui rami UsersSSO o GroupsSSO e viene configurata tramite i 
            /// parametri passati.
            /// Di default se non è passato nessun valore a <c>propertiesToLoad</c> si tenta di caricarle tutte.
            /// La versione attuale consente una ricerca localizzata su un solo attributo del ramo.
            /// </summary>
            /// <param name="filter">Filtro di ricerca (può contenere il carattere *).</param>
            /// <param name="property">Attributo su cui sarà localizzata la ricerca.</param>
            /// <param name="scope">Il ramo radice su cui localizzare la ricerca (UsersSSO o GroupsSSO).</param>
            /// <param name="propertiesToLoad">Una lista con gli attributi del ramo da estrarre.</param>
            /// <returns></returns>
            public SearchResultCollection Find(string Idutente,
                PathScope scope,
                PropertyType property,
                string filter,
                params string[] propertiesToLoad)
            {
                return this.Find(Idutente, scope, @"(&(" + this.Properties[property] + "=" + filter + "))", propertiesToLoad);
            }

            internal void InsertUserLdap(Dictionary<PropertyType, IEnumerable<string>> valuesToSave)
            {
                string pathAddin;
                DirectoryEntry newUser;

                pathAddin = "ou=UsersSSO,";
                // Ripristino il path
                this.RootDirectory.Path = @"LDAP://" + this.Host + ":" + this.Port + "/" + pathAddin + this.Container;
                // creo il nodo che contiene il nuovo utente.
                newUser =
                    this.RootDirectory.Children.Add("uid=" + valuesToSave[PropertyType.UserId].FirstOrDefault(), "top");
                newUser.Properties["cn"].Add(valuesToSave[PropertyType.UserId].FirstOrDefault());
                newUser.Properties["sn"].Add(valuesToSave[PropertyType.Surname].FirstOrDefault());
                newUser.Properties["objectclass"].Add("person");
                newUser.Properties["objectclass"].Add("organizationalPerson");
                newUser.Properties["objectclass"].Add("inetorgperson");
                newUser.Properties["objectclass"].Add("inpdapinetorgperson");

                foreach (PropertyType prop in valuesToSave.Keys)
                {
                    if (prop == PropertyType.Surname)
                    {
                        continue;
                    }

                    foreach (string value in valuesToSave[prop])
                    {
                        newUser.Properties[this.Properties[prop]].Add(value);
                    }
                }

                newUser.CommitChanges();
            }

            public Boolean InsertGroupLdap(List<List<RuoloGdp>> listaGruppi, string IdUtente)
            {
                Boolean ritorno = false;
                string PathInizio = "LDAP://" + this.Host + ":" + this.Port + "/";
                string pathAddin = ",ou=roles,ou=GroupsSSO,dc=inpdap,dc=it";

                foreach (List<RuoloGdp> ListRuolo in listaGruppi)
                {
                    foreach (RuoloGdp RuoloGdp in ListRuolo)
                    {
                        try
                        {
                            var result = this.Find(RuoloGdp.CodRuoloGdp + "|" + RuoloGdp.CodApplicazioneGDP, LdapAdmin.PathScope.Group
                                , LdapAdmin.PropertyType.uniquemember, "uid=" + IdUtente + ", ou = UsersSSO, dc = inpdap, dc = it");
                            if (result.Count == 0)
                            {
                                this.RootDirectory.Path = @PathInizio + "cn=" + RuoloGdp.CodRuoloGdp + ",ou=" + RuoloGdp.CodApplicazioneGDP + pathAddin;
                                this.RootDirectory.Properties[this.properties[PropertyType.uniquemember]].Add("uid=" + IdUtente + ",ou=UsersSSO,dc=inpdap,dc=it");
                                this.RootDirectory.CommitChanges();
                            }
                            ritorno = true;
                        }

                        catch (System.DirectoryServices.DirectoryServicesCOMException E)
                        {
                            throw E;

                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
                return ritorno;
            }

            public Boolean InsertRoleLdap(string IdUtente, List<string> valuesRuoliUsers)
            {
                Boolean ritorno = false;
                try
                {
                    string pathInizio = "LDAP://" + this.Host + ":" + this.Port + "/";
                    string pathAddin = ",ou=UsersSSO,dc=inpdap,dc=it";
                    //string path;
                    foreach (string RuoloLdap in valuesRuoliUsers)
                    {
                        var result = this.Find(IdUtente, LdapAdmin.PathScope.User, LdapAdmin.PropertyType.GroupMembers, RuoloLdap);
                        if (result.Count == 0)
                        {
                            this.RootDirectory.Path = @pathInizio + "uid=" + IdUtente + pathAddin;
                            this.RootDirectory.Properties[this.properties[PropertyType.GroupMembers]].Add(RuoloLdap);
                            this.RootDirectory.CommitChanges();
                        }
                    }
                    ritorno = true;
                }

                catch (System.DirectoryServices.DirectoryServicesCOMException E)
                {
                    throw E;

                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return ritorno;
            }
            // modifica Flavio Giovannini 01/11/2016 CodAppRuoloGdp
            //internal void deleteprofiloLdap(string idutente, RuoloGdp ruoloGDP)
            internal void deleteprofiloLdap(string idutente, string ruoloGDP, string applicazionegdp )
            {
                // cancello in UserSSO
                string pathInizio = "LDAP://" + this.Host + ":" + this.Port + "/";
                string pathAddinUsr = ",ou=UsersSSO,dc=inpdap,dc=it";
                if (applicazionegdp != null)
                {
                    var resultUsr = this.Find(idutente, LdapAdmin.PathScope.User, LdapAdmin.PropertyType.GroupMembers, applicazionegdp);
                    if (resultUsr.Count > 0)
                    {
                        this.RootDirectory.Path = @pathInizio + "uid=" + idutente + pathAddinUsr;
                        this.RootDirectory.Properties[this.properties[PropertyType.GroupMembers]].Remove(applicazionegdp);
                        this.RootDirectory.CommitChanges();
                    }
                    // cancello in GroupSSO
                }
                string pathAddinGr = ",ou=roles,ou=GroupsSSO,dc=inpdap,dc=it";
                // modifica Flavio 28-10-2016
                // controllo se l'applicazione è null in questo caso non deve fare la cancellazione su ldap per GroupsSSO
                //var resultGr = this.Find(ruoloGDP.CodRuoloGdp + "|" + ruoloGDP.CodApplicazioneGDP, LdapAdmin.PathScope.Group   
                var resultGr = this.Find(ruoloGDP + "|" + applicazionegdp, LdapAdmin.PathScope.Group
                                   , LdapAdmin.PropertyType.uniquemember, "uid=" + idutente + ", ou = UsersSSO, dc = inpdap, dc = it");
                if (resultGr.Count > 0)
                {
                    //this.RootDirectory.Path = @pathInizio + "cn=" + ruoloGDP.CodRuoloGdp + ",ou=" + ruoloGDP.CodApplicazioneGDP + pathAddinGr;
                    this.RootDirectory.Path = @pathInizio + "cn=" + ruoloGDP + ",ou=" + applicazionegdp + pathAddinGr;
                    this.RootDirectory.Properties[this.properties[PropertyType.uniquemember]].Remove("uid=" + idutente + ",ou=UsersSSO,dc=inpdap,dc=it");
                    this.RootDirectory.CommitChanges();
                }
            }

        }
        

    }

}
