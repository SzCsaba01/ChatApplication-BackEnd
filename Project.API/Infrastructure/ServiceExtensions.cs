using Project.Data.DTO.Authentication;
using Project.Services.Business;
using Project.Services.Contracts;

namespace Project.API.Infrastructure;

public static class ServiceExtensions {
    public static IServiceCollection RegisterServices(this IServiceCollection services) {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFriendRequestService, FriendRequestService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IUserActivityService, UserActivityService>();

        services.AddSignalR();


        services.AddCors(options => options.AddPolicy(
            name: "NgOrigins",
            policy => {
                policy.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader().AllowCredentials();
            }));

        return services;
    }
}
