using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Juntendo.MedPhys.Esapi.IscFluence
{
    public class IscPlanSelect
    {
        public string PatientName { get; set; }
        public string PatientId { get; set; }
        public string PlanId { get; set; }
        public List<string> TemplatePlanIds { get; set; }
        public string SelectedTemplatePlanId { get; set; }
        public double Margin { get; set; } = 1.25;
        public double FlushFluenceValue { get; set; } = 0.5;
        public double MinimumFluenceValue { get; set; } = 0.5;

        /// <summary>
        /// Test Constructor
        /// </summary>
        public IscPlanSelect()
        {
            PatientName = "テスト　テスト";
            PatientId = "0123456789";
            PlanId = "1-1-1";
            TemplatePlanIds = new List<string> { "tempISC", "orig1-1-1" };

            SelectedTemplatePlanId = "";
            Margin = 1.25;
            FlushFluenceValue =  0.5;
            MinimumFluenceValue = 0.5;
        }

        public IscPlanSelect(string planId, string courseId, Patient currentPatient)
        {
            PatientName = currentPatient.Name;
            PatientId = currentPatient.Id;
            PlanId = planId;

            var currentCourse = Helpers.GetCourse(currentPatient, courseId);

            TemplatePlanIds = new List<string>();

            foreach (var p in currentCourse.PlanSetups)
            {
                if (p.Id != planId)
                {
                    TemplatePlanIds.Add(p.Id);
                }
            }
            
            SelectedTemplatePlanId = "";

            Margin = 0.4; // from the experimental study
            FlushFluenceValue = 0.5;
            MinimumFluenceValue = 0.5;
        }

    }
}
