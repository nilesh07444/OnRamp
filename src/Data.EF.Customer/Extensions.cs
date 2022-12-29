using Common.Command;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.EF.Customer
{
    public static class Extensions
    {
        public static void Dispatch<TCommmand>(this DbMigration migration, TCommmand command) where TCommmand : ICommand
        {
            if (ServiceLocator.IsLocationProviderSet)
                new CommandDispatcher().Dispatch(command);
        }
    }
}
