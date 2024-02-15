var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseDefaultFiles();
//app.UseStaticFiles();

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

//app.UseAuthorization();
//app.UseAuthentication();

app.MapFallbackToFile("/index.html");

app.Run();
