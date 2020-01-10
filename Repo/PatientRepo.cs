using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MasterPatientIndex.Model;

namespace MasterPatientIndex.Repo
{
    public class PatientRepo 
    {
        readonly PatientContext _libraryContext;  
        readonly ILogger _logger;
  
        public PatientRepo(PatientContext context, ILogger logger)  
        {  
            _libraryContext = context;
            _logger = logger;  
        }  
  
        public bool addPatient(Patient p, string oldId)
        {
            _logger?.LogInformation("addPatient");

            //add Patient (if no id passed in)
            p.enterpriseId = oldId ?? System.Guid.NewGuid().ToString();
            if (oldId == null)
                _libraryContext.Patients.Add(new Patient{enterpriseId = p.enterpriseId});

            foreach (var v in p.memberRecords) 
            {
                //ensure key doesn't exist, excluding oldId (delete hasn't occured yet)
                if (_libraryContext.PatientRecords
                    .Where(x => x.source == v.source && x.medicalRecordNumber == v.medicalRecordNumber)
                    .Where(x => oldId == null || x.patiententerpriseId != oldId)
                    .Count() > 0)
                    {
                        return false;
                    }
                v.patiententerpriseId = p.enterpriseId;
                _libraryContext.PatientRecords.Add(v);
            }
            
            _libraryContext.SaveChanges();
            return true;
        }

        public void addTestPatientMemberRecord()
        {
            _libraryContext.PatientRecords.Add(new PatientMemberRecord()
            {
                patiententerpriseId = "xyz" + _libraryContext.PatientRecords.ToListAsync().Result.Count() % 3,
                source = "HCGC",
                medicalRecordNumber = "444x" + System.DateTime.Now.Ticks.ToString(),
                firstName = "Bob",  
                lastName = "Ross",  
                socialSecurityNumber = "123-12-1234",
                address = new Address{
                     addressLine1 = "some street", addressLine2 = "", city = "", state = "MD", zipCode = "90037"  
                }
            });
            _libraryContext.SaveChanges();

        }

        public bool updatePatient(Patient p, string enterpriseId, ref bool isDup)
        {
            _logger?.LogInformation("updatePatient");
            isDup = false;

            //ensure patient exists, Where is faster than Contains due to SQL-side filtering
            if (_libraryContext.Patients
                .Where(x => enterpriseId == x.enterpriseId)
                .ToListAsync().Result.Count() < 1)
                return false;

            //get records
            var v = _libraryContext.PatientRecords
                .Where(x => enterpriseId == x.patiententerpriseId)
                .ToListAsync().Result;

            //delete records
            if (v.Count() > 0)
                _libraryContext.PatientRecords.RemoveRange(v.ToArray());

            //add new records
            if (addPatient(p, enterpriseId))
                return true;
            
            isDup = true;
            return false;
        }

        public bool deletePatient(string enterpriseId)
        {
            _logger?.LogInformation("deletePatient");
            var v = _libraryContext.PatientRecords
                .Where(x => enterpriseId == x.patiententerpriseId)
                .ToListAsync().Result;  
            
            //must delete records before patient due to foreign key
            if (v.Count() > 0)
                _libraryContext.PatientRecords.RemoveRange(v.ToArray());
            
            var v2 = _libraryContext.Patients
                .Where(x => enterpriseId == x.enterpriseId)
                .ToListAsync().Result;  
            
            if (v2.Count() < 1)
                return false;
                
            //delete Patient
            _libraryContext.Patients.RemoveRange(v2.ToArray());
            _libraryContext.SaveChanges();
            return true;
        }
        
        public List<Patient> getPatients(string source, string recordNum)  
        {  
            _logger?.LogInformation("getPatient");

            bool skipSearch1  = string.IsNullOrEmpty(source);
            bool skipSearch2 = string.IsNullOrEmpty(recordNum);

            //get the patient id from the record, then get all the records from the id
            List<PatientMemberRecord> v2 = null;
            if (!skipSearch1 || !skipSearch2) 
                v2 = _libraryContext.PatientRecords
                .Select(x => new PatientMemberRecord
                    {patiententerpriseId = x.patiententerpriseId, source = x.source, medicalRecordNumber = x.medicalRecordNumber})
                .Where(x => skipSearch1 || x.source == source)
                .Where(x => skipSearch2 || x.medicalRecordNumber == recordNum)
                .ToListAsync().Result;

            //no results?
            if (v2 != null && v2.Count() < 1)
                return new List<Patient>();

            //currently only supports all Patients or 1 Patient, needs a little more work
            //to support multiple Patients via single search term (source or recordNum)
            var v3 = _libraryContext.PatientRecords
                .Where(x => v2 == null || v2.Count() < 1 || v2.FirstOrDefault().patiententerpriseId == x.patiententerpriseId)
                .OrderBy(x => x.patiententerpriseId)
                .ToListAsync().Result;  
            
            List<Patient> pats = new List<Patient>();
            
            Patient p = null;

            //build Patients from PatientRecords
            foreach (var record in v3)
            {
                if (p == null || p.enterpriseId != record.patiententerpriseId)
                {
                    p = new Patient();
                    p.enterpriseId = record.patiententerpriseId;
                    p.memberRecords = new List<PatientMemberRecord>();
                    pats.Add(p);
                }
                p.memberRecords.Add(record);
            }

            return pats;
        }

        public static PatientMemberRecord createPatientMemberRecord(string src, string mrn)
        {
            return new PatientMemberRecord()
            {
                source = src,
                medicalRecordNumber = mrn,
                firstName = "Bob",  
                lastName = "Ross",  
                socialSecurityNumber = "123-12-1234",
                address = new Address{
                        addressLine1 = "some street", addressLine2 = "", city = "Baltimore", state = "MD", zipCode = "90037"  
                }
            };
        }

        public static void testRepo(PatientContext dbc)
        {
            System.Console.WriteLine("must start with new database using args of --init true");

            PatientRepo pr = new PatientRepo(dbc, null);

            //Get many
            System.Console.WriteLine("Get many");
            IEnumerable<Patient> allrecords = pr.getPatients("", "");
            System.Console.WriteLine("records: " + allrecords.Count());
            foreach (var record in allrecords)
                System.Console.WriteLine("record: " + record.enterpriseId + "=" + record.memberRecords.Count());
            Debug.Assert(allrecords.Count() == 1);

            //Get none
            System.Console.WriteLine("Get none");
            IEnumerable<Patient> norecords = pr.getPatients("a", "b");
            System.Console.WriteLine("records: " + norecords.Count());
            Debug.Assert(norecords.Count() == 0);
            
            //Get one
            System.Console.WriteLine("Get one");
            IEnumerable<Patient> records = pr.getPatients("JHU", "123x");
            System.Console.WriteLine("records: " + records.Count());
            Debug.Assert(records.Count() == 1);
            foreach (var record in records)
                System.Console.WriteLine("record: " + record.enterpriseId + "=" + record.memberRecords.Count());

            //Add
            System.Console.WriteLine("add");
            bool result = pr.addPatient(new Patient()
            {
                memberRecords = new List<PatientMemberRecord>() 
                {
                    PatientRepo.createPatientMemberRecord("JHU","123xz"),
                    PatientRepo.createPatientMemberRecord("JHU","123xy")
                }
            }, null);
            System.Console.WriteLine("result: " + result);
            Debug.Assert(result);

            //Add duplicate (expected failure)
            System.Console.WriteLine("add failure");
            result = pr.addPatient(new Patient()
            {
                memberRecords = new List<PatientMemberRecord>() 
                {
                    PatientRepo.createPatientMemberRecord("JHU","123xz"),
                    PatientRepo.createPatientMemberRecord("JHU","123xx")
                }
            }, null);
            System.Console.WriteLine("result: " + result);
            Debug.Assert(!result);

            //Get one (multiple memberRecords)
            System.Console.WriteLine("Get one");
            records = pr.getPatients("JHU", "123xy");
            System.Console.WriteLine("records: " + records.Count());
            foreach (var record in records)
                System.Console.WriteLine("record: " + record.enterpriseId + "=" + record.memberRecords.Count());
            Debug.Assert(records.Count() > 0);
            Debug.Assert(records.First().memberRecords.Count() == 2);

            //Update
            System.Console.WriteLine("Update");
            bool b = false;

            PatientMemberRecord changedRecord = PatientRepo.createPatientMemberRecord("JHU","123xz");
            changedRecord.firstName = "Rob";
            Patient changedPatient = new Patient
                {memberRecords = new List<PatientMemberRecord>{
                    changedRecord,
                    PatientRepo.createPatientMemberRecord("JHU","123xx")
                    }};
            string eid = records.First().enterpriseId;
            Debug.Assert(pr.updatePatient(changedPatient, eid, ref b));

            records = pr.getPatients("JHU", "123xx");
            Debug.Assert(records.Count() > 0);
            Debug.Assert(records.First().memberRecords.Count() == 2);
            Debug.Assert(records.First().enterpriseId == eid);

            //Delete
            System.Console.WriteLine("Delete");
            Debug.Assert(pr.deletePatient("xyz"));

            allrecords = pr.getPatients("", "");
            Debug.Assert(allrecords.Count() == 1);
            Debug.Assert(allrecords.First().enterpriseId == eid);
        }
    }
}
