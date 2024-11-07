using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Models;
using Azure.Messaging.ServiceBus;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using VerificationProvider.Data.Context;
using Microsoft.EntityFrameworkCore;
using VerificationProvider.Inferfaces;

namespace VerificationProvider.Services;

public class VerificationService(ILogger<VerificationService> logger, IServiceProvider serviceProvider) : IVerificationService
{
    private readonly ILogger<VerificationService> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public VerificationRequest UnpackVerificationRequest(ServiceBusReceivedMessage message)
    {
        try
        {
            var verificationRequest = JsonConvert.DeserializeObject<VerificationRequest>(message.Body.ToString());
            if (verificationRequest != null && !string.IsNullOrEmpty(verificationRequest.Email))
                return verificationRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError($"error : Unpack verificationrequest :: {ex.Message}");
        }

        return null!;
    }

    public string GenerateCode(int length)
    {
        try
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var rnd = new Random();
            var code = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                code.Append(chars[rnd.Next(chars.Length)]);
            }
            return code.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GenerateVerificationCode.GenerateCode :: {ex.Message}");
        }
        return null!;

    }
    public EmailRequest GenerateEmailRequest(VerificationRequest verificationRequest, string code)
    {
        try
        {
            if (!string.IsNullOrEmpty(verificationRequest.Email) && !string.IsNullOrEmpty(code))
            {
                var emailRequest = new EmailRequest()
                {
                    To = verificationRequest.Email,
                    Subject = $"Verification Code {code}",
                    Body = $@"
                                <html>
                                <head>
                                    <style>
                                        .container {{
                                            font-family: Arial, sans-serif;
                                            max-width: 600px;
                                            margin: 0 auto;
                                            padding: 20px;
                                            border: 1px solid #e0e0e0;
                                            border-radius: 8px;
                                            background-color: #f9f9f9;
                                        }}
                                        .header {{
                                            font-size: 24px;
                                            font-weight: bold;
                                            color: #333333;
                                            text-align: center;
                                            margin-bottom: 20px;
                                        }}
                                        .content {{
                                            font-size: 16px;
                                            color: #555555;
                                            line-height: 1.6;
                                        }}
                                        .code {{
                                            display: block;
                                            font-size: 20px;
                                            font-weight: bold;
                                            color: #1a73e8;
                                            margin-top: 10px;
                                            text-align: center;
                                        }}
                                        .footer {{
                                            font-size: 12px;
                                            color: #aaaaaa;
                                            text-align: center;
                                            margin-top: 30px;
                                        }}
                                    </style>
                                </head>
                                <body>
                                    <div class='container'>
                                        <div class='header'>Your Verification Code</div>
                                        <div class='content'>
                                            Hello,<br /><br />
                                            You requested a verification code. Use the code below to proceed:
                                            <span class='code'>{code}</span>
                                            <br />
                                            If you didn't request this, please ignore this email.
                                        </div>
                                        <div class='footer'>This is an automated message. Please do not reply.</div>
                                    </div>
                                </body>
                                </html>
                                ",
                    PlainTextContent = $@"Email: {verificationRequest.Email} Code: {code}",
                };

                _logger.LogInformation($"EmailRequest created successfully for {verificationRequest.Email} with code {code}.");
                return emailRequest;
            }
            else
            {
                _logger.LogWarning("Invalid verification request or code. EmailRequest not generated.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GenerateVerificationCode.GenerateEmailRequest :: {ex.Message}");
        }
        return null!;
    }
    public async Task<bool> SaveVerifactionRequest(VerificationRequest verificationRequest, string code)
    {
        try
        {
            using var context = _serviceProvider.GetRequiredService<DataContext>();
            var existingRequest = await context.verificationRequest.FirstOrDefaultAsync(x => x.Email == verificationRequest.Email);

            if (existingRequest != null)
            {
                existingRequest.Code = code;
                existingRequest.ExpirationDate = DateTime.Now.AddMinutes(5);
                context.Entry(existingRequest).State = EntityState.Modified;
            }
            else
            {
                context.verificationRequest.Add(new Data.Entities.VerificationRequestEntity() { Email = verificationRequest.Email, Code = code });
            }

            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GenerateVerificationCode.SaveVerificationRequest :: {ex.Message}");
        }
        return false;

    }

    public string GenerateServiceBusEmailRequest(EmailRequest emailRequest)
    {
        try
        {
            var payload = JsonConvert.SerializeObject(emailRequest);
            if (!string.IsNullOrEmpty(payload))
            {
                return payload;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GenerateVerificationCode.GenerateServiceBusEmailRequest :: {ex.Message}");
        }
        return null!;


    }
}
