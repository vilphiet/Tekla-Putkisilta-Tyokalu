// PutkisiltaMacro.cs
// Tekla Open API -makro putkisillan rungon luontiin + dialogi parametreille
// Kopioi mallikansion macros\modeling\ -kansioon ja aja Applications & components -luettelosta

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
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
                MessageBox.Show("Ei yhteyttä Tekla-malliin.");
                return;
            }

            using (var form = new PutkisiltaDialog())
            {
                if (form.ShowDialog() != DialogResult.OK)
                    return;

                double width = form.WidthValue;
                double height = form.HeightValue;
                double maxSpacing = form.MaxSpacingValue;
                string material = form.MaterialValue;
                int heightPos = form.HeightPositionValue;

                string chordTop = form.ProfileChordTop;
                string chordBot = form.ProfileChordBottom;
                string frameVert = form.ProfileFrameVertical;
                string frameHoriz = form.ProfileFrameHorizontal;
                string endVert = form.ProfileEndVertical;
                string endHoriz = form.ProfileEndHorizontal;
                string diagWall = form.ProfileDiagonalWall;
                string diagRF = form.ProfileDiagonalRoofFloor;

                Picker picker = new Picker();
                Point p1 = picker.PickPoint("Valitse sillan alkupiste");
                Point p2 = picker.PickPoint("Valitse sillan loppupiste");
                if (p1 == null || p2 == null) return;

                double zShift = 0;
                if (heightPos == 1) zShift = -height / 2.0;
                else if (heightPos == 2) zShift = height / 2.0;

                Vector direction = new Vector(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
                double length = direction.GetLength();
                if (length < 100) return;

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

                CreateChords(model, framePositions, right, up, width, height, chordTop, chordBot, material);
                for (int i = 0; i < framePositions.Count; i++)
                {
                    bool isEnd = i == 0 || i == framePositions.Count - 1;
                    string vProf = isEnd && !string.IsNullOrEmpty(endVert) ? endVert : frameVert;
                    string hProf = isEnd && !string.IsNullOrEmpty(endHoriz) ? endHoriz : frameHoriz;
                    CreateFrame(model, framePositions[i], right, up, width, height, vProf, hProf, material);
                }

                CreateWallDiagonals(model, framePositions, right, up, width, height, diagWall, material);
                CreateRoofFloorDiagonals(model, framePositions, right, up, width, height, diagRF, material);

                model.CommitChanges();
                MessageBox.Show($"Putkisilta luotu.\nPituus: {length:F0} mm\nKehiä: {bayCount + 1}\nJako: {actualSpacing:F0} mm");
            }
        }

        static void CreateChords(Model model, List<Point> positions, Vector right, Vector up, double width, double height, string topProf, string botProf, string mat)
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
                    CreateBeam(model, start, end, profiles[c], mat, "1");
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

    public class PutkisiltaDialog : Form
    {
        public double WidthValue { get; private set; } = 1200;
        public double HeightValue { get; private set; } = 1800;
        public double MaxSpacingValue { get; private set; } = 3000;
        public string MaterialValue { get; private set; } = "S355J2";
        public int HeightPositionValue { get; private set; } = 0;

        public string ProfileChordTop { get; private set; } = "SHS150*150*6";
        public string ProfileChordBottom { get; private set; } = "SHS150*150*6";
        public string ProfileFrameVertical { get; private set; } = "SHS100*100*5";
        public string ProfileFrameHorizontal { get; private set; } = "SHS100*100*5";
        public string ProfileEndVertical { get; private set; } = "";
        public string ProfileEndHorizontal { get; private set; } = "";
        public string ProfileDiagonalWall { get; private set; } = "SHS80*80*4";
        public string ProfileDiagonalRoofFloor { get; private set; } = "SHS80*80*4";

        private TextBox txtWidth, txtHeight, txtMaxSpacing, txtMaterial;
        private ComboBox cmbHeightPos;
        private ComboBox cmbChordTop, cmbChordBot, cmbFrameVert, cmbFrameHoriz;
        private ComboBox cmbEndVert, cmbEndHoriz, cmbDiagWall, cmbDiagRF;

        public PutkisiltaDialog()
        {
            this.Text = "Putkisilta";
            this.Size = new Size(420, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            string[] profiles = { "SHS80*80*4", "SHS100*100*5", "SHS120*120*5", "SHS150*150*6", "SHS200*200*8", "RHS100*50*4", "RHS150*100*6", "RHS200*100*6" };

            int y = 15;
            AddLabel("Leveys (mm):", 12, y);
            txtWidth = AddTextBox("1200", 160, y); y += 28;
            AddLabel("Korkeus (mm):", 12, y);
            txtHeight = AddTextBox("1800", 160, y); y += 28;
            AddLabel("Max kehäjako (mm):", 12, y);
            txtMaxSpacing = AddTextBox("3000", 160, y); y += 28;
            AddLabel("Materiaali:", 12, y);
            txtMaterial = AddTextBox("S355J2", 160, y); y += 28;
            AddLabel("Korkeusasema:", 12, y);
            cmbHeightPos = new ComboBox { Location = new Point(160, y), Size = new Size(160, 21), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbHeightPos.Items.AddRange(new object[] { "Keskikorkeus", "Alapinta", "Yläpinta" });
            cmbHeightPos.SelectedIndex = 0;
            this.Controls.Add(cmbHeightPos); y += 35;

            var grp = new GroupBox { Text = "Profiilit", Location = new Point(12, y), Size = new Size(380, 300) };
            this.Controls.Add(grp);
            int gy = 25;
            cmbChordTop = AddProfileCombo(grp, "Yläpaarre:", gy, profiles); gy += 32;
            cmbChordBot = AddProfileCombo(grp, "Alapaarre:", gy, profiles); gy += 32;
            cmbFrameVert = AddProfileCombo(grp, "Kehä pysty:", gy, profiles); gy += 32;
            cmbFrameHoriz = AddProfileCombo(grp, "Kehä vaaka:", gy, profiles); gy += 32;
            cmbEndVert = AddProfileCombo(grp, "Pääty pysty:", gy, profiles); gy += 32;
            cmbEndHoriz = AddProfileCombo(grp, "Pääty vaaka:", gy, profiles); gy += 32;
            cmbDiagWall = AddProfileCombo(grp, "Seinä diag.:", gy, profiles); gy += 32;
            cmbDiagRF = AddProfileCombo(grp, "Katto/lattia diag.:", gy, profiles);

            var btnOk = new Button { Text = "OK", Location = new Point(140, 490), Size = new Size(80, 28), DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Peruuta", Location = new Point(230, 490), Size = new Size(80, 28), DialogResult = DialogResult.Cancel };
            btnOk.Click += (s, e) => ReadValues();
            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }

        void AddLabel(string text, int x, int y)
        {
            this.Controls.Add(new Label { Text = text, Location = new Point(x, y + 3), AutoSize = true });
        }

        TextBox AddTextBox(string text, int x, int y)
        {
            var tb = new TextBox { Text = text, Location = new Point(x, y), Size = new Size(100, 20) };
            this.Controls.Add(tb);
            return tb;
        }

        ComboBox AddProfileCombo(GroupBox parent, string label, int y, string[] items)
        {
            parent.Controls.Add(new Label { Text = label, Location = new Point(10, y + 3), AutoSize = true });
            var cmb = new ComboBox { Location = new Point(140, y), Size = new Size(200, 21), DropDownStyle = ComboBoxStyle.DropDown };
            cmb.Items.AddRange(items);
            if (items.Length > 0) cmb.SelectedIndex = 0;
            parent.Controls.Add(cmb);
            return cmb;
        }

        void ReadValues()
        {
            double.TryParse(txtWidth.Text, out double w); WidthValue = w > 0 ? w : 1200;
            double.TryParse(txtHeight.Text, out double h); HeightValue = h > 0 ? h : 1800;
            double.TryParse(txtMaxSpacing.Text, out double ms); MaxSpacingValue = ms > 0 ? ms : 3000;
            MaterialValue = string.IsNullOrWhiteSpace(txtMaterial.Text) ? "S355J2" : txtMaterial.Text;
            HeightPositionValue = cmbHeightPos.SelectedIndex;

            ProfileChordTop = cmbChordTop.Text;
            ProfileChordBottom = cmbChordBot.Text;
            ProfileFrameVertical = cmbFrameVert.Text;
            ProfileFrameHorizontal = cmbFrameHoriz.Text;
            ProfileEndVertical = cmbEndVert.Text;
            ProfileEndHorizontal = cmbEndHoriz.Text;
            ProfileDiagonalWall = cmbDiagWall.Text;
            ProfileDiagonalRoofFloor = cmbDiagRF.Text;
        }
    }
}
