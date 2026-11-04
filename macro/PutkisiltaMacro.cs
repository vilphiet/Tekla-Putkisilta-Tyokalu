// PutkisiltaMacro.cs
// Tekla Open API -makro putkisillan rungon luontiin
// Parametrit ovat alla. Muokkaa niitä tarvittaessa ja aja makro.
// Kopioi mallikansion macros\modeling\ -kansioon

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using TSG = Tekla.Structures.Geometry3d;

namespace Tekla.Technology.Akit.UserScript
{
    public class PutkisiltaMacro
    {
        public static void Run()
        {
            Model model = new Model();
            if (!model.GetConnectionStatus())
            {
                MessageBox.Show("Ei yhteyttä Tekla-malliin.");
                return;
            }

            // ========== PARAMETRIT ==========
            double width = 1200;
            double height = 1800;
            double maxSpacing = 3000;
            string material = "S355J2";
            string classStr = "1";
            string prefix = "P";
            int startNumber = 1;

            string chordTop = "SHS150*150*6";
            string chordBot = "SHS150*150*6";
            string frameVert = "SHS100*100*5";
            string frameHoriz = "SHS100*100*5";
            string endVert = "";
            string endHoriz = "";
            string diagWall = "SHS80*80*4";
            string diagRF = "SHS80*80*4";
            // =================================

            Picker picker = new Picker();
            TSG.Point p1 = picker.PickPoint("Valitse sillan alkupiste");
            TSG.Point p2 = picker.PickPoint("Valitse sillan loppupiste");
            if (p1 == null || p2 == null) return;

            TSG.Vector direction = new TSG.Vector(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            double length = direction.GetLength();
            if (length < 100) return;

            direction.Normalize();
            TSG.Vector up = new TSG.Vector(0, 0, 1);
            TSG.Vector right = direction.Cross(up);
            if (right.GetLength() < 0.1) right = new TSG.Vector(1, 0, 0);
            right.Normalize();

            int bayCount = Math.Max(1, (int)Math.Ceiling(length / maxSpacing));
            double actualSpacing = length / bayCount;

            List<TSG.Point> framePositions = new List<TSG.Point>();
            for (int i = 0; i <= bayCount; i++)
                framePositions.Add(p1 + direction * (i * actualSpacing));

            CreateChords(model, framePositions, right, up, width, height, chordTop, chordBot, material, classStr, prefix, startNumber);
            for (int i = 0; i < framePositions.Count; i++)
            {
                bool isEnd = (i == 0 || i == framePositions.Count - 1);
                string vProf = (isEnd && !string.IsNullOrEmpty(endVert)) ? endVert : frameVert;
                string hProf = (isEnd && !string.IsNullOrEmpty(endHoriz)) ? endHoriz : frameHoriz;
                CreateFrame(model, framePositions[i], right, up, width, height, vProf, hProf, material, classStr, prefix, startNumber, chordTop, chordBot);
            }

            CreateWallDiagonals(model, framePositions, right, up, width, height, diagWall, material, classStr, prefix, startNumber, chordTop, chordBot);
            CreateRoofFloorDiagonals(model, framePositions, right, up, width, height, diagRF, material, classStr, prefix, startNumber, chordTop, chordBot);

            model.CommitChanges();
            string msg = "Putkisilta luotu.\nPituus: " + length.ToString("F0") + " mm\nKehiä: " + (bayCount + 1).ToString() + "\nJako: " + actualSpacing.ToString("F0") + " mm";
            MessageBox.Show(msg);
        }

        static void CreateChords(Model model, List<TSG.Point> positions, TSG.Vector right, TSG.Vector up, double width, double height, string topProf, string botProf, string mat, string classStr, string prefix, int startNumber)
        {
            double topHalf = GetProfileSize(topProf) / 2.0;
            double botHalf = GetProfileSize(botProf) / 2.0;
            double halfW = width / 2.0;
            double halfHTop = height / 2.0 + topHalf;
            double halfHBot = height / 2.0 + botHalf;

            CreateChordPair(model, positions, right, up, halfW + topHalf, halfHTop, topProf, mat, classStr, prefix, startNumber);
            CreateChordPair(model, positions, right, up, halfW + botHalf, -halfHBot, botProf, mat, classStr, prefix, startNumber);
        }

        static void CreateChordPair(Model model, List<TSG.Point> positions, TSG.Vector right, TSG.Vector up, double xOff, double zOff, string profile, string mat, string classStr, string prefix, int startNumber)
        {
            for (int i = 0; i < positions.Count - 1; i++)
            {
                TSG.Point startL = positions[i] + right * (-xOff) + up * zOff;
                TSG.Point endL = positions[i + 1] + right * (-xOff) + up * zOff;
                TSG.Point startR = positions[i] + right * xOff + up * zOff;
                TSG.Point endR = positions[i + 1] + right * xOff + up * zOff;
                CreateBeam(model, startL, endL, profile, mat, classStr, prefix, startNumber);
                CreateBeam(model, startR, endR, profile, mat, classStr, prefix, startNumber);
            }
        }

        static double GetProfileSize(string profile)
        {
            if (string.IsNullOrEmpty(profile)) return 100;
            string cleaned = profile.ToUpper().Replace("SHS", "").Replace("RHS", "");
            string[] parts = cleaned.Split(new char[] { '*', 'X', 'x' });
            double size;
            if (parts.Length >= 1 && double.TryParse(parts[0].Trim(), out size))
                return size;
            return 100;
        }

        static void CreateFrame(Model model, TSG.Point center, TSG.Vector right, TSG.Vector up, double width, double height, string vertProf, string horizProf, string mat, string classStr, string prefix, int startNumber, string topProf, string botProf)
        {
            double topHalf = GetProfileSize(topProf) / 2.0;
            double botHalf = GetProfileSize(botProf) / 2.0;
            double halfW = width / 2.0 + Math.Max(topHalf, botHalf);
            double halfHTop = height / 2.0 + topHalf;
            double halfHBot = height / 2.0 + botHalf;

            TSG.Point tl = center + right * halfW + up * halfHTop;
            TSG.Point tr = center + right * (-halfW) + up * halfHTop;
            TSG.Point bl = center + right * halfW + up * (-halfHBot);
            TSG.Point br = center + right * (-halfW) + up * (-halfHBot);

            CreateBeam(model, bl, tl, vertProf, mat, classStr, prefix, startNumber);
            CreateBeam(model, br, tr, vertProf, mat, classStr, prefix, startNumber);
            CreateBeam(model, tl, tr, horizProf, mat, classStr, prefix, startNumber);
            CreateBeam(model, bl, br, horizProf, mat, classStr, prefix, startNumber);
        }

        static void CreateWallDiagonals(Model model, List<TSG.Point> positions, TSG.Vector right, TSG.Vector up, double width, double height, string prof, string mat, string classStr, string prefix, int startNumber, string topProf, string botProf)
        {
            double topHalf = GetProfileSize(topProf) / 2.0;
            double botHalf = GetProfileSize(botProf) / 2.0;
            double halfW = width / 2.0 + Math.Max(topHalf, botHalf);
            double halfHTop = height / 2.0 + topHalf;
            double halfHBot = height / 2.0 + botHalf;
            int mid = positions.Count / 2;

            for (int side = 0; side < 2; side++)
            {
                double xOff = (side == 0) ? halfW : -halfW;
                for (int i = 0; i < positions.Count - 1; i++)
                {
                    TSG.Point a = positions[i] + right * xOff + up * (-halfHBot);
                    TSG.Point b = positions[i + 1] + right * xOff + up * halfHTop;
                    TSG.Point c = positions[i] + right * xOff + up * halfHTop;
                    TSG.Point d = positions[i + 1] + right * xOff + up * (-halfHBot);

                    if (i < mid)
                        CreateBeam(model, a, b, prof, mat, classStr, prefix, startNumber);
                    else
                        CreateBeam(model, c, d, prof, mat, classStr, prefix, startNumber);
                }
            }
        }

        static void CreateRoofFloorDiagonals(Model model, List<TSG.Point> positions, TSG.Vector right, TSG.Vector up, double width, double height, string prof, string mat, string classStr, string prefix, int startNumber, string topProf, string botProf)
        {
            double topHalf = GetProfileSize(topProf) / 2.0;
            double botHalf = GetProfileSize(botProf) / 2.0;
            double halfW = width / 2.0 + Math.Max(topHalf, botHalf);

            double zTop = height / 2.0 + topHalf;
            double zBot = -(height / 2.0 + botHalf);

            double[] zOffs = new double[] { zTop, zBot };
            for (int z = 0; z < zOffs.Length; z++)
            {
                double zOff = zOffs[z];
                for (int i = 0; i < positions.Count - 1; i++)
                {
                    TSG.Point leftStart = positions[i] + right * (-halfW) + up * zOff;
                    TSG.Point rightStart = positions[i] + right * halfW + up * zOff;
                    TSG.Point leftEnd = positions[i + 1] + right * (-halfW) + up * zOff;
                    TSG.Point rightEnd = positions[i + 1] + right * halfW + up * zOff;
                    TSG.Point midPoint = new TSG.Point(
                        (positions[i].X + positions[i + 1].X) * 0.5,
                        (positions[i].Y + positions[i + 1].Y) * 0.5,
                        (positions[i].Z + positions[i + 1].Z) * 0.5
                    ) + up * zOff;

                    CreateBeam(model, leftStart, midPoint, prof, mat, classStr, prefix, startNumber);
                    CreateBeam(model, rightStart, midPoint, prof, mat, classStr, prefix, startNumber);
                    CreateBeam(model, midPoint, leftEnd, prof, mat, classStr, prefix, startNumber);
                    CreateBeam(model, midPoint, rightEnd, prof, mat, classStr, prefix, startNumber);
                }
            }
        }

        static void CreateBeam(Model model, TSG.Point start, TSG.Point end, string profile, string material, string classStr, string prefix, int startNumber)
        {
            if (start == null || end == null || string.IsNullOrEmpty(profile)) return;

            Beam beam = new Beam(start, end);
            beam.Profile.ProfileString = profile;
            beam.Material.MaterialString = string.IsNullOrEmpty(material) ? "S355J2" : material;
            beam.Class = string.IsNullOrEmpty(classStr) ? "1" : classStr;
            if (!string.IsNullOrEmpty(prefix))
            {
                beam.PartNumber.Prefix = prefix;
                beam.PartNumber.StartNumber = startNumber;
            }
            beam.Position.Depth = Position.DepthEnum.MIDDLE;
            beam.Position.Plane = Position.PlaneEnum.MIDDLE;
            beam.Insert();
        }
    }
}
