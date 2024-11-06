using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VerificationProvider.Data.Context;
using VerificationProvider.Inferfaces;

namespace VerificationProvider.Services;

public class VerificationCleanerService(ILogger<VerificationCleanerService> logger, DataContext dataConext) : IVerificationCleanerService
{
    private readonly ILogger<VerificationCleanerService> _logger = logger;
    private readonly DataContext _dataConext = dataConext;


    public async Task RemoveExipiredRecordAsync()
    {
        try
        {
            var expired = await _dataConext.verificationRequest.Where(x => x.ExpirationDate <= DateTime.UtcNow).ToListAsync();
            _dataConext.RemoveRange(expired);
            await _dataConext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : VerificationCleanerService.RemoveExpiredRecordAsync :: {ex.Message}");
        }

    }
}
