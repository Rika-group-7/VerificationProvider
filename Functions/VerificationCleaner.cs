using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VerificationProvider.Inferfaces;

namespace VerificationProvider.Functions;

public class VerificationCleaner(ILogger<VerificationCleaner> logger, IVerificationCleanerService verificationCleanerService)
{
    private readonly ILogger<VerificationCleaner> _logger = logger;
    private readonly IVerificationCleanerService _verificationCleanerService = verificationCleanerService;

    [Function("VerificationCleaner")]
    public async Task Run([Microsoft.Azure.Functions.Worker.TimerTrigger("0 */1 * * * *")] Microsoft.Azure.Functions.Worker.TimerInfo myTimer)
    {
        try
        {
            await _verificationCleanerService.RemoveExipiredRecordAsync();
            Console.WriteLine($"Success : VerificationCleaner.Run");
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : VerificationCleaner.Run :: {ex.Message}");
        }

    }

}
