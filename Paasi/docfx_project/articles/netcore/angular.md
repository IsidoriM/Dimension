<h1>Integrazione PASSI</h1>

Questa guida spiega come integrare e utilizzare PASSI all'interno di una applicazione WEB realizzata in .NetCore, versione 6.0 o superiore, attraverso la quale viene fornita una applicazione Angular/React o una qualsiasi altra Single Page Application.

In questo caso, la parte in tecnologia dotnet dovrà fornire una singola pagina dove verrà renderizzata l'applicazione, e una serie di API che verranno consumate dalla stessa per poter mostrare i propri contenuti

<h2>Inizializzazione</h2>
Per l'utilizzo di PASSI è sufficiente effettuare le seguenti configurazioni nello Startup o nel Program della tua applicazione.

Nella fase di creazione dei servizi, aggiungere ai servizi le funzionalità di PASSI

```
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddPassiAuthentication(builder.Configuration);

var app = builder.Build();
```

Inoltre, permetterà di utilizzare all'interno dei propri servizi le implementazioni di 

| Servizio | Descrizione |
| -------- | ----------- |
| <a href="/api/Passi.Core.Application.Services.IPassiService.html">IPassiService</a> | Servizio per ottenere i dati dell'utente loggato |
| <a href="/api/Passi.Core.Application.Services.IPassiSecureService.html">IPassiSecureService</a> | Servizio per ottenere l'access token e per effettuare la cifratura dei dati |
| <a href="/api/Passi.Core.Application.Services.IPassiConventionService.html">IPassiConventionService</a> | Servizio per ottenere le convenzioni per l'utente loggato |
| <a href="/api/Passi.Core.Application.Services.ICLogService.html">ICLogService</a> | Servizio di CLog |

Successivamente, nella fase di creazione del builder applicativo, aggiungere ai servizi UsePassiAuthentication

```
app.UseRouting();

app.UsePassiAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

Questo permetterà alla tua applicazione di gestire l'autenticazione tramite i classici tag [Authorize] da inserire in capo alle Action/Controller

Un controller in tecnologia dotnet, dovrà fornire la root per l'applicazione SPA. Questa chiamata equivale ad una generica pagina Razor, che restituirà una View autenticata.

E' importante che questa chiamata non abbia come root /api, ma abbia un qualsiasi altro path (ad esempio /home/index)

La chiamata sarà utile anche per generare il SessionToken che l'app userà poi durante tutto il proprio ciclo di vita

```
[Authorize]
[HttpGet("/home/index")]
public async Task<IActionResult> Index()
{
    var token = await passiSecureService.SessionTokenAsync();
    ViewBag.Token = token;
    return View();
}
```

La view così generata potrà fornire il SessionToken all'app SPA, da utilizzare in tutte le successive chiamate.
Si consiglia di memorizzare questo token all'interno del session storage.

Tutte le successive chiamate saranno delle API la cui autenticazione prevede che venga passato anche il token generato tramite l'header SessionToken.
E' importante inoltre che i path di queste chiamate siano tutti con prefisso /api

```
[Authorize]
[HttpGet("/api/me")]
public async Task<IActionResult> Me()
{
    var me = await passiService.MeAsync();
    return Json(me);
}
```