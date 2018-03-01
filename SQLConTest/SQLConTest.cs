using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace SQLConTest
{
    class SQLConTest
    {
        static void Main(string[] args)
        {
            var dbConfigurator = new DBConfigurator();

            dbConfigurator.ConnectionsTest();

            //Console.WriteLine("Press any keys to continue...");
            //Console.ReadLine();
        }
    }
}
