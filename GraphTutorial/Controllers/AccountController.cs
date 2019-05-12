using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using GraphTutorial.TokenStorage;

namespace GraphTutorial.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public void SignIn()
        {
            if (!Request.IsAuthenticated) {
                Request.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
            
        }

        public ActionResult SignOut()
        {
            if (Request.IsAuthenticated)
            {
                string signedInUserId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
                SessionTokenStore sessionTokenStore = new SessionTokenStore(signedInUserId,HttpContext);
                sessionTokenStore.clear();
                Request.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            }
                return RedirectToAction("Index","Home");
        }
    }
}