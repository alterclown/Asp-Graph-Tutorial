﻿using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using GraphTutorial.Helpers;
using GraphTutorial.TokenStorage;
using System.IdentityModel.Claims;

namespace GraphTutorial.App_Start
{
    public partial class Startup
    {
        private static string appId = ConfigurationManager.AppSettings["ida:AppId"];
        private static string appSecret = ConfigurationManager.AppSettings["ida:AppSecret"];
        private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        private static string graphScopes = ConfigurationManager.AppSettings["ida:AppScopes"];

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(
              new OpenIdConnectAuthenticationOptions
              {
                  ClientId = appId,
                  Authority = "https://login.microsoftonline.com/common/v2.0",
                  Scope = $"openid email profile offline_access {graphScopes}",
                  RedirectUri = redirectUri,
                  PostLogoutRedirectUri = redirectUri,
                  TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = false},
                  Notifications = new OpenIdConnectAuthenticationNotifications
                  {
                      AuthenticationFailed = OnAuthenticationFailedAsync,
                      AuthorizationCodeReceived = OnAuthorizationCodeReceivedAsync
                  }
              }
            );
        }
        private static Task OnAuthenticationFailedAsync(AuthenticationFailedNotification<OpenIdConnectMessage,OpenIdConnectAuthenticationOptions> notification)
        {
            notification.HandleResponse();
            string redirect = $"/Home/Error?message={notification.Exception.Message}";
            if ( notification.ProtocolMessage!=null && !string.IsNullOrEmpty(notification.ProtocolMessage.ErrorDescription)) {
                redirect += $"&debug={notification.ProtocolMessage.ErrorDescription}";
            }
            notification.Response.Redirect(redirect);
            return Task.FromResult(0);
        }

        private async Task OnAuthorizationCodeReceivedAsync(AuthorizationCodeReceivedNotification notification)
        {
            // Get the signed in user's id and create a token cache
            string signedInUserId = notification.AuthenticationTicket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value;
            SessionTokenStore tokenStore = new SessionTokenStore(signedInUserId,
                notification.OwinContext.Environment["System.Web.HttpContextBase"] as HttpContextBase);

            var idClient = new ConfidentialClientApplication(
                appId, redirectUri, new ClientCredential(appSecret), tokenStore.GetMsalCacheInstance(), null);

            try
            {
                string[] scopes = graphScopes.Split(' ');

                var result = await idClient.AcquireTokenByAuthorizationCodeAsync(
                    notification.Code, scopes);

                var userDetails = await GraphHelper.GetUserDetailsAsync(result.AccessToken);

                var cachedUser = new CachedUser()
                {
                    DisplayName = userDetails.DisplayName,
                    Email = string.IsNullOrEmpty(userDetails.Mail) ?
                    userDetails.UserPrincipalName : userDetails.Mail,
                    Avatar = string.Empty
                };

                tokenStore.SaveUserDetails(cachedUser);
            }
            catch (MsalException ex)
            {
                string message = "AcquireTokenByAuthorizationCodeAsync threw an exception";
                notification.HandleResponse();
                notification.Response.Redirect($"/Home/Error?message={message}&debug={ex.Message}");
            }
            catch (Microsoft.Graph.ServiceException ex)
            {
                string message = "GetUserDetailsAsync threw an exception";
                notification.HandleResponse();
                notification.Response.Redirect($"/Home/Error?message={message}&debug={ex.Message}");
            }
        }
    }
}