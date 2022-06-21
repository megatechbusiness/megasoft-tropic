using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Megasoft2
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        MegasoftEntities sc = new MegasoftEntities();
        private readonly string[] allowedActivities;
        public CustomAuthorizeAttribute(params string[] Activity)
        {
            this.allowedActivities = Activity;
        }


        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {

            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return false;
            }

            if (HttpContext.Current.Request.Cookies["SysproDatabase"] == null)
            {
                return false;
            }

            var IsAdmin = (from a in sc.mtUsers where a.Username == HttpContext.Current.User.Identity.Name.ToUpper() select a.Administrator).FirstOrDefault();
            if (IsAdmin == true)
            {
                return true;
            }

            bool authorize = false;

            var Sysprodb = HttpContext.Current.Request.Cookies["SysproDatabase"].Value;

            var UseRoles = (from a in sc.mtSystemSettings select a).FirstOrDefault();
            if (UseRoles.UseRoles == true)
            {
                foreach (var role in allowedActivities)
                {
                    var HasAccess = (from a in sc.mtUserRoles
                                     join b in sc.mtRoleFunctions on a.Role equals b.Role
                                     where a.Username.Equals(HttpContext.Current.User.Identity.Name.ToUpper())
                                     && b.ProgramFunction.Equals(role)
                                     select 1);
                    if (HasAccess.Count() > 0)
                    {
                        authorize = true;
                    }
                }
            }
            else
            {
                foreach (var role in allowedActivities)
                {
                    var HasAccess = (from a in sc.mtUsers
                                     join b in sc.mtOpFunctions on a.Username equals b.Username
                                     where a.Username.Equals(HttpContext.Current.User.Identity.Name.ToUpper())
                                     && b.ProgramFunction.Equals(role)
                                     select 1);
                    if (HasAccess.Count() > 0)
                    {
                        authorize = true;
                    }
                }
            }




            return authorize;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                var returnurl = filterContext.HttpContext.Request.Url;

                filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary(
                    new
                    {
                        controller = "Login",
                        action = "Index",
                        //ReturnUrl = returnurl
                        ReturnUrl = filterContext.HttpContext.Request.Url.GetComponents(UriComponents.PathAndQuery, UriFormat.SafeUnescaped)

                    })
                );

            }
            else
            {
                if (HttpContext.Current.Request.Cookies["SysproDatabase"] == null)
                {

                    filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary(
                    new
                    {
                        controller = "Login",
                        action = "Index",
                        //ReturnUrl = returnurl
                        ReturnUrl = filterContext.HttpContext.Request.Url.GetComponents(UriComponents.PathAndQuery, UriFormat.SafeUnescaped)

                    })
                );
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary(
                    new
                    {
                        controller = "AccessDenied",
                        action = "Index"
                    })
                );
                }

            }
        }
    }
}