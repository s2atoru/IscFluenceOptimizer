using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Juntendo.MedPhys.Esapi.IscFluenceOptimizer
{
    public class MakeIscPlan
    {
        public static PlanSetup CopyAndMakeNewIscPlan(double thresholdPc, string newPlanId, Course course, PlanSetup originalPlanSetup, int numberOfSteps = 1)
        {

            if (course.PlanSetups.Where(p => p.Id == newPlanId).Count() > 0)
            {
                throw new ArgumentException($"{newPlanId} already exists");
            }

            Patient patient = originalPlanSetup.Course.Patient;
            
            patient.BeginModifications();
            
            CopyPlan.CopyDynamicMlcPlan(newPlanId, course, (ExternalPlanSetup)originalPlanSetup);
            PlanSetup newPlanSetup = Esapi.IscFluenceOptimizer.Helpers.GetPlanSetup(course, newPlanId);

            // Get Body and Maximum dose
            var query = from s in originalPlanSetup.StructureSet.Structures where (s.Id == "BODY" || s.Id == "Body") select s;
            if (query.Count() != 1) throw new InvalidOperationException("No BODY in StructureSet");
            var body = query.First();
            double binWidth = 0.001;
            DVHData dvh = originalPlanSetup.GetDVHCumulativeData(body, DoseValuePresentation.Relative, VolumePresentation.Relative, binWidth);
            double maximumDosePc = dvh.MaxDose.Dose;

            int numberOfMainBeams = originalPlanSetup.Beams.Count();

            if(maximumDosePc <= thresholdPc)
            {
                throw new InvalidOperationException($"Maximum dose ({maximumDosePc}) is less than threholdPc ({thresholdPc})");
            }

            double thresholdPcStep = (maximumDosePc - thresholdPc) / numberOfSteps;
            for (int i = 0; i < numberOfSteps; i++)
            {
                double thresholdPcTmp = thresholdPc + thresholdPcStep * (numberOfSteps - 1 - i);
                var iscFluenceOptimizer = new IscFluenceOptimizer(newPlanSetup, thresholdPcTmp);

                foreach (var beam in iscFluenceOptimizer.BeamInfos)
                {
                    var newFluence = beam.GetReducedFluence();
                    beam.BeamEsapi.SetOptimalFluence(newFluence);
                }

                var externalPlanSetup = (ExternalPlanSetup)newPlanSetup;

                externalPlanSetup.CalculateLeafMotions(new LMCVOptions(true));
                externalPlanSetup.CalculateDose();
                var newDvh = newPlanSetup.GetDVHCumulativeData(body, DoseValuePresentation.Relative, VolumePresentation.Relative, binWidth);
                var newMaximumDosePc = newDvh.MaxDose.Dose;
            }

            return newPlanSetup;
        }
    }
}
