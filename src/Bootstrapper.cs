using System.Collections.Generic;
using System.Web;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Bootstrapper;
using Nancy.Security;
using Nancy.TinyIoc;
using NancyBoilerplate.Web.Core;
using Raven.Client.Document;

namespace NancyBoilerplate.Web
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            var store = new DocumentStore
            {
                ConnectionStringName = "RavenDBLocalIIS"
            };

            store.Initialize();
            container.Register(store);
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);

            var formsAuthConfiguration =
                new FormsAuthenticationConfiguration()
                {
                    RedirectUrl = "~/signin",
                    UserMapper = container.Resolve<IUserMapper>(),
                };

            Csrf.Enable(pipelines);
            FormsAuthentication.Enable(pipelines, formsAuthConfiguration);
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            container.Register<IUserMapper, UserMapper>();

            var documentStore = container.Resolve<DocumentStore>();
            var documentSession = documentStore.OpenSession();

            container.Register(documentSession);
        }
    }
}