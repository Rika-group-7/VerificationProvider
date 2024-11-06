using Microsoft.AspNetCore.Http;
using VerificationProvider.Models;

namespace VerificationProvider.Inferfaces
{
    public interface IValidateVerificationCodeService
    {
        Task<ValidateRequest> UnpackValidateRequest(HttpRequest reqeust);
        Task<bool> ValidateCodeAsync(ValidateRequest request);
    }
}