using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Juntendo.MedPhys.CoordinateTransform;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Juntendo.MedPhys.Esapi.IscFluenceOptimizer
{
    public class IscFluenceOptimizer
    {
        // List of the values and the coordinates of the points above the dose threshold 
        public List<PointDoseBreakdown> PointsAboveThreshold = new List<PointDoseBreakdown>();

        public double DoseThresholdPc;

        public double PrescribedDosePerFraction;

        public double DoseThresholdAbs;

        public int NumberOfBeams;

        public List<BeamInfo> BeamInfos = new List<BeamInfo>();

        public IscFluenceOptimizer(PlanSetup planSetup, double doseThresholdPc = 10.0)
        {
            DoseThresholdPc = doseThresholdPc;

            PrescribedDosePerFraction = planSetup.UniqueFractionation.PrescribedDosePerFraction.Dose;

            DoseThresholdAbs = (doseThresholdPc/100.0) * PrescribedDosePerFraction;

            NumberOfBeams = planSetup.Beams.Count();

            Dose doseEsapi = planSetup.Dose;

            int xDose3dSize = doseEsapi.XSize;
            int yDose3dSize = doseEsapi.YSize;
            int zDose3dSize = doseEsapi.ZSize;

            double xDose3dRes = doseEsapi.XRes;
            double yDose3dRes = doseEsapi.YRes;
            double zDose3dRes = doseEsapi.ZRes;

            double xDose3dOrigin = doseEsapi.Origin.x;
            double yDose3dOrigin = doseEsapi.Origin.y;
            double zDose3dOrigin = doseEsapi.Origin.z;

            double[,,] dose3dArray = Helpers.EsapiDoseToDose3dArray(doseEsapi);

            var beamDoseArrays = new List<double[,,]>();
            var beamMus = new List<double>();
            var beamMuPerGys = new List<double>();

            double planNormalizationValue = planSetup.PlanNormalizationValue;

            for (int m = 0; m < NumberOfBeams; m++)
            {
                Beam beam = planSetup.Beams.ElementAt(m);
                beamDoseArrays.Add(Helpers.EsapiDoseToDose3dArray(beam.Dose));
                beamMus.Add(beam.Meterset.Value);
                beamMuPerGys.Add(beam.MetersetPerGy);
            }

            for (int i = 0; i < zDose3dSize; i++)
            {
                for (int j = 0; j < yDose3dSize; j++)
                {
                    for (int k = 0; k < xDose3dSize; k++)
                    {
                        double dosePc = dose3dArray[i, j, k];

                        if (dosePc > doseThresholdPc)
                        {
                            double z = zDose3dOrigin + i * zDose3dRes;
                            double y = yDose3dOrigin + j * yDose3dRes;
                            double x = xDose3dOrigin + k * xDose3dRes;

                            VVector p = new VVector(x, y, z);

                            List<double> beamDoses = new List<double>();
                            double doseSum = 0.0;
                            
                            for (int m = 0; m < NumberOfBeams; m++)
                            {
                                var dose = beamDoseArrays[m][i, j, k];
                                var mu = beamMus[m];
                                var muPerGy = beamMuPerGys[m];
                                double beamDose = Helpers.BeamPointDose(dose, muPerGy, mu);
                                beamDose *= PrescribedDosePerFraction;
                                beamDose /= planNormalizationValue;
                                doseSum += beamDose;
                                beamDoses.Add(beamDose);
                            }

                            double totalDose = (PrescribedDosePerFraction * dosePc) / 100.0;

                            //Console.WriteLine($"doseSum: {doseSum:f}, totalDose {totalDose:f}, diff {doseSum - totalDose:g}, ratio {doseSum/totalDose:g}");

                            //if(Math.Abs(doseSum-totalDose) > 1.0E-2)
                            //{
                            //    throw new InvalidOperationException("Total doses does not coincide");
                            //}

                            //this.PointsAboveThreshold.Add(new PointDoseBreakdown(x, y, z, totalDose, beamDoses));
                            this.PointsAboveThreshold.Add(new PointDoseBreakdown(x, y, z, doseSum, beamDoses));
                        }
                    }
                }
            }

            var beams = planSetup.Beams;
            for (int i = 0; i < beams.Count(); i++)
            {
                Beam beam = beams.ElementAt(i);
                ControlPoint controlPoint0 = beam.ControlPoints.First();
                double gantryAngle = controlPoint0.GantryAngle;
                double collimatorAngle = controlPoint0.CollimatorAngle;
                double couchAngle = controlPoint0.PatientSupportAngle;
                double[] isocenter = { beam.IsocenterPosition.x, beam.IsocenterPosition.y, beam.IsocenterPosition.z };
                BeamGeometry beamGeometry = new BeamGeometry(gantryAngle, collimatorAngle, couchAngle, isocenter);

                List<DoseReductionFactor> doseReductionFactors = new List<DoseReductionFactor>();
                foreach (PointDoseBreakdown p in PointsAboveThreshold)
                {
                    double x = p.X;
                    double y = p.Y;
                    double z = p.Z;

                    double totalDose = p.TotalDose;
                    double totalSqauaredDose = p.TotalSquaredDose;
                    double beamDose = p.BeamDoses[i];

                    double doseRed = totalDose - DoseThresholdAbs;
                    double doseReductionFactor = 1.0 - (beamDose * doseRed) / totalSqauaredDose;

                    double[] pPCS = new double[3] { x, y, z };
                    double[] projectedPoint = beamGeometry.ProjectedPointAtIsocenterPlaneInUCS(pPCS);

                    doseReductionFactors.Add(new DoseReductionFactor
                    {
                        X = projectedPoint[0],
                        Y = projectedPoint[2],
                        BeamDose = beamDose,
                        TotalDose = totalDose,
                        DoseThreshold = DoseThresholdAbs,
                        Value = doseReductionFactor
                    });
                }

                BeamInfos.Add(new BeamInfo(beam, beamGeometry, doseReductionFactors));
            }
        }
    }
}
