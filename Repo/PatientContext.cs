using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MasterPatientIndex.Model;

//dotnet tool install --global dotnet-ef --version 3.0.0

namespace MasterPatientIndex.Repo
{
    public class PatientContext : DbContext 
    {
        public PatientContext(DbContextOptions<PatientContext> options):base(options)  
        {  
            Database.Migrate();  
        }          
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<PatientMemberRecord> PatientRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PatientMemberRecord>()
                .HasKey(c => new { c.source, c.medicalRecordNumber });

            modelBuilder.Entity<Patient>().HasData(
                new Patient[] { 
                    new Patient {enterpriseId = "xyz"},  
                    new Patient {enterpriseId = "xyz0"},  
                    new Patient {enterpriseId = "xyz1"},  
                    new Patient {enterpriseId = "xyz2"},  
                    new Patient {enterpriseId = "xyz3"},  
                    }
                );
            
            modelBuilder.Entity<PatientMemberRecord>().HasData(
            new PatientMemberRecord
            {           
                patiententerpriseId = "xyz",
                source = "JHU",
                medicalRecordNumber = "123x",
                firstName = "Bob",  
                lastName = "Ross",  
                socialSecurityNumber = "123-12-1234",
                addressId = 1
            });
            
            modelBuilder.Entity<Address>().HasData(
                new Address{
                    AddressId = 1,
                     addressLine1 = "", addressLine2 = "", city = "", state = "",
                     zipCode = "90036"}  
            );
        }  

    }
}
