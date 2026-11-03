// PutkisiltaPlugin.cs
// Tekla Open API Plug-in - Putkisillan rungon mallinnustyökalu
//
// Käännä Visual Studiossa Class Library (.NET Framework) -projektina.
// Viittaa: Tekla.Structures.dll, Tekla.Structures.Model.dll, Tekla.Structures.Plugins.dll, Tekla.Structures.Geometry3d.dll
// Kopioi .dll kansioon: ...\environments\common\extensions\Putkisilta\
//
// Testiversio – parametrit kovakoodattuina. Dialogi lisätään myöhemmin.

using System;
using System.Collections.Generic;
using Tekla.Structures.Plugins;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model.UI;

namespace PutkisiltaPlugin
{
    [Plugin("Putkisilta")]
    [PluginUserInterface("PutkisiltaPlugin.PutkisiltaForm")]
    public class PutkisiltaPlugin : PluginBase
    {
        private readonly Model _model;

        public PutkisiltaPlugin(StructuresData data)
        {
            _model = new Model();
        }

        public override List<InputDefinition> DefineInput()
        {
            List<InputDefinition> inputs = new List<InputDefinition>();
            Picker picker = new Picker();
            Point p1 = picker.PickPoint("Valitse sillan alkupiste");
            Point p2 = picker.PickPoint("Valitse sillan loppupiste");
            if (p1 != null) inputs.Add(new InputDefinition(p1));
            if (p2 != null) inputs.Add(new InputDefinition(p2));
            return inputs;
        }

        public override bool Run(List<InputDefinition> input)
        {
            if (input == null || input.Count < 2) return false;
            if (!_model.GetConnectionStatus()) return false;

            Point p1 = (Point)input[0].GetInput();
            Point p2 = (Point)input[1].GetInput();

            // --- PARAMETRIT (sama kuin makrossa) ---
            double width = 1200.0;
            double height = 1800.0;
            double maxSpacing = 3000.0;
            string profileChordTop = "SHS150*150*6";
            string profileChordBottom = "SHS150*150*6";
            string profileFrameVertical = "SHS100*100*5";
            string profileFrameHorizontal = "SHS100*100*5";
            string profileDiagonalWall = "SHS80*80*4";
            string profileDiagonalRoofFloor = "SHS80*80*4";
            string material = "S355J2";
            // ---------------------------------------

            Vector direction = new Vector(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            double length = direction.GetLength();
            if (length < 100) return false;

            direction.Normalize();
            Vector up = new Vector(0, 0, 1);
            Vector right = direction.Cross(up);
            if (right.GetLength() < 0.1) right = new Vector(1, 0, 0);
            right.Normalize();

            int bayCount = Math.Max(1, (int)Math.Ceiling(length / maxSpacing));
            double actualSpacing = length / bayCount;

            List<Point> framePositions = new List<Point>();
            for (int i = 0; i <= bayCount; i++)
                framePositions.Add(p1 + direction * (i * actualSpacing));

            CreateChords(framePositions, right, up, width, height, profileChordTop, profileChordBottom, material);
            for (int i = 0; i < framePositions.Count; i++)
                CreateFrame(framePositions[i], right, up, width, height, profileFrameVertical, profileFrameHorizontal, material);

            CreateWallDiagonals(framePositions, right, up, width, height, profileDiagonalWall, material);
            CreateRoofFloorDiagonals(framePositions, right, up, width, height, profileDiagonalRoofFloor, material);

            _model.CommitChanges();
            return true;
        }

        void CreateChords(List<Point> positions, Vector right, Vector up, double width, double height, string topProf, string botProf, string mat)
        {
            double halfW = width / 2.0;
            double halfH = height / 2.0;
            Point[] offsets = {
                new Point( halfW, 0,  halfH),
                new Point(-halfW, 0,  halfH),
                new Point( halfW, 0, -halfH),
                new Point(-halfW, 0, -halfH)
            };
            string[] profiles = { topProf, topProf, botProf, botProf };

            for (int c = 0; c < 4; c++)
                for (int i = 0; i < positions.Count - 1; i++)
                {
                    Point start = positions[i] + right * offsets[c].X + up * offsets[c].Z;
                    Point end = positions[i + 1] + right * offsets[c].X + up * offsets[c].Z;
                    CreateBeam(start, end, profiles[c], mat, "1");
                }
        }

        void CreateFrame(Point center, Vector right, Vector up, double width, double height, string vertProf, string horizProf, string mat)
        {
            double halfW = width / 2.0;
            double halfH = height / 2.0;
            Point tl = center + right * halfW + up * halfH;
            Point tr = center + right * (-halfW) + up * halfH;
            Point bl = center + right * halfW + up * (-halfH);
            Point br = center + right * (-halfW) + up * (-halfH);

            CreateBeam(bl, tl, vertProf, mat, "2");
            CreateBeam(br, tr, vertProf, mat, "2");
            CreateBeam(tl, tr, horizProf, mat, "3");
            CreateBeam(bl, br, horizProf, mat, "3");
        }

        void CreateWallDiagonals(List<Point> positions, Vector right, Vector up, double width, double height, string prof, string mat)
        {
            double halfW = width / 2.0;
            double halfH = height / 2.0;
            int mid = positions.Count / 2;

            for (int side = 0; side < 2; side++)
            {
                double xOff = (side == 0) ? halfW : -halfW;
                for (int i = 0; i < positions.Count - 1; i++)
                {
                    Point a = positions[i] + right * xOff + up * (-halfH);
                    Point b = positions[i + 1] + right * xOff + up * halfH;
                    Point c = positions[i] + right * xOff + up * halfH;
                    Point d = positions[i + 1] + right * xOff + up * (-halfH);

                    if (i < mid)
                        CreateBeam(a, b, prof, mat, "4");
                    else
                        CreateBeam(c, d, prof, mat, "4");
                }
            }
        }

        void CreateRoofFloorDiagonals(List<Point> positions, Vector right, Vector up, double width, double height, string prof, string mat)
        {
            double halfW = width / 2.0;
            double halfH = height / 2.0;
            double[] zOffs = { halfH, -halfH };

            foreach (double zOff in zOffs)
            {
                for (int i = 0; i < positions.Count - 1; i++)
                {
                    Point leftStart = positions[i] + right * (-halfW) + up * zOff;
                    Point rightStart = positions[i] + right * halfW + up * zOff;
                    Point leftEnd = positions[i + 1] + right * (-halfW) + up * zOff;
                    Point rightEnd = positions[i + 1] + right * halfW + up * zOff;
                    Point midPoint = (positions[i] + positions[i + 1]) * 0.5 + up * zOff;

                    CreateBeam(leftStart, midPoint, prof, mat, "5");
                    CreateBeam(rightStart, midPoint, prof, mat, "5");
                    CreateBeam(midPoint, leftEnd, prof, mat, "5");
                    CreateBeam(midPoint, rightEnd, prof, mat, "5");
                }
            }
        }

        void CreateBeam(Point start, Point end, string profile, string material, string classStr)
        {
            Beam beam = new Beam(start, end)
            {
                Profile = { ProfileString = profile },
                Material = { MaterialString = material },
                Class = classStr
            };
            beam.Position.Depth = Position.DepthEnum.MIDDLE;
            beam.Position.Plane = Position.PlaneEnum.MIDDLE;
            beam.Insert();
        }
    }

    public class StructuresData
    {
        // Lisätään myöhemmin dialogin kentät
    }
}
