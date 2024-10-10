<img src="https://www.inps.it/etc.clientlibs/inps-site-common/clientlibs/clientlib-commons/resources/img/logo-inps.svg" width="20"> **Libreria PASSI**
===================

Il presente progetto contiene la libreria e la documentazione di **PASSI** per applicativi .NET 6 o superiore.<br/>
PASSI si occupa, per le applicazioni Web Internet, di centralizzare l’autenticazione e l’autorizzazione dell’utente che accede ai servizi dell’Istituto.

# Struttura del codice
Il progetto contiene sia la libreria **PASSI** di produzione (*Passi.Authentication.Cookie*), sia la libreria **PASSI Fake** (*Passi.Authentication.Fake*) che gli sviluppatori possono referenziare andando a scaricare il pacchetto *Nuget* dal repository Nexus dell'istituto.

In particolare la gerarchia delle dipendenze delle due librerie segue questa struttura:
1.	Passi.Authentication.Cookie
    - Passi.Core
    - Passi.Core.Services
    - Passi.Core.Store.Sql
2.	Passi.Authentication.Fake
    - Passi.Core
    - Passi.Core.Services
    - Passi.Core.Store.Fake

# Test
All'interno della directory **Test** sono presenti test di unità e test di integrazione.<br/>
Fare attenzione alla code coverage raggiunta.

# Documentazione per lo sviluppatore
È presente una documentazione interattiva per lo sviluppatore all'interno della directory **docfx_project**.

Per prima cosa installare o aggiornare docfx
```console
dotnet tool update -g docfx
```
È possibile consultarla lanciando questo comando locale in CMD o Shell:
```console
docfx docfx_project/docfx.json --serve
```
Una volta eseguito il comando dirigersi sulla Url http://localhost:8080/.

È possibile generare una documentazione Html offline lanciando il comando locale in CMD o Shell:
```console
docfx docfx_project/docfx.json
```
La documentazione si troverà al seguente percorso *docfx_project/_site/index.html*.
