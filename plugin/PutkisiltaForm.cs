// PutkisiltaForm.cs
// Käyttöliittymä Putkisilta-plug-inille

using System;
using System.Windows.Forms;
using Tekla.Structures.Dialog;

namespace PutkisiltaPlugin
{
    public partial class PutkisiltaForm : PluginFormBase
    {
        public PutkisiltaForm()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Apply();
            this.Close();
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            this.Apply();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
