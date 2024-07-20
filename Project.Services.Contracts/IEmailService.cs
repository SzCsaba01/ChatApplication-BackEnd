using Project.Data.Dto.Email;

namespace Project.Services.Contracts;
public interface IEmailService {
    Task SendVerificationEmail(SendVerificationOrForgotPasswordEmailDto sendVerificationEmailDto);
    Task SendForgotPasswordEmailAsync(SendVerificationOrForgotPasswordEmailDto sendForgotPasswordEmailDto);
    string CreateForgotPasswordMessageBody(SendVerificationOrForgotPasswordEmailDto sendForgotPasswordEmailDto);
    string CreateVerificationMessageBody(SendVerificationOrForgotPasswordEmailDto sendVerificationEmailDto);
}
