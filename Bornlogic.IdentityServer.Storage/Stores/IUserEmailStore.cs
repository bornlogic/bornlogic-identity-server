namespace Bornlogic.IdentityServer.Storage.Stores
{
    public interface IUserEmailStore
    {
        Task<bool> UserEmailIsConfirmedAsync(string subjectId);
    }
}
