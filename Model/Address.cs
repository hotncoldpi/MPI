using System.ComponentModel.DataAnnotations;

namespace MasterPatientIndex.Model
{
    public class Address 
    {
        [Key]
        public long  AddressId {get;set;}
        public string addressLine1 {get; set;}
        public string addressLine2 {get; set;}
        public string city {get; set;}
        public string state {get; set;}
        public string zipCode {get; set;}
    }
}
