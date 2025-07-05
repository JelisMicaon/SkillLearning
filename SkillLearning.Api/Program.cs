using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Amazon.XRay.Recorder.Handlers.AwsSdk.Internal;
using AspNetCore.Swagger.Themes;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SkillLearning.Api.Extensions;
using SkillLearning.Api.Middlewares;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(policyName: "fixed", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// AWS X-Ray
AWSXRayRecorder.InitializeInstance(builder.Configuration);
AWSSDKHandler.RegisterXRayForAllServices();
builder.Services.AddTransient<XRayPipelineHandler>();

// Services
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomServices(builder.Configuration);

// App
var app = builder.Build();

// Middleware pipeline
app.UseXRay("SkillLearning");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI(ModernStyle.Futuristic);
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SkillLearning API v1"));

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireRateLimiting("fixed");

await app.RunAsync();