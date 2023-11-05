using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OpenIddict.AuthorizationServer.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => options.LoginPath = "/account/login");

builder.Services.AddOpenIddict()
    //OpenIddict core/çekirdek yapılandırmaları gerçekleştiriliyor.
    .AddCore(options =>
    {
        //Entity Framework Core kullanılacağı bildiriliyor.
        options.UseEntityFrameworkCore()
               //Kullanılacak context nesnesi bildiriliyor.
               .UseDbContext<ApplicationDbContext>();
    })
    //OpenIddict server yapılandırmaları gerçekleştiriliyor.
    .AddServer(options =>
    {
        //Token talebinde bulunulacak endpoint'i set ediyoruz.
        options.SetTokenEndpointUris("/connect/token");
        //Akış türü olarak Client Credentials Flow'u etkinleştiriyoruz.
        options.AllowClientCredentialsFlow();
        //Signing ve encryption sertifikalarını ekliyoruz.
        options.AddEphemeralEncryptionKey()
               .AddEphemeralSigningKey()
               //Normalde OpenIddict üretilecek token'ı güvenlik amacıyla şifreli bir şekilde bizlere sunmaktadır.
               //Haliyle jwt.io sayfasında bu token'ı çözümleyip görmek istediğimizde şifresinden dolayı
               //incelemede bulunamayız. Bu DisableAccessTokenEncryption özelliği sayesinde üretilen access token'ın
               //şifrelenmesini iptal ediyoruz.
               .DisableAccessTokenEncryption();
        //OpenIddict Server servislerini IoC Container'a ekliyoruz.
        options.UseAspNetCore()
               //EnableTokenEndpointPassthrough : OpenID Connect request'lerinin OpenIddict tarafından işlenmesi için gerekli konfigürasyonu sağlar.
               .EnableTokenEndpointPassthrough();
        //Yetkileri(scope) belirliyoruz.
        options.RegisterScopes("read", "write");
    });


//OpenIddict'i SQL Server'ı kullanacak şekilde yapılandırıyoruz.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer"));
    //OpenIddict tarafından ihtiyaç duyulan Entity sınıflarını kaydediyoruz.
    options.UseOpenIddict();
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
