using Nancy;
using Nancy.Security;
using Nancy.ModelBinding;
using NancyBoilerplate.Web.Entities;
using Raven.Client;
using System;
using System.Linq;
using NancyBoilerplate.Web.Core;
using System.Collections.Generic;

namespace NancyBoilerplate.Web.Modules
{
    public class IndexModule : NancyModule
    {
        private readonly IDocumentSession _session;

        private readonly UserMapper _userMapper;

        public IndexModule(IDocumentSession session, UserMapper userMapper)
        {
            _session = session;
            _userMapper = userMapper;
            Get["/"] = Get_Index;
        }

        private dynamic Get_Index(object arg)
        {
            if (Context.CurrentUser == null)
            {
                var users = _session.Query<User>()
                .Customize(q => q.WaitForNonStaleResults()).ToArray();

                if (users.Length > 0)
                {
                    return Response.AsRedirect("~/signin");
                }
                else
                {
                    List<string> claims = new List<string> { "user", "administrator" };
                    var user = _userMapper.CreateUser("admin", "vadimpostu@yahoo.com", "admin", claims.ToArray());
                }
            }
            
            if(Context.CurrentUser.HasClaim("administrator"))
                return Response.AsRedirect("~/admin/users");

            return Response.AsRedirect("~/user/books");
        }
    }
}