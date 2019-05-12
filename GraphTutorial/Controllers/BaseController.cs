using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GraphTutorial.Models;
using GraphTutorial.TokenStorage;
using System.Security.Claims;
using System.Web;
using Microsoft.Owin.Security.Cookies;

namespace GraphTutorial.Controllers
{
    public abstract class BaseController : Controller
    {
        protected void Flash(string message, string debug=null) {
            var alerts= TempData.ContainsKey(Alert.AlertKey)?(List<Alert>)TempData[Alert.AlertKey]:new List<Alert>();
            alerts.Add(new Alert {
                Message = message,
                Debug =debug
            });
        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Request.IsAuthenticated) {
                string signedInUserId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
                SessionTokenStore sessionTokenStore = new SessionTokenStore(signedInUserId,HttpContext);

                if (sessionTokenStore.HasData())
                {
                    ViewBag.User = sessionTokenStore.GetUserDetails();
                }
                else {
                    Request.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
                    filterContext.Result = RedirectToAction("Index","Home");
                }
            }
            base.OnActionExecuting(filterContext);
        }
       

    }
}