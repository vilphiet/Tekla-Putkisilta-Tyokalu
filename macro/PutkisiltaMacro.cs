// PutkisiltaMacro.cs
// Tekla Open API -makro putkisillan rungon luontiin
// Dialogi parametreille
// Kopioi mallikansion macros\modeling\ -kansioon ja tallenna ANSI-muodossa

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using TSG = Tekla.Structures.Geometry3d;

namespace Tekla.Technology.Akit.UserScript
{
    public class Script
    {
        public static void Run(Tekla.Technology.Akit.IScript akit)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PutkisiltaForm());
        }
    }

    public class PutkisiltaForm : Form
    {
        TextBox txtWidth, txtHeight, txtMaxSpacing, txtMaterial, txtClass, txtPrefix, txtStartNumber;
        ComboBox cmbChordTop, cmbChordBot, cmbFrameVert, cmbFrameHoriz, cmbEndVert, cmbEndHoriz, cmbDiagWall, cmbDiagRF;
        Button btnCreate, btnCancel;

        public PutkisiltaForm()
        {
            this.Text = "Putkisilta";
            this.Width = 480;
            this.Height = 620;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;

            int y = 15;
            int labelW = 160;
            int inputX = 180;
            int inputW = 260;

            AddLabel("Leveys (sisämitta mm):", 15, y, labelW);
            txtWidth = AddTextBox("1200", inputX, y, inputW); y += 30;

            AddLabel("Korkeus (sisämitta mm):", 15, y, labelW);
            txtHeight = AddTextBox("1800", inputX, y, inputW); y += 30;

            AddLabel("Max kehäjako (mm):", 15, y, labelW);
            txtMaxSpacing = AddTextBox("3000", inputX, y, inputW); y += 40;

            AddLabel("Yläpaarre:", 15, y, labelW);
            cmbChordTop = AddCombo(inputX, y, inputW, "SHS150*150*6"); y += 30;

            AddLabel("Alapaarre:", 15, y, labelW);
            cmbChordBot = AddCombo(inputX, y, inputW, "SHS150*150*6"); y += 30;

            AddLabel("Kehä pysty:", 15, y, labelW);
            cmbFrameVert = AddCombo(inputX, y, inputW, "SHS100*100*5"); y += 30;

            AddLabel("Kehä vaaka:", 15, y, labelW);
            cmbFrameHoriz = AddCombo(inputX, y, inputW, "SHS100*100*5"); y += 30;

            AddLabel("Pääty pysty (tyhjä=sama):", 15, y, labelW);
            cmbEndVert = AddCombo(inputX, y, inputW, ""); y += 30;

            AddLabel("Pääty vaaka (tyhjä=sama):", 15, y, labelW);
            cmbEndHoriz = AddCombo(inputX, y, inputW, ""); y += 30;

            AddLabel("Seinädiagonaali:", 15, y, labelW);
            cmbDiagWall = AddCombo(inputX, y, inputW, "SHS80*80*4"); y += 30;

            AddLabel("Katto/lattia diagonaali:", 15, y, labelW);
            cmbDiagRF = AddCombo(inputX, y, inputW, "SHS80*80*4"); y += 40;

            AddLabel("Materiaali:", 15, y, labelW);
            txtMaterial = AddTextBox("S355J2", inputX, y, inputW); y += 30;

            AddLabel("Luokka:", 15, y, labelW);
            txtClass = AddTextBox("1", inputX, y, 80);

            AddLabel("Etuliite:", 250, y, 70);
            txtPrefix = AddTextBox("P", 320, y, 50);

            AddLabel("Aloitusnro:", 380, y, 70);
            txtStartNumber = AddTextBox("1", 450, y, 50); y += 50;

            btnCreate = new Button();
            btnCreate.Text = "Luo silta";
            btnCreate.Left = 120;
            btnCreate.Top = y;
            btnCreate.Width = 100;
            btnCreate.Click += BtnCreate_Click;
            this.Controls.Add(btnCreate);

            btnCancel = new Button();
            btnCancel.Text = "Peruuta";
            btnCancel.Left = 240;
            btnCancel.Top = y;
            btnCancel.Width = 100;
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);
        }

        Label AddLabel(string text, int x, int y, int w)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Left = x;
            lbl.Top = y + 3;
            lbl.Width = w;
            lbl.AutoSize = false;
            this.Controls.Add(lbl);
            return lbl;
        }

        TextBox AddTextBox(string text, int x, int y, int w)
        {
            TextBox tb = new TextBox();
            tb.Text = text;
            tb.Left = x;
            tb.Top = y;
            tb.Width = w;
            this.Controls.Add(tb);
            return tb;
        }

        ComboBox AddCombo(int x, int y, int w, string defaultVal)
        {
            ComboBox cb = new ComboBox();
            cb.Left = x;
            cb.Top = y;
            cb.Width = w;
            cb.DropDownStyle = ComboBoxStyle.DropDown;
            cb.Items.AddRange(new object[] {
                "SHS60*60*3", "SHS80*80*4", "SHS100*100*5", "SHS120*120*5",
                "SHS150*150*6", "SHS200*200*8", "RHS100*50*4", "RHS150*100*6",
                "RHS200*100*6", ""
            });
            if (!string.IsNullOrEmpty(defaultVal))
                cb.Text = defaultVal;
            this.Controls.Add(cb);
            return cb;
        }

        void BtnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                double width = double.Parse(txtWidth.Text);
                double height = double.Parse(txtHeight.Text);
                double maxSpacing = double.Parse(txtMaxSpacing.Text);
                string material = txtMaterial.Text.Trim();
                string classStr = txtClass.Text.Trim();
                string prefix = txtPrefix.Text.Trim();
                int startNumber = int.Parse(txtStartNumber.Text);

                string chordTop = cmbChordTop.Text.Trim();
                string chordBot = cmbChordBot.Text.Trim();
                string frameVert = cmbFrameVert.Text.Trim();
                string frameHoriz = cmbFrameHoriz.Text.Trim();
                string endVert = cmbEndVert.Text.Trim();
                string endHoriz = cmbEndHoriz.Text.Trim();
                string diagWall = cmbDiagWall.Text.Trim();
                string diagRF = cmbDiagRF.Text.Trim();

                this.Hide();

                Model model = new Model();
                if (!model.GetConnectionStatus())
                {
                    MessageBox.Show("Ei yhteyttä Tekla-malliin.");
                    this.Show();
                    return;
                }

                Picker picker = new Picker();
                TSG.Point p1 = picker.PickPoint("Valitse sillan alkupiste");
                TSG.Point p2 = picker.PickPoint("Valitse sillan loppupiste");
                if (p1 == null || p2 == null)
                {
                    this.Show();
                    return;
                }

                TSG.Vector direction = new TSG.Vector(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
                double length = direction.GetLength();
                if (length < 100)
                {
                    MessageBox.Show("Pisteet liian lähellä.");
                    this.Show();
                    return;
                }

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

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Virhe: " + ex.Message);
                this.Show();
            }
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
