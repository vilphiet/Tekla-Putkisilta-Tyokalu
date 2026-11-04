// PutkisiltaMacro.cs
// Tekla Open API -makro putkisillan rungon luontiin
// Korjattu: N-ristikko seinille, katto/lattia yksi side per väli, yhtenäiset paarteet
// Profiilit kirjoitettavissa (katalogista), Part + Assembly prefix
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
        TextBox txtWidth, txtHeight, txtMaxSpacing, txtMaterial, txtClass;
        TextBox txtPartPrefix, txtPartStart, txtAsmPrefix, txtAsmStart;
        TextBox txtChordTop, txtChordBot, txtFrameVert, txtFrameHoriz;
        TextBox txtEndVert, txtEndHoriz, txtDiagWall, txtDiagRF;
        Button btnCreate, btnCancel;

        public PutkisiltaForm()
        {
            this.Text = "Pipe Bridge";
            this.Width = 520;
            this.Height = 680;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;

            int y = 15;
            int labelW = 170;
            int inputX = 190;
            int inputW = 280;

            AddLabel("Width (inner mm):", 15, y, labelW);
            txtWidth = AddTextBox("1200", inputX, y, inputW); y += 28;

            AddLabel("Height (inner mm):", 15, y, labelW);
            txtHeight = AddTextBox("1800", inputX, y, inputW); y += 28;

            AddLabel("Max frame spacing (mm):", 15, y, labelW);
            txtMaxSpacing = AddTextBox("3000", inputX, y, inputW); y += 35;

            AddLabel("Top chord profile:", 15, y, labelW);
            txtChordTop = AddTextBox("CFRHS150X150X6", inputX, y, inputW); y += 28;

            AddLabel("Bottom chord profile:", 15, y, labelW);
            txtChordBot = AddTextBox("CFRHS150X150X6", inputX, y, inputW); y += 28;

            AddLabel("Frame vertical profile:", 15, y, labelW);
            txtFrameVert = AddTextBox("CFRHS100X100X5", inputX, y, inputW); y += 28;

            AddLabel("Frame horizontal profile:", 15, y, labelW);
            txtFrameHoriz = AddTextBox("CFRHS100X100X5", inputX, y, inputW); y += 28;

            AddLabel("End vertical (empty=same):", 15, y, labelW);
            txtEndVert = AddTextBox("", inputX, y, inputW); y += 28;

            AddLabel("End horizontal (empty=same):", 15, y, labelW);
            txtEndHoriz = AddTextBox("", inputX, y, inputW); y += 28;

            AddLabel("Wall diagonal profile:", 15, y, labelW);
            txtDiagWall = AddTextBox("CFRHS80X80X4", inputX, y, inputW); y += 28;

            AddLabel("Roof/floor diagonal:", 15, y, labelW);
            txtDiagRF = AddTextBox("CFRHS80X80X4", inputX, y, inputW); y += 35;

            AddLabel("Material:", 15, y, labelW);
            txtMaterial = AddTextBox("S355J2", inputX, y, inputW); y += 28;

            AddLabel("Class:", 15, y, labelW);
            txtClass = AddTextBox("1", inputX, y, 80); y += 35;

            AddLabel("Part prefix:", 15, y, labelW);
            txtPartPrefix = AddTextBox("P", inputX, y, 80);
            AddLabel("Start no:", 280, y, 60);
            txtPartStart = AddTextBox("1", 340, y, 60); y += 28;

            AddLabel("Assembly prefix:", 15, y, labelW);
            txtAsmPrefix = AddTextBox("A", inputX, y, 80);
            AddLabel("Start no:", 280, y, 60);
            txtAsmStart = AddTextBox("1", 340, y, 60); y += 40;

            btnCreate = new Button();
            btnCreate.Text = "Create";
            btnCreate.Left = 140;
            btnCreate.Top = y;
            btnCreate.Width = 100;
            btnCreate.Click += BtnCreate_Click;
            this.Controls.Add(btnCreate);

            btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Left = 260;
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

        void BtnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                double width = double.Parse(txtWidth.Text);
                double height = double.Parse(txtHeight.Text);
                double maxSpacing = double.Parse(txtMaxSpacing.Text);
                string material = txtMaterial.Text.Trim();
                string classStr = txtClass.Text.Trim();
                string partPrefix = txtPartPrefix.Text.Trim();
                int partStart = int.Parse(txtPartStart.Text);
                string asmPrefix = txtAsmPrefix.Text.Trim();
                int asmStart = int.Parse(txtAsmStart.Text);

                string chordTop = txtChordTop.Text.Trim();
                string chordBot = txtChordBot.Text.Trim();
                string frameVert = txtFrameVert.Text.Trim();
                string frameHoriz = txtFrameHoriz.Text.Trim();
                string endVert = txtEndVert.Text.Trim();
                string endHoriz = txtEndHoriz.Text.Trim();
                string diagWall = txtDiagWall.Text.Trim();
                string diagRF = txtDiagRF.Text.Trim();

                this.Hide();

                Model model = new Model();
                if (!model.GetConnectionStatus())
                {
                    MessageBox.Show("No connection to Tekla model.");
                    this.Show();
                    return;
                }

                Picker picker = new Picker();
                TSG.Point p1 = picker.PickPoint("Pick start point");
                TSG.Point p2 = picker.PickPoint("Pick end point");
                if (p1 == null || p2 == null)
                {
                    this.Show();
                    return;
                }

                TSG.Vector direction = new TSG.Vector(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
                double length = direction.GetLength();
                if (length < 100)
                {
                    MessageBox.Show("Points too close.");
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

                // Continuous chords
                CreateContinuousChords(model, p1, p2, right, up, width, height, chordTop, chordBot, material, classStr, partPrefix, partStart, asmPrefix, asmStart);

                // Frames
                for (int i = 0; i < framePositions.Count; i++)
                {
                    bool isEnd = (i == 0 || i == framePositions.Count - 1);
                    string vProf = (isEnd && !string.IsNullOrEmpty(endVert)) ? endVert : frameVert;
                    string hProf = (isEnd && !string.IsNullOrEmpty(endHoriz)) ? endHoriz : frameHoriz;
                    CreateFrame(model, framePositions[i], right, up, width, height, vProf, hProf, material, classStr, partPrefix, partStart, asmPrefix, asmStart, chordTop, chordBot);
                }

                // Wall diagonals - N-truss (starts falling)
                CreateWallDiagonalsN(model, framePositions, right, up, width, height, diagWall, material, classStr, partPrefix, partStart, asmPrefix, asmStart, chordTop, chordBot);

                // Roof/floor diagonals - one per bay
                CreateRoofFloorDiagonalsKT(model, framePositions, right, up, width, height, diagRF, material, classStr, partPrefix, partStart, asmPrefix, asmStart, chordTop, chordBot);

                model.CommitChanges();

                string msg = "Pipe bridge created.\nLength: " + length.ToString("F0") + " mm\nFrames: " + (bayCount + 1).ToString() + "\nSpacing: " + actualSpacing.ToString("F0") + " mm";
                MessageBox.Show(msg);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                this.Show();
            }
        }

        static void CreateContinuousChords(Model model, TSG.Point p1, TSG.Point p2, TSG.Vector right, TSG.Vector up, double width, double height, string topProf, string botProf, string mat, string classStr, string partPrefix, int partStart, string asmPrefix, int asmStart)
        {
            double topHalf = GetProfileSize(topProf) / 2.0;
            double botHalf = GetProfileSize(botProf) / 2.0;
            double halfW = width / 2.0;
            double halfHTop = height / 2.0 + topHalf;
            double halfHBot = height / 2.0 + botHalf;

            // Top chords continuous
            TSG.Point topL1 = p1 + right * (-halfW - topHalf) + up * halfHTop;
            TSG.Point topL2 = p2 + right * (-halfW - topHalf) + up * halfHTop;
            TSG.Point topR1 = p1 + right * (halfW + topHalf) + up * halfHTop;
            TSG.Point topR2 = p2 + right * (halfW + topHalf) + up * halfHTop;
            CreateBeam(model, topL1, topL2, topProf, mat, classStr, partPrefix, partStart, asmPrefix, asmStart);
            CreateBeam(model, topR1, topR2, topProf, mat, classStr, partPrefix, partStart, asmPrefix, asmStart);

            // Bottom chords continuous
            TSG.Point botL1 = p1 + right * (-halfW - botHalf) + up * (-halfHBot);
            TSG.Point botL2 = p2 + right * (-halfW - botHalf) + up * (-halfHBot);
            TSG.Point botR1 = p1 + right * (halfW + botHalf) + up * (-halfHBot);
            TSG.Point botR2 = p2 + right * (halfW + botHalf) + up * (-halfHBot);
            CreateBeam(model, botL1, botL2, botProf, mat, classStr, partPrefix, partStart, asmPrefix, asmStart);
            CreateBeam(model, botR1, botR2, botProf, mat, classStr, partPrefix, partStart, asmPrefix, asmStart);
        }

        static double GetProfileSize(string profile)
        {
            if (string.IsNullOrEmpty(profile)) return 100;
            string cleaned = profile.ToUpper().Replace("CFRHS", "").Replace("SHS", "").Replace("RHS", "");
            string[] parts = cleaned.Split(new char[] { '*', 'X', 'x' });
            double size;
            if (parts.Length >= 1 && double.TryParse(parts[0].Trim(), out size))
                return size;
            return 100;
        }

        static void CreateFrame(Model model, TSG.Point center, TSG.Vector right, TSG.Vector up, double width, double height, string vertProf, string horizProf, string mat, string classStr, string partPrefix, int partStart, string asmPrefix, int asmStart, string topProf, string botProf)
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

            CreateBeam(model, bl, tl, vertProf, mat, classStr, partPrefix, partStart, asmPrefix, asmStart);
            CreateBeam(model, br, tr, vertProf, mat, classStr, partPrefix, partStart, asmPrefix, asmStart);
            CreateBeam(model, tl, tr, horizProf, mat, classStr, partPrefix, partStart, asmPrefix, asmStart);
            CreateBeam(model, bl, br, horizProf, mat, classStr, partPrefix, partStart, asmPrefix, asmStart);
        }

        // N-truss walls: direction changes at midpoint, starts with falling diagonal
        static void CreateWallDiagonalsN(Model model, List<TSG.Point> positions, TSG.Vector right, TSG.Vector up, double width, double height, string prof, string mat, string classStr, string partPrefix, int partStart, string asmPrefix, int asmStart, string topProf, string botProf)
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
                    TSG.Point botStart = positions[i] + right * xOff + up * (-halfHBot);
                    TSG.Point topEnd = positions[i + 1] + right * xOff + up * halfHTop;
                    TSG.Point topStart = positions[i] + right * xOff + up * halfHTop;
                    TSG.Point botEnd = positions[i + 1] + right * xOff + up * (-halfHBot);

                    // Reversed: start with falling diagonal
                    if (i < mid)
                        CreateBeam(model, topStart, botEnd, prof, mat, classStr, partPrefix, partStart, asmPrefix, asmStart);
                    else
                        CreateBeam(model, botStart, topEnd, prof, mat, classStr, partPrefix, partStart, asmPrefix, asmStart);
                }
            }
        }

        // Roof/floor: only one diagonal brace per bay
        static void CreateRoofFloorDiagonalsKT(Model model, List<TSG.Point> positions, TSG.Vector right, TSG.Vector up, double width, double height, string prof, string mat, string classStr, string partPrefix, int partStart, string asmPrefix, int asmStart, string topProf, string botProf)
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
                    TSG.Point left = positions[i] + right * (-halfW) + up * zOff;
                    TSG.Point rightPt = positions[i] + right * halfW + up * zOff;
                    TSG.Point leftNext = positions[i + 1] + right * (-halfW) + up * zOff;
                    TSG.Point rightNext = positions[i + 1] + right * halfW + up * zOff;

                    // Only one diagonal per bay
                    // Alternate direction
                    if (i % 2 == 0)
                        CreateBeam(model, left, rightNext, prof, mat, classStr, partPrefix, partStart, asmPrefix, asmStart);
                    else
                        CreateBeam(model, rightPt, leftNext, prof, mat, classStr, partPrefix, partStart, asmPrefix, asmStart);
                }
            }
        }

        static void CreateBeam(Model model, TSG.Point start, TSG.Point end, string profile, string material, string classStr, string partPrefix, int partStart, string asmPrefix, int asmStart)
        {
            if (start == null || end == null || string.IsNullOrEmpty(profile)) return;

            Beam beam = new Beam(start, end);
            beam.Profile.ProfileString = profile;
            beam.Material.MaterialString = string.IsNullOrEmpty(material) ? "S355J2" : material;
            beam.Class = string.IsNullOrEmpty(classStr) ? "1" : classStr;

            if (!string.IsNullOrEmpty(partPrefix))
            {
                beam.PartNumber.Prefix = partPrefix;
                beam.PartNumber.StartNumber = partStart;
            }
            if (!string.IsNullOrEmpty(asmPrefix))
            {
                beam.AssemblyNumber.Prefix = asmPrefix;
                beam.AssemblyNumber.StartNumber = asmStart;
            }

            beam.Position.Depth = Position.DepthEnum.MIDDLE;
            beam.Position.Plane = Position.PlaneEnum.MIDDLE;

            if (!beam.Insert())
            {
                MessageBox.Show("Failed to create beam:\nProfile: " + profile + "\nMaterial: " + material);
            }
        }
    }
}
