using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VerificationProvider.Inferfaces;

namespace VerificationProvider.Functions;

public class ValidateVerificationCode(ILogger<ValidateVerificationCode> logger, IValidateVerificationCodeService service)
{
    private readonly ILogger<ValidateVerificationCode> _logger = logger;
    private readonly IValidateVerificationCodeService _service = service;

    [Function("ValidateVerificationCode")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "verification")] HttpRequest http)
    {
        try
        {
            var validateRequest = await _service.UnpackValidateRequest(http);
            if (validateRequest != null)
            {
                var validateResult = await _service.ValidateCodeAsync(validateRequest);
                Console.WriteLine($"SUCCESS : ValidateVerificationCode.Run");
                if (validateResult) return new OkResult();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ValidateVerificationCode.Run :: {ex.Message}");
        }

        return new UnauthorizedResult();

    }
}
