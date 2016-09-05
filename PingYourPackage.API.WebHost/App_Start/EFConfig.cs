using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PingYourPackage.Domain.Migrations;
using System.Data.Entity.Migrations;

namespace PingYourPackage.API.WebHost.App_Start
{
    public class EFConfig
    {
        public static void Initialize()
        {
            RunMigrations();
        }

        private static void RunMigrations()
        {
            var efMigrationSettings = new Configuration();
            var efMigrator = new DbMigrator(efMigrationSettings);
            efMigrator.Update();
        }
    }
}