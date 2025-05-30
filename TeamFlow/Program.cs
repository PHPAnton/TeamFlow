using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TeamFlow.Data;
using TeamFlow.Services;
using TeamFlow.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 1. Подключение к PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Host=localhost;Database=teamflow;Username=postgres;Password=Nikond3300";

builder.Services.AddDbContext<TeamFlowContext>(options =>
    options.UseNpgsql(connectionString));

// 2. JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),

            ClockSkew = TimeSpan.FromMinutes(5) // 🔥 Убираем доп. "поблажку" в 5 минут (по умолчанию она есть)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });


// 3. Сервисы
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173") // Vue dev
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddScoped<IEmailService, DummyEmailService>();
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TeamFlow API", Version = "v1" });
});

// 4. Поддержка статических файлов для Vue (в продакшене)
builder.Services.AddSpaStaticFiles(config =>
{
    config.RootPath = "ClientApp/dist"; // Vue билдится сюда
});

var app = builder.Build();

// 5. Middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TeamFlow API v1"));
}
else
{
    app.UseSpaStaticFiles(); // Только в продакшене
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Для wwwroot
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.UseRouting();

// === Логируем токен ===
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].ToString();
    Console.WriteLine("=== Запрос пришел с токеном ===");
    Console.WriteLine(authHeader);
    await next();
});

app.UseAuthentication();
app.UseAuthorization();
// 6. SPA fallback
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp";

    if (app.Environment.IsDevelopment())
    {
        // Vue Vite запускается отдельно вручную (npm run dev)
        // Никакого proxy тут не нужно
    }
});

app.Run();
