using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PINProvUtilita.Models
{
    [Serializable]
    public class ListaContatti
    {
        string codiceFiscale;

        public string CodiceFiscale
        {
            get { return codiceFiscale; }
            set { codiceFiscale = value; }
        }

        string cognome;

        public string Cognome
        {
            get { return cognome; }
            set { cognome = value; }
        }

        string nome;

        public string Nome
        {
            get { return nome; }
            set { nome = value; }
        }

        string telefono;

        public string Telefono
        {
            get { return telefono; }
            set { telefono = value; }
        }

        string cellulare;

        public string Cellulare
        {
            get { return cellulare; }
            set { cellulare = value; }
        }

        string email;

        public string Email
        {
            get { return email; }
            set { email = value; }
        }

        string pec;

        public string Pec
        {
            get { return pec; }
            set { pec = value; }
        }

        string data_ultimo_accesso;

        public string Data_ultimo_accesso
        {
            get { return data_ultimo_accesso; }
            set { data_ultimo_accesso = value; }
        }

        string infoprivacy;

        public string Infoprivacy
        {
            get { return infoprivacy; }
            set { infoprivacy = value; }
        }

        string statoverificacellulare;

        public string Statoverificacellulare
        {
            get { return statoverificacellulare; }
            set { statoverificacellulare = value; }
        }

        string statoverificaemail;

        public string Statoverificaemail
        {
            get { return statoverificaemail; }
            set { statoverificaemail = value; }
        }

        string statoverificapec;

        public string Statoverificapec
        {
            get { return statoverificapec; }
            set { statoverificapec = value; }
        }

    }
}