using System;
using System.Security.Principal;
using System.Web;
using System.Web.Security;

namespace CQRS.Sample.GUI
{
    public static class FormAutenticationExtensions
    {
        public static int SetAuthCookie(this HttpResponseBase responseBase, string name, bool rememberMe, Guid userData)
        {
            var cookie = FormsAuthentication.GetAuthCookie(name, rememberMe);
            var ticket = FormsAuthentication.Decrypt(cookie.Value);

            var newTicket = new FormsAuthenticationTicket(ticket.Version,
                                                          ticket.Name,
                                                          ticket.IssueDate,
                                                          ticket.Expiration,
                                                          ticket.IsPersistent,
                                                          userData.ToString(),
                                                          ticket.CookiePath);
            var encTicket = FormsAuthentication.Encrypt(newTicket);

            cookie.Value = encTicket;

            responseBase.Cookies.Add(cookie);

            return encTicket.Length;
        }

        public static void InitPrincipal(this HttpContext httpContext)
        {
            if (httpContext.Request.IsAuthenticated)
            {
                var cookie = httpContext
                    .Request
                    .Cookies[FormsAuthentication.FormsCookieName];

                if (null != cookie)
                {
                    var decrypted = FormsAuthentication.Decrypt(cookie.Value);
                    Guid streamID;
                    if (null != decrypted && Guid.TryParse(decrypted.UserData, out streamID))
                    {
                        var username = httpContext.User.Identity.Name;
                        var principal = new CustomPrincipal(new CustomIdentitiy(username, streamID));

                        httpContext.User = principal;
                    }
                }
            }
        }


        public static Guid GetStreamId(this HttpContextBase context)
        {
            if (context.Request.IsAuthenticated && context.User is CustomPrincipal)
            {
                return ((CustomIdentitiy) ((CustomPrincipal) context.User).Identity).StreamId;
            }
            else
            {
                return Guid.Empty;
            }
        }
        
        public static Guid GetStreamId(this HttpContext context)
        {
            if (context.Request.IsAuthenticated && context.User is CustomPrincipal)
            {
                return ((CustomIdentitiy) ((CustomPrincipal) context.User).Identity).StreamId;
            }
            else
            {
                return Guid.Empty;
            }
        }
    }

    internal class CustomPrincipal : GenericPrincipal
    {
        public CustomPrincipal(CustomIdentitiy identity) : base(identity, new string[0]) {}
    }

    internal class CustomIdentitiy : GenericIdentity
    {
        public CustomIdentitiy(string userName, Guid streamId) : base(userName)
        {
            StreamId = streamId;
        }

        public Guid StreamId { get; private set; }

        public override string ToString()
        {
            return String.Format("{0} ({1})", Name, StreamId);
        }
    }
}