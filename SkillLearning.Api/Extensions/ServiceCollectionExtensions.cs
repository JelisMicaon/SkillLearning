using Amazon.XRay.Recorder.Core.Internal.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SkillLearning.Api.Services;
using SkillLearning.Application.Common.Behaviors;
using SkillLearning.Application.Common.Configuration;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Features.Auth.Commands;
using SkillLearning.Infrastructure.Persistence;
using SkillLearning.Infrastructure.Persistence.Repositories;
using SkillLearning.Infrastructure.Services;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Text;

namespace SkillLearning.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCustomServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Configurações fortemente tipadas
            var jwtSection = configuration.GetSection("Jwt");
            var jwt = jwtSection.Get<JwtSettings>() ?? throw new InvalidOperationException("JWT configuration is missing.");
            services.Configure<JwtSettings>(jwtSection);
            services.Configure<KafkaSettings>(configuration.GetSection("Kafka"));
            services.Configure<RedisSettings>(configuration.GetSection("Redis"));

            // 2. Redis
            var redisConn = configuration["Redis:ConnectionString"];
            if (string.IsNullOrWhiteSpace(redisConn))
                throw new InvalidOperationException("Redis connection string missing.");

            services.AddStackExchangeRedisCache(opt => opt.Configuration = redisConn);
            services.AddSingleton<ICacheService, RedisCacheService>();

            // 3. XRay Interceptor
            services.AddSingleton<ConcurrentDictionary<DbCommand, Subsegment>>();
            services.AddSingleton<QueryPerformanceInterceptor>();

            // 4. DBContext com Interceptor
            var writeConn = configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Connection string 'Default' não configurada.");
            var readConn = configuration.GetConnectionString("ReadOnly") ?? throw new InvalidOperationException("Connection string 'ReadOnly' não configurada.");

            services.AddDbContext<ApplicationWriteDbContext>((sp, options) =>
            {
                options.UseNpgsql(writeConn, npgsql =>
                    npgsql.MigrationsAssembly(typeof(ApplicationWriteDbContext).Assembly.GetName().Name));

                var interceptor = sp.GetRequiredService<QueryPerformanceInterceptor>();
                options.AddInterceptors(interceptor);
            });

            services.AddDbContext<ApplicationReadDbContext>((sp, options) =>
            {
                options.UseNpgsql(readConn, npgsql =>
                    npgsql.MigrationsAssembly(typeof(ApplicationReadDbContext).Assembly.GetName().Name));

                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

                var interceptor = sp.GetRequiredService<QueryPerformanceInterceptor>();
                options.AddInterceptors(interceptor);
            });
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationWriteDbContext>());
            services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<ApplicationReadDbContext>());

            // 5. CQRS, Validation
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));
            services.AddValidatorsFromAssembly(typeof(RegisterUserCommand).Assembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // 6. Serviços internos
            services.AddTransient<IActivityNotifier, SignalRActivityNotifier>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IEventPublisher, KafkaProducerService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuthService, AuthService>();

            // 7. JWT Auth
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", opts =>
                {
                    opts.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
                    };
                });

            services.AddAuthorization();

            // 8. Swagger
            services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SkillLearning API",
                    Version = "v1",
                    Description = "API para micro-aulas e habilidades"
                });

                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    Description = "Insira o token JWT: Bearer {seu_token}"
                });

                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }
    }
}