﻿using Microsoft.Extensions.Configuration;
using Project.Data.Dto.Email;
using Project.Services.Contracts;
using SendGrid.Helpers.Mail;
using SendGrid;
using Project.Data;

namespace Project.Services.Business;
public class EmailService : IEmailService {
    private readonly IConfiguration _config;
    public EmailService(IConfiguration config) {
        _config = config;
    }
    public string CreateForgotPasswordMessageBody(SendVerificationOrForgotPasswordEmailDto sendForgotPasswordEmailDto) {
        var bodyBuilder =
             $"You have {AppConstants.TOKEN_VALIDATION_TIME} minutes to click the link and reset your password " +
             $"<a href={sendForgotPasswordEmailDto.Uri}>link</a>";

        return bodyBuilder;
    }

    public string CreateVerificationMessageBody(SendVerificationOrForgotPasswordEmailDto sendVerificationEmailDto) {
        var bodyBuilder =
            $"You have {AppConstants.VERIFICATION_TOKEN_VALIDATION_TIME} hours to click the link before it expires " +
            $"<a href={sendVerificationEmailDto.Uri}>link</a>";

        return bodyBuilder;
    }

    public async Task SendForgotPasswordEmailAsync(SendVerificationOrForgotPasswordEmailDto sendForgotPasswordEmailDto) {
        var apiKey = _config["SendGrid:ApiKey"];
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(_config["SendGrid:Email"]);
        var subject = $"Chat Application Forgot Password";
        var to = new EmailAddress(sendForgotPasswordEmailDto.Email);
        var plainTextContent = "";

        sendForgotPasswordEmailDto.Uri = Path.Combine(AppConstants.FE_APP_URL, sendForgotPasswordEmailDto.Token);
        var htmlContent = CreateForgotPasswordMessageBody(sendForgotPasswordEmailDto);

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        await client.SendEmailAsync(msg);
    }

    public async Task SendVerificationEmail(SendVerificationOrForgotPasswordEmailDto sendVerificationEmailDto) {
        var apiKey = _config["SendGrid:ApiKey"];
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(_config["SendGrid:Email"]);
        var subject = $"Chat Application verify email";
        var to = new EmailAddress(sendVerificationEmailDto.Email);
        var plainTextContent = "";

        sendVerificationEmailDto.Uri = Path.Combine(AppConstants.FE_APP_CR_URL, sendVerificationEmailDto.Token);
        var htmlContent = CreateVerificationMessageBody(sendVerificationEmailDto);

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        await client.SendEmailAsync(msg);
    }
}
