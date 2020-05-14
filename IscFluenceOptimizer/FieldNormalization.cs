using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Juntendo.MedPhys.Esapi.IscFluenceOptimizer
{
    public class FieldNormalization
    {
        public static void ChangeToNoFieldNormalization(PlanSetup planSetup)
        {
            var beams = planSetup.Beams;

            if (beams.First().NormalizationMethod == "NO_ISQLAW_NORM")
            {
                return;
            }

            double muSum = 0.0;
            foreach (var b in beams)
            {
                muSum += b.Meterset.Value;
            }

            var isocenter = beams.First().IsocenterPosition;
            var totalDose = planSetup.Dose;

            double doseAtIsocenter = totalDose.GetDoseToPoint(isocenter).Dose;

            var weights = new List<double>();
            foreach (var b in beams)
            {
                var beamParas = b.GetEditableParameters();
                weights.Add(b.Meterset.Value / muSum);
            }

            //var normMethod = planSetup.GetCalculationOptions("AAA_13623");
            planSetup.SetCalculationOption("AAA_13623", "FieldNormalizationType", "No field normalization");
            var externalPlanSetup = (ExternalPlanSetup)planSetup;

            var numberOfBeams = beams.Count();
            for (int i = 0; i < beams.Count(); i++)
            {
                var editableParams = beams.ElementAt(i).GetEditableParameters();
                editableParams.WeightFactor = weights[i]*numberOfBeams;
                beams.ElementAt(i).ApplyParameters(editableParams);
            }

            externalPlanSetup.CalculateDose();

            double planNormalization0 = planSetup.PlanNormalizationValue;
            double doseAtIsocenter0 = planSetup.Dose.GetDoseToPoint(isocenter).Dose;

            planSetup.PlanNormalizationValue = doseAtIsocenter0 / doseAtIsocenter * planNormalization0;

        }
    }
}
