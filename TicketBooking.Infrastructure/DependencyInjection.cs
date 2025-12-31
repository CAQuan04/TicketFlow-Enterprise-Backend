using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping; // ✅ THÊM DÒNG NÀY ĐỂ DÙNG ENUM
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Application.Common.Interfaces.AI;
using TicketBooking.Application.Common.Interfaces.Authentication;
using TicketBooking.Application.Common.Interfaces.Data;
using TicketBooking.Application.Common.Interfaces.Payments;
using TicketBooking.Application.Common.Interfaces.RealTime;
using TicketBooking.Domain.Events;
using TicketBooking.Infrastructure.AI;
using TicketBooking.Infrastructure.Authentication;
using TicketBooking.Infrastructure.Authentication.Social;
using TicketBooking.Infrastructure.Data;
using TicketBooking.Infrastructure.FileStorage;
using TicketBooking.Infrastructure.Payments;
using TicketBooking.Infrastructure.Search;
using TicketBooking.Infrastructure.Search.Models;
using TicketBooking.Infrastructure.Services;

namespace TicketBooking.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // --- 1. DB & CORE ---
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            // --- 2. AUTH ---
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
            services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.Configure<GoogleSettings>(configuration.GetSection(GoogleSettings.SectionName));
            services.AddTransient<ISocialAuthService, GoogleAuthService>();

            // --- 3. INFRA SERVICES ---
            services.AddTransient<DataSeeder>();
            services.AddScoped<IStorageService, LocalStorageService>();
            services.AddScoped<ISearchService, ElasticSearchService>();
            services.AddTransient<IQrCodeService, QrCodeService>();
            services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
            services.AddTransient<IEmailService, SmtpEmailService>();
            services.AddScoped<IWalletService, WalletService>();
            services.Configure<VnPaySettings>(configuration.GetSection(VnPaySettings.SectionName));
            services.AddTransient<IPaymentGateway, VnPayPaymentGateway>();
            services.AddTransient<IVnPayValidationService, VnPayValidationService>();
            services.AddTransient<ISqlConnectionFactory, SqlConnectionFactory>();
            services.AddTransient<INotificationService, SignalRNotificationService>();
            services.AddTransient<INotificationHandler<EventPublishedEvent>, SyncEventToElasticHandler>();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis");
                options.InstanceName = "TicketFlow_";
            });

            services.AddSingleton<IRecommendationService, RecommendationService>();

            // --- 4. ELASTICSEARCH & AI ---
            services.AddHttpClient<IAiEmbeddingService, GeminiEmbeddingService>();

            var esUrl = configuration["ElasticSearch:Uri"] ?? "http://localhost:9200";

            services.AddSingleton<ElasticsearchClient>(_ =>
            {
                var settings = new ElasticsearchClientSettings(new Uri(esUrl))
                    .DefaultIndex("events");

                var client = new ElasticsearchClient(settings);

                var existsResponse = client.Indices.Exists("events");

                if (!existsResponse.Exists)
                {
                    // Cú pháp chuẩn cho Elastic.Clients.Elasticsearch v8.x
                    client.Indices.Create("events", c => c
                        .Mappings(m => m
                            .Properties<EventDocument>(p => p
                                .Keyword(k => k.Id)          // Map Id sang Keyword
                                .Text(t => t.Name)           // Map Name sang Text
                                .Text(t => t.Description)    // Map Description sang Text
                                .Text(t => t.VenueName)      // Map VenueName sang Text
                                .DenseVector(n => n.Embedding, dv => dv
                                    .Dims(768)               // Kích thước Vector (Gemini)
                                    .Index(true)             // Cho phép tìm kiếm KNN
                                    .Similarity(Elastic.Clients.Elasticsearch.Mapping.DenseVectorSimilarity.Cosine)    // Dùng chuỗi "cosine" thay vì Enum để tránh lỗi
                                )
                            )
                        )
                    );
                }

                return client;
            });

            return services;
        }
    }
}