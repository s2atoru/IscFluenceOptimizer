using System;
using System.IO;
using System.Linq;
using System.Text;

using VMS.TPS.Common.Model.API;

using Juntendo.MedPhys.Esapi.MlcAndJaw;

namespace Juntendo.MedPhys.Esapi.IscFluence
{
    public class IscFluence
    {
        // Lengths are in mm

        public int SizeX { get; set; }
        public int SizeY { get; set; }
        public double SpacingX { get; set; }
        public double SpacingY { get; set; }
        public double OriginX { get; set; }
        public double OriginY { get; set; }
        public double[,] Values;

        // Mininum and Maximum indices for finite fluence points
        public int MinEdgeIndexX { get; set; }
        public int MaxEdgeIndexX { get; set; }
        public int MinEdgeIndexY { get; set; }
        public int MaxEdgeIndexY { get; set; }

        public double[] XCoordinates;
        public double[] YCoordinates;

        public string BeamId { get; set; }
        public string InputFileName { get; set; }
        public string InputDirectoryName { get; set; }

        public IscFluence(string InputFilePath)
        {
            using (var reader = new StreamReader(InputFilePath))
            {

                InputFileName = Path.GetFileName(InputFilePath);
                InputDirectoryName = Path.GetDirectoryName(InputFilePath);

                // Read Beam Id
                BeamId = (reader.ReadLine().Split())[1];

                // Discard the next line
                reader.ReadLine();

                // Read sizex
                SizeX = int.Parse((reader.ReadLine().Split())[1]);

                // Read sizey
                SizeY = int.Parse((reader.ReadLine().Split())[1]);

                // Read spacingx
                SpacingX = double.Parse((reader.ReadLine().Split())[1]);

                // Read spacingy
                SpacingY = double.Parse((reader.ReadLine().Split())[1]);

                // Read originx
                OriginX = double.Parse((reader.ReadLine().Split())[1]);

                // Read originy
                OriginY = double.Parse((reader.ReadLine().Split())[1]);

                // Discard the next line
                reader.ReadLine();

                // Allocate and initialize XCoordinates
                XCoordinates = new double[SizeX];
                for (int i = 0; i < SizeX; i++)
                {
                    XCoordinates[i] = OriginX + SpacingX * (double)i;
                }

                // Allocate and initialize YCoordinates
                // Note: Negative sign in front of the second term.
                // OriginY is the coordinate at the upper-left corner
                // and the direction of Y is from the bottom to the top.
                // The index i is increasing when going from the top to the bottom.
                YCoordinates = new double[SizeY];
                for (int i = 0; i < SizeY; i++)
                {
                    YCoordinates[i] = OriginY - SpacingY * (double)i;
                }

                // Allocate and initialize Values
                Values = new double[SizeY, SizeX];

                int minEdgeIndexX = SizeX - 1;
                int maxEdgeIndexX = 0;
                int minEdgeIndexY = SizeY - 1;
                int maxEdgeIndexY = 0;

                for (int i = 0; i < SizeY; i++)
                {
                    var vs = reader.ReadLine().Split();
                    for (int j = 0; j < SizeX; j++)
                    {
                        var value = double.Parse(vs[j]);

                        if (value != 0.0 )
                        {
                            if (j < minEdgeIndexX)
                            {
                                minEdgeIndexX = j;
                            }
                            if (j > maxEdgeIndexX)
                            {
                                maxEdgeIndexX = j;
                            }
                            if (i < minEdgeIndexY)
                            {
                                minEdgeIndexY = i;
                            }
                            if (i > maxEdgeIndexY)
                            {
                                maxEdgeIndexY = i;
                            }
                        }

                        Values[i, j] = value;
                    }
                }

                MinEdgeIndexX = minEdgeIndexX;
                MaxEdgeIndexX = maxEdgeIndexX;
                MinEdgeIndexY = minEdgeIndexY;
                MaxEdgeIndexY = maxEdgeIndexY;

            }
        }

        /// <summary>
        /// Constructor from Beam
        /// </summary>
        /// <param name="beam"> Beam (ESAPI) </param>
        public IscFluence(Beam beam)
        {

            // Read Beam Id
            BeamId = beam.Id;

            var inputFluence = beam.GetOptimalFluence();

            if (inputFluence == null)
            {
                throw new InvalidOperationException($"No Fluence for {BeamId:s}");
            }

            // Read sizex
            SizeX = inputFluence.XSizePixel;

            // Read sizey
            SizeY = inputFluence.YSizePixel;

            // Read spacingx
            SpacingX = inputFluence.XSizeMM / SizeX;

            // Read spacingy
            SpacingY = inputFluence.YSizeMM / SizeY;

            // Read originx
            OriginX = inputFluence.XOrigin;

            // Read originy
            OriginY = inputFluence.YOrigin;

            // Allocate and initialize XCoordinates
            XCoordinates = new double[SizeX];
            for (int i = 0; i < SizeX; i++)
            {
                XCoordinates[i] = OriginX + SpacingX * (double)i;
            }

            // Allocate and initialize YCoordinates
            // Note: Negative sign in front of the second term.
            // OriginY is the coordinate at the upper-left corner
            // and the direction of Y is from the bottom to the top.
            // The index i is increasing when going from the top to the bottom.
            YCoordinates = new double[SizeY];
            for (int i = 0; i < SizeY; i++)
            {
                YCoordinates[i] = OriginY - SpacingY * (double)i;
            }

            int minEdgeIndexX = SizeX - 1;
            int maxEdgeIndexX = 0;
            int minEdgeIndexY = SizeY - 1;
            int maxEdgeIndexY = 0;

            // Allocate and initialize Values
            var inputValues = inputFluence.GetPixels();
            Values = new double[SizeY, SizeX];
            for (int i = 0; i < SizeY; i++)
            {
                for (int j = 0; j < SizeX; j++)
                {
                    var value = inputValues[i, j];

                    if (value != 0.0)
                    {
                        if (j < minEdgeIndexX)
                        {
                            minEdgeIndexX = j;
                        }
                        if (j > maxEdgeIndexX)
                        {
                            maxEdgeIndexX = j;
                        }
                        if (i < minEdgeIndexY)
                        {
                            minEdgeIndexY = i;
                        }
                        if (i > maxEdgeIndexY)
                        {
                            maxEdgeIndexY = i;
                        }
                    }

                    Values[i, j] = value;
                }
            }

            MinEdgeIndexX = minEdgeIndexX;
            MaxEdgeIndexX = maxEdgeIndexX;
            MinEdgeIndexY = minEdgeIndexY;
            MaxEdgeIndexY = maxEdgeIndexY;

        }

        public void WriteToFile(string OutputFilePath)
        {
            // Output a text file with BOM
            using (var writer = new StreamWriter(OutputFilePath, false, new UTF8Encoding(true)))
            {
                writer.WriteLine("# " + BeamId + " - Fluence");
                writer.WriteLine("optimalfluence");
                writer.WriteLine(string.Format("sizex\t{0:D}", SizeX));
                writer.WriteLine(string.Format("sizey\t{0:D}", SizeY));
                writer.WriteLine(string.Format("spacingx\t{0:G}", SpacingX));
                writer.WriteLine(string.Format("spacingy\t{0:G}", SpacingY));
                writer.WriteLine(string.Format("originx\t{0:G}", OriginX));
                writer.WriteLine(string.Format("originy\t{0:G}", OriginY));
                writer.WriteLine("values");

                for (int i = 0; i < SizeY; i++)
                {
                    string line = "";
                    for (int j = 0; j < SizeX; j++)
                    {
                        line += string.Format("{0:G}\t", Values[i, j]);
                    }
                    writer.WriteLine(line);
                }
                writer.WriteLine("");
            }
        }

        /// <summary>
        /// Shape IscFluence using MLC and Jaw positions of a template beam
        /// </summary>
        /// <param name="templateBeam"> Template Beam </param>
        /// <param name="margin"> Margin for the beam edge </param>
        /// <param name="flushValue"> Fluence value for a flush region </param>
        public void ShapeUsingTemplateBeam(Beam templateBeam,
            double margin = 1.25, double flushValue = 0.5, double minimumFluence = 0)
        {
            // Read the MLC information of the first control point
            var mlcPosition = new MlcAndJawPositions(templateBeam.ControlPoints.First());

            if (mlcPosition.HasMLC == false)
            {
                throw new IndexOutOfRangeException($"No MLC for {templateBeam:s}");
            }

            var gantryAngle = templateBeam.ControlPoints.First().GantryAngle;

            // Take account for a flush region
            // Just for a breast ISC
            var minEdgeIndexX = MinEdgeIndexX;
            var maxEdgeIndexX = MaxEdgeIndexX;
            if (0.0 <= gantryAngle && gantryAngle < 180.0)
            {
                minEdgeIndexX = 0;
            }
            else
            {
                maxEdgeIndexX = SizeX;
            }

            for (int i = 0; i < SizeY; i++)
            {
                var y = YCoordinates[i];

                for (int j = 0; j < SizeX; j++)
                {
                    var x = XCoordinates[j];

                    if (minEdgeIndexX > j || maxEdgeIndexX < j || MinEdgeIndexY > i || MaxEdgeIndexY < i)
                    {
                        Values[i, j] = 0.0;
                        continue;
                    }

                    var isInField = mlcPosition.IsInFieldWithMargin(x, y, margin);

                    if ((isInField == false) && (Values[i, j] > 0))
                    {
                        Values[i, j] = 0;
                        continue;
                    }
                    else if (isInField == true)
                    {
                        if (Values[i, j] == 0)
                        {
                            Values[i, j] = flushValue;
                            continue;
                        }
                        else if (Values[i, j] < minimumFluence)
                        {
                            Values[i, j] = minimumFluence;
                            continue;
                        }
                    }
                }
            }
        }
    }
}
