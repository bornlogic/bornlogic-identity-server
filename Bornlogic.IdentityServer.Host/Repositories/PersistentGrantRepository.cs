using Bornlogic.IdentityServer.Storage.Models;
using Bornlogic.IdentityServer.Storage.Stores;

namespace Bornlogic.IdentityServer.Host.Repositories
{
    internal static class PersitentGrantMemoryRepository
    {
        public static IList<PersistedGrant> Grants = new List<PersistedGrant>();
    }

    internal class PersistentGrantRepository : IPersistentGrantRepository
    {
        public Task Insert(PersistedGrant persistedGrant)
        {
            PersitentGrantMemoryRepository.Grants.Add(persistedGrant);

            return Task.CompletedTask;
        }

        public async Task<PersistedGrant> GetByKey(string key)
        {
            return PersitentGrantMemoryRepository.Grants.FirstOrDefault(a => a.Key == key);
        }

        public async Task<IEnumerable<PersistedGrant>> GetByFilters(PersistedGrantFilter filter)
        {
            return PersitentGrantMemoryRepository.Grants.Where(a =>
                (filter.ClientId == null || a.ClientId == filter.ClientId) &&
                (filter.SubjectId == null || a.SubjectId == filter.SubjectId) &&
                (filter.SessionId == null || a.SessionId == filter.SessionId) &&
                (filter.Type == null || a.Type == filter.Type));
        }

        public Task DeleteByKey(string key)
        {
            PersitentGrantMemoryRepository.Grants = PersitentGrantMemoryRepository.Grants.Where(a => a.Key != key).ToList();

            return Task.CompletedTask;
        }

        public Task DeleteByFilters(PersistedGrantFilter filter)
        {
            PersitentGrantMemoryRepository.Grants = PersitentGrantMemoryRepository.Grants.Where(a =>
                !((filter.ClientId == null || a.ClientId == filter.ClientId) &&
                  (filter.SubjectId == null || a.SubjectId == filter.SubjectId) &&
                  (filter.SessionId == null || a.SessionId == filter.SessionId) &&
                  (filter.Type == null || a.Type == filter.Type))).ToList();

            return Task.CompletedTask;
        }
    }
}
