using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using MasterPatientIndex.Model;
using MasterPatientIndex.Repo;

namespace MasterPatientIndex.Controllers
{
     public class DurationLoggingAttribute :  ActionFilterAttribute
     {
        DateTime dt;

        public DurationLoggingAttribute()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            dt = DateTime.Now;
         }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            TimeSpan ts = DateTime.Now - dt;
            string s = filterContext.ActionDescriptor.DisplayName;
            MPIController mpi = filterContext.Controller as MPIController;
            mpi.Logger.LogDebug(s + " duration=" +ts.TotalMilliseconds + "ms");
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class MPIController : ControllerBase
    {
        private readonly ILogger<MPIController> _logger;
        private readonly PatientContext _pc;

        public ILogger<MPIController> Logger { get { return _logger; } }

        public MPIController(ILogger<MPIController> logger, PatientContext pc)
        {
            _logger = logger;
            _pc = pc;
        }

        [HttpPost]
        [DurationLoggingAttribute]
        public IActionResult Post(Patient p)
        {
            Logger.LogInformation("Post()");

            PatientRepo pr = new PatientRepo(_pc, Logger);
            if (pr.addPatient(p, null))
                return Ok(p);
            return UnprocessableEntity("duplicate PatientMemberRecord composite key");
        }
        
        [HttpDelete]
        [Route("{enterpriseId}")]
        [DurationLoggingAttribute]
        public IActionResult Delete(string enterpriseId)
        {
            Logger.LogInformation("Delete():" + enterpriseId);

            PatientRepo pr = new PatientRepo(_pc, Logger);
            
            if (pr.deletePatient(enterpriseId))
                return Ok();
            return NotFound();
        }

        [HttpPut]
        [Route("{enterpriseId}")]
        [DurationLoggingAttribute]
        public IActionResult Put(Patient p, string enterpriseId)
        {
            Logger.LogInformation("Put():" + enterpriseId);

            PatientRepo pr = new PatientRepo(_pc, Logger);
            
            bool isDup = false;
            if (pr.updatePatient(p, enterpriseId, ref isDup))
                return Ok();
            
            if (isDup) 
                return UnprocessableEntity("duplicate PatientMemberRecord composite key");
            
            return NotFound();
        }

        [HttpGet]
        [Route("{source}/{mrn}")]
        [DurationLoggingAttribute]
        public IActionResult Get(string source, string mrn)
        {
            Logger.LogInformation("Get():" + source + "/" + mrn);

            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(mrn))
                return NotFound();
            
            PatientRepo pr = new PatientRepo(_pc, Logger);
            
            IEnumerable<Patient> records = pr.getPatients(source, mrn);
            if (records.Count() > 0)
                return Ok(records.First());

            return NotFound();
        }

        [HttpGet]
        [DurationLoggingAttribute]
        public IEnumerable<Patient> Get()
        {
            PatientRepo pr = new PatientRepo(_pc, Logger);
            IEnumerable<Patient> records = pr.getPatients(Request.Query["source"], Request.Query["record"]);
            return records;
        }
    }
}
