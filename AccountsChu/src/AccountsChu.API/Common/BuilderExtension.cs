using Microsoft.AspNetCore.Authentication.JwtBearer;
using AccountsChu.Infrastructure.Repositories;
using AccountsChu.Domain.Commands.Customer;
using AccountsChu.Domain.Commands.Account;
using AccountsChu.Infrastructure.Services;
using AccountsChu.Infrastructure.Handlers;
using AccountsChu.Infrastructure.Data;
using AccountsChu.Domain.Repositories;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AccountsChu.Domain.Services;
using AccountsChu.Domain.Handlers;
using AccountsChu.Domain.Commands;
using Microsoft.OpenApi.Models;
using System.Reflection;
using FluentValidation;
using System.Text;
using MediatR;
using System;
using Refit;

namespace AccountsChu.API.Common
{
    public static class BuilderExtension
    {
        public static void ApplyMigrations(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }      

        public static void AddDocumentation(this WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(opt => 
            {
                opt.SwaggerDoc("v1", new OpenApiInfo { Title = "API Banco Chu S.A.", Version = "v1" });

                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    In = ParameterLocation.Header,
                    Description = "Adicione somente o token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });

                opt.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme()
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{ }
                    }
                });

                //opt.CustomSchemaIds(n => n.FullName);
            });
        }

        public static void AddDataContexts(this WebApplicationBuilder builder)
        {
            var cnnStr = builder.Configuration.GetConnectionString("DefaultConnection")
                     ?? throw new Exception("Connection string not found!");

            builder.Services.AddDbContext<AppDbContext>(options
                => options.UseNpgsql(cnnStr, x => x.MigrationsAssembly("AccountsChu.API")));
        }

        public static void AddCrossOrigin(this WebApplicationBuilder builder)
        {
            builder.Services.AddCors(
                options => options.AddDefaultPolicy(x => x.AllowAnyOrigin()));
        }

        public static void AddServices(this WebApplicationBuilder builder)
        {
            #region Services

            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<ICustomerContextService, CustomerContextService>();

            #endregion

            #region MediatR Handlers

            builder.Services.AddScoped<IRequestHandler<CreateAccountCommand, GenericCommandResult>, AccountHandler>();
            builder.Services.AddScoped<IRequestHandler<TedTransactionAccountCommand, GenericCommandResult>, AccountHandler>();
            builder.Services.AddScoped<IRequestHandler<StatementAccountCommand, GenericCommandResult>, AccountHandler>();
            
            builder.Services.AddScoped<IRequestHandler<CreateCustomerCommand, GenericCommandResult>, CustomerHandler>();
            builder.Services.AddScoped<IRequestHandler<LoginCustomerCommand, GenericCommandResult>, CustomerHandler>();

            #endregion


            #region Repositories

            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

            #endregion

        }

        public static IServiceCollection AddMediatRConfiguration(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(BuilderExtension).Assembly);
            });

            services.AddScoped<IValidator<CreateAccountCommand>, AccountCommandValidator>();
            services.AddScoped<IValidator<CreateCustomerCommand>, CustomerCommandValidator>();

            return services;
        }

        public static void AddAuthentication(this WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer("Bearer", options =>
            {

                options.RequireHttpsMetadata = false;
                options.SaveToken = true;

                var keyString = builder.Configuration["Jwt:Key"];
                if (string.IsNullOrWhiteSpace(keyString))
                    throw new Exception("JWT Key Não foi encontrado!");

                var key = Encoding.UTF8.GetBytes(keyString);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateActor = true,
                };
            });
        }

        public static void AddRefit(this WebApplicationBuilder builder)
        {
            builder.Services.AddMemoryCache();
            builder.Services.AddTransient<CachingHttpMessageHandler>();

            builder.Services
                .AddRefitClient<IBrasilApiService>()
                .AddHttpMessageHandler<CachingHttpMessageHandler>()
                .ConfigureHttpClient(c =>
                    c.BaseAddress = new Uri(builder.Configuration["BrasilApi"]));
        }
    }
}
