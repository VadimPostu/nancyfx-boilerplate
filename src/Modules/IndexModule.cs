using Nancy;

namespace NancyBoilerplate.Web.Modules
{
    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get["/"] = Get_Index;
        }

        private object Get_Index(object arg)
        {
            return Response.AsJson(Context.CurrentUser);
        }
    }
}