﻿using TrelloMini.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using TrelloMini.Authorization;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Antiforgery;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//KHAI BÁO DB CONTEXT
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Antiforgery for SPA protection
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
});

// CORS - allow local dev origins and credentials
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        policy.WithOrigins("https://localhost:5001", "http://localhost:5000", "https://trellomini.somee.com", "http://trellomini.somee.com")
              .AllowCredentials()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Google-only authentication with cookies
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

// Cần đảm bảo rằng các giá trị này được cấu hình an toàn, ví dụ:
// - Đối với phát triển cục bộ: Sử dụng User Secrets. Chạy lệnh sau trong thư mục dự án:
//   dotnet user-secrets set "Authentication:Google:ClientId" "YOUR_CLIENT_ID"
//   dotnet user-secrets set "Authentication:Google:ClientSecret" "YOUR_CLIENT_SECRET"
// - Đối với triển khai: Sử dụng biến môi trường hoặc dịch vụ quản lý bí mật (ví dụ: Azure Key Vault).
if (string.IsNullOrEmpty(googleClientId))
{
    throw new InvalidOperationException("Google Client ID is not configured. Please set 'Authentication:Google:ClientId' in your configuration.");
}
if (string.IsNullOrEmpty(googleClientSecret))
{
    throw new InvalidOperationException("Google Client Secret is not configured. Please set 'Authentication:Google:ClientSecret' in your configuration.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Google";
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
})
.AddGoogle("Google", options =>
{
    options.ClientId = googleClientId;
    options.ClientSecret = googleClientSecret;
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.SaveTokens = true;
    
    // Configure redirect URIs - must match Google Console settings
    options.AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    options.TokenEndpoint = "https://oauth2.googleapis.com/token";
    options.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
    
    // Important: Add scopes
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    
    options.Events.OnRemoteFailure = context =>
    {
        Console.WriteLine($"Google Auth Failed: {context.Failure?.Message}");
        context.HandleResponse();
        context.Response.Redirect("/login.html?error=google_auth_failed");
        return Task.CompletedTask;
    };
});

// Authorization: register ProjectOwner policy and handler
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ProjectOwner", policy => policy.Requirements.Add(new ProjectOwnerRequirement()));
});

builder.Services.AddScoped<IAuthorizationHandler, ProjectOwnerHandler>();

var app = builder.Build();

// Apply pending migrations automatically at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        dbContext.Database.Migrate();
        Console.WriteLine("✓ Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Error applying migrations: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

// If you moved HTML files into "wwwroot/html", also serve that folder at the web root
var env = app.Environment;
var htmlFolder = Path.Combine(env.ContentRootPath, "wwwroot", "html");
if (Directory.Exists(htmlFolder))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(htmlFolder),
        RequestPath = ""
    });
}

// Redirect root requests to the SPA login page
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/" || context.Request.Path == string.Empty)
    {
        context.Response.Redirect("/login.html");
        return;
    }
    await next();
});

// Use CORS BEFORE HTTPS redirect to allow authentication callbacks
app.UseCors("DefaultCors");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Antiforgery middleware - validate for unsafe methods on API endpoints
var antiforgery = app.Services.GetRequiredService<IAntiforgery>();
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api") &&
        (HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method) || HttpMethods.IsDelete(context.Request.Method)))
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context);
        }
        catch (AntiforgeryValidationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Invalid antiforgery token.");
            return;
        }
    }

    await next();
});

app.MapControllers();

app.Run();
