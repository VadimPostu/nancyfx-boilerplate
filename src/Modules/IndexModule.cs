using Nancy;
using Nancy.Security;

namespace NancyBoilerplate.Web.Modules
{
    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get["/"] = Get_Index;
        }

        private dynamic Get_Index(object arg)
        {
            if (Context.CurrentUser == null)
                return Response.AsRedirect("~/signin");
            
            if(Context.CurrentUser.HasClaim("administrator"))
                return Response.AsRedirect("~/admin");

            return Response.AsRedirect("~/user");
        }
    }
}