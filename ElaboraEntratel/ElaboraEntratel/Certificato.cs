using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mycertificatebatch
{
    class Certificato
    {
        private int _Esito;
        public int Esito
        {
            get { return _Esito; }
            set { if (true) { _Esito = value; } }
        }


        private string _Messaggio;
        public string Messaggio
        {
            get { return _Messaggio; }
            set { if (true) { _Messaggio = value; } }
        }

        private string _Serial;
        public string Serial
        {
            get { return _Serial; }
            set { if (true) {_Serial= value;} }
        }

        private string _CodiceFiscale;
        public string CodiceFiscale
        {
            get { return _CodiceFiscale; }
            set { if (true) { _CodiceFiscale = value; } }
        }

        private string _BusinessCategory;
        public string BusinessCategory
        {
            get { return _BusinessCategory; }
            set { if (true) { _BusinessCategory = value; } }
        }

        private DateTime? _data;
        public DateTime? data
        {
            get { return _data; }
            set { if (true) { _data = value; } }
        }


        private DateTime? _emissione;
        public DateTime? emissione
        {
            get { return _emissione; }
            set { if (true) { _emissione = value; } }
        }

        private DateTime? _scadenza;
        public DateTime? scadenza
        {
            get { return _scadenza; }
            set { if (true) { _scadenza = value; } }
        }

        private DateTime? _revoca;
        public DateTime? revoca
        {
            get { return _revoca; }
            set { if (true) { _revoca = value; } }
        }

        private string _MotivoRevoca;
        public string MotivoRevoca
        {
            get { return _MotivoRevoca; }
            set { if (true) { _MotivoRevoca = value; } }
        }

        private string _CommonName;
        public string CommonName
        {
            get { return _CommonName; }
            set { if (true) { _CommonName = value; } }
        }

    }
}
