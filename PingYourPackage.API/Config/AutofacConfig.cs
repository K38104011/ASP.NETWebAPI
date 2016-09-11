using Autofac;
using Autofac.Integration.WebApi;
using PingYourPackage.Domain.Entities;
using PingYourPackage.Domain.Entities.Core;
using PingYourPackage.Domain.Entities.Service;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace PingYourPackage.API.Config
{
    public class AutofacConfig
    {
        public static void Initialize(HttpConfiguration config)
        {
            Initialize(config, RegisterServices(new ContainerBuilder()));
        }

        public static void Initialize(HttpConfiguration config, IContainer container)
        {
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }

        private static IContainer RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly()).PropertiesAutowired();

            // EF DbContext
            builder.RegisterType<EntitiesContext>().As<DbContext>().InstancePerRequest();

            builder.RegisterGeneric(typeof(EntityRepository<>))
                .As(typeof(IEntityRepository<>))
                .InstancePerRequest();

            // Services
            builder.RegisterType<CryptoService>()
                .As<ICryptoService>()
                .InstancePerRequest();

            builder.RegisterType<MembershipService>()
                .As<IMembershipService>()
                .InstancePerRequest();

            builder.RegisterType<ShipmentService>()
                .As<IShipmentService>()
                .InstancePerRequest();

            return builder.Build();
        }

    }
}
