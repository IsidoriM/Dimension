using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Math;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using tar_cs;
using Org.BouncyCastle.Asn1;
using System.Web;
using log4net;
using Org.BouncyCastle.Utilities.Collections;
using System.Collections;
using System.Text.RegularExpressions;
using ElaboraEntratel;



[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace mycertificatebatch
{
    class Program
    {
        static string certTarName = null;
        static string crlTarName = null;
        static Stream crlStream = null;
        static String pathToTar = null;//@"C:\testcrl\cert-crl";
        static String pathToOutUntar = null;//@"C:\testcrl\cert-crl";
        static X509Crl crl;
        static string sConn = "mycertificatebatch.Properties.Settings.myBatchDBConnectionString";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static int totali_fisiche = 0;
        static int inseriti = 0;
        static StringBuilder errorMessage = new StringBuilder();

        //roba per email

        static string codapp;
        static string huname;
        static string hpswd;  
        static string[] destinatariTo;
        static string[] destinatariCC;
        static string mittente;
        static string subject;
        static Certificato dummyCert;
        static SqlConnection conn;
        static string[] certROOTDirectories = null;


        static void Main(string[] args)
        {

            ConnectionStringSettings setta = ConfigurationManager.ConnectionStrings[sConn];

            // apro connessione2
            conn = new SqlConnection(setta.ConnectionString);
            conn.Open();
            dummyCert = new Certificato();
            dummyCert.CodiceFiscale = "NA";
            dummyCert.Esito = 2;
            string vers = ConfigurationManager.AppSettings["Versione"].ToString();

            dummyCert.Messaggio = "Versione " + vers + "  -- Inizio Elaborazione";
            dummyCert.data = DateTime.Now.AddSeconds(-1);
            insertLogEvent(dummyCert, conn);
            conn.Close();

            ArrayList filedachiudere = null;
            try
            {
                
                log.Info("##Versione 1.1.12");
               

                pathToTar = ConfigurationManager.AppSettings["pathToTar"];
                pathToOutUntar = ConfigurationManager.AppSettings["pathToOutUntar"];


                //codapp = ConfigurationManager.AppSettings["WSICONAMAILCodiceApplicazione"];
                //huname = ConfigurationManager.AppSettings["WSICONAMAILUserName"];
                //hpswd = ConfigurationManager.AppSettings["WSICONAMAILPassword"];
                if (ConfigurationManager.AppSettings["WSICONAMAILDestinatariTo"] != null &&
                     ConfigurationManager.AppSettings["WSICONAMAILDestinatariTo"].Length > 0)
                {
                    destinatariTo = ConfigurationManager.AppSettings["WSICONAMAILDestinatariTo"].Split(new char[] { ',' });
                    log.Info("### Destinatari To:" + ConfigurationManager.AppSettings["WSICONAMAILDestinatariTo"]);
                }
                if (ConfigurationManager.AppSettings["WSICONAMAILDestinatariCC"] != null &&
                     ConfigurationManager.AppSettings["WSICONAMAILDestinatariCC"].Length > 0)
                {
                    destinatariCC = ConfigurationManager.AppSettings["WSICONAMAILDestinatariCC"].Split(new char[] { ',' });
                    log.Info("### Destinatari CC:" + ConfigurationManager.AppSettings["WSICONAMAILDestinatariCC"]);
                }
                mittente = ConfigurationManager.AppSettings["WSICONAMAILMittente"];
                subject = ConfigurationManager.AppSettings["WSICONAMAILSubject"];
                ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[sConn];

                // apro connessione
                conn = new SqlConnection(settings.ConnectionString);
                conn.Open();
                dummyCert = new Certificato();
                dummyCert.CodiceFiscale = "NA";
                dummyCert.Esito = 2;
                string versione = ConfigurationManager.AppSettings["Versione"].ToString();

                dummyCert.Messaggio = "Versione " + versione +  "  -- Inizio Elaborazione";
                dummyCert.data = DateTime.Now.AddSeconds(-1);
                insertLogEvent(dummyCert, conn);
                 
                /*estraggo la root su cui lavorare se null non c'è da caricare nulla
                *-- Estrae anche CRL lo stream lo memorizza in var di classe crlStream
                */
                log.Info("##Inizio Estrazioene e aggiornamento stato Revoca certificati preesistenti:(pathToTar-pathToOutUntar)" + pathToTar + "-" + pathToOutUntar);
                
                certROOTDirectories = estraiCrlANDRootDirectoryCertificate(conn, out filedachiudere );
                log.Info("## Fine Estrazioene e aggiornamento stato Revoca certificati preesistenti:(certROOTDirectory)" + "certROOTDirectory");
                if (certROOTDirectories != null)
                {
                    foreach (String certROOTDirectory in certROOTDirectories)
                    {
                        doProcess(certROOTDirectory);
                    }
                }

                if (crlStream != null)
                {
                    crlStream.Close();
                }
                //cancella la crl estratta in outhtar
                string[] files = Directory.GetFiles(pathToOutUntar);
                foreach (string fileName in files)
                {
                    if (!fileName.Contains("tar"))
                    {
                        File.Delete(fileName);
                    }
                }

                //cancello file vecchi in pathtar
                string[] oldFiles = Directory.GetFiles(pathToTar);
                foreach (string fileName in oldFiles)
                {
                    if (File.GetCreationTime(fileName) < DateTime.Now.AddDays(-15))
                    {
                        File.Delete(fileName);
                    }
                }
               
            }
            catch (Exception e)
            {
                errorMessage.AppendFormat(Messaggi.ERROR_MESSAGE, "Main", e.Message);
                errorMessage.AppendLine();

                
                dummyCert = new Certificato();
                dummyCert.CodiceFiscale = "NA";
                dummyCert.Esito = 2;
                dummyCert.Messaggio = "Errore --" + e.Message;
                dummyCert.data = DateTime.Now.AddSeconds(-1);
                insertLogEvent(dummyCert, conn);

            }
            finally
            {

                // condizione per chiuderea parsed i tar in input
                if (inseriti>0 && inseriti == totali_fisiche)
                {
                    foreach (string x in filedachiudere)
                    {

                        File.Move(x, x + ".PARSED");
                    }
                    
                }

                DateTime now = DateTime.Now;
                StringBuilder EmailMessageToSend = new StringBuilder();
                EmailMessageToSend.AppendFormat(Messaggi.ADDCERT_MESSAGE, now, inseriti, totali_fisiche);
                EmailMessageToSend.AppendLine();
                EmailMessageToSend.AppendFormat(Messaggi.PARSED_CERT_FILE, "\n"+certTarName);
                EmailMessageToSend.AppendLine();
                EmailMessageToSend.AppendFormat(Messaggi.PARSED_CRL_FILE, "\n"+crlTarName);
                EmailMessageToSend.AppendLine();

                if (errorMessage.Length > 0)
                {
                    EmailMessageToSend.AppendLine();
                    EmailMessageToSend.Append(errorMessage.ToString());

                    //Invio Email.........>>>>>
                    log.Error("######Riepilogo degli Errori: " + errorMessage.ToString());
                }

                EsitoMail esitoMail = new EsitoMail();
                ClientIcona Client = new ClientIcona();

                esitoMail = Client.inviaEmail(codapp, huname, hpswd, destinatariTo, destinatariCC, mittente, subject, EmailMessageToSend.ToString());

                if (esitoMail.Esito == "ND")
                {

                    dummyCert.Messaggio = "Errore ElaboraEntratel.GestoreMAIL.InvioMail:  " + " Errore Invio Email Manca destinatario:" + esitoMail.ErrDescription + "--" + destinatariTo[0]; 
                    insertLogEvent(dummyCert, conn);
                }
                if (esitoMail.Esito == "ER")
                ///// IM: in coda per Invio Email - ER: errore (comunicazione non inserita) - PK: KeyGest Duplicata
                {
                    
                    dummyCert.Messaggio = "Errore ElaboraEntratel.GestoreMAIL.InvioMail:  " + " Errore Invio Email Manca la comunicazione:" + esitoMail.ErrDescription + "--" + destinatariTo[0];
                    insertLogEvent(dummyCert, conn);
                }

                if (esitoMail.Esito == "IM")
                ///// IM: in coda per Invio Email - ER: errore (comunicazione non inserita) - PK: KeyGest Duplicata
                {
                    dummyCert.Messaggio = "ElaboraEntratel.GestoreMAIL.InvioMail:  " + " Invio Email:" + " Chiave Gestionale: " + esitoMail.Chiave + "--" + destinatariTo[0];
                    insertLogEvent(dummyCert, conn);
                    //log.Info("ElaboraEntratel.GestoreMAIL.InvioMail:  " + " Invio Email:" + " Chiave Gestionale: " + esitoMail.Chiave + "--" + destinatariTo[0]);
                }


                log.Info("########Body Email Inviata:" + EmailMessageToSend.ToString());


                dummyCert.data = DateTime.Now.AddSeconds(+1);

                string versione = ConfigurationManager.AppSettings["Versione"].ToString();
                dummyCert.Messaggio = "Versione " + versione +  " --  Fine Elaborazione";
                insertLogEvent(dummyCert, conn);

                conn.Dispose();

                // cancella in outtar le rootdirectory dei certificati estratti... qui ci va un ciclo
                certROOTDirectories= certROOTDirectories = Directory.GetDirectories(pathToOutUntar);
                if (certROOTDirectories != null)
                {
                    foreach (string certROOTDirectory in certROOTDirectories)
                    {
                        Directory.Delete(certROOTDirectory, true);
                    }
                }

            
            
            }


            
        }





        static void doProcess(String certROOTDirectory)
        {
            try
            {

             
                if (certROOTDirectory != null)
                {
                    // aggiorna stato revoca precedenti
                    //int  aggiornati= aggiornaStatoRevocaCertificatiPresenti(conn);

                    //le directory di primo livello che contengono busibess category - se pregresso invece la BC è nel nome del certificato al suo interno                        
                    string[] cert1LDirectories = Directory.GetDirectories(certROOTDirectory);
                    foreach (string cert1LDirectory in cert1LDirectories)
                    {
                        lavoraIESimoCertificatoDaFile(cert1LDirectory, conn);
                    }


                }
               



                

            }
            catch (Exception e)
            {
                errorMessage.AppendFormat(Messaggi.ERROR_MESSAGE, "doProcess", e.Message);
                errorMessage.AppendLine();

            }

            
            
        }


        public static  Boolean isCurrentlatestFile(string current, string inmemory)
        {
            Boolean toreturn=false;

            if (inmemory.Equals(""))
            {
                toreturn= true;
                return toreturn;
            }

            int intcurrent = Int32.Parse(getDateFromFileName(current));
            int intinmemory = Int32.Parse(getDateFromFileName(inmemory));

            if (intcurrent > intinmemory)
            {
                toreturn = true;
            }

            return toreturn;
                  
        }

        public static string[] estraiCrlANDRootDirectoryCertificate(SqlConnection conn, out ArrayList filedachiudere)
        {
            string[] tarEntries = Directory.GetFiles(pathToTar);
            string[] certROOTDirectories = null;
            
            string latestCrlName = "";

            filedachiudere = new ArrayList();
            
            // ciclo sui tar e li estraggo: uno è la crl l'altro è il file che contiene i certificati
            foreach (string fileName in tarEntries)
            {   //se tar occorre scompattare
                if (fileName.EndsWith("tar", true, null))
                {
                    if (fileName.EndsWith("R.tar"))
                    {
                        if (isCurrentlatestFile(fileName, latestCrlName))
                        {
                            if (!latestCrlName.Equals(""))
                            {
                                File.Move(latestCrlName, latestCrlName + ".PARSED");
                            }
                            latestCrlName = fileName;
                        }
                        
                    }
                    else if (fileName.EndsWith("V.tar"))
                    {
                        
                        certTarName += fileName+"\n";
                        extractTar(fileName, pathToOutUntar);
                        //File.Move(fileName, fileName + ".PARSED");
                        filedachiudere.Add(fileName);
                    }
                    else
                    {
                        continue;
                    }


                    
                }
                
            }
            if (!latestCrlName.Equals(""))
            {
                crlTarName = latestCrlName;
                extractTar(latestCrlName, pathToOutUntar);
                //File.Move(latestCrlName, latestCrlName + ".PARSED");
                filedachiudere.Add(latestCrlName);
            }


            //Qui inizio a ad analizzare i contenuti della directory che non sono: tar, una crl(finisce con R)
            // e la rootdirectory dei certificati
 

            //CRL
            string[] fileList = Directory.GetFiles(pathToOutUntar);
            foreach (string fileName in fileList)
            {

                if (!fileName.EndsWith("tar", true, null))
                {
                    //se finisce con R è la CRL
                    if (fileName.EndsWith("R", true, null))
                    {
                        crlStream = File.OpenRead(fileName);
                        if (crlStream == null)
                        {
                            errorMessage.AppendFormat(Messaggi.ERROR_MESSAGE, "estraiCrlANDRootDirectoryCertificate", "Nessun file per la CRL trovato");
                            errorMessage.AppendLine();                            
                            log.Error("Lo stream relativa al file della crl è null");
                            return null;
                        }

                        //in initX509CRL c'è la logica per aggiornare la revoca preesistente
                        if (initX509CRL(crlStream, conn) == null)
                        {
                            return null;
                        }
                    }
                    
                }
            }


            // Fine

            if (crlStream == null)
            {
                errorMessage.AppendFormat(Messaggi.ERROR_MESSAGE, "estraiCrlANDRootDirectoryCertificate", "Nessun file per la CRL trovato");
                errorMessage.AppendLine();
                log.Error("Lo stream relativa al file della crl è null");
                return null;
            }

            //RR
             // ricava la root directory - attento che devi prendere solo quella con un certo nome
            if (Directory.GetDirectories(pathToOutUntar).Length > 0)
            {
                certROOTDirectories = Directory.GetDirectories(pathToOutUntar);
            }

            return certROOTDirectories;

        }


       

        public static Boolean lavoraIESimoCertificatoDaFile(string cert1LDirectory, SqlConnection conn)
        {
            // solo persone fisiche, se cert1LDirectory contiene piva esci dal metodo 


            
            // estrai da cert1LDirectory la businesscategory patterb matching TODO
 
            // recupera certificato come da cifra.der

            string[] certPaths=  Directory.GetFiles(cert1LDirectory);
            // 2 è lo standard !! Estrai BC da nome della directory cert1LDirectory
            if (certPaths.Length <= 2 && certPaths.Length>0)
            {
                String certPath=null;
                try
                {

                    foreach (String x in certPaths)
                    {
                        if (x.Contains("cifra"))
                        {
                            certPath = x;
                            break;
                        }
                    }
                    if (certPath == null)
                    {
                        certPath = certPaths[0];
                    }
                    
                    using (Stream certStream = File.OpenRead(certPath))
                    {
                        log.Debug("In lavoraIESimoCertificatoDaFile Sto lavorando il file: " + certPath);
                        X509Certificate cert = new X509CertificateParser().ReadCertificate(certStream);
                        Certificato certificato = parsaCertificato(cert, crlStream);
                        if (certificato.CodiceFiscale != null)
                        {
                            certificato.BusinessCategory = getBusinessCategory(certPath);
                            insertCertificate(certificato, conn);
                        }
                        else
                        {
                            log.Debug("Condizione anomala, nel certificato non è stato rilevato né un codice fiscale né una partita iva");
                        }
                    }
                }
                catch (Exception e)
                {
                    errorMessage.AppendFormat(Messaggi.ERROR_MESSAGE_PARAM, "lavoraIESimoCertificatoDaFile", certPath, e.Message);
                    errorMessage.AppendLine();   
                    log.Error("Errore durante la lavorazione di " + certPath, e);
                }
            }
            // Di più è il pregresso !! Estrai BC dal nome di ciascun certificato nel ciclo certPath 
            else
            {
                foreach (String certPath in certPaths)
                {
                    try
                    {
                        log.Debug("In lavoraIESimoCertificatoDaFile Sto lavorando il file: " + certPath);
                        using (Stream certStream = File.OpenRead(certPath))
                        {
                            X509Certificate cert = new X509CertificateParser().ReadCertificate(certStream);

                            Certificato certificato = parsaCertificato(cert, crlStream);
                            if (certificato.CodiceFiscale != null)
                            {
                                certificato.BusinessCategory = getBusinessCategory(certPath);
                                insertCertificate(certificato, conn);
                            }
                            else
                            {
                                log.Debug("Condizione anomala, nel certificato non è stato rilevato né un codice fiscale né una partita iva");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        errorMessage.AppendFormat(Messaggi.ERROR_MESSAGE_PARAM, "lavoraIESimoCertificatoDaFile", certPath, e.Message);
                        errorMessage.AppendLine();   
                        log.Error("Errore durante la lavorazione di " + certPath, e);
                    }
                }

            }
            return true;
        }

        /**
         * estrae il tar
         * 
         * */
        public static Boolean extractTar(String pathToTar, String pathToOutUntar)
        {

            Boolean ok =true;

           try{
           using (FileStream unarchFile = File.OpenRead(pathToTar)) 
           { TarReader reader = new TarReader(unarchFile);
           reader.ReadToEnd(pathToOutUntar);
           }

           }
            catch(Exception e){

                errorMessage.AppendFormat(Messaggi.ERROR_MESSAGE, "extractTar", e.Message);
                errorMessage.AppendLine();
                ok=false;
                log.Error("Errore estrazione tar", e);
            }

            return ok;
        }

        //valorizza Certificato con i valori ricavati da X509Certificate
        public static Certificato parsaCertificato(X509Certificate cert, Stream crlSteram)
        {
            Certificato certificato = new Certificato();
            BigInteger serial = cert.SerialNumber;            
            X509CrlEntry crlEntry=  GetrevocationDate(serial);
            DateTime? revocationDate = null;
            String motivoRevoca = null;

            if (crlEntry != null)
            {
                revocationDate = crlEntry.RevocationDate;
                Asn1OctetString motivo = crlEntry.GetExtensionValue("2.5.29.21");
                motivoRevoca = crlEntry.GetExtensionValue("2.5.29.21").ToString();
                
            }
            

            //da salvare
            certificato.Serial = cert.SerialNumber.ToString(16);
            certificato.emissione = cert.NotBefore;
            certificato.scadenza = cert.NotAfter;
            certificato.revoca = revocationDate;
            certificato.MotivoRevoca = motivoRevoca;
            String dn = cert.SubjectDN.ToString();
            String cnToSave = dn.Split(',').Where(i => i.Contains("CN=")).Select(i => i.Replace("CN=", "")).FirstOrDefault();
            certificato.CommonName = cnToSave;
            
            certificato.CodiceFiscale =  getCodiceFiscaleFromCN(cnToSave);
            //String serialToSave = serial.ToString();
            //DateTime? emessoToSave = cert.NotBefore;
            //DateTime? scadutoToSave = cert.NotAfter;
            //DateTime? revocatoToSave = revocationDate;
            return certificato;

        }




        public static X509Crl initX509CRL(Stream crlRawData, SqlConnection conn)
        {
            try
            {
                StreamReader reader = new StreamReader(crlRawData);
                string key = reader.ReadToEnd();
                string final = key.Trim();
                crl = new X509CrlParser().ReadCrl((Encoding.ASCII.GetBytes(final)));
                if (crl == null)
                {
                    errorMessage.AppendFormat(Messaggi.ERROR_MESSAGE, "initX509CRL", "Non è stato possibile ricavare la CRL, controllare il file fornito");
                    errorMessage.AppendLine();
                    log.Error("Non è stato possibile ricavare la CRL, controllare il file fornito");
                    return null; 
                }
            }
            catch (Exception e)
            {
                errorMessage.AppendFormat(Messaggi.ERROR_MESSAGE, "initX509CRL", e.Message);
                errorMessage.AppendLine();
                log.Error("Errore in parsing CRL", e);
            }

            ISet set =  crl.GetRevokedCertificates();

            
            IEnumerator enumcrl = set.GetEnumerator();
            int i = 0;

            
            while (enumcrl.MoveNext())
            {
                X509CrlEntry  item = (X509CrlEntry)enumcrl.Current;
                log.Debug("Siamo al giro "+ i);
                log.Debug("Serial:"+ item.SerialNumber.ToString(16));
                log.Debug("RevokeDate:" + item.RevocationDate.ToString());
                i++;
                Certificato cert = new Certificato();
                cert.Serial=item.SerialNumber.ToString(16);
                cert.revoca = item.RevocationDate;
                updateStatoRevoca(cert, conn);

                /**DateTime halfYearAgo = DateTime.Now.AddMonths(-6);
                log.Debug("###Giro:" + i);
                if (cert.revoca < halfYearAgo)
                {
                    log.Debug("analizzando  data revoca" + cert.revoca);
                }
                else
                {
                    updateStatoRevoca(cert, conn);
                }
                 */
                // Perform logic on the item
            }
            
            

            return crl;
        }

        // crlRawData could a type of System.String and pass the path to a CRL file there.
        public static X509CrlEntry GetrevocationDate(BigInteger serial)
        {
            
            
            
            //X509Crl crl = new X509CrlParser().ReadCrl(crlRawData);

           
            //Console.Out.WriteLine(cert.SerialNumber);
            //X509CrlEntry entry = crl.GetRevokedCertificate(cert.SerialNumber);
            X509CrlEntry entry = crl.GetRevokedCertificate(serial);

            
            

            return entry;
        }


        // Roba Pattern


        public static string getBusinessCategory(string input)
        {

            //string input = "/content/alternate-1.aspx";
            string toreturn = null;
            // Here we call Regex.Match.
            Match match = Regex.Match(input, @"([A-Za-z]\d{2})[-,_]",
                RegexOptions.IgnoreCase);

            // Here we check the Match instance.
            if (match.Success)
            {
                // Finally, we get the Group value and display it.
                toreturn = match.Groups[1].Value;
                log.Debug("Rilevata la seguente business category" + toreturn);
            }

            return toreturn;
        }

        public static string getCodiceFiscaleFromCN(string input)
        {

            //string input = "/content/alternate-1.aspx";
            string toreturn = null;
            // Here we call Regex.Match.
            Match match = Regex.Match(input, @"([A-Za-z]{6}.*|\d{11})-\w{3}",
                RegexOptions.IgnoreCase);

            // Here we check the Match instance.
            if (match.Success)
            {
                // Finally, we get the Group value and display it.
                toreturn = match.Groups[1].Value;
                log.Debug("Rilevato il seguente Codice Fiscale" + toreturn);
            }

            return toreturn;
        }


        public static string getDateFromFileName(string input)
        {

            //string input = "/content/alternate-1.aspx";
            string toreturn = null;
            // Here we call Regex.Match.
            Match match = Regex.Match(input, @"\.(\d{8})\.",
                RegexOptions.IgnoreCase);

            // Here we check the Match instance.
            if (match.Success)
            {
                // Finally, we get the Group value and display it.
                toreturn = match.Groups[1].Value;
                log.Debug("Rilevata la seguente data nel nome del file" + toreturn);
            }

            return toreturn;
        }



        // Roba con DB#################################################################################################



        public static List<Certificato> serachActiveCertificate(SqlConnection conn)
        {
            List<Certificato> listToReturn = new List<Certificato>();
            string query = @"SELECT [serial] FROM [mybatchdb].[dbo].[EntratelCert] where revocato is null and scaduto >= GETDATE()";
            SqlCommand command = new SqlCommand(query, conn);            
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    Certificato cert = new Certificato();
                    cert.Serial = reader[0].ToString();
                    
                    listToReturn.Add(cert);
                }
            }
            finally
            {
                // Always call Close when done reading.
                reader.Close();
            }
            return listToReturn;
        }


        public static Boolean updateStatoRevoca(Certificato cert, SqlConnection conn)
        {
            //string update = @"update  [dbo].[EntratelCert] set DataRevoca=@revocato where SerialNumber =@serial";
            string update = @"spUpdateRevocaCertificato";
            SqlCommand cmd = new SqlCommand(update, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@SerialNumber", SqlDbType.VarChar, 50).Value = cert.Serial;
            cmd.Parameters.Add("@DataRevoca", SqlDbType.DateTime).Value = cert.revoca;
            SqlParameter output1 = new SqlParameter("@retcode", SqlDbType.Int);
            output1.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(output1);

            SqlParameter output2 = new SqlParameter("@msgCode", SqlDbType.VarChar, 200);
            output2.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(output2);
            try
            {
                cmd.ExecuteNonQuery();
                if (((int)output1.Value) == 1)
                {
                    log.Error("La SP di aggiornamento  ha restituito questo errore:" + output2.Value);
                }
            }
            catch (Exception ex)
            {
                errorMessage.AppendFormat(Messaggi.ERROR_MESSAGE, "updateStatoRevoca", ex.Message);
                errorMessage.AppendLine();
                log.Error("Errore durante update stato revoca", ex);
                return false;
            }
            return true;
        }

        public static int insertLogEvent(Certificato cert, SqlConnection conn)
        {
            int toreturn = 0;
            string insertLog = "spInsertLogEntratel";
            SqlCommand cmd = new SqlCommand(insertLog, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            DateTime now = DateTime.Now;
            //cmd.Parameters.Add("@data", SqlDbType.DateTime).Value = now;            
            cmd.Parameters.Add("@CodiceFiscale", SqlDbType.VarChar, 16).Value = cert.CodiceFiscale;
            cmd.Parameters.Add("@Esito", SqlDbType.Int).Value = cert.Esito;

            if (cert.Esito == 1) //1 errore SP;  2 dummycert
            {
                cmd.Parameters.Add("@Messaggio", SqlDbType.VarChar, 4000).Value = cert.Messaggio;
                cmd.Parameters.Add("@data", SqlDbType.DateTime).Value = now; 
            }
            else if (cert.Esito == 2)
            {
                cmd.Parameters.Add("@Messaggio", SqlDbType.VarChar, 4000).Value = cert.Messaggio;
                cmd.Parameters.Add("@data", SqlDbType.DateTime).Value = cert.data; 
            }
            else
            {
                cmd.Parameters.Add("@data", SqlDbType.DateTime).Value = now; 
                cmd.Parameters.Add("@Messaggio", SqlDbType.VarChar, 4000).Value = "Certificato correttamente inserito";
            }
            
            SqlParameter output1 = new SqlParameter("@retcode", SqlDbType.Int);
            output1.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(output1);

            SqlParameter output2 = new SqlParameter("@msgCode", SqlDbType.VarChar, 200);
            output2.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(output2);
            try
            {
                log.Debug("Inserimento Evento in LOG;" + cert.CodiceFiscale);
                cmd.ExecuteNonQuery();

                
               log.Debug("Esito Inserimento SP  dei log:" + output2.Value);
                
            }
            catch (SqlException ex)
            {
                errorMessage.AppendFormat(Messaggi.ERROR_MESSAGE, "insertLogEvent",ex.Message);
                errorMessage.AppendLine();
                log.Error("Errore durante l'inserimento in LogEvent", ex);

            }

            catch (Exception ex)
            {
                errorMessage.AppendFormat(Messaggi.ERROR_MESSAGE, "insertLogEvent", ex.Message);
                errorMessage.AppendLine();
                log.Error("Errore durante l'inserimento in LogEvent", ex);

            }
            return toreturn;
        }

        public static int insertCertificate(Certificato cert, SqlConnection conn)
        {
            int toreturn = 0;
            //BigInteger serial = new BigInteger("1988753");

            totali_fisiche++;






            //cmd.CommandType = CommandType.Text;
            //string insert = "INSERT INTO [dbo].[EntratelCert]([SerialNumber],[Tipo],[DataEmissione],[DataScadenza],[CodiceFiscale],[DataRevoca]) VALUES (";
            //insert += "@serial,@bcat,@emesso,@scaduto,@codfis,@revocato)";

            string insert = "spInsertCertificato";

            SqlCommand cmd = new SqlCommand(insert, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@SerialNumber", SqlDbType.VarChar, 50).Value = cert.Serial;
            cmd.Parameters.Add("@Tipo", SqlDbType.VarChar, 10).Value = cert.BusinessCategory;
            cmd.Parameters.Add("@DataEmissione", SqlDbType.DateTime).Value = cert.emissione;
            cmd.Parameters.Add("@DataScadenza", SqlDbType.DateTime).Value = cert.scadenza;
            cmd.Parameters.Add("@CodiceFiscale", SqlDbType.VarChar, 16).Value = cert.CodiceFiscale;
            cmd.Parameters.Add("@CommonName", SqlDbType.VarChar, 50).Value = cert.CommonName;
            SqlParameter output1 = new SqlParameter("@retcode", SqlDbType.Int);
            output1.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(output1);

            SqlParameter output2 = new SqlParameter("@msgCode", SqlDbType.VarChar,200);
            output2.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(output2);
           
            if (cert.revoca != null)
            {
                cmd.Parameters.Add("@DataRevoca", SqlDbType.DateTime).Value = cert.revoca;
            }
            else
            {
                cmd.Parameters.Add("@DataRevoca", SqlDbType.DateTime).Value = DBNull.Value;
            }
            try
            {
                log.Debug("Inserimento Seriale;" + cert.Serial);
                cmd.ExecuteNonQuery();

                if (((int)output1.Value) == 1)
                {
                    cert.Esito = 1;
                    cert.Messaggio = output2.Value + "";
                    log.Error("La SP di inserimento ha restituito questo errore:" + output2.Value);

                }
                else
                {
                    inseriti++;
                }
                insertLogEvent(cert, conn);
            }
            catch (SqlException ex)
            {
                errorMessage.AppendFormat(Messaggi.ERROR_MESSAGE, "insertCertificate", ex.Message);
                errorMessage.AppendLine();
                cert.Esito = 1;
                cert.Messaggio = ex.StackTrace;
                insertLogEvent(cert, conn);
                log.Error("Errore durante l'inserimento", ex);

            }
            catch (Exception ex)
            {
                errorMessage.AppendFormat(Messaggi.ERROR_MESSAGE, "insertCertificate", ex.Message);
                errorMessage.AppendLine();
            }
            return toreturn;
        }

    }
}
