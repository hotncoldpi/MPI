using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MasterPatientIndex.Repo;
using MasterPatientIndex.Model;

namespace MasterPatientIndex
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "test")
            {
                System.Console.WriteLine("testing...");

                //load config file
                var config = new ConfigurationBuilder()
                    .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true)
                    .Build();
                
                //setup db
                var optionsBuilder = new DbContextOptionsBuilder<PatientContext>();
                optionsBuilder.UseSqlServer(
                    config.GetConnectionString("test")
                );

                try
                {
                    //connect to db
                    PatientContext dbc = new PatientContext(optionsBuilder.Options);

                    MasterPatientIndex.Repo.PatientRepo.testRepo(dbc);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("caught error: " + ex.Message + 
                        " inner: " + ex.InnerException?.Message);
                }
                return;
            }
            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
