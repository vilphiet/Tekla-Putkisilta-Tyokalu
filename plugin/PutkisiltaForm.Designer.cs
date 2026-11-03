// PutkisiltaForm.Designer.cs
// Generoitu käyttöliittymä – muokattavissa Visual Studiossa

namespace PutkisiltaPlugin
{
    partial class PutkisiltaForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblWidth;
        private System.Windows.Forms.TextBox txtWidth;
        private System.Windows.Forms.Label lblHeight;
        private System.Windows.Forms.TextBox txtHeight;
        private System.Windows.Forms.Label lblMaxSpacing;
        private System.Windows.Forms.TextBox txtMaxSpacing;
        private System.Windows.Forms.Label lblMaterial;
        private System.Windows.Forms.TextBox txtMaterial;
        private System.Windows.Forms.Label lblHeightPos;
        private System.Windows.Forms.ComboBox cmbHeightPos;
        private System.Windows.Forms.GroupBox grpProfiles;
        private System.Windows.Forms.Label lblChordTop;
        private System.Windows.Forms.ComboBox cmbChordTop;
        private System.Windows.Forms.Label lblChordBot;
        private System.Windows.Forms.ComboBox cmbChordBot;
        private System.Windows.Forms.Label lblFrameVert;
        private System.Windows.Forms.ComboBox cmbFrameVert;
        private System.Windows.Forms.Label lblFrameHoriz;
        private System.Windows.Forms.ComboBox cmbFrameHoriz;
        private System.Windows.Forms.Label lblEndVert;
        private System.Windows.Forms.ComboBox cmbEndVert;
        private System.Windows.Forms.Label lblEndHoriz;
        private System.Windows.Forms.ComboBox cmbEndHoriz;
        private System.Windows.Forms.Label lblDiagWall;
        private System.Windows.Forms.ComboBox cmbDiagWall;
        private System.Windows.Forms.Label lblDiagRF;
        private System.Windows.Forms.ComboBox cmbDiagRF;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Button cancelButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblWidth = new System.Windows.Forms.Label();
            this.txtWidth = new System.Windows.Forms.TextBox();
            this.lblHeight = new System.Windows.Forms.Label();
            this.txtHeight = new System.Windows.Forms.TextBox();
            this.lblMaxSpacing = new System.Windows.Forms.Label();
            this.txtMaxSpacing = new System.Windows.Forms.TextBox();
            this.lblMaterial = new System.Windows.Forms.Label();
            this.txtMaterial = new System.Windows.Forms.TextBox();
            this.lblHeightPos = new System.Windows.Forms.Label();
            this.cmbHeightPos = new System.Windows.Forms.ComboBox();
            this.grpProfiles = new System.Windows.Forms.GroupBox();
            this.lblChordTop = new System.Windows.Forms.Label();
            this.cmbChordTop = new System.Windows.Forms.ComboBox();
            this.lblChordBot = new System.Windows.Forms.Label();
            this.cmbChordBot = new System.Windows.Forms.ComboBox();
            this.lblFrameVert = new System.Windows.Forms.Label();
            this.cmbFrameVert = new System.Windows.Forms.ComboBox();
            this.lblFrameHoriz = new System.Windows.Forms.Label();
            this.cmbFrameHoriz = new System.Windows.Forms.ComboBox();
            this.lblEndVert = new System.Windows.Forms.Label();
            this.cmbEndVert = new System.Windows.Forms.ComboBox();
            this.lblEndHoriz = new System.Windows.Forms.Label();
            this.cmbEndHoriz = new System.Windows.Forms.ComboBox();
            this.lblDiagWall = new System.Windows.Forms.Label();
            this.cmbDiagWall = new System.Windows.Forms.ComboBox();
            this.lblDiagRF = new System.Windows.Forms.Label();
            this.cmbDiagRF = new System.Windows.Forms.ComboBox();
            this.okButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.grpProfiles.SuspendLayout();
            this.SuspendLayout();

            this.lblWidth.Text = "Leveys (mm):"; this.lblWidth.Location = new System.Drawing.Point(12, 15); this.lblWidth.AutoSize = true;
            this.txtWidth.Location = new System.Drawing.Point(140, 12); this.txtWidth.Size = new System.Drawing.Size(100, 20);
            this.txtWidth.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, "Width", true));

            this.lblHeight.Text = "Korkeus (mm):"; this.lblHeight.Location = new System.Drawing.Point(12, 41); this.lblHeight.AutoSize = true;
            this.txtHeight.Location = new System.Drawing.Point(140, 38); this.txtHeight.Size = new System.Drawing.Size(100, 20);
            this.txtHeight.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, "Height", true));

            this.lblMaxSpacing.Text = "Max kehäjako (mm):"; this.lblMaxSpacing.Location = new System.Drawing.Point(12, 67); this.lblMaxSpacing.AutoSize = true;
            this.txtMaxSpacing.Location = new System.Drawing.Point(140, 64); this.txtMaxSpacing.Size = new System.Drawing.Size(100, 20);
            this.txtMaxSpacing.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, "MaxSpacing", true));

            this.lblMaterial.Text = "Materiaali:"; this.lblMaterial.Location = new System.Drawing.Point(12, 93); this.lblMaterial.AutoSize = true;
            this.txtMaterial.Location = new System.Drawing.Point(140, 90); this.txtMaterial.Size = new System.Drawing.Size(100, 20);
            this.txtMaterial.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, "Material", true));

            this.lblHeightPos.Text = "Korkeusasema:"; this.lblHeightPos.Location = new System.Drawing.Point(12, 119); this.lblHeightPos.AutoSize = true;
            this.cmbHeightPos.Location = new System.Drawing.Point(140, 116); this.cmbHeightPos.Size = new System.Drawing.Size(140, 21);
            this.cmbHeightPos.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHeightPos.Items.AddRange(new object[] { "Keskikorkeus", "Alapinta", "Yläpinta" });
            this.cmbHeightPos.DataBindings.Add(new System.Windows.Forms.Binding("SelectedIndex", this, "HeightPosition", true));

            this.grpProfiles.Text = "Profiilit (RHS/SHS)";
            this.grpProfiles.Location = new System.Drawing.Point(12, 150);
            this.grpProfiles.Size = new System.Drawing.Size(360, 280);

            string[] profiles = { "SHS80*80*4", "SHS100*100*5", "SHS120*120*5", "SHS150*150*6", "SHS200*200*8", "RHS100*50*4", "RHS150*100*6", "RHS200*100*6" };

            int y = 25;
            AddProfileRow(this.grpProfiles, ref y, "Yläpaarre:", this.cmbChordTop, profiles, "ProfileChordTop");
            AddProfileRow(this.grpProfiles, ref y, "Alapaarre:", this.cmbChordBot, profiles, "ProfileChordBottom");
            AddProfileRow(this.grpProfiles, ref y, "Kehä pysty:", this.cmbFrameVert, profiles, "ProfileFrameVertical");
            AddProfileRow(this.grpProfiles, ref y, "Kehä vaaka:", this.cmbFrameHoriz, profiles, "ProfileFrameHorizontal");
            AddProfileRow(this.grpProfiles, ref y, "Pääty pysty:", this.cmbEndVert, profiles, "ProfileEndVertical");
            AddProfileRow(this.grpProfiles, ref y, "Pääty vaaka:", this.cmbEndHoriz, profiles, "ProfileEndHorizontal");
            AddProfileRow(this.grpProfiles, ref y, "Seinä diag.:", this.cmbDiagWall, profiles, "ProfileDiagonalWall");
            AddProfileRow(this.grpProfiles, ref y, "Katto/lattia diag.:", this.cmbDiagRF, profiles, "ProfileDiagonalRoofFloor");

            this.okButton.Text = "OK"; this.okButton.Location = new System.Drawing.Point(120, 450); this.okButton.Size = new System.Drawing.Size(75, 28);
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            this.applyButton.Text = "Käytä"; this.applyButton.Location = new System.Drawing.Point(205, 450); this.applyButton.Size = new System.Drawing.Size(75, 28);
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            this.cancelButton.Text = "Peruuta"; this.cancelButton.Location = new System.Drawing.Point(290, 450); this.cancelButton.Size = new System.Drawing.Size(75, 28);
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 500);
            this.Controls.Add(this.lblWidth); this.Controls.Add(this.txtWidth);
            this.Controls.Add(this.lblHeight); this.Controls.Add(this.txtHeight);
            this.Controls.Add(this.lblMaxSpacing); this.Controls.Add(this.txtMaxSpacing);
            this.Controls.Add(this.lblMaterial); this.Controls.Add(this.txtMaterial);
            this.Controls.Add(this.lblHeightPos); this.Controls.Add(this.cmbHeightPos);
            this.Controls.Add(this.grpProfiles);
            this.Controls.Add(this.okButton); this.Controls.Add(this.applyButton); this.Controls.Add(this.cancelButton);
            this.Text = "Putkisilta";
            this.grpProfiles.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void AddProfileRow(System.Windows.Forms.GroupBox parent, ref int y, string label, System.Windows.Forms.ComboBox cmb, string[] items, string binding)
        {
            var lbl = new System.Windows.Forms.Label { Text = label, Location = new System.Drawing.Point(10, y), AutoSize = true };
            cmb.Location = new System.Drawing.Point(140, y - 3);
            cmb.Size = new System.Drawing.Size(180, 21);
            cmb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            cmb.Items.AddRange(items);
            cmb.DataBindings.Add(new System.Windows.Forms.Binding("Text", this, binding, true));
            parent.Controls.Add(lbl);
            parent.Controls.Add(cmb);
            y += 32;
        }
    }
}
