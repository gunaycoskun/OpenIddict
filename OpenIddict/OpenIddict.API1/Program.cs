using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["AuthenticationSettings:Authority"];
        //Token'daki 'JwtRegisteredClaimNames.Aud' karşılık verilen değerin ta kendisi...
        options.Audience = builder.Configuration["AuthenticationSettings:Audience"];
        options.RequireHttpsMetadata = false;

        //Gelen token'da 'scope' claim'i içerisinde kümülatif olarak yan yana yazılmış
        //yetkileri ayırarak tek tek scope claim'i olarak tekrardan ayarlayabilmek için
        //token'ın doğrulanması ardından 'OnTokenValidated' event'inde aşağıdaki çalışmayı
        //gerçekleştirmemiz gerekmektedir.
        options.Events = new()
        {
            OnTokenValidated = async context =>
            {
                if (context.Principal?.Identity is ClaimsIdentity claimsIdentity)
                {
                    Claim? scopeClaim = claimsIdentity.FindFirst("scope");
                    if (scopeClaim is not null)
                    {
                        claimsIdentity.RemoveClaim(scopeClaim);
                        claimsIdentity.AddClaims(scopeClaim.Value.Split(" ").Select(s => new Claim("scope", s)).ToList());
                    }
                }

                await Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("APolicy", policy => policy.RequireClaim("scope", "read"));
    options.AddPolicy("BPolicy", policy => policy.RequireClaim("scope", "write"));
    options.AddPolicy("CPolicy", policy => policy.RequireClaim("scope", "read", "write"));
    options.AddPolicy("DPolicy", policy => policy.RequireClaim("ozel-claim", "ozel-claim-value"));
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
