using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Juntendo.MedPhys.CoordinateTransform
{
    //TODO: Make the code to take account of a patient position other than head-first spine
    /// <summary>
    /// Class for Beam geometry
    /// </summary>
    /// <remarks>
    /// Scales are in mm and angle in degree
    /// Linac scale: IEC 61217 (Varian 1217)
    /// Planning coordinate: Varian Eclipse default fixed in the room.
    /// (Only the directions of the axes matter.)
    /// Linac scale used in Juntendo University Hospital is Varian IEC 601-2-1
    /// (The sense of Couch rotation is reversed.)
    /// </remarks>
    public class BeamGeometry
    {
        // Gantry angle in degree
        public double GantryAngle { get; set; }

        // Collimator angle in degree
        public double CollimatorAngle { get; set; }

        // Couch angle in degree
        public double CouchAngle { get; set; }

        // Isocenter coordinate in planning coordinate system in mm
        public double[] Isocenter = new double[3] { 0.0, 0.0, 0.0 };

        // Source position in the planning coordinate system
        public double[] SourcePosition = new double[3] { 0.0, 0.0, 0.0 };

        public double SourceToAxisDistance { get; set; } = 1000.0;

        public PatientOrientation PatientOrientation = PatientOrientation.NoOrientation;

        /// <summary>
        /// Constructor for BeamGeometry
        /// </summary>
        /// <param name="gantryAngle"> Gantry angle in degree </param>
        /// <param name="collimatorAngle"> Collimator angle in degree </param>
        /// <param name="couchAngle"> Couch angle in radian </param>
        /// <param name="isocenter"> Isocenter coordinate in the planning coordinate system in mm </param>
        /// <para name="patientOrientation"> Enum for patient orientation </para>
        public BeamGeometry(double gantryAngle, double collimatorAngle,
            double couchAngle, double[] isocenter,
            PatientOrientation patientOrientation = PatientOrientation.NoOrientation)
        {
            this.GantryAngle = gantryAngle;
            this.CollimatorAngle = collimatorAngle;
            this.CouchAngle = couchAngle;
            for (int i = 0; i < 3; i++)
            {
                this.Isocenter[i] = isocenter[i];
            }

            CoordinateTransform3D.SourceCoordinateInPlanningCoordinate(
                SourcePosition, isocenter, gantryAngle, collimatorAngle, couchAngle, SourceToAxisDistance);

        }

        public void UpdateSourcePosition()
        {
            CoordinateTransform3D.SourceCoordinateInPlanningCoordinate(
               SourcePosition, Isocenter, GantryAngle, CollimatorAngle, CouchAngle, SourceToAxisDistance);
        }

        /// <summary>
        /// Transform Planning coordinate to Unit coordinate
        /// </summary>
        /// <param name="planningCoordinate"> Planning coordinate to transform </param>
        /// <param name="unitCoordinate"> Transformed Unit coordinate </param>
        public void PCStoUCS(double[] planningCoordinate, double[] unitCoordinate)
        {
            CoordinateTransform3D.PlanningToUnitCoordinate(Isocenter,
            GantryAngle, CollimatorAngle, CouchAngle,
            planningCoordinate, unitCoordinate);

            return;
        }

        /// <summary>
        /// Transform Unit coordinate to Planning coordinate
        /// </summary>
        /// <param name="unitCoordinate"> Unit coordinate to transform </param>
        /// <param name="planningCoordinate"> Transformed Planning coordinate </param>
        public void UCStoPCS(double[] unitCoordinate, double[] planningCoordinate)
        {
            CoordinateTransform3D.UnitToPlanningCoordinate(Isocenter,
            GantryAngle, CollimatorAngle, CouchAngle,
            unitCoordinate, planningCoordinate);

            return;
        }

        public double[] ProjectedPointAtIsocenterPlaneInUCS(double[] pointPCS, double SAD = 1000)
        {
            double[] sourceUCS = new double[3] { 0.0, -SAD, 0.0 };
            double[] pointUCS = new double[3] { 0.0, 0.0, 0.0 };
            CoordinateTransform3D.PlanningToUnitCoordinate(Isocenter,
            GantryAngle, CollimatorAngle, CouchAngle,
            pointPCS, pointUCS);

            double[] sourceToPointUCS = new double[3] { pointUCS[0] - sourceUCS[0], pointUCS[1] - sourceUCS[1], pointUCS[2] - sourceUCS[2] };

            if (sourceToPointUCS[1] < Double.Epsilon)
            {
                throw new InvalidOperationException("pointPCS is on or above the source plane");
            }

            double sf = SAD / Math.Abs(sourceToPointUCS[1]);
            double[] sourceToPointAtIcpUCS = new double[3] { sf * sourceToPointUCS[0], sf * sourceToPointUCS[1], sf * sourceToPointUCS[2] };

            double[] projectedPointAtIcpUCS = new double[3] {
                sourceUCS[0]+sourceToPointAtIcpUCS[0],
                sourceUCS[1]+sourceToPointAtIcpUCS[1],
                sourceUCS[2]+sourceToPointAtIcpUCS[2] };

            return projectedPointAtIcpUCS;
        }
    }
}
