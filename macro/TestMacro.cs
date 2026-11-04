// TestMacro.cs
// Minimaalinen testimakro – luo yhden palkin kahden pisteen välille
// Kopioi mallikansion macros\modeling\ -kansioon

using System;
using System.Windows.Forms;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using TSG = Tekla.Structures.Geometry3d;

namespace UserMacros
{
    public class TestMacro
    {
        public static void Run()
        {
            Model model = new Model();
            if (!model.GetConnectionStatus())
            {
                MessageBox.Show("Ei yhteyttä Tekla-malliin.");
                return;
            }

            Picker picker = new Picker();
            TSG.Point p1 = picker.PickPoint("Valitse alkupiste");
            TSG.Point p2 = picker.PickPoint("Valitse loppupiste");
            if (p1 == null || p2 == null) return;

            Beam beam = new Beam(p1, p2);
            beam.Profile.ProfileString = "SHS100*100*5";
            beam.Material.MaterialString = "S355J2";
            beam.Class = "1";
            beam.Position.Depth = Position.DepthEnum.MIDDLE;
            beam.Position.Plane = Position.PlaneEnum.MIDDLE;
            beam.Insert();

            model.CommitChanges();
            MessageBox.Show("Testipalkki luotu.");
        }
    }
}
