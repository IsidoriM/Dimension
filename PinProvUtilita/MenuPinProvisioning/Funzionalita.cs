using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MenuPinProvisioning
{
    [Serializable]
    public class Funzionalita
    {
        private bool attivo;
        private string descrizione;
        private int idFunzionalita;
        private int? idFunzionalitaPadre;
        private string testoAlternativo;
        private string url;

        public bool Attivo
        {
            get { return attivo; }
            set { attivo = value; }
        }

        public string Descrizione
        {
            get { return descrizione; }
            set { descrizione = value; }
        }

        public int IdFunzionalita
        {
            get { return idFunzionalita; }
            set { idFunzionalita = value; }
        }

        public int? IdFunzionalitaPadre
        {
            get { return idFunzionalitaPadre; }
            set { idFunzionalitaPadre = value; }
        }

        public string Target { get; set; }

        public string TestoAlternativo
        {
            get { return this.testoAlternativo ?? string.Empty; }
            set { this.testoAlternativo = value; }
        }

        public string Url
        {
            get { return url; }
            set { url = value; }
        }
    }
}
