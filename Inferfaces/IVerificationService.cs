using Azure.Messaging.ServiceBus;
using VerificationProvider.Models;

namespace VerificationProvider.Inferfaces
{
    public interface IVerificationService
    {
        string GenerateCode(int length);
        EmailRequest GenerateEmailRequest(VerificationRequest verificationRequest, string code);
        string GenerateServiceBusEmailRequest(EmailRequest emailRequest);
        Task<bool> SaveVerifactionRequest(VerificationRequest verificationRequest, string code);
        VerificationRequest UnpackVerificationRequest(ServiceBusReceivedMessage message);
    }
}