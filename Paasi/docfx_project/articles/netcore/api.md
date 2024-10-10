<h1>Come richiamare delle API integrate con PASSI</h1>

Una Single Page Application dovrà molto spesso interfacciarsi con delle API costruite appositamente per lei.
Queste API dovranno a loro volta essere integrate con PASSI e dovrà esser loro fornito lo stesso utente usato nell'applicazione SPA.

<h2>Prerequisiti</h2>
Affinchè le API possano essere richiamate dalla SPA e possano condividere i dati dell'utente loggato, è necessario che:

<ul>
  <li>Le API e il servizio che le usa siano configurati per utilizzare lo stesso ServiceId</li>
  <li>Le API e il servizio che le usa devono stare sullo sesso dominio *inps.it (ad esempio, https://servizio.web.inps.it e https://servizio.api.inps.it)</li>
  <li>Le chiamate verso le API devono essere fatte inserendo negli Header il valore del SessionToken</li>
</ul>

<h2>SessionToken</h2>
Ad ogni chiamata verso le API, la SPA dovrà aggiungere agli header i seguenti campi:

| Chiave | Valore | Descrizione |
| -------- | ----------- | ----------- |
| SessionToken | {token di sessione} | Token generato attraverso la funzione SessionToken() di IPassiSecure |


