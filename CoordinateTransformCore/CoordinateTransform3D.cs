﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Juntendo.MedPhys.CoordinateTransform
{

    public enum PatientOrientation
    {
        NoOrientation = 0,              ///< No orientation. 
        HeadFirstSupine = 1,            ///< Head first - supine. 
        HeadFirstProne = 2,             ///< Head first - prone. 
        HeadFirstDecubitusRight = 3,    ///< Head first - decubitus right. 
        HeadFirstDecubitusLeft = 4,     ///< Head first - decubitus left. 
        FeetFirstSupine = 5,            ///< Feet first - supine. 
        FeetFirstProne = 6,             ///< Feet first - prone. 
        FeetFirstDecubitusRight = 7,    ///< Feet first - decubitus right. 
        FeetFirstDecubitusLeft = 8,     ///< Feet first - decubitus left. 
        /// Sitting is not defined.
    }

    public struct PatientToCouchCsTranform
    {
        static public double[,,] Matrix =
        { { // NoOrientation = 0
            {1.0, 0.0, 0.0,},
            {0.0, 1.0, 0.0,},
            {0.0, 0.0, 1.0,} },
          { // HeadFirstSupine = 1  
            {1.0, 0.0, 0.0,},
            {0.0, 1.0, 0.0,},
            {0.0, 0.0, 1.0,} },
          { // HeadFirstProne = 2 
            {-1.0, 0.0, 0.0,},
            {0.0, -1.0, 0.0,},
            {0.0, 0.0, 1.0,} },
          {  // HeadFirstDecubitusRight = 3
            {0.0, 1.0, 0.0,},
            {-1.0, 0.0, 0.0,},
            {0.0, 0.0, 1.0,} },
          { // HeadFirstDecubitusLeft = 4
            {0.0, -1.0, 0.0,},
            {1.0, 0.0, 0.0,},
            {0.0, 0.0, 1.0,} },
          { // FeetFirstSupine = 5
            {-1.0, 0.0, 0.0,},
            {0.0, 1.0, 0.0,},
            {0.0, 0.0, -1.0,} },
          { // FeetFirstProne = 6
            {1.0, 0.0, 0.0,},
            {0.0, -1.0, 0.0,},
            {0.0, 0.0, -1.0,} },
          { //FeetFirstDecubitusRight = 7
            {0.0, -1.0, 0.0,},
            {-1.0, 0.0, 0.0,},
            {0.0, 0.0, -1.0,} },
          { //FeetFirstDecubitusLeft = 8
            {0.0, 1.0, 0.0,},
            {1.0, 0.0, 0.0,},
            {0.0, 0.0, -1.0,} }
        };
           

    }

    /// <summary>
    /// Coordinate Transform in the 3 dimension
    /// </summary>
    /// <remarks>
    /// Scales are in mm.
    /// Linac scale: IEC 61217 (Varian 1217)
    /// Planning coordinate: Varian Eclipse default fixed in the room.
    /// (Only the directions of the axes matter.)
    /// Linac scale used in Juntendo University Hospital is Varian IEC 601-2-1.
    /// (The sense of Couch rotation is reversed.)
    /// <para>
    /// The axis directions in Unit coordinate system follow the DICOM default with the head-first-spine.
    /// It is different from the convention in PLUNC.
    /// </para>
    /// </remarks>
    public class CoordinateTransform3D
    {
        /// <summary>
        /// Calculate the source coordinate in the planning coordinate system
        /// </summary>
        /// <param name="sourceCoordinateInPCS"> Source coordinate in the planning coordinate system (output) </param>
        /// <param name="isocenter">Isocenter coordinate in the planning coordinate system in mm </param>
        /// <param name="gantryAngle"> Gantry angle in degree </param>
        /// <param name="collimatorAngle"> Collimator angle in degree </param>
        /// <param name="couchAngle"> Couch angle in degree </param>
        /// <param name="SAD"> Source to axis distance in mm </param>
        public static void SourceCoordinateInPlanningCoordinate
            (double[] sourceCoordinateInPCS,
                double[] isocenter,
                double gantryAngle, double collimatorAngle, double couchAngle,
                double SAD = 1000.0)
        {
            double[] sourceCoordinateInUCS = { 0, -SAD, 0 };

            for (int i = 0; i < 3; i++)
            {
                sourceCoordinateInPCS[i] = 0.0;
            }

            UnitToPlanningCoordinate(isocenter, gantryAngle, collimatorAngle, couchAngle,
                sourceCoordinateInUCS, sourceCoordinateInPCS);

            return;
        }

        public static double SourceToPointDistance(double[] pointCoordinate, double[] isocenter,
            double gantryAngle, double collimatorAngle, double couchAngle, double SAD = 1000.0)
        {
            double[] pointCoordinateInUCS = { 0, 0, 0 };
            PlanningToUnitCoordinate(isocenter,
            gantryAngle, collimatorAngle, couchAngle,
            pointCoordinate, pointCoordinateInUCS);

            double distance = Math.Sqrt(
                Math.Pow(pointCoordinateInUCS[0], 2)
                + Math.Pow(pointCoordinateInUCS[1]+SAD, 2)
                + Math.Pow(pointCoordinateInUCS[2], 2));

            return distance;
        }

        public static double OffAxisDistanceAtIsocenterPlane(double[] pointCoordinate, double[] isocenter,
            double gantryAngle, double collimatorAngle, double couchAngle, double SAD = 1000.0)
        {
            double[] pointCoordinateInUCS = { 0, 0, 0 };
            PlanningToUnitCoordinate(isocenter,
            gantryAngle, collimatorAngle, couchAngle,
            pointCoordinate, pointCoordinateInUCS);

            double radialDistance = Math.Sqrt(
                Math.Pow(pointCoordinateInUCS[0], 2)
                + Math.Pow(pointCoordinateInUCS[2], 2));
            double verticalDistance = Math.Abs(pointCoordinateInUCS[1] + SAD);

            double offAxisDistance = radialDistance * SAD / verticalDistance;

            return offAxisDistance;
        }

        public static double InverseSquareFactorFromIsocenter(double[] pointCoordinate, double[] isocenter,
            double gantryAngle, double collimatorAngle, double couchAngle, double SAD = 1000.0)
        {
            double[] pointCoordinateInUCS = { 0, 0, 0 };
            PlanningToUnitCoordinate(isocenter,
            gantryAngle, collimatorAngle, couchAngle,
            pointCoordinate, pointCoordinateInUCS);

            double verticalDistance = Math.Abs(pointCoordinateInUCS[1] + SAD); ;
            double inverseSquareFactor = Math.Pow((verticalDistance / SAD), 2);

            return inverseSquareFactor;
        }

        public static void PlanningToUnitCoordinate(double[] isocenter,
            double gantryAngle, double collimatorAngle, double couchAngle,
            double[] originalVector, double[] transformedVector)
        {
            var tmpVector1 = new double[3];
            Translation3D(isocenter, originalVector, tmpVector1);

            var tmpVector2 = new double[3];
            CouchRotation(couchAngle, tmpVector1, tmpVector2);

            var tmpVector3 = new double[3];
            GantryRotation(gantryAngle, tmpVector2, tmpVector3);
            
            CollimatorRotation(collimatorAngle, tmpVector3, transformedVector);
        }

        public static void UnitToPlanningCoordinate(double[] isocenter,
            double gantryAngle, double collimatorAngle, double couchAngle,
            double[] originalVector, double[] transformedVector)
        {
            var tmpVector1 = new double[3];
            CollimatorRotation(-collimatorAngle, originalVector, tmpVector1);
            var tmpVector2 = new double[3];
            GantryRotation(-gantryAngle, tmpVector1, tmpVector2);
            var tmpVector3 = new double[3];
            CouchRotation(-couchAngle, tmpVector2, tmpVector3);

            var translationVector = new double[3];
            for (int i =0; i<3; i++)
            {
                translationVector[i] = -isocenter[i];
            }
            Translation3D(translationVector, tmpVector3, transformedVector);
        }

        public static void Translation3D(double[] translationVector, double[] originalVector, double[] translatedVector)
        {
            for (int i = 0; i < 3; i++)
            {
                translatedVector[i] = originalVector[i] - translationVector[i];
            }
        }

        public static void GantryRotation(double angleInDegree, double[] originalVector, double[] transformedVector)
        {
            double angleInRadian = Radians(angleInDegree);
            RotationZ(angleInRadian, originalVector, transformedVector);
        }
        public static void CollimatorRotation(double angleInDegree, double[] originalVector, double[] transformedVector)
        {
            double angleInRadian = Radians(angleInDegree);
            RotationY(-angleInRadian, originalVector, transformedVector);
        }
        public static void CouchRotation(double angleInDegree, double[] originalVector, double[] transformedVector)
        {
            double angleInRadian = Radians(angleInDegree);
            RotationY(angleInRadian, originalVector, transformedVector);
        }

        public static void RotationX(double angle, double[] originalVector, double[] rotatedVector)
        {
            double[,] rotationMatrix = RotationMatrixX3D(angle);
            MatrixTransform3D(rotationMatrix, originalVector, rotatedVector);            
        }

        public static void RotationY(double angle, double[] originalVector, double[] rotatedVector)
        {
            double[,] rotationMatrix = RotationMatrixY3D(angle);
            MatrixTransform3D(rotationMatrix, originalVector, rotatedVector);
        }

        public static void RotationZ(double angle, double[] originalVector, double[] rotatedVector)
        {
            double[,] rotationMatrix = RotationMatrixZ3D(angle);
            MatrixTransform3D(rotationMatrix, originalVector, rotatedVector);
        }

        public static void MatrixTransform3D(double[,] transformMatrix, double[] originalVector, double[] transformedVector)
        {
            for (int i = 0; i < 3; i++)
            {
                transformedVector[i] = 0.0;
                for (int j = 0; j < 3; j++)
                {
                    transformedVector[i] += transformMatrix[i, j] * originalVector[j];
                }
            }
        }

        public static double[,] RotationMatrixX3D(double angle)
        {
            double[,] matrix = {
                { 1, 0, 0 },
                { 0, Math.Cos(angle), Math.Sin(angle)},
                { 0, -Math.Sin(angle), Math.Cos(angle)}
            };
            return matrix;
        }

        public static double[,] RotationMatrixY3D(double angle)
        {
            double[,] matrix = {
                { Math.Cos(angle), 0, -Math.Sin(angle)},
                { 0, 1, 0 },
                { Math.Sin(angle), 0, Math.Cos(angle)}
            };
            return matrix;
        }

        public static double[,] RotationMatrixZ3D(double angle)
        {
            double[,] matrix = {
                { Math.Cos(angle), Math.Sin(angle), 0},
                { -Math.Sin(angle), Math.Cos(angle), 0 },
                { 0, 0, 1 }
            };
            return matrix;
        }

        public static double Radians(double degrees)
        {
            var value = degrees*Math.PI / 180;
            return value;
        }
    }
}
