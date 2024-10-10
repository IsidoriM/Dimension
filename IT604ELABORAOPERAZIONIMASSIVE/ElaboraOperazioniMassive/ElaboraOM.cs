using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElaboraOperazioniMassive.DAL;
using ElaboraOperazioniMassive.Operazioni;
using System.Data;

namespace ElaboraOperazioniMassive
{
    class ElaboraOM
    {

        static void InvioMailElaborazione(int contatoreRimozioneok, int contatoreRimozioneko)
        {
            string descrizionePassaggio = "";
            try
            {
                System.Text.StringBuilder RiepilogoElaborazione = new System.Text.StringBuilder();
                //Aggiunge tre righe di separazione
                RiepilogoElaborazione.AppendLine("-----------------------------------------------------------------------------------------------------");
                RiepilogoElaborazione.AppendLine(" ------------------------------------ RIEPILOGO ELABORAZIONE OPERAZIONI MASSIVE ---------------------------");
                RiepilogoElaborazione.AppendLine(" ------------------------------------                                          ---------------------------");
                RiepilogoElaborazione.AppendLine(String.Format("Rimozioni Elaborate:                                {0}", contatoreRimozioneok.ToString("N0")));
                RiepilogoElaborazione.AppendLine("");
                RiepilogoElaborazione.AppendLine(String.Format("Rimozioni non Elaborate:                            {0}", contatoreRimozioneko.ToString("N0")));
                MAIL invioMail = new MAIL();
                descrizionePassaggio = "Invio:";
                invioMail.InvioMail(RiepilogoElaborazione, false);
            }
            catch (Exception ex)
            {
                StringBuilder StringErrore = new StringBuilder();
                StringErrore.Append(descrizionePassaggio);
                StringErrore.Append(ex.Message.ToString());
                throw new Exception(StringErrore.ToString());
            }
        }

        static void Main(string[] args)
        {
            //Carico il dataset delle operazioni massive con Data=null 
            int contok=0;
            int contko=0;
            SicurezzaDAL GetOperazioni = new SicurezzaDAL();
            DataSet ListaOperazioni = new DataSet();
            //Console.WriteLine("20201026nuovalista" + ListaOperazioni.Tables.Count.ToString());
            ListaOperazioni = GetOperazioni.GetOperazioniMassive();
            
            //Per ogni elemento in Lista Operazioni

            try
            {
                foreach ( DataTable table in ListaOperazioni.Tables)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        //Console.WriteLine("20201026righetabella" + table.Rows.Count.ToString());
                        //variabile che assume valore r se si vuole effettuare rimozione e a se si vuole effettuare autorizzazione 
                        String flagar = row["Assegnazione/Rimozione"].ToString().Trim();
                  
                    
                        // Revoca in profilo Servizio
                        if (flagar=="r" ||flagar =="R")
                        {   
                            //Console.WriteLine(row["Utente"].ToString()+"Rimozione Autorizzazioni");
                            Revoca revoca = new Revoca();
                            int  revocaeffettuata = revoca.rimozione(row);
                            if (revocaeffettuata <8 && revocaeffettuata > 0)
                                {
                                    //Console.WriteLine(row["Utente"].ToString() + "Autorizzazioni Rimosse retcode: "+revocaeffettuata);
                                    int updata= GetOperazioni.UpdateData(int.Parse(row["idOperazioneDettaglio"].ToString()));
                                    contok++;
                                }

                            else
                            {
                                Console.WriteLine(row["Utente"].ToString() + "Errore stored procedure");
                                contko++;
                            }

                        }


                    }
                }


                InvioMailElaborazione(contok, contko);
            }
            catch (Exception exCons)
            {
                Console.Write(exCons.Message);
                throw exCons;
            }
            
           



        }
    }
}
