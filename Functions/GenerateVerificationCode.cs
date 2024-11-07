using Microsoft.Azure.Functions.Worker.Extensions.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VerificationProvider.Inferfaces;
using Azure.Messaging.ServiceBus;


namespace VerificationProvider.Functions;

public class GenerateVerificationCode(ILogger<GenerateVerificationCode> logger, IVerificationService verificationService)
{
    private readonly ILogger<GenerateVerificationCode> _logger = logger;
    private readonly IVerificationService _verificationService = verificationService;

    [Function(nameof(GenerateVerificationCode))]
    [ServiceBusOutput("email-queue", Connection = "ServiceBusConnection")]
    public async Task<string> Run(
        [ServiceBusTrigger("verification-queue", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)

    {
        try
        {
            _logger.LogInformation("Starting GenerateVerificationCode function.");

            var verificationRequest = _verificationService.UnpackVerificationRequest(message);
            if (verificationRequest == null)
            {
                _logger.LogWarning("Verification request unpacking failed. The message may be malformed or missing required data.");
                await messageActions.DeadLetterMessageAsync(message, new Dictionary<string, object> { { "Invalid Request", "Unable to unpack verification request." } });
                return null!;
            }

            _logger.LogInformation("Successfully unpacked verification request.");

            var code = _verificationService.GenerateCode(6);
            if (string.IsNullOrEmpty(code))
            {
                _logger.LogWarning("Failed to generate verification code.");
                await messageActions.DeadLetterMessageAsync(message, new Dictionary<string, object> { { "Code Generation Failed", "Failed to generate verification code." } });
                return null!;
            }

            _logger.LogInformation("Generated verification code: {Code}", code);

            var result = await _verificationService.SaveVerifactionRequest(verificationRequest, code);
            if (!result)
            {
                _logger.LogWarning("Failed to save verification request in the database.");
                await messageActions.DeadLetterMessageAsync(message, new Dictionary<string, object> { { "Database Error", "Failed to save verification request." } });
                return null!;
            }

            _logger.LogInformation("Verification request saved successfully.");

            var emailRequest = _verificationService.GenerateEmailRequest(verificationRequest, code);
            if (emailRequest == null)
            {
                _logger.LogWarning("Failed to generate email request.");
                await messageActions.DeadLetterMessageAsync(message, new Dictionary<string, object> { { "Email Request Generation Failed", "Unable to generate email request." } });
                return null!;
            }

            _logger.LogInformation("Generated email request successfully.");

            var payload = _verificationService.GenerateServiceBusEmailRequest(emailRequest);
            if (string.IsNullOrEmpty(payload))
            {
                _logger.LogWarning("Failed to generate Service Bus email request payload.");
                await messageActions.DeadLetterMessageAsync(message, new Dictionary<string, object> { { "Payload Generation Failed", "Unable to generate email payload." } });
            }

            _logger.LogInformation("Service Bus email request payload generated successfully. Completing message.");

            await messageActions.CompleteMessageAsync(message);
            return payload;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR: GenerateVerificationCode.Run() encountered an exception.");
            await messageActions.DeadLetterMessageAsync(message, new Dictionary<string, object> { { "Unhandled Exception", ex.Message } });
            return null!;
        }
    }

}