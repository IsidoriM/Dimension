<h1>Integrazione PASSI</h1>

Questa guida spiega come integrare e utilizzare PASSI all'interno di una applicazione WEB realizzata in .NetCore, versione 8.0 o superiore

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

Questo permetterà alla tua applicazione di gestire l'autenticazione tramite i classici tag [Authorize] da inserire in capo alle Action/Controller
E' importante che tutte le Action create rispondano a path che **non abbiano come suffisso "/api"**

E' inoltre fondamentale che la configurazione utilizzata nell'AddPassiAuthentication sia direttamente quello fornito dal builder dello Startup. **Non utilizzare configurationBuilder custom, inizializzati manualmente**

Ad esempio, una API che restituisce i dati utente, si presenterà come segue:
```
[Authorize]
[HttpGet("/user/me")]
public async Task<IActionResult> Me()
{
    var me = await passiService.MeAsync();
    return Json(me);
}
```
dove passiService è un elemento di tipo IPassiService fornito direttamente dalla libreria.

Inoltre, permetterà di utilizzare all'interno dei propri servizi le implementazioni di 

| Servizio | Descrizione |
| -------- | ----------- |
| <a href="/api/Passi.Core.Application.Services.IPassiService.html">IPassiService</a> | Servizio per ottenere i dati dell'utente loggato |
| <a href="/api/Passi.Core.Application.Services.IPassiSecureService.html">IPassiSecureService</a> | Servizio per ottenere l'access token e per effettuare la cifratura dei dati |
| <a href="/api/Passi.Core.Application.Services.IPassiConventionService.html">IPassiConventionService</a> | Servizio per ottenere le convenzioni per l'utente loggato |
| <a href="/api/Passi.Core.Application.Services.ICLogService.html">ICLogService</a> | Servizio di CLog |

Successivamente, nella fase di creazione del builder applicativo, aggiungere ai servizi UsePassiAutentication

```
app.UseRouting();

app.UsePassiAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```