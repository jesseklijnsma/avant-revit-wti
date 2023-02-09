using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avant.WTI.Drip.Form
{
    partial class WTIForm
    {

        // Value Event Listeners

        private void Combo_pipetype_SelectedIndexChanged(object sender, EventArgs e)
        {
            PipeType pt = (PipeType)combo_pipetype.SelectedValue;

            if (this.data.pipetype == pt) return;
            this.data.pipetype = pt;
            UpdateSizes();
        }

        private void Combo_transportsystem_SelectedIndexChanged(object sender, EventArgs e)
        {
            PipingSystemType pst = (PipingSystemType)combo_transportsystem.SelectedValue;
            this.data.transportSystemType = pst;
        }

        private void Combo_distributionsystem_SelectedIndexChanged(object sender, EventArgs e)
        {
            PipingSystemType pst = (PipingSystemType)combo_distributionsystem.SelectedValue;
            this.data.distributionSystemType = pst;
        }

        private void Combo_valvefamily_SelectedIndexChanged(object sender, EventArgs e)
        {
            FamilySymbol valve = (FamilySymbol)combo_valvefamily.SelectedValue;
            this.data.valvefamily = valve;
        }

        private void Num_interdistance_ValueChanged(object sender, EventArgs e)
        {
            this.data.intermediateDistance = (int)num_interdistance.Value;
            ReloadPreview();
        }

        private void Num_backwalldistance_ValueChanged(object sender, EventArgs e)
        {
            this.data.backwallDistance = (int)num_backwalldistance.Value;
            ReloadPreview();
        }

        private void Num_valvecolumndistance_ValueChanged(object sender, EventArgs e)
        {
            this.data.valvecolumnDistance = (int)num_valvecolumndistance.Value;
            ReloadPreview();
        }

        private void Num_pipecolumndistance_ValueChanged(object sender, EventArgs e)
        {
            this.data.pipecolumnDistance = (int)num_pipecolumndistance.Value;
            ReloadPreview();
        }

        private void Num_transportheight_ValueChanged(object sender, EventArgs e)
        {
            this.data.transportlineheight = (int)num_transportheight.Value;
        }

        private void Combo_transportdiameter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.combo_transportdiameter.SelectedValue == null) return;
            double size = (double)combo_transportdiameter.SelectedValue;
            this.data.transport_diameter = size;
        }

        private void Num_distributionheight_ValueChanged(object sender, EventArgs e)
        {
            this.data.distributionlineheight = (int)num_distributionheight.Value;
        }

        private void Combo_distributiondiameter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.combo_distributiondiameter.SelectedValue == null) return;
            double size = (double)combo_distributiondiameter.SelectedValue;
            this.data.distribution_diameter = size;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            dripGenerator.GenerateDrip();
            this.Close();
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.data.convertPlaceholders = button_convertplaceholders.Checked;
        }

        private void selectButton_Click(object sender, EventArgs e)
        {
            SelectSourceLines();
        }
    }
}
