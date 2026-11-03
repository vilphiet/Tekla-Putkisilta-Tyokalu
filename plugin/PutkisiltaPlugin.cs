// PutkisiltaPlugin.cs
// Täysi Tekla Open API Plug-in putkisillan rungon mallintamiseen
//
// Käännä Visual Studiossa Class Library (.NET Framework).
// Viittaukset: Tekla.Structures, Tekla.Structures.Model, Tekla.Structures.Plugins,
//              Tekla.Structures.Geometry3d, Tekla.Structures.Dialog
// Kopioi .dll → environments\common\extensions\Putkisilta\

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Tekla.Structures.Plugins;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Dialog;

namespace PutkisiltaPlugin
{
    [Plugin("Putkisilta")]
    [PluginUserInterface(typeof(PutkisiltaForm))]
    public class PutkisiltaPlugin : PluginBase
    {
        private readonly Model _model;
        private readonly StructuresData _data;

        public PutkisiltaPlugin(StructuresData data)
        {
            _model = new Model();
            _data = data;
        }

        public override List<InputDefinition> DefineInput()
        {
            var inputs = new List<InputDefinition>();
            var picker = new Picker();
            Point p1 = picker.PickPoint("Valitse sillan alkupiste");
            Point p2 = picker.PickPoint("Valitse sillan loppupiste");
            if (p1 != null) inputs.Add(new InputDefinition(p1));
            if (p2 != null) inputs.Add(new InputDefinition(p2));
            return inputs;
        }

        public override bool Run(List<InputDefinition> input)
        {
            if (input == null || input.Count < 2 || !_model.GetConnectionStatus())
                return false;

            Point p1 = (Point)input[0].GetInput();
            Point p2 = (Point)input[1].GetInput();

            double width = _data.Width > 0 ? _data.Width : 1200;
            double height = _data.Height > 0 ? _data.Height : 1800;
            double maxSpacing = _data.MaxSpacing > 0 ? _data.MaxSpacing : 3000;
            string mat = string.IsNullOrEmpty(_data.Material) ? "S355J2" : _data.Material;

            string chordTop = string.IsNullOrEmpty(_data.ProfileChordTop) ? "SHS150*150*6" : _data.ProfileChordTop;
            string chordBot = string.IsNullOrEmpty(_data.ProfileChordBottom) ? "SHS150*150*6" : _data.ProfileChordBottom;
            string frameVert = string.IsNullOrEmpty(_data.ProfileFrameVertical) ? "SHS100*100*5" : _data.ProfileFrameVertical;
            string frameHoriz = string.IsNullOrEmpty(_data.ProfileFrameHorizontal) ? "SHS100*100*5" : _data.ProfileFrameHorizontal;
            string diagWall = string.IsNullOrEmpty(_data.ProfileDiagonalWall) ? "SHS80*80*4" : _data.ProfileDiagonalWall;
            string diagRF = string.IsNullOrEmpty(_data.ProfileDiagonalRoofFloor) ? "SHS80*80*4" : _data.ProfileDiagonalRoofFloor;

            double zShift = 0;
            if (_data.HeightPosition == 1) zShift = -height / 2.0;
            else if (_data.HeightPosition == 2) zShift = height / 2.0;

            Vector direction = new Vector(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            double length = direction.GetLength();
            if (length < 100) return false;

            direction.Normalize();
            Vector up = new Vector(0, 0, 1);
            Vector right = direction.Cross(up);
            if (right.GetLength() < 0.1) right = new Vector(1, 0, 0);
            right.Normalize();

            p1 += up * zShift;
            p2 += up * zShift;

            int bayCount = Math.Max(1, (int)Math.Ceiling(length / maxSpacing));
            double actualSpacing = length / bayCount;

            var framePositions = new List<Point>();
            for (int i = 0; i <= bayCount; i++)
                framePositions.Add(p1 + direction * (i * actualSpacing));

            CreateChords(framePositions, right, up, width, height, chordTop, chordBot, mat);
            for (int i = 0; i < framePositions.Count; i++)
            {
                bool isEnd = i == 0 || i == framePositions.Count - 1;
                string vProf = isEnd && !string.IsNullOrEmpty(_data.ProfileEndVertical) ? _data.ProfileEndVertical : frameVert;
                string hProf = isEnd && !string.IsNullOrEmpty(_data.ProfileEndHorizontal) ? _data.ProfileEndHorizontal : frameHoriz;
                CreateFrame(framePositions[i], right, up, width, height, vProf, hProf, mat);
            }

            CreateWallDiagonals(framePositions, right, up, width, height, diagWall, mat);
            CreateRoofFloorDiagonals(framePositions, right, up, width, height, diagRF, mat);

            _model.CommitChanges();
            return true;
        }

        void CreateChords(List<Point> positions, Vector right, Vector up, double width, double height, string topProf, string botProf, string mat)
        {
            double halfW = width / 2.0;
            double halfH = height / 2.0;
            var offsets = new[] {
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
            var beam = new Beam(start, end)
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
        [StructuresField("Width")] public double Width;
        [StructuresField("Height")] public double Height;
        [StructuresField("MaxSpacing")] public double MaxSpacing;
        [StructuresField("Material")] public string Material;
        [StructuresField("HeightPosition")] public int HeightPosition;

        [StructuresField("ProfileChordTop")] public string ProfileChordTop;
        [StructuresField("ProfileChordBottom")] public string ProfileChordBottom;
        [StructuresField("ProfileFrameVertical")] public string ProfileFrameVertical;
        [StructuresField("ProfileFrameHorizontal")] public string ProfileFrameHorizontal;
        [StructuresField("ProfileEndVertical")] public string ProfileEndVertical;
        [StructuresField("ProfileEndHorizontal")] public string ProfileEndHorizontal;
        [StructuresField("ProfileDiagonalWall")] public string ProfileDiagonalWall;
        [StructuresField("ProfileDiagonalRoofFloor")] public string ProfileDiagonalRoofFloor;
    }
}
