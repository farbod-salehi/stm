using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using STM.Api.Middlewares;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Authentication
var jwtSettingsSection = builder.Configuration.GetSection("Jwt");
var issuer = jwtSettingsSection["Issuer"];
var audience = jwtSettingsSection["Audience"];
var key = jwtSettingsSection["Key"];

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Order of middleware is important

// 1. Logging (first, to see everything)
app.UseMiddleware<LoggingMiddleware>();

// 2. Error handling (wraps the rest)
app.UseMiddleware<ErrorHandlingMiddleware>();

// 3. Language (Accept-Language → HttpContext.Items["lang"])
app.UseMiddleware<LanguageMiddleware>();

// 4. Routing
app.UseRouting();

// 5. Authentication & Authorization (built-in)
app.UseAuthentication();
app.UseAuthorization();

// 6. User context (extract userId from claims → HttpContext.Items["userId"])
app.UseMiddleware<UserContextMiddleware>();

// 7. Map controllers
app.MapControllers();


app.Run();
