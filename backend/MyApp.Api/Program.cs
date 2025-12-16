using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MyApp.Api.Middleware;
using MyApp.Api.Models;
using MyApp.Api.Services;
using MyApp.Application.Auth;
using MyApp.Application.DTOs;
using MyApp.Application.Interfaces;
using MyApp.Application.Services;
using MyApp.Domain;
using MyApp.Infrastructure.Data;
using Serilog;
using Serilog.Events;
using System.Security.Claims;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// For UnitTest Log
Console.OutputEncoding = Encoding.UTF8;

// 🔹 1. Serilog：從 appsettings.json / appsettings.Development.json 讀取設定
//    （使用你在 appsettings.json 裡的 "Serilog" 區段）
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // 讀取 Serilog 設定
    .Enrich.FromLogContext()                       // 再補上 FromLogContext（即使 config 也有，重複沒關係）
    .CreateLogger();

// 讓 Host 使用 Serilog 當 logger
builder.Host.UseSerilog();

// 🔹 2. DbContext 使用 SQL Server & 連線字串
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔹 3. DI 註冊 Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiResponseFilter>();
})
.ConfigureApiBehaviorOptions(options =>
{
    // 統一 ModelState / FluentValidation 自動驗證失敗的回傳格式
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(ms => ms.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var response = new ApiErrorResponse
        {
            StatusCode = StatusCodes.Status400BadRequest,
            Message = "驗證失敗",
            TraceId = context.HttpContext.TraceIdentifier,
            Errors = errors
        };

        return new BadRequestObjectResult(response);
    };
});

// 掃描 Application 裡的所有 Validators
builder.Services.AddValidatorsFromAssemblyContaining<ProductCreateDto>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyApp.Api", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "請輸入：Bearer {token}"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    c.AddSecurityRequirement(document =>
    {
        // ✅ 這裡要用同一個 schemeId
        var schemeRef = new OpenApiSecuritySchemeReference("Bearer", document);

        var requirement = new OpenApiSecurityRequirement
        {
            [schemeRef] = new List<string>()
        };

        return requirement;
    });

    c.EnableAnnotations();
});

// 🔹 4. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// 🔹 5. 綁定 JwtSettings / LoggingOptions
// appsettings.json & appsettings.Development.json：請確保都是用 "Jwt" / "AppLogging"
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<LoggingOptions>(builder.Configuration.GetSection("AppLogging"));

// AuthService
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// CurrentUser + HttpContextAccessor
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// 🔹 6. JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),

            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// 🔹 7. 自動建立 DB + 自動套用 migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine("資料庫遷移失敗：" + ex.Message);
    }
}

// 🔹 8. Middleware 管線

// 全域錯誤處理，盡量放前面包住後面所有 middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");

// 先做 Authentication，讓 HttpContext.User 有值（含 JWT Claims）
app.UseAuthentication();

// 再把 UserName / UserId / Role / TraceId 塞到 Serilog LogContext
app.UseMiddleware<SerilogUserEnricherMiddleware>();

// Request/Response Body logging（會讀到上面塞的 UserName）
app.UseMiddleware<RequestResponseLoggingMiddleware>();

// Request Summary Log（Method + Path + StatusCode + Elapsed）
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "[Request] {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.000} ms";

    // 依狀態碼決定 LogLevel
    options.GetLevel = (http, elapsed, ex) =>
    {
        // 把 preflight 的 OPTIONS 壓到 Debug（因為我們 global level 是 Information，所以看不到）
        if (http.Request.Method == HttpMethods.Options)
            return LogEventLevel.Debug;

        if (ex != null || http.Response.StatusCode >= 500)
            return LogEventLevel.Error;

        if (http.Response.StatusCode >= 400)
            return LogEventLevel.Warning;

        return LogEventLevel.Information;
    };
});

app.UseAuthorization();

app.MapControllers();

app.Run();
