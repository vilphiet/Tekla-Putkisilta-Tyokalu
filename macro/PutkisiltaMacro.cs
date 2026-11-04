// PutkisiltaMacro.cs
// Tekla Open API -makro putkisillan rungon luontiin + dialogi parametreille
// Yhteensopiva Teklan makrokääntäjän kanssa (vanhempi C#)
// Kopioi mallikansion macros\modeling\ -kansioon

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

            PutkisiltaDialog form = new PutkisiltaDialog();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            double width = form.WidthValue;
            double height = form.HeightValue;
            double maxSpacing = form.MaxSpacingValue;
            string material = form.MaterialValue;
            string classStr = form.ClassValue;
            string prefix = form.PrefixValue;
            int startNumber = form.StartNumberValue;
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

            p1 = p1 + up * zShift;
            p2 = p2 + up * zShift;

            int bayCount = Math.Max(1, (int)Math.Ceiling(length / maxSpacing));
            double actualSpacing = length / bayCount;

            List<Point> framePositions = new List<Point>();
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

        static void CreateChords(Model model, List<Point> positions, Vector right, Vector up, double width, double height, string topProf, string botProf, string mat, string classStr, string prefix, int startNumber)
        {
            double topHalf = GetProfileSize(topProf) / 2.0;
            double botHalf = GetProfileSize(botProf) / 2.0;
            double halfW = width / 2.0;
            double halfHTop = height / 2.0 + topHalf;
            double halfHBot = height / 2.0 + botHalf;

            CreateChordPair(model, positions, right, up, halfW + topHalf, halfHTop, topProf, mat, classStr, prefix, startNumber);
            CreateChordPair(model, positions, right, up, halfW + botHalf, -halfHBot, botProf, mat, classStr, prefix, startNumber);
        }

        static void CreateChordPair(Model model, List<Point> positions, Vector right, Vector up, double xOff, double zOff, string profile, string mat, string classStr, string prefix, int startNumber)
        {
            for (int i = 0; i < positions.Count - 1; i++)
            {
                Point startL = positions[i] + right * (-xOff) + up * zOff;
                Point endL = positions[i + 1] + right * (-xOff) + up * zOff;
                Point startR = positions[i] + right * xOff + up * zOff;
                Point endR = positions[i + 1] + right * xOff + up * zOff;
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

        static void CreateFrame(Model model, Point center, Vector right, Vector up, double width, double height, string vertProf, string horizProf, string mat, string classStr, string prefix, int startNumber, string topProf, string botProf)
        {
            double topHalf = GetProfileSize(topProf) / 2.0;
            double botHalf = GetProfileSize(botProf) / 2.0;
            double halfW = width / 2.0 + Math.Max(topHalf, botHalf);
            double halfHTop = height / 2.0 + topHalf;
            double halfHBot = height / 2.0 + botHalf;

            Point tl = center + right * halfW + up * halfHTop;
            Point tr = center + right * (-halfW) + up * halfHTop;
            Point bl = center + right * halfW + up * (-halfHBot);
            Point br = center + right * (-halfW) + up * (-halfHBot);

            CreateBeam(model, bl, tl, vertProf, mat, classStr, prefix, startNumber);
            CreateBeam(model, br, tr, vertProf, mat, classStr, prefix, startNumber);
            CreateBeam(model, tl, tr, horizProf, mat, classStr, prefix, startNumber);
            CreateBeam(model, bl, br, horizProf, mat, classStr, prefix, startNumber);
        }

        static void CreateWallDiagonals(Model model, List<Point> positions, Vector right, Vector up, double width, double height, string prof, string mat, string classStr, string prefix, int startNumber, string topProf, string botProf)
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
                    Point a = positions[i] + right * xOff + up * (-halfHBot);
                    Point b = positions[i + 1] + right * xOff + up * halfHTop;
                    Point c = positions[i] + right * xOff + up * halfHTop;
                    Point d = positions[i + 1] + right * xOff + up * (-halfHBot);

                    if (i < mid)
                        CreateBeam(model, a, b, prof, mat, classStr, prefix, startNumber);
                    else
                        CreateBeam(model, c, d, prof, mat, classStr, prefix, startNumber);
                }
            }
        }

        static void CreateRoofFloorDiagonals(Model model, List<Point> positions, Vector right, Vector up, double width, double height, string prof, string mat, string classStr, string prefix, int startNumber, string topProf, string botProf)
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
                    Point leftStart = positions[i] + right * (-halfW) + up * zOff;
                    Point rightStart = positions[i] + right * halfW + up * zOff;
                    Point leftEnd = positions[i + 1] + right * (-halfW) + up * zOff;
                    Point rightEnd = positions[i + 1] + right * halfW + up * zOff;
                    Point midPoint = (positions[i] + positions[i + 1]) * 0.5 + up * zOff;

                    CreateBeam(model, leftStart, midPoint, prof, mat, classStr, prefix, startNumber);
                    CreateBeam(model, rightStart, midPoint, prof, mat, classStr, prefix, startNumber);
                    CreateBeam(model, midPoint, leftEnd, prof, mat, classStr, prefix, startNumber);
                    CreateBeam(model, midPoint, rightEnd, prof, mat, classStr, prefix, startNumber);
                }
            }
        }

        static void CreateBeam(Model model, Point start, Point end, string profile, string material, string classStr, string prefix, int startNumber)
        {
            Beam beam = new Beam(start, end);
            beam.Profile.ProfileString = profile;
            beam.Material.MaterialString = material;
            beam.Class = classStr;
            beam.PartNumber.Prefix = prefix;
            beam.PartNumber.StartNumber = startNumber;
            beam.Position.Depth = Position.DepthEnum.MIDDLE;
            beam.Position.Plane = Position.PlaneEnum.MIDDLE;
            beam.Insert();
        }
    }

    public class PutkisiltaDialog : Form
    {
        public double WidthValue = 1200;
        public double HeightValue = 1800;
        public double MaxSpacingValue = 3000;
        public string MaterialValue = "S355J2";
        public string ClassValue = "1";
        public string PrefixValue = "P";
        public int StartNumberValue = 1;
        public int HeightPositionValue = 0;

        public string ProfileChordTop = "SHS150*150*6";
        public string ProfileChordBottom = "SHS150*150*6";
        public string ProfileFrameVertical = "SHS100*100*5";
        public string ProfileFrameHorizontal = "SHS100*100*5";
        public string ProfileEndVertical = "";
        public string ProfileEndHorizontal = "";
        public string ProfileDiagonalWall = "SHS80*80*4";
        public string ProfileDiagonalRoofFloor = "SHS80*80*4";

        private TextBox txtWidth, txtHeight, txtMaxSpacing, txtMaterial, txtClass, txtPrefix, txtStartNumber;
        private ComboBox cmbHeightPos;
        private ComboBox cmbChordTop, cmbChordBot, cmbFrameVert, cmbFrameHoriz;
        private ComboBox cmbEndVert, cmbEndHoriz, cmbDiagWall, cmbDiagRF;

        public PutkisiltaDialog()
        {
            this.Text = "Putkisilta";
            this.Size = new Size(420, 640);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            string[] profiles = new string[] { "SHS80*80*4", "SHS100*100*5", "SHS120*120*5", "SHS150*150*6", "SHS200*200*8", "RHS100*50*4", "RHS150*100*6", "RHS200*100*6" };

            int y = 15;
            AddLabel("Leveys (mm):", 12, y);
            txtWidth = AddTextBox("1200", 160, y); y += 28;
            AddLabel("Korkeus (mm):", 12, y);
            txtHeight = AddTextBox("1800", 160, y); y += 28;
            AddLabel("Max kehäjako (mm):", 12, y);
            txtMaxSpacing = AddTextBox("3000", 160, y); y += 28;
            AddLabel("Materiaali:", 12, y);
            txtMaterial = AddTextBox("S355J2", 160, y); y += 28;
            AddLabel("Class:", 12, y);
            txtClass = AddTextBox("1", 160, y); y += 28;
            AddLabel("Prefix:", 12, y);
            txtPrefix = AddTextBox("P", 160, y); y += 28;
            AddLabel("Start number:", 12, y);
            txtStartNumber = AddTextBox("1", 160, y); y += 28;
            AddLabel("Korkeusasema:", 12, y);
            cmbHeightPos = new ComboBox();
            cmbHeightPos.Location = new Point(160, y);
            cmbHeightPos.Size = new Size(160, 21);
            cmbHeightPos.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbHeightPos.Items.Add("Keskikorkeus");
            cmbHeightPos.Items.Add("Alapinta");
            cmbHeightPos.Items.Add("Yläpinta");
            cmbHeightPos.SelectedIndex = 0;
            this.Controls.Add(cmbHeightPos); y += 35;

            GroupBox grp = new GroupBox();
            grp.Text = "Profiilit";
            grp.Location = new Point(12, y);
            grp.Size = new Size(380, 300);
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

            Button btnOk = new Button();
            btnOk.Text = "OK";
            btnOk.Location = new Point(140, 570);
            btnOk.Size = new Size(80, 28);
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Click += new EventHandler(btnOk_Click);

            Button btnCancel = new Button();
            btnCancel.Text = "Peruuta";
            btnCancel.Location = new Point(230, 570);
            btnCancel.Size = new Size(80, 28);
            btnCancel.DialogResult = DialogResult.Cancel;

            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }

        void btnOk_Click(object sender, EventArgs e)
        {
            ReadValues();
        }

        void AddLabel(string text, int x, int y)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Location = new Point(x, y + 3);
            lbl.AutoSize = true;
            this.Controls.Add(lbl);
        }

        TextBox AddTextBox(string text, int x, int y)
        {
            TextBox tb = new TextBox();
            tb.Text = text;
            tb.Location = new Point(x, y);
            tb.Size = new Size(100, 20);
            this.Controls.Add(tb);
            return tb;
        }

        ComboBox AddProfileCombo(GroupBox parent, string label, int y, string[] items)
        {
            Label lbl = new Label();
            lbl.Text = label;
            lbl.Location = new Point(10, y + 3);
            lbl.AutoSize = true;
            parent.Controls.Add(lbl);

            ComboBox cmb = new ComboBox();
            cmb.Location = new Point(140, y);
            cmb.Size = new Size(200, 21);
            cmb.DropDownStyle = ComboBoxStyle.DropDown;
            for (int i = 0; i < items.Length; i++)
                cmb.Items.Add(items[i]);
            if (items.Length > 0) cmb.SelectedIndex = 0;
            parent.Controls.Add(cmb);
            return cmb;
        }

        void ReadValues()
        {
            double w;
            if (!double.TryParse(txtWidth.Text, out w) || w <= 0) w = 1200;
            WidthValue = w;

            double h;
            if (!double.TryParse(txtHeight.Text, out h) || h <= 0) h = 1800;
            HeightValue = h;

            double ms;
            if (!double.TryParse(txtMaxSpacing.Text, out ms) || ms <= 0) ms = 3000;
            MaxSpacingValue = ms;

            MaterialValue = string.IsNullOrEmpty(txtMaterial.Text) ? "S355J2" : txtMaterial.Text;
            ClassValue = string.IsNullOrEmpty(txtClass.Text) ? "1" : txtClass.Text;
            PrefixValue = string.IsNullOrEmpty(txtPrefix.Text) ? "P" : txtPrefix.Text;

            int sn;
            if (!int.TryParse(txtStartNumber.Text, out sn) || sn <= 0) sn = 1;
            StartNumberValue = sn;

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
