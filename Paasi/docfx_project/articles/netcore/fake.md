<h1>Uso in sviluppo della libreria PASSI</h1>

Questa guida spiega come utilizzare in fase di sviluppo locale la libreria PASSI (che può essere trovata su Nexus) in modo da poter testare le funzionalità di autenticazione.
L'autenticazione avverrà localmente sull'applicativo in base ad alcuni dati inseriti dall'utente.
Quando l'applicazione verrà deployata, la libreria PASSI verrà sostituita con la versione di produzione, in modo da potersi correttamente integrare con il SSO di Inps


<h2>Come impostare l'utente loggato</h2>
Lo sviluppatore può definire all'interno del suo appsettings.json quale dovrà essere l'utente che effettua il login andando a definirne le caratteristiche.

Per far questo, basta definire nel proprio appsettings la seguente configurazione:

```
"User": 
{
    "UserId": "VRDGNN64C10L117Y",
    "FiscalCode": "VRDGNN64C10L117Y",
    "Name": "Giovanni",
    "Surname": "Verdi",
    "Sex": "Male",
    "Email": "giovanniverdi@email.it",
    "PEC": "giovanniverdi@pec.it",
    "Profile": 
    {
      "ProfileId": 3,
      "EnteId": "CITTADINO",
      "EnteDescription": "Cittadino",
      "OfficeCode": ""    
    }
}
```

Nel caso in cui non siano forniti alcuni valori, questi verranno generati automaticamente.

Se non si è pertanto interessati a provare con un utente specifico che si vuole passare da configurazione, è possibile anche lasciare il tag user vuoto nell'appsettings (o non definirlo affatto).

L'utente così definito sarà quello restituito dalle varie funzionalità di IPassiService