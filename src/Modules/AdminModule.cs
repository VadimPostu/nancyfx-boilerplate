using Nancy;
using Nancy.Security;

namespace NancyBoilerplate.Web.Modules
{
    public class AdminModule : NancyModule
    {
        public AdminModule()
        {
            this.RequiresClaims(new[] { "administrator" });

            Get["/admin"] = Get_Admin;
        }

        private object Get_Admin(object arg)
        {
            return "administrators only";
        }
    }
}