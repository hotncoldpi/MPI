using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace MasterPatientIndex.Model
{
    public class Patient 
    {
        [Key]
        public string enterpriseId {get;set;} // global identifier
        
        [NotMapped]
        public List<PatientMemberRecord> memberRecords {get; set;}  // individual Member records    
    }
}
