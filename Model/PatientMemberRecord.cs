using System;
using System.ComponentModel.DataAnnotations;

namespace MasterPatientIndex.Model
{
    public class PatientMemberRecord {
        public Patient patient {get;set;}
        public string patiententerpriseId {get;set;}
        public string   source {get; set;}
        public string medicalRecordNumber {get; set;}
        public string firstName {get; set;}
        public string lastName {get; set;}
        public string socialSecurityNumber {get; set;}
        public Address address {get; set;}
        public long addressId {get; set;}
    }

}
