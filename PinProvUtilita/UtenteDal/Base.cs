using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace UtenteDal
{
    public abstract class Base
    {
        #region Oggetti e metodi DB

        public SqlTransaction Transazione = null;
        public SqlConnection Connessione = null;

        internal System.Data.DataTable FillDataSet(string StringaConnessione, string ComandoSQL,
                                            bool Stored, params SqlParameter[] Parametri)
        {
            DataTable ritorno = new DataTable();
            SqlConnection connessione = new SqlConnection(StringaConnessione);
            SqlCommand oggettoCommand = new SqlCommand();

            try
            {
                oggettoCommand.CommandText = ComandoSQL;
                oggettoCommand.Connection = connessione;
                oggettoCommand.CommandType = (Stored ? CommandType.StoredProcedure : CommandType.Text);
                AllegaParametri(oggettoCommand, Parametri);
                using (SqlDataAdapter oggettoDataAd = new SqlDataAdapter(oggettoCommand))
                {
                    oggettoDataAd.Fill(ritorno);
                }
            }
            catch
            {
                throw;
            }

            return ritorno;
        }

        internal SqlDataReader ExecuteReader(string StringaConnessione, params SqlParameter[] Parametri)
        {
            SqlDataReader ritorno = null;
            if (StringaConnessione == "")
                throw new ArgumentNullException("Connessione");

            SqlCommand oggettoCommand = new SqlCommand();
            bool connessioneAperta = false;

            SqlConnection connessione = null;
            try
            {
                connessione = new SqlConnection(StringaConnessione);
                PreparaOggettoCommand(oggettoCommand, connessione, null, CommandType.StoredProcedure,
                                        this.nomeSp, Parametri, out connessioneAperta);
                ritorno = oggettoCommand.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch
            {
                if (connessioneAperta)
                    connessione.Close();
                throw;
            }
            return ritorno;
        }

        internal SqlDataReader ExecuteReader(string StringaConnessione, string ComandoSQL, bool Stored)
        {
            SqlDataReader ritorno = null;
            if (StringaConnessione == "")
                throw new ArgumentNullException("Connessione");

            SqlCommand oggettoCommand = new SqlCommand();
            bool connessioneAperta = false;

            SqlConnection connessione = null;
            try
            {
                connessione = new SqlConnection(StringaConnessione);
                PreparaOggettoCommand(oggettoCommand, connessione, null,
                                        (Stored ? CommandType.StoredProcedure : CommandType.Text),
                                        ComandoSQL, null, out connessioneAperta);
                ritorno = oggettoCommand.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch
            {
                if (connessioneAperta)
                    connessione.Close();
                throw;
            }

            return ritorno;
        }

        internal int ExecuteNonQuery(string StringaConnessione, bool ApriTransazione, params SqlParameter[] Parametri)
        {
            int ritorno = 0;
            if (StringaConnessione == "")
                throw new ArgumentNullException("StringaConnessione");

            SqlCommand oggettoCommand = new SqlCommand();
            bool connessioneAperta = false;
            try
            {
                Connessione = new SqlConnection(StringaConnessione);
                if (ApriTransazione)
                {
                    Connessione.Open();
                    connessioneAperta = true;
                    Transazione = Connessione.BeginTransaction();
                }

                PreparaOggettoCommand(oggettoCommand, Connessione, Transazione, CommandType.StoredProcedure,
                                        this.nomeSp, Parametri, out connessioneAperta);
                ritorno = oggettoCommand.ExecuteNonQuery();
                oggettoCommand.Parameters.Clear();
            }
            catch
            {
                throw;
            }
            finally
            {
                if ((connessioneAperta) && !ApriTransazione)
                    Connessione.Close();
            }

            return ritorno;
        }

        internal int ExecuteNonQuery(ref SqlTransaction Transazione, ref SqlConnection Connessione, params SqlParameter[] Parametri)
        {
            int ritorno = 0;
            if (Transazione == null)
                throw new ArgumentNullException("Transazione");

            SqlCommand oggettoCommand = new SqlCommand();
            try
            {
                PreparaOggettoCommand(oggettoCommand, ref Connessione, ref Transazione, CommandType.StoredProcedure,
                                        this.nomeSp, Parametri);
                ritorno = oggettoCommand.ExecuteNonQuery();
                oggettoCommand.Parameters.Clear();
            }
            catch
            {
                throw;
            }

            return ritorno;
        }

        internal int ExecuteNonQuery(ref SqlTransaction Transazione, ref SqlConnection Connessione, string ComandoSQL)
        {
            int ritorno = 0;
            if (Transazione == null)
                throw new ArgumentNullException("Transazione");

            SqlCommand oggettoCommand = new SqlCommand();
            try
            {
                PreparaOggettoCommand(oggettoCommand, ref Connessione, ref Transazione, CommandType.Text,
                                        ComandoSQL, null);
                ritorno = oggettoCommand.ExecuteNonQuery();
            }
            catch
            {
                throw;
            }

            return ritorno;
        }

        internal int ExecuteNonQuery(ref SqlTransaction Transazione, ref SqlConnection Connessione, string ComandoSQL, params SqlParameter[] Parametri)
        {
            int ritorno = 0;
            if (Transazione == null)
                throw new ArgumentNullException("Transazione");

            SqlCommand oggettoCommand = new SqlCommand();
            try
            {
                PreparaOggettoCommand(oggettoCommand, ref Connessione, ref Transazione, CommandType.Text, ComandoSQL, Parametri);
                ritorno = oggettoCommand.ExecuteNonQuery();
                oggettoCommand.Parameters.Clear();
            }
            catch
            {
                throw;
            }

            return ritorno;
        }

        internal object ExecuteScalar(string StringaConnessione, params SqlParameter[] Parametri)
        {
            object ritorno = null;
            if (StringaConnessione == "")
                throw new ArgumentNullException("StringaConnessione");

            SqlCommand oggettoCommand = new SqlCommand();
            bool connessioneAperta = false;
            SqlConnection connessione = null;
            try
            {
                connessione = new SqlConnection(StringaConnessione);
                PreparaOggettoCommand(oggettoCommand, connessione, null, CommandType.StoredProcedure,
                                        this.nomeSp, Parametri, out connessioneAperta);
                ritorno = oggettoCommand.ExecuteScalar();
                oggettoCommand.Parameters.Clear();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (connessioneAperta)
                    connessione.Close();
            }

            return ritorno;
        }

        internal SqlParameter CreaParametro(string Nome, SqlDbType Tipo, int Dimensione,
                                                    bool Input, Object Valore)
        {
            SqlParameter ritorno = new SqlParameter(Nome, Tipo);

            if (Dimensione > 0)
                ritorno.Size = Dimensione;

            if (!Input)
                ritorno.Direction = ParameterDirection.Output;
            else
                ritorno.Value = Valore;

            return ritorno;
        }
        internal SqlParameter CreaParametroRichiesta(string Nome, SqlDbType Tipo, int Dimensione,
                                                    bool Input, Object Valore)
        {
            SqlParameter ritorno = new SqlParameter(Nome, Tipo);

            if (Dimensione > 0)
                ritorno.Size = Dimensione;

            if (!Input)
                ritorno.Direction = ParameterDirection.Output;
            else
                ritorno.Value = Valore;

            return ritorno;
        }

        internal SqlParameter CreaParametroRichiesta(string Nome, DbType Tipo, bool Input, Object Valore)
        {
            SqlParameter ritorno = new SqlParameter(Nome, Tipo);

            ritorno.DbType = Tipo;

            if (!Input)
                ritorno.Direction = ParameterDirection.Output;
            else
                ritorno.Value = Valore;

            return ritorno;
        }
        //internal SqlParameter CreaParametro(string Nome, SqlDbType Tipo, int Dimensione,
        //                                            bool Input, Object Valore)
        //{
        //    SqlParameter ritorno = new SqlParameter(Nome, Tipo);

        //    if (Dimensione > 0)
        //        ritorno.Size = Dimensione;

        //    if (!Input)
        //        ritorno.Direction = ParameterDirection.Output;
        //    else
        //        ritorno.Value = Valore;

        //    return ritorno;
        //}

        //internal SqlParameter CreaParametroRichiesta(string Nome, DbType Tipo, bool Input, Object Valore)
        //{
        //    SqlParameter ritorno = new SqlParameter(Nome, Tipo);

        //    ritorno.DbType = Tipo;

        //    if (!Input)
        //        ritorno.Direction = ParameterDirection.Output;
        //    else
        //        ritorno.Value = Valore;

        //    return ritorno;
        //}  
        private void PreparaOggettoCommand(SqlCommand OggettoCommand, SqlConnection Connessione,
                                                    SqlTransaction Transazione, CommandType TipoCommand,
                                                    string TestoCommand, SqlParameter[] Parametri,
                                                    out bool ConnessioneAperta)
        {
            if (OggettoCommand == null)
            {
                throw new ArgumentNullException("OggettoCommand");
            }
            if ((TestoCommand == null) || (TestoCommand.Length == 0))
            {
                throw new ArgumentNullException("TestoCommand");
            }
            if (Connessione.State != ConnectionState.Open)
            {
                ConnessioneAperta = true;
                Connessione.Open();
            }
            else
            {
                ConnessioneAperta = false;
            }
            OggettoCommand.Connection = Connessione;
            OggettoCommand.CommandText = TestoCommand;

            if (Transazione != null && Transazione.Connection != null)
                OggettoCommand.Transaction = Transazione;

            OggettoCommand.CommandType = TipoCommand;
            if (Parametri != null)
                AllegaParametri(OggettoCommand, Parametri);
        }

        private void PreparaOggettoCommand(SqlCommand OggettoCommand, ref SqlConnection Connessione,
                                                    ref SqlTransaction Transazione, CommandType TipoCommand,
                                                    string TestoCommand, SqlParameter[] Parametri)
        {
            if (OggettoCommand == null)
                throw new ArgumentNullException("OggettoCommand");

            if ((TestoCommand == null) || (TestoCommand.Length == 0))
                throw new ArgumentNullException("TestoCommand");

            OggettoCommand.Connection = Connessione;
            OggettoCommand.CommandText = TestoCommand;
            if (Transazione != null)
            {
                if (Transazione.Connection == null)
                    throw new ArgumentException("Transazione chiusa: è necessaria una transazione attiva.", "Transazione");

                OggettoCommand.Transaction = Transazione;
            }
            OggettoCommand.CommandType = TipoCommand;
            if (Parametri != null)
                AllegaParametri(OggettoCommand, Parametri);
        }

        private void AllegaParametri(SqlCommand OggettoCommand, SqlParameter[] Parametri)
        {
            if (OggettoCommand == null)
                throw new ArgumentNullException("OggettoCommand");

            if (Parametri != null)
            {
                foreach (SqlParameter parametro in Parametri)
                {
                    if (parametro != null)
                    {
                        if (((parametro.Direction == ParameterDirection.InputOutput) ||
                                (parametro.Direction == ParameterDirection.Input)) && (parametro.Value == null))
                            parametro.Value = DBNull.Value;
                        OggettoCommand.Parameters.Add(parametro);
                    }
                }
            }
        }

        #endregion Oggetti e metodi DB

        #region Oggetti Comuni

        internal string nomeSp = string.Empty;
        //internal readonly string connAssegnazionePIN;
        internal readonly string connSicurezzaPinProvisioning;
        internal readonly string connAssegnazionePINProvisiong;
        internal readonly string connSicurezzaClogPinProvisioning;
        internal readonly string connDocumentazionePinProvisioning;
        internal readonly string connUtenzePinProvisioning;
        internal readonly string connSegnalazioniPinProvisioning;
        internal readonly string connSegnalazioniSicurezzaPinProvisioning;
        internal readonly string connVerificaContatti;

        //internal readonly string connDBS_Comuni;


        internal enum FormatoData
        {
            GGMMAAAA,
            AAAAMMGG,
            AAAAGGMM
        }

        public Base()
        {
            if (ConfigurationManager.ConnectionStrings["SicurezzaPinProvisioning"] != null)
            {
                connSicurezzaPinProvisioning = ConfigurationManager.ConnectionStrings["SicurezzaPinProvisioning"].ConnectionString;
            }
            if (ConfigurationManager.ConnectionStrings["AssegnazionePINProvisiong"] != null)
            {
                connAssegnazionePINProvisiong = ConfigurationManager.ConnectionStrings["AssegnazionePINProvisiong"].ConnectionString;
            }
            if (ConfigurationManager.ConnectionStrings["SicurezzaClogPinProvisioning"] != null)
            {
                connSicurezzaClogPinProvisioning = ConfigurationManager.ConnectionStrings["SicurezzaClogPinProvisioning"].ConnectionString;
            }
            if (ConfigurationManager.ConnectionStrings["DocumentazionePinProvisioning"] != null)
            {
                connDocumentazionePinProvisioning = ConfigurationManager.ConnectionStrings["DocumentazionePinProvisioning"].ConnectionString;
            }
            if (ConfigurationManager.ConnectionStrings["UtenzePinProvisioning"] != null)
            {
                connUtenzePinProvisioning = ConfigurationManager.ConnectionStrings["UtenzePinProvisioning"].ConnectionString;
            }
            //if (ConfigurationManager.ConnectionStrings["dbs_comuni"] != null)
            //{
            //    connDBS_Comuni = ConfigurationManager.ConnectionStrings["dbs_comuni"].ConnectionString;
            //}
            if (ConfigurationManager.ConnectionStrings["SegnalazioniPinProvisioning"] != null)
            {
                connSegnalazioniPinProvisioning = ConfigurationManager.ConnectionStrings["SegnalazioniPinProvisioning"].ConnectionString;
            }
            if (ConfigurationManager.ConnectionStrings["SegnalazioniSicurezzaPinProvisioning"] != null)
            {
                connSegnalazioniSicurezzaPinProvisioning = ConfigurationManager.ConnectionStrings["SegnalazioniSicurezzaPinProvisioning"].ConnectionString;
            }

            if (ConfigurationManager.ConnectionStrings["VerificaContatti"] != null)
            {
                connVerificaContatti = ConfigurationManager.ConnectionStrings["VerificaContatti"].ConnectionString;
            }

        }

        //internal int _codiceRitorno;

        //public int CodiceRitornoSP
        //{
        //    get { return _codiceRitorno; }
        //}

        //internal string _descrizioneRitorno;

        //public string DescrizioneRitornoSP
        //{
        //    get { return _descrizioneRitorno; }
        //}

        internal object NullSeVuoto(string Valore)
        {
            if (Valore != "")
                return Valore;
            else
                return null;
        }

        internal object NullSeVuoto(int Valore)
        {
            if (Valore > 0)
                return Valore;
            else
                return null;
        }

        internal object NullSeVuoto(float Valore)
        {
            if (Valore > 0)
                return Valore;
            else
                return null;
        }

        internal object NullSeZero(int Valore)
        {
            if (Valore == 0)
                return null;
            else
                return Valore;
        }

        internal object NullSeZero(float Valore)
        {
            if (Valore == 0F)
                return null;
            else
                return Valore;
        }

        internal string VuotoSeNull(object Valore)
        {
            if (Valore == null)
                return "";
            else
                return Valore.ToString();
        }

        internal string VuotoSeNullDB(object Valore)
        {
            if (Valore == DBNull.Value)
                return "";
            else
                return Valore.ToString();
        }

        internal string DataFormatoRidotto(object Valore)
        {
            if (!Valore.Equals(DBNull.Value) && !(string.IsNullOrEmpty(Valore.ToString())))
            {
                return (Convert.ToDateTime(Valore)).ToShortDateString();
            }
            else
                return TrattinoSeVuoto("");
        }

        internal string TrattinoSeVuoto(object Valore)
        {
            bool vuoto = false;

            if (Valore == DBNull.Value)
                vuoto = true;
            else if (string.IsNullOrEmpty(Valore.ToString()))
                vuoto = true;

            if (vuoto)
                return "--";
            else
                return Valore.ToString();
        }

        internal string FormattaData(DateTime Data, FormatoData Formato)
        {
            string ritorno = "";

            switch (Formato)
            {
                case FormatoData.GGMMAAAA:
                    ritorno = String.Format("{0}/{1}/{2}", Data.Day.ToString().PadLeft(2, '0'),
                                                    Data.Month.ToString().PadLeft(2, '0'),
                                                    Data.Year.ToString());

                    break;
                case FormatoData.AAAAMMGG:
                    ritorno = String.Format("{0}-{1}-{2}", Data.Year.ToString(),
                                                    Data.Month.ToString().PadLeft(2, '0'),
                                                    Data.Day.ToString().PadLeft(2, '0'));

                    break;
                case FormatoData.AAAAGGMM:
                    ritorno = String.Format("{0}-{1}-{2}", Data.Year.ToString(),
                                                    Data.Day.ToString().PadLeft(2, '0'),
                                                    Data.Month.ToString().PadLeft(2, '0'));
                    break;
            }

            return ritorno;
        }

        internal string FormattaData(string Data, FormatoData Formato)
        {
            string ritorno = "";

            if (Data != "")
            {
                switch (Formato)
                {
                    case FormatoData.GGMMAAAA:
                        ritorno = String.Format("{0}/{1}/{2}", Data.Substring(0, 2),
                                                        Data.Substring(3, 2),
                                                        Data.Substring(6, 4));
                        break;
                    case FormatoData.AAAAMMGG:
                        ritorno = String.Format("{0}-{1}-{2}", Data.Substring(6, 4),
                                                        Data.Substring(3, 2),
                                                        Data.Substring(0, 2));
                        break;
                    case FormatoData.AAAAGGMM:
                        ritorno = String.Format("{0}-{1}-{2}", Data.Substring(6, 4),
                                                        Data.Substring(0, 2),
                                                        Data.Substring(3, 2));
                        break;
                }
            }

            return ritorno;
        }

        #endregion Oggetti Comuni
        //internal string nomeSp = string.Empty;
        //struct pubblica per le informazioni degli errori (codice e descrizione)
        //
        public struct errore
        {
            public int codiceErrore;
            public string descrizioneErrore;
        }

        public errore dettaglioErrore;
    }
}
