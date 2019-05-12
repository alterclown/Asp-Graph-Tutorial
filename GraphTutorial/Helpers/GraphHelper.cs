using Microsoft.Graph;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GraphTutorial.TokenStorage;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web;
namespace GraphTutorial.Helpers
{
    public static class GraphHelper

    {
        private static string appId = ConfigurationManager.AppSettings["ida:AppId"];
        private static string appSecret = ConfigurationManager.AppSettings["ida:AppSecret"];
        private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        private static string graphScopes = ConfigurationManager.AppSettings["ida:AppScopes"];

        public static async Task<IEnumerable<Event>> GetEventsAsync()
        {
            var graphClient = GetAuthenticatedClient();
            var events = await graphClient.Me.Events.Request().Select("subject,organizer,start,end").OrderBy("createdDateTime DESC").GetAsync();

            return events.CurrentPage;
        }

        private static GraphServiceClient GetAuthenticatedClient()
        {
            return new GraphServiceClient(new DelegateAuthenticationProvider(
                async (requestMessage) =>
                {
                    string signedInUserId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
                    SessionTokenStore sessionTokenStore = new SessionTokenStore(signedInUserId, new HttpContextWrapper(HttpContext.Current));

                    var idClient = new ConfidentialClientApplication(appId, redirectUri, new ClientCredential(appSecret), sessionTokenStore.GetMsalCacheInstance(), null);

                    var accounts = await idClient.GetAccountsAsync();

                    var result = await idClient.AcquireTokenSilentAsync(
                        graphScopes.Split( ' ' ), accounts.FirstOrDefault());
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer",result.AccessToken);
                }));
        }

            public static async Task<User> GetUserDetailsAsync(string accessToken) {
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    }));
            return await graphClient.Me.Request().GetAsync();
        }
    }
}