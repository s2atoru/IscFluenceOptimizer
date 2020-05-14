using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;

namespace Juntendo.MedPhys.Esapi.MlcAndJaw
{
    public class MlcAndJawPositions
    {
        /// <summary>
        /// enum for leaf banks
        /// </summary>
        public enum LeafBank { B = 0, A = 1}

        static int NumberOfLeaves { get; } = 60;
        static int NumberOfOuterLeaves { get; } = 20;
        static int NumberOfInnerLeaves { get; } = 40;

        // Lengths are in mm
        static double LeafWidthOfOuterLeaf { get; } = 10.0;
        static double LeafWidthOfInnerLeaf { get; } = 5.0;

        // Minimum leaf transverse position
        // Transverse means the transverse to the MLC moving direction. 
        static double MinimumLeafTransversePosition { get; } = -200.0;
        public double[] LeafTransversePositions;
        public double[,] LeafEndPositions;
        public double[] LeafWidths;

        public double X1Position { get; set; }
        public double X2Position { get; set; }
        public double Y1Position { get; set; }
        public double Y2Position { get; set; }

        public bool HasMLC { get; private set; } = true;

        public MlcAndJawPositions(ControlPoint controlPoint)
        {
            LeafTransversePositions = new double[NumberOfLeaves];
            LeafWidths = new double[NumberOfLeaves];
            LeafEndPositions = new double[2, NumberOfLeaves];

            int NumberOfLowerOuterLeaves = NumberOfOuterLeaves / 2;
            for (int i = 0; i < NumberOfLowerOuterLeaves; i++)
            {
                LeafWidths[i] = LeafWidthOfOuterLeaf;
                LeafWidths[NumberOfLeaves - 1 - i] = LeafWidthOfOuterLeaf;
            }
            for (int i = NumberOfLowerOuterLeaves; i < NumberOfLowerOuterLeaves + NumberOfInnerLeaves; i++)
            {
                LeafWidths[i] = LeafWidthOfInnerLeaf;
            }
            LeafTransversePositions[0] = MinimumLeafTransversePosition + LeafWidths[0] / 2.0;
            for (int i = 1; i < NumberOfLeaves; i++)
            {
                LeafTransversePositions[i] = LeafTransversePositions[i - 1] + (LeafWidths[i - 1] + LeafWidths[i]) / 2.0;
            }

            // Check if the control point has MLC.
            if (controlPoint.LeafPositions.Length == 0)
            {
                HasMLC = false;
            }
            else
            {
                HasMLC = true;
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < NumberOfLeaves; j++)
                    {
                        LeafEndPositions[i, j] = controlPoint.LeafPositions[i, j];
                    }
                }
            }

            X1Position = controlPoint.JawPositions.X1;
            X2Position = controlPoint.JawPositions.X2;
            Y1Position = controlPoint.JawPositions.Y1;
            Y2Position = controlPoint.JawPositions.Y2;
        }

        /// <summary>
        /// Check if (x, y) is in the field stored in this class
        /// </summary>
        /// <remarks>
        /// The edge of the field is defined as the outside of the field.
        /// </remarks>
        /// <param name="x"> x coordinate of a point to be checked </param>
        /// <param name="y"> y coordinate of a point to be checked </param>
        /// <returns> true if (x,y) in the field </returns>
        public bool IsInField(double x, double y)
        {
            // Under the jaws
            if ((x <= X1Position) || (x >= X2Position) || (y <= Y1Position) || (y >= Y2Position))
            {
                return false;
            }

            if (HasMLC == false)
            {
                return true;
            }

            double bankALeafPosition = 0;
            double bankBLeafPosition = 0;

            // Point is below the center of the bottom leaves
            if (y < LeafTransversePositions[0])
            {
                bankBLeafPosition = LeafEndPositions[(int)LeafBank.B, 0];
                bankALeafPosition = LeafEndPositions[(int)LeafBank.A, 0];
            }
            // Point is above the center of the top leaves
            else if (y >= LeafTransversePositions[NumberOfLeaves - 1])
            {
                bankBLeafPosition = LeafEndPositions[(int)LeafBank.B, NumberOfLeaves - 1];
                bankALeafPosition = LeafEndPositions[(int)LeafBank.A, NumberOfLeaves - 1];
            }
            else
            {
                int iLower = 0;
                int iUpper = 0;

                for (int i = 1; i < NumberOfLeaves; i++)
                {
                    if (y < LeafTransversePositions[i])
                    {
                        iLower = i - 1;
                        iUpper = i;
                        break;
                    }
                }

                var leafTransversePositionLower = LeafTransversePositions[iLower];
                var leafTransversePositionUpper = LeafTransversePositions[iUpper];

                var leafEndPositionLowerB = LeafEndPositions[(int)LeafBank.B, iLower];
                var leafEndPositionUpperB = LeafEndPositions[(int)LeafBank.B, iUpper];

                var leafEndPositionLowerA = LeafEndPositions[(int)LeafBank.A, iLower];
                var leafEndPositionUpperA = LeafEndPositions[(int)LeafBank.A, iUpper];

                // If the Y-jaw positions are between the transversal centers of the corresponding MLCs
                // Y1 Jaw
                if (LeafTransversePositions[iLower] <= Y1Position
                    && LeafTransversePositions[iLower]+LeafWidths[iLower]/2> Y1Position)
                {
                    leafTransversePositionLower = Y1Position;
                }
                else if (LeafTransversePositions[iUpper] > Y1Position
                    && LeafTransversePositions[iLower] + LeafWidths[iLower] / 2 <= Y1Position)
                {
                    leafEndPositionLowerA = leafEndPositionUpperA;
                    leafEndPositionLowerB = leafEndPositionUpperB;
                    leafTransversePositionLower = Y1Position;
                }

                // Y2 Jaw
                if (LeafTransversePositions[iLower] <= Y2Position
                    && LeafTransversePositions[iLower] + LeafWidths[iLower] / 2 >= Y2Position)
                {
                    leafTransversePositionUpper = Y2Position;
                    leafEndPositionUpperA = leafEndPositionLowerA;
                    leafEndPositionUpperB = leafEndPositionLowerB;
                }
                else if (LeafTransversePositions[iUpper] > Y2Position
                    && LeafTransversePositions[iLower] + LeafWidths[iLower] / 2 < Y2Position)
                {
                    leafTransversePositionUpper = Y2Position;
                }

                // Linear interpolation
                bankBLeafPosition = Helpers.LinearInterpolation1D(y,
                    leafTransversePositionLower, leafTransversePositionUpper,
                    leafEndPositionLowerB, leafEndPositionUpperB);
                bankALeafPosition = Helpers.LinearInterpolation1D(y,
                    leafTransversePositionLower, leafTransversePositionUpper,
                    leafEndPositionLowerA, leafEndPositionUpperA);
            }

            if (bankBLeafPosition < x && x < bankALeafPosition)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Check if (x, y) is in the field stored in this class
        /// </summary>
        /// <remarks>
        /// No interpolation is performed
        /// </remarks>
        /// <remarks>
        /// The edge of the field is defined as the outside of the field.
        /// </remarks>
        /// <param name="x"> x coordinate of a point to be checked </param>
        /// <param name="y"> y coordinate of a point to be checked </param>
        /// <returns> true if (x,y) in the field </returns>
        public bool IsInField0(double x, double y)
        {
            // Under the jaws
            if ((x <= X1Position) || (x >= X2Position) || (y <= Y1Position) || (y >= Y2Position))
            {
                return false;
            }

            if (HasMLC == false)
            {
                return true;
            }

            double bankALeafPosition = 0;
            double bankBLeafPosition = 0; 

            for (int i = 0; i < NumberOfLeaves; i++)
            {
                if (y < LeafTransversePositions[i] + LeafWidths[i] / 2)
                {
                    bankALeafPosition = LeafEndPositions[(int)LeafBank.A, i];
                    bankBLeafPosition = LeafEndPositions[(int)LeafBank.B, i];
                    break;
                }
            }
            
            if (bankBLeafPosition < x && x < bankALeafPosition)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Check if (x, y) is in the field stored in this class
        /// </summary>
        /// <remarks>
        /// The edge of the field is defined as the outside of the field.
        /// </remarks>
        /// <param name="x"> x coordinate of a point to be checked in mm </param>
        /// <param name="y"> y coordinate of a point to be checked in mm </param>
        /// <param name="margin"> extra margin for the field edges in mm </param>
        /// <returns> true if (x,y) in the field </returns>
        public bool IsInFieldWithMargin(double x, double y, double margin)
        {
            // Under the jaws
            if ((x <= X1Position - margin) || (x >= X2Position + margin)
                || (y <= Y1Position - margin ) || (y >= Y2Position + margin))
            {
                return false;
            }

            if (HasMLC == false)
            {
                return true;
            }

            double bankALeafPosition = 0;
            double bankBLeafPosition = 0;

            // Point is below the center of the bottom leaves
            if (y < LeafTransversePositions[0])
            {
                bankBLeafPosition = LeafEndPositions[(int)LeafBank.B, 0];
                bankALeafPosition = LeafEndPositions[(int)LeafBank.A, 0];
            }
            // Point is above the center of the top leaves
            else if (y >= LeafTransversePositions[NumberOfLeaves - 1])
            {
                bankBLeafPosition = LeafEndPositions[(int)LeafBank.B, NumberOfLeaves - 1];
                bankALeafPosition = LeafEndPositions[(int)LeafBank.A, NumberOfLeaves - 1];
            }
            else
            {
                int iLower = 0;
                int iUpper = 0;

                for (int i = 1; i < NumberOfLeaves; i++)
                {
                    if (y < LeafTransversePositions[i])
                    {
                        iLower = i - 1;
                        iUpper = i;
                        break;
                    }
                }

                var leafTransversePositionLower = LeafTransversePositions[iLower];
                var leafTransversePositionUpper = LeafTransversePositions[iUpper];

                var leafEndPositionLowerB = LeafEndPositions[(int)LeafBank.B, iLower];
                var leafEndPositionUpperB = LeafEndPositions[(int)LeafBank.B, iUpper];

                var leafEndPositionLowerA = LeafEndPositions[(int)LeafBank.A, iLower];
                var leafEndPositionUpperA = LeafEndPositions[(int)LeafBank.A, iUpper];

                // If the Y-jaw positions are between the transversal centers of the corresponding MLCs
                // Y1 Jaw
                if (LeafTransversePositions[iLower] <= Y1Position
                    && LeafTransversePositions[iLower] + LeafWidths[iLower] / 2 > Y1Position)
                {
                    leafTransversePositionLower = Y1Position;
                }
                else if (LeafTransversePositions[iUpper] > Y1Position
                    && LeafTransversePositions[iLower] + LeafWidths[iLower] / 2 <= Y1Position)
                {
                    leafEndPositionLowerA = leafEndPositionUpperA;
                    leafEndPositionLowerB = leafEndPositionUpperB;
                    leafTransversePositionLower = Y1Position;
                }

                // Y2 Jaw
                if (LeafTransversePositions[iLower] <= Y2Position
                    && LeafTransversePositions[iLower] + LeafWidths[iLower] / 2 >= Y2Position)
                {
                    leafTransversePositionUpper = Y2Position;
                    leafEndPositionUpperA = leafEndPositionLowerA;
                    leafEndPositionUpperB = leafEndPositionLowerB;
                }
                else if (LeafTransversePositions[iUpper] > Y2Position
                    && LeafTransversePositions[iLower] + LeafWidths[iLower] / 2 < Y2Position)
                {
                    leafTransversePositionUpper = Y2Position;
                }

                // Linear interpolation
                bankBLeafPosition = Helpers.LinearInterpolation1D(y,
                    leafTransversePositionLower, leafTransversePositionUpper,
                    leafEndPositionLowerB, leafEndPositionUpperB);
                bankALeafPosition = Helpers.LinearInterpolation1D(y,
                    leafTransversePositionLower, leafTransversePositionUpper,
                    leafEndPositionLowerA, leafEndPositionUpperA);
            }

            if (bankBLeafPosition - margin < x && x < bankALeafPosition + margin)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// MLC position index for the coordinate y
        /// </summary>
        /// <remarks>
        /// y is in the direction transverse to the MLC movement
        /// </remarks>
        /// <param name="y">
        /// Coordinate in the direction transverse to the MLC movement
        /// </param>
        /// <returns>
        /// MLC position index corresponding to the coordinate y
        /// </returns>
        public int MlcPositionIndex(double y)
        {

            int positionIndex = -1;

            if (y < LeafTransversePositions[0] - LeafWidths[0]/2
                || y > LeafTransversePositions[NumberOfLeaves-1] + LeafWidths[NumberOfLeaves-1] / 2 )
            {
                throw new ArgumentException($"y is out of range: y = {y:g}", "y");
            }

            for (int i = 0; i < NumberOfLeaves; i++)
            {
                double yEdge = LeafTransversePositions[i] + LeafWidths[i] / 2;
                if (y <= yEdge)
                {
                    positionIndex = i;
                    break;
                }
            }

            return positionIndex;
        }

    }

}
