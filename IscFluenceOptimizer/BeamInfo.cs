using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Juntendo.MedPhys.CoordinateTransform;
using Juntendo.MedPhys.Esapi.IscFluence;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Juntendo.MedPhys.Esapi.IscFluenceOptimizer
{
    public class BeamInfo
    {
        public Beam BeamEsapi;
        public BeamGeometry BeamGeometry;
        public List<DoseReductionFactor> DoseReductionFactors;
        public IscFluence.IscFluence IscFluence;

        public BeamInfo(Beam beam, BeamGeometry beamGeometry, List<DoseReductionFactor> doseReductionFactors)
        {
            BeamEsapi = beam;
            BeamGeometry = beamGeometry;
            DoseReductionFactors = doseReductionFactors;
            IscFluence = new Esapi.IscFluence.IscFluence(beam);
        }

        /// <summary>
        /// Get reduced fluence
        /// </summary>
        /// <remarks>
        /// IscFluence will be overwritten with reduced fluence.
        /// </remarks>
        /// <returns> Reduced fluence </returns>
        public Fluence GetReducedFluence()
        {
            double xOrigin = IscFluence.OriginX;
            double yOrigin = IscFluence.OriginY;
            double xRes = IscFluence.SpacingX;
            double yRes = IscFluence.SpacingY;
            int xSize = IscFluence.SizeX;
            int ySize = IscFluence.SizeY;
            double[,] minDoseReductionFactorMap
                = MinDoseReductionFactorMap(xOrigin, yOrigin, xRes, yRes, xSize, ySize);

            var newFluenceValues = new float[ySize, xSize];
            for (int i = 0; i < ySize; i++)
            {
                for (int j = 0; j < xSize; j++)
                {
                    double value = IscFluence.Values[i, j];
                    IscFluence.Values[i, j] = value * minDoseReductionFactorMap[i, j];
                    newFluenceValues[i, j] = (float)IscFluence.Values[i, j];
                }
            }
            
            var fluence = new Fluence(newFluenceValues, xOrigin, yOrigin);
            return fluence;
        }

        /// <summary>
        /// Write reduced fluence to a file
        /// </summary>
        /// <remark>
        /// IscFluence will be overwritten with reduced fluence.
        /// </remark>
        /// <param name="outputFilePath"> Path to the output file </param>
        public void WriteReducedFluenceToFile(string outputFilePath)
        {
            double xOrigin = IscFluence.OriginX;
            double yOrigin = IscFluence.OriginY;
            double xRes = IscFluence.SpacingX;
            double yRes = IscFluence.SpacingY;
            int xSize = IscFluence.SizeX;
            int ySize = IscFluence.SizeY;
            double[,] minDoseReductionFactorMap
                = MinDoseReductionFactorMap(xOrigin, yOrigin, xRes, yRes, xSize, ySize);

            for (int i = 0; i < ySize; i++)
            {
                for (int j = 0; j < xSize; j++)
                {
                    double value = IscFluence.Values[i, j];
                    IscFluence.Values[i, j] = value * minDoseReductionFactorMap[i, j];
                }
            }

            IscFluence.WriteToFile(outputFilePath);
        }

        public double[,] MinDoseReductionFactorMap(double xOrigin, double yOrigin, double xRes, double yRes, int xSize, int ySize)
        {
            double[,] minDoseReductionFactorMap = new double[ySize, xSize];
            for (int i = 0; i < ySize; i++)
            {
                for (int j = 0; j < xSize; j++)
                {
                    minDoseReductionFactorMap[i, j] = 1.0;
                }
            }

            foreach (DoseReductionFactor doseReductionFactor in DoseReductionFactors)
            {
                double x = doseReductionFactor.X;
                double y = doseReductionFactor.Y;

                int ix = Juntendo.MedPhys.Esapi.IscFluence.Helpers.GridIndex(x, xOrigin, xRes);
                if (ix >= xSize)
                {
                    throw new InvalidOperationException(String.Format($"ix is out of bounds x: {x:f}, ix: {ix:d}, xSize: {xSize:d}"));
                }

                // yOrigin has the maximum Y value
                // Y index increases form the max Y to the min Y
                int iy = Juntendo.MedPhys.Esapi.IscFluence.Helpers.GridIndex(y, yOrigin, -yRes);
                if (iy >= ySize)
                {
                    throw new InvalidOperationException(String.Format($"iy is out of bounds y: {y:f},  iy: {iy:d}, ySize: {ySize:d}"));
                }

                double doseReductionFactorValue = doseReductionFactor.Value;
                if (minDoseReductionFactorMap[iy, ix] > doseReductionFactorValue)
                {
                    minDoseReductionFactorMap[iy, ix] = doseReductionFactorValue;
                }
            }

            return minDoseReductionFactorMap;
        }
    }

    public struct DoseReductionFactor
    {
        public double X;
        public double Y;

        public double DoseThreshold;

        public double TotalDose;
        public double BeamDose;

        public double Value;
    }
}
