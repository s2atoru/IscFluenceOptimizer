using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Juntendo.MedPhys.Esapi.IscFluenceOptimizer
{
    public class ParameterSettingViewModel
    {
        public string PatientName { get; set; }
        public string PatientId { get; set; }
        public string PlanId { get; set; }
        public string NewPlanId { get; set; }
        public double DoseThresholdPc { get; set; }
        public int NumberOfSteps { get; set; }

        public ParameterSettingViewModel(string planId, string courseId, Patient currentPatient)
        {
            PatientName = currentPatient.Name;
            PatientId = currentPatient.Id;
            PlanId = planId;
            NewPlanId = "New_" + planId;
            DoseThresholdPc = 107.0;
            NumberOfSteps = 1;
        }
    }

}
