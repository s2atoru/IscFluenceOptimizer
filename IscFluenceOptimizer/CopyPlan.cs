using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Juntendo.MedPhys.Esapi.IscFluenceOptimizer
{
    public class CopyPlan
    {
        public static void CopyStaticMlcPlan(string planId, Course course, ExternalPlanSetup originalPlan)
        {
            var structureSet = originalPlan.StructureSet;
            var plan = course.AddExternalPlanSetup(structureSet);

            if(course.PlanSetups.Where(p => p.Id == planId).Count() > 0)
            {
                throw new ArgumentException($"{planId} already exists");
            }

            plan.Id = planId;

            var dosePerFraction = originalPlan.UniqueFractionation.PrescribedDosePerFraction;
            var numberOfFractions = originalPlan.UniqueFractionation.NumberOfFractions != null ? originalPlan.UniqueFractionation.NumberOfFractions.Value : 0;

            const double prescribedPercentage = 1.0; // Note: 100% corresponds to 1.0
            plan.UniqueFractionation.SetPrescription(numberOfFractions, dosePerFraction, prescribedPercentage);

            foreach (var beam in originalPlan.Beams)
            {
                CopyStaticMlcBeam(beam, plan);
            }
            if (originalPlan.Beams.First().NormalizationMethod == "NO_ISQLAW_NORM")
            {
                plan.SetCalculationOption("AAA_13623", "FieldNormalizationType", "No field normalization");

            }
            plan.PlanNormalizationValue = originalPlan.PlanNormalizationValue;
            plan.CalculateDose();
        }

        public static void CopyDynamicMlcPlan(string planId, Course course, ExternalPlanSetup originalPlan)
        {
            var structureSet = originalPlan.StructureSet;
            var plan = course.AddExternalPlanSetup(structureSet);

            if (course.PlanSetups.Where(p => p.Id == planId).Count() > 0)
            {
                throw new ArgumentException($"{planId} already exists");
            }

            plan.Id = planId;

            var dosePerFraction = originalPlan.UniqueFractionation.PrescribedDosePerFraction;
            var numberOfFractions = originalPlan.UniqueFractionation.NumberOfFractions != null ? originalPlan.UniqueFractionation.NumberOfFractions.Value : 0;

            const double prescribedPercentage = 1.0; // Note: 100% corresponds to 1.0
            plan.UniqueFractionation.SetPrescription(numberOfFractions, dosePerFraction, prescribedPercentage);

            foreach (var beam in originalPlan.Beams)
            {
                CopyFluenceBeam(beam, plan);
            }
            if (originalPlan.Beams.First().NormalizationMethod == "NO_ISQLAW_NORM")
            {
                plan.SetCalculationOption("AAA_13623", "FieldNormalizationType", "No field normalization");

            }
            plan.PlanNormalizationValue = originalPlan.PlanNormalizationValue;
            plan.SetCalculationModel(CalculationType.PhotonLeafMotions, "Varian Leaf Motion Calculator [13.6.23]");
            plan.CalculateLeafMotions(new LMCVOptions(true));
            plan.CalculateDose();
        }

        /// <summary>
        /// Create a copy of an existing beam (beams are unique to plans).
        /// </summary>
        public static void CopyStaticMlcBeam(Beam originalBeam, ExternalPlanSetup plan)
        {
            var MachineParameters = new ExternalBeamMachineParameters(originalBeam.TreatmentUnit.Id,
                    originalBeam.EnergyModeDisplayName,
                    originalBeam.DoseRate,
                    originalBeam.Technique.Id, string.Empty);

            // Create a new beam.
            var collimatorAngle = originalBeam.ControlPoints.First().CollimatorAngle;
            var gantryAngle = originalBeam.ControlPoints.First().GantryAngle;
            var PatientSupportAngle = originalBeam.ControlPoints.First().PatientSupportAngle;
            var jawPositions = originalBeam.ControlPoints.First().JawPositions;
            var leafPositions = originalBeam.ControlPoints.First().LeafPositions;
            var isocenter = originalBeam.IsocenterPosition;

            var beam = plan.AddMLCBeam(MachineParameters, leafPositions, jawPositions, collimatorAngle, gantryAngle,
              PatientSupportAngle, isocenter);

            if (plan.Beams.Where(b => b.Id == originalBeam.Id).Count() > 0)
            {
                throw new InvalidOperationException($"{originalBeam.Id} already exists");
            }

            beam.Id = originalBeam.Id;

            // Copy control points from the original beam.
            var editableParams = beam.GetEditableParameters();
            editableParams.WeightFactor = originalBeam.WeightFactor;
            beam.ApplyParameters(editableParams);
        }

        /// <summary>
        /// Create a copy of an existing beam (beams are unique to plans).
        /// </summary>
        public static void CopyDynamicMlcBeam(Beam originalBeam, ExternalPlanSetup plan)
        {
            var MachineParameters = new ExternalBeamMachineParameters(originalBeam.TreatmentUnit.Id,
                    originalBeam.EnergyModeDisplayName,
                    originalBeam.DoseRate,
                    originalBeam.Technique.Id, string.Empty);

            // Create a new beam.
            var collimatorAngle = originalBeam.ControlPoints.First().CollimatorAngle;
            var gantryAngle = originalBeam.ControlPoints.First().GantryAngle;
            var PatientSupportAngle = originalBeam.ControlPoints.First().PatientSupportAngle;
            var isocenter = originalBeam.IsocenterPosition;
            var metersetWeights = originalBeam.ControlPoints.Select(cp => cp.MetersetWeight);
            var beam = plan.AddSlidingWindowBeam(MachineParameters, metersetWeights, collimatorAngle, gantryAngle,
                PatientSupportAngle, isocenter);

            if (plan.Beams.Where(b => b.Id == originalBeam.Id).Count() > 0)
            {
                throw new InvalidOperationException($"{originalBeam.Id} already exists");
            }

            beam.Id = originalBeam.Id;

            // Copy control points from the original beam.
            var editableParams = beam.GetEditableParameters();
            for (var i = 0; i < editableParams.ControlPoints.Count(); i++)
            {
                editableParams.ControlPoints.ElementAt(i).LeafPositions = originalBeam.ControlPoints.ElementAt(i).LeafPositions;
                editableParams.ControlPoints.ElementAt(i).JawPositions = originalBeam.ControlPoints.ElementAt(i).JawPositions;
            }
            editableParams.WeightFactor = originalBeam.WeightFactor;
            beam.ApplyParameters(editableParams);

            var fluence = originalBeam.GetOptimalFluence();
            beam.SetOptimalFluence(fluence);
        }

        /// <summary>
        /// Create a copy of an existing beam (beams are unique to plans).
        /// </summary>
        public static void CopyFluenceBeam(Beam originalBeam, ExternalPlanSetup plan)
        {
            var MachineParameters = new ExternalBeamMachineParameters(originalBeam.TreatmentUnit.Id,
                    originalBeam.EnergyModeDisplayName,
                    originalBeam.DoseRate,
                    originalBeam.Technique.Id, string.Empty);

            // Create a new beam.
            var collimatorAngle = originalBeam.ControlPoints.First().CollimatorAngle;
            var gantryAngle = originalBeam.ControlPoints.First().GantryAngle;
            var PatientSupportAngle = originalBeam.ControlPoints.First().PatientSupportAngle;
            var isocenter = originalBeam.IsocenterPosition;
            var metersetWeights = originalBeam.ControlPoints.Select(cp => cp.MetersetWeight);
            var beam = plan.AddSlidingWindowBeam(MachineParameters, metersetWeights, collimatorAngle, gantryAngle,
                PatientSupportAngle, isocenter);

            beam.Id = originalBeam.Id;

            // Copy control points from the original beam.
            var editableParams = beam.GetEditableParameters();
            for (var i = 0; i < editableParams.ControlPoints.Count(); i++)
            {
                editableParams.ControlPoints.ElementAt(i).LeafPositions = originalBeam.ControlPoints.ElementAt(i).LeafPositions;
                editableParams.ControlPoints.ElementAt(i).JawPositions = originalBeam.ControlPoints.ElementAt(i).JawPositions;
            }
            editableParams.WeightFactor = originalBeam.WeightFactor;
            beam.ApplyParameters(editableParams);

            var fluence = originalBeam.GetOptimalFluence();
            beam.SetOptimalFluence(fluence);

        }
    }
}
