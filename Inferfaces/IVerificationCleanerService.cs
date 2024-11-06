namespace VerificationProvider.Inferfaces
{
    public interface IVerificationCleanerService
    {
        Task RemoveExipiredRecordAsync();
    }
}