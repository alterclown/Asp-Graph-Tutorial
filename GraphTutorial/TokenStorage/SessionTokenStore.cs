using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System.Threading;
using System.Web;

namespace GraphTutorial.TokenStorage
{
    public class CachedUser
    {
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Avatar { get; set; }
    }
    public class SessionTokenStore
    {
        private static ReaderWriterLockSlim sessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly string userId = string.Empty;
        private readonly string cacheId = string.Empty;
        private readonly string cachedUserId = string.Empty;
        private HttpContextBase httpContext = null;

        TokenCache tokenCache = new TokenCache();

        public SessionTokenStore( string userId, HttpContextBase httpContext)
        {
            this.userId = userId;
            cacheId = $"{userId}_TokenCache";
            cachedUserId = $"{userId}_UserCache";
            this.httpContext = httpContext;
            Load();

        }

        public TokenCache GetMsalCacheInstance() {
            tokenCache.SetBeforeAccess(BeforeAccessNotification);
            tokenCache.SetAfterAccess(AfterAccessNotification);
            Load();
            return tokenCache;
        }

        public bool HasData() {
            return (httpContext.Session[cacheId] != null && ((byte[])httpContext.Session[cacheId]).Length > 0);
        }

        public void clear() {
            httpContext.Session.Remove(cacheId);
        }
        private void Load() {
            sessionLock.EnterReadLock();
            tokenCache.Deserialize((byte[])httpContext.Session[cacheId]);
            sessionLock.ExitReadLock();
        }
        private void Persist() {
            sessionLock.EnterReadLock();
            httpContext.Session[cacheId] = tokenCache.Serialize();
            sessionLock.ExitReadLock();
        }
        private void BeforeAccessNotification(TokenCacheNotificationArgs args) {
            Load();
        }

        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (args.HasStateChanged) {
                Persist();
            }
        }
        public void SaveUserDetails(CachedUser user) {
            sessionLock.EnterReadLock();
            httpContext.Session[cachedUserId] = JsonConvert.SerializeObject(user);
            sessionLock.ExitReadLock();
        }
        public CachedUser GetUserDetails() {
            sessionLock.EnterReadLock();
            var cachedUser = JsonConvert.DeserializeObject<CachedUser>((string)httpContext.Session[cachedUserId]);
            sessionLock.ExitReadLock();
            return cachedUser;
        }
    }
}