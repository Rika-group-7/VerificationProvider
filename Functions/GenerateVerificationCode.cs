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
            var verificationRequest = _verificationService.UnpackVerificationRequest(message);
            if (verificationRequest != null)
            {
                var code = _verificationService.GenerateCode(6);
                if (!string.IsNullOrEmpty(code))
                {
                    var result = await _verificationService.SaveVerifactionRequest(verificationRequest, code);
                    if (result)
                    {
                        var emailRequest = _verificationService.GenerateEmailRequest(verificationRequest, code);
                        if (emailRequest != null)
                        {
                            var payload = _verificationService.GenerateServiceBusEmailRequest(emailRequest);
                            if (!string.IsNullOrEmpty(payload))
                            {
                                await messageActions.CompleteMessageAsync(message);
                                return payload;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GenerateVerificationCode.Run() :: {ex.Message}");
        }
        
        return null!;
    }

}