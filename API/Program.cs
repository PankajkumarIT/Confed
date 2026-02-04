using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon;
using API;
using API.Data;
using API.Data.IRepositories;
using API.Data.Repositories;
using API.Helpers;
using API.Helpers.Models;
using API.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using Amazon.S3.Transfer;
var builder = WebApplication.CreateBuilder(args);
var awsRegion = builder.Configuration["AWS:Region"] ?? "ap-south-1";
builder.Services.AddSingleton<IAmazonS3>(sp =>new AmazonS3Client(RegionEndpoint.GetBySystemName(awsRegion)));
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(dataSource));
var appSettingSection = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingSection);
builder.Services.AddScoped<IUnitofWork, UnitOfWork>();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>,ConfigureSwaggerOptions>();
var appSetting = appSettingSection.Get<AppSettings>();
var key = Encoding.ASCII.GetBytes(appSetting.Secret);
builder.Services.ConfigureJwtAuthentication(key);
builder.Services.ConfigureCustomAuthorization();
builder.Services.Configure<EncryptionSettings>(builder.Configuration.GetSection("EncryptionSettings"));
builder.Services.Configure<S3ServiceModel>(builder.Configuration.GetSection("S3ServiceModel"));
builder.Services.AddSingleton<TransferUtility>(sp =>
{
    var s3Client = sp.GetRequiredService<IAmazonS3>();
    return new TransferUtility(s3Client);
});
builder.Services.AddScoped<IEncryptionHelper, EncryptionHelper>();
builder.Services.AddScoped<IS3Helper, S3Helper>();
builder.Services.AddScoped<IAuthorizationHandler, IsAccssAuthorizationHandler>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy", builder =>
    {
        builder.WithOrigins("http://localhost:4200", "http://localhost:8081")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()        
            .WithExposedHeaders("X-Data-Hash"); 
    });
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//    dbContext.Database.Migrate();
//}
app.UseHttpsRedirection();
app.UseCors("MyPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();