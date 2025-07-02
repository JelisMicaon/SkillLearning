using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Amazon.XRay.Recorder.Handlers.AwsSdk.Internal;
using AspNetCore.Swagger.Themes;
using Microsoft.EntityFrameworkCore;
using SkillLearning.Api.Middlewares;
using SkillLearning.Infrastructure.Extensions;
using SkillLearning.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// AWS X-Ray
AWSXRayRecorder.InitializeInstance(builder.Configuration);
AWSSDKHandler.RegisterXRayForAllServices();
builder.Services.AddTransient<XRayPipelineHandler>();

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomServices(builder.Configuration);

// App
var app = builder.Build();

// Auto-migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (context.Database.IsNpgsql())
    {
        await context.Database.MigrateAsync();
        app.Logger.LogInformation("Database migrations applied successfully.");
    }
}

// Middleware pipeline
app.UseXRay("SkillLearning");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI(ModernStyle.Futuristic);
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SkillLearning API v1"));

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();