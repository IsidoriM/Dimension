using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PINProvUtilita.Models
{
    [Serializable]
    public class TipoDocumento
    {
        private string descrizione;

        private string idTipoDocumento;

        public string Descrizione
        {
            get
            {
                return this.descrizione;
            }

            set
            {
                this.descrizione = ToDefault(value);
            }
        }

        public string IdTipoDocumento
        {
            get
            {
                return this.idTipoDocumento;
            }

            set
            {
                this.idTipoDocumento = value;
            }
        }

        private static string ToDefault(string value)
        {
            return !string.IsNullOrEmpty(value) ? value.ToUpperInvariant() : value;
        }
    }
}