////////////////////////////////////////////////////////////////////////////////
// Helpers.cs
//
// Helper methods to manipulate courses etc.
//  
// Applies to: ESAPI v13, v13.5, v13.6.
//
// Copyright (c) 2015 Varian Medical Systems, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in 
//  all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VMS.TPS.Common.Model.API;

namespace Juntendo.MedPhys.Esapi.IscFluence
{
    public static class Helpers
    {

        public static bool CheckStructures(Patient patient)
        {
            if (patient.StructureSets.Any()) return true;
            const string message = "Patient does not have any structures.";
            const string title = "Invalid patient";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        public static Course GetCourse(Patient patient, string courseId)
        {
            var res = patient.Courses.Where(c => c.Id == courseId);
            if (res.Any())
            {
                return res.Single();
            }
            else
            {
                throw new ArgumentException("No corresponding Course", courseId);
            }
        }

        public static PlanSetup GetPlanSetup(Course course, string planId)
        {
            var res = course.PlanSetups.Where(p => p.Id == planId);
            if (res.Any())
            {
                return res.Single();
            }
            {
                throw new ArgumentException("No corresponding PlanSetup", planId);
            }
        }

        /// <summary>
        /// Grid index for pixels or voxels 
        /// </summary>
        /// <param name="x"> Coordinate of a point </param>
        /// <param name="origin"> Origin of the pixels or voxels </param>
        /// <param name="res"> Resolution of the pixels or voxels </param>
        /// <returns></returns>
        public static int GridIndex(double x, double origin, double res)
        {
            double start = origin - res / 2;
            double distance = x - start;

            return ((int)(Math.Abs(distance / res)));

        }

    }
}
