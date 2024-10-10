using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
            .AddJsonOptions(option =>
            {
                option.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
                option.JsonSerializerOptions.WriteIndented = true;
            });
builder.Services.AddPassiAuthentication(builder.Configuration);

var app = builder.Build();

app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("AllowAll");

app.UsePassiAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();



public partial class Program { }
