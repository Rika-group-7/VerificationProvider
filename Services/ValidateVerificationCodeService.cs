using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Data.Context;
using VerificationProvider.Inferfaces;
using VerificationProvider.Models;

namespace VerificationProvider.Services;

public class ValidateVerificationCodeService(ILogger<ValidateVerificationCodeService> logger, DataContext dataContext) : IValidateVerificationCodeService
{
    private readonly ILogger<ValidateVerificationCodeService> _logger = logger;
    private readonly DataContext _dataContext = dataContext;


    public async Task<bool> ValidateCodeAsync(ValidateRequest request)
    {
        try
        {
            var entity = await _dataContext.verificationRequest.FirstOrDefaultAsync(x => x.Email == request.Email && x.Code == request.Code);
            if (entity != null)
            {
                _dataContext.verificationRequest.Remove(entity);
                await _dataContext.SaveChangesAsync();
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ValidateVerificationCode.ValidateCodeAsync :: {ex.Message}");
        }

        return false;

    }

    public async Task<ValidateRequest> UnpackValidateRequest(HttpRequest reqeust)
    {
        try
        {
            var body = await new StreamReader(reqeust.Body).ReadToEndAsync();
            if (body != null && !string.IsNullOrEmpty(body))
            {
                var validateRequest = JsonConvert.DeserializeObject<ValidateRequest>(body);
                if (validateRequest != null) return validateRequest;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ValidateVerificationCode.UnpackValidateRequest :: {ex.Message}");
        }
        return null!;



    }


}
