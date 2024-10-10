using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElaboraOperazioniMassive.Entities
{
    public enum LogEvents
    {
        AttivazioneRichiestaPin = 1,
        InserimentoUtente = 2,
        ModificaUtente = 3,
        RevocaUtente = 4,
        CreazioneProfilo = 5,
        RevocaProfilo = 6,
        AssociazioneGruppo = 7,
        RevocaGruppo = 8,
        AssociazioneUfficio = 9,
        RevocaUfficio = 10,
        AssociazioneServizio = 11,
        RevocaServizio = 12,
        RiassegnazioneProgressivoBusta = 13,
        ////InserimentoRichiestaOnline = 14,                 //// non usato
        ////ModificaRichiestaOnline = 15,                    //// non usato
        RespingimentoRichiestaPin = 16,
        ////InserimentoUfficio = 17,                         //// non usato
        ////AggiornamentoUfficio = 18,                       //// non usato
        ////CancellazioneUfficio = 19,                       //// non usato
        AssegnazioneOtp = 20,
        RevocaOtp = 21,
        Errore = 22,
        TestOtp = 23,
        ////ModificaGruppoOUffici = 24,                      //// non usato
        ////ModificaProfilo = 25,                            //// non usato
        ////RevocaAutorizzazioneSpecifica = 26,              //// non usato
        CaricamentoDocumento = 27,
        ReinvioSecondaPartePin = 28,
        AggiornamentoPinDaOnlineADispositivo = 29,
        RespingimentoRichiestaPinDaOnlineADispositivo = 30,
        SospensioneProfilo = 31,
        RiattivazioneProfilo = 32,
    }
}
