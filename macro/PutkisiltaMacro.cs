// PutkisiltaMacro.cs
// Yksinkertainen Tekla Open API -makro putkisillan rungon luontiin
// Kopioi tiedosto mallikansion macros\modeling\ -kansioon ja aja Applications & components -luettelosta
//
// Testiversio – kovakoodatut oletusarvot. Muokkaa parametreja alla.

using System;
using System.Collections.Generic;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model.UI;

namespace UserMacros
{
    public class PutkisiltaMacro
    {
        public static void Run()
        {
            Model model = new Model();
            if (!model.GetConnectionStatus())
            {
                System.Windows.Forms.MessageBox.Show("Ei yhteyttä Tekla-malliin.");
                return;
            }

            // --- PARAMETRIT (muokkaa näitä) ---
            double width = 1200.0;          // mm, sisänurkkiin
            double height = 1800.0;         // mm, sisänurkkiin
            double maxSpacing = 3000.0;     // mm, maksimikehäjako
            string profileChordTop = "SHS150*150*6";
            string profileChordBottom = "SHS150*150*6";
            string profileFrameVertical = "SHS100*100*5";
            string profileFrameHorizontal = "SHS100*100*5";
            string profileDiagonalWall = "SHS80*80*4";
            string profileDiagonalRoofFloor = "SHS80*80*4";
            string material = "S355J2";
            // ----------------------------------

            Picker picker = new Picker();
            Point p1 = picker.PickPoint("Valitse sillan alkupiste (keskikorkeus)");
            Point p2 = picker.PickPoint("Valitse sillan loppupiste (keskikorkeus)");

            if (p1 == null || p2 == null) return;

            Vector direction = new Vector(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            double length = direction.GetLength();
            if (length < 100) return;

            direction.Normalize();
            Vector up = new Vector(0, 0, 1);
            Vector right = direction.Cross(up);
            if (right.GetLength() < 0.1) right = new Vector(1, 0, 0);
            right.Normalize();

            // Laske kehien määrä ja tasainen jako
            int bayCount = Math.Max(1, (int)Math.Ceiling(length / maxSpacing));
            double actualSpacing = length / bayCount;

            List<Point> framePositions = new List<Point>();
            for (int i = 0; i <= bayCount; i++)
            {
                Point pos = p1 + direction * (i * actualSpacing);
                framePositions.Add(pos);
            }

            // Luo paarteet
            CreateChords(model, framePositions, right, up, width, height, profileChordTop, profileChordBottom, material);

            // Luo kehät (pystyt + vaakatuet)
            for (int i = 0; i < framePositions.Count; i++)
            {
                bool isEnd = (i == 0 || i == framePositions.Count - 1);
                string vertProf = isEnd ? profileFrameVertical : profileFrameVertical;
                string horizProf = isEnd ? profileFrameHorizontal : profileFrameHorizontal;
                CreateFrame(model, framePositions[i], right, up, width, height, vertProf, horizProf, material);
            }

            // Luo seinien N-diagonaalit
            CreateWallDiagonals(model, framePositions, right, up, width, height, profileDiagonalWall, material);

            // Luo katon ja lattian KT-diagonaalit
            CreateRoofFloorDiagonals(model, framePositions, right, up, width, height, profileDiagonalRoofFloor, material);

            model.CommitChanges();
            System.Windows.Forms.MessageBox.Show($"Putkisilta luotu.\nPituus: {length:F0} mm\nKehiä: {bayCount + 1}\nJako: {actualSpacing:F0} mm");
        }

        static void CreateChords(Model model, List<Point> positions, Vector right, Vector up, double width, double height, string topProf, string botProf, string mat)
        {
            double halfW = width / 2.0;
            double halfH = height / 2.0;

            Point[] offsets = new Point[]
            {
                new Point( halfW, 0,  halfH),
                new Point(-halfW, 0,  halfH),
                new Point( halfW, 0, -halfH),
                new Point(-halfW, 0, -halfH)
            };

            string[] profiles = { topProf, topProf, botProf, botProf };

            for (int c = 0; c < 4; c++)
            {
                for (int i = 0; i < positions.Count - 1; i++)
                {
                    Point start = positions[i] + right * offsets[c].X + up * offsets[c].Z;
                    Point end = positions[i + 1] + right * offsets[c].X + up * offsets[c].Z;
                    CreateBeam(model, start, end, profiles[c], mat, "1");
                }
            }
        }

        static void CreateFrame(Model model, Point center, Vector right, Vector up, double width, double height, string vertProf, string horizProf, string mat)
        {
            double halfW = width / 2.0;
            double halfH = height / 2.0;

            Point tl = center + right * halfW + up * halfH;
            Point tr = center + right * (-halfW) + up * halfH;
            Point bl = center + right * halfW + up * (-halfH);
            Point br = center + right * (-halfW) + up * (-halfH);

            CreateBeam(model, bl, tl, vertProf, mat, "2");
            CreateBeam(model, br, tr, vertProf, mat, "2");

            CreateBeam(model, tl, tr, horizProf, mat, "3");
            CreateBeam(model, bl, br, horizProf, mat, "3");
        }

        static void CreateWallDiagonals(Model model, List<Point> positions, Vector right, Vector up, double width, double height, string prof, string mat)
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
                        CreateBeam(model, a, b, prof, mat, "4");
                    else
                        CreateBeam(model, c, d, prof, mat, "4");
                }
            }
        }

        static void CreateRoofFloorDiagonals(Model model, List<Point> positions, Vector right, Vector up, double width, double height, string prof, string mat)
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

                    CreateBeam(model, leftStart, midPoint, prof, mat, "5");
                    CreateBeam(model, rightStart, midPoint, prof, mat, "5");
                    CreateBeam(model, midPoint, leftEnd, prof, mat, "5");
                    CreateBeam(model, midPoint, rightEnd, prof, mat, "5");
                }
            }
        }

        static void CreateBeam(Model model, Point start, Point end, string profile, string material, string classStr)
        {
            Beam beam = new Beam(start, end);
            beam.Profile.ProfileString = profile;
            beam.Material.MaterialString = material;
            beam.Class = classStr;
            beam.Position.Depth = Position.DepthEnum.MIDDLE;
            beam.Position.Plane = Position.PlaneEnum.MIDDLE;
            beam.Insert();
        }
    }
}
