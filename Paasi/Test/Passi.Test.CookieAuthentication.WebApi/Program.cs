namespace Passi.Test.CookieAuthentication.WebApi
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddPassiAuthentication(builder.Configuration);

            var app = builder.Build();

            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UsePassiAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            if (!args.Any())
            {
                app.Run();
            }
        }
    }
}