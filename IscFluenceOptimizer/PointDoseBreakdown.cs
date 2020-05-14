using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Juntendo.MedPhys.Esapi.IscFluenceOptimizer
{
    public class PointDoseBreakdown
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double TotalDose {get;set;}
        public double TotalSquaredDose { get; set; }

        public List<double> BeamDoses = new List<double>();

        public PointDoseBreakdown(double x, double y, double z, double totalDose, List<double> beamDoses)
        {
            X = x;
            Y = y;
            Z = z;
            TotalDose = totalDose;
            BeamDoses = beamDoses;

            TotalSquaredDose = 0.0;
            foreach (double beamDose in beamDoses)
            {
                TotalSquaredDose += beamDose * beamDose;
            }
        }
    }
}
