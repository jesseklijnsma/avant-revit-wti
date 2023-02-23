using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Visual;
using System.Configuration;

namespace Avant.WTI.Drip.Form
{
    partial class WTIForm
    {


        private bool isLoading = false;

        #region Input controls value changed events

        private void Combo_pipetype_SelectedValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            PipeType pt = (PipeType)combo_pipetype.SelectedValue;

            // Make sure it really changed, so UpdateSizes will not reset the size input
            if (this.data.pipetype == pt) return;
            this.data.pipetype = pt;
            UpdateSizes();
            if (pt != null) Properties.Settings.Default.PreviousPipeType = pt.Name;
        }



        private void Combo_transportsystem_SelectedValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            PipingSystemType pst = (PipingSystemType)combo_transportsystem.SelectedValue;
            this.data.transportSystemType = pst;
            if(pst != null) Properties.Settings.Default.PreviousTransportSystem = pst.Name;
        }

        private void Combo_distributionsystem_SelectedValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            PipingSystemType pst = (PipingSystemType)combo_distributionsystem.SelectedValue;
            this.data.distributionSystemType = pst;
            if (pst != null) Properties.Settings.Default.PreviousDistributionSystem = pst.Name;
        }

        private void Combo_valvefamily_SelectedValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            FamilySymbol valve = (FamilySymbol)combo_valvefamily.SelectedValue;
            this.data.valvefamily = valve;
            if (valve != null) Properties.Settings.Default.PreviousValveFamily = valve.Name;
        }

        private void Num_interdistance_ValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            this.data.intermediateDistance = (int)num_interdistance.Value;
            ReloadPreview();
            Properties.Settings.Default.PreviousIntermediateDistance = (int)num_interdistance.Value;
        }

        private void Num_backwalldistance_ValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            this.data.backwallDistance = (int)num_backwalldistance.Value;
            ReloadPreview();
            Properties.Settings.Default.PreviousBackwallDistance = (int)num_backwalldistance.Value;
        }

        private void Num_valvecolumndistance_ValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            this.data.valvecolumnDistance = (int)num_valvecolumndistance.Value;
            ReloadPreview();
            Properties.Settings.Default.PreviousValveColumnDistance = (int)num_valvecolumndistance.Value;
        }

        private void Num_pipecolumndistance_ValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            this.data.pipecolumnDistance = (int)num_pipecolumndistance.Value;
            ReloadPreview();
            Properties.Settings.Default.PreviousPipeColumnDistance = (int)num_pipecolumndistance.Value;
        }

        private void num_valveheight_ValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            this.data.valveheight = (int)num_valveheight.Value;
            Properties.Settings.Default.PreviousValveHeight = (int)num_valveheight.Value;
        }

        private void Num_transportheight_ValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            this.data.transportlineheight = (int)num_transportheight.Value;
            Properties.Settings.Default.PreviousTransportHeight = (int)num_transportheight.Value;
        }

        private void Combo_transportdiameter_SelectedValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            if (this.combo_transportdiameter.SelectedValue == null) return;
            double size = (double)combo_transportdiameter.SelectedValue;
            this.data.transport_diameter = size;
            Properties.Settings.Default.PreviousTransportDiameter = size;

        }

        private void Num_distributionheight_ValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            this.data.distributionlineheight = (int)num_distributionheight.Value;
            Properties.Settings.Default.PreviousDistributionHeight = (int)num_distributionheight.Value;
        }

        private void Combo_distributiondiameter_SelectedValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            if (this.combo_distributiondiameter.SelectedValue == null) return;
            double size = (double)combo_distributiondiameter.SelectedValue;
            this.data.distribution_diameter = size;
            Properties.Settings.Default.PreviousDistributionDiameter = size;
        }
        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            this.data.convertPlaceholders = button_convertplaceholders.Checked;
            Properties.Settings.Default.PreviousDoConvertPlaceholders = button_convertplaceholders.Checked;
        }

        #endregion

        #region Button click events

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            bool validRun = dripGenerator.GenerateDrip();
            if (!validRun) return;


            Properties.Settings.Default.Save();

            this.Close();
        }

        private void SelectButton_Click(object sender, EventArgs e)
        {
            SelectSourceLines();
        }

        #endregion
    }
}
