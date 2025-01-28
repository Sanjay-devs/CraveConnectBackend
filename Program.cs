using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Test.BAL.Interfaces;
using Test.BAL.Intrfaces;
using Test.BAL.Services;
using Test.Context;
using Test.DAL.Interface;
using Test.DAL.Interfaces;
using Test.DAL.Repos;
using Test.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure JWT Authentication
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        };

        //options.Events = new JwtBearerEvents
        //{
        //    OnAuthenticationFailed = context =>
        //    {
        //        context.Response.StatusCode = 440;
        //        context.Response.ContentType = "application/json";
        //        return context.Response.WriteAsync("{\"statusCode\":440,\"statusMessage\":\"Invalid session. Please log in again.\"}");
        //    }
        //};
    });

builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("connection")),
    ServiceLifetime.Scoped);



// Add services to DI container
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUserMasterService, UserMasterService>();
builder.Services.AddScoped<IUserMasterRepo, UserMasterRepo>();
builder.Services.AddScoped<IMasterMgmtService, MasterMgmtServices>();
builder.Services.AddScoped<IMasterMgmtRepo, MasterMgmtRepo>();
builder.Services.AddScoped<IJwtToken, JwtToken>();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthentication(); // Enable JWT Authentication
app.UseAuthorization();

app.UseStaticFiles();

// Enable static file serving for the "UploadedFiles" folder
app.UseFileServer(new FileServerOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles")),
    RequestPath = "/UploadedFiles",
    EnableDirectoryBrowsing = true // Optional, for browsing the files
});



app.MapControllers();

app.Run();
