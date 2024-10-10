<h1>Unit test con PASSI</h1>

In questa sezione andremo a creare degli esempi sul come realizzare dei test di unità per le applicazioni che utilizzano la libreria PASSI.

<h2>Caso d'uso</h2>
Esponiamo un caso d'uso esemplificativo della libreria PASSI, che può essere preso come esempio generico per la realizzazione dei test.

Supponiamo di avere una Action (Me) di un Controller (HomeController) che utilizza PASSI.
HomeController utilizza IPassiService ed è autenticato.
La Action Me non fa altro che restituire i dati dell'utente sotto forma di json.
```
[Authorize]
[HttpGet("/user/me")]
public async Task<IActionResult> Me()
{
    var me = await passiService.MeAsync();
    return Json(me);
}
```

I test di unità vengono eseguiti utilizzando la libreria fake disponibile per lo sviluppo. Pertanto, non sarà necessario inserire dati particolari per testare l'applicazione, in quanto il comportamento di default della libreria fake consiste nel simulare un utente loggato.

L'esempio sottostante è generato usando xUnit, con Moq e Bogus per la generazione dei dati. 


```
var user = new Faker<User>()
    .CustomInstantiator(f => new User())
    .RuleFor(u => u.UserId, f => f.Person.CodiceFiscale())
    .RuleFor(u => u.HasMultipleProfile, f => true)
    .RuleFor(u => u.BirthProvince, f => f.Address.CityPrefix())
    .RuleFor(u => u.BirthCity, f => f.Address.City())
    .RuleFor(u => u.Gender, f => f.PickRandom<Core.Domain.Const.Gender>())
    .RuleFor(u => u.Name, f => f.Person.FirstName)
    .RuleFor(u => u.BirthDay, f => f.Date.Between(DateTime.UtcNow.AddYears(-50), DateTime.UtcNow).ToString())
    .RuleFor(u => u.Surname, f => f.Person.LastName)
    .Generate();

var mockPassiService = new Mock<IPassiService>();
mockPassiService.Setup(_ => _.MeAsync()).ReturnsAsync(user);

var homeController = new HomeController(mockPassiService.Object);

var result = homeController.Me();

var viewResult = Assert.IsType<JsonResult>(result);
var dataResult = Assert.IsAssignableFrom<User>(viewResult.Value);
Assert.True(dataResult.Name == user.Name);
Assert.True(dataResult.UserId == user.UserId);
Assert.True(dataResult.BirthDay == user.BirthDay);

```
Analogamente, è possibile testare allo stesso modo gli altri servizi della libreria PASSI.