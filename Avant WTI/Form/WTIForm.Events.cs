using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using System;
using System.Windows.Forms;
using Avant.WTI.Util;

namespace Avant.WTI.Form
{
    partial class WTIForm
    {


        private bool isLoading = false;
        private Tab ActiveTab = Tab.DRIP;


        #region Tabs
        
    private void TabChanged(object sender, TabControlEventArgs e)
        {
            switch (e.TabPage.Name)
            {
                case "DripTab":
                    ActiveTab = Tab.DRIP;
                    dripAlpha = 0xFF;
                    drainAlpha = 0x7F;
                    break;
                case "DrainTab":
                    ActiveTab = Tab.DRAIN;
                    dripAlpha = 0x7F;
                    drainAlpha = 0xFF;
                    break;
                default:
                    break;
            }
            canvas.Invalidate();
        }

        #endregion


        #region Input controls value changed events

        private void Combo_pipetype_SelectedValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            PipeType pt = (PipeType)combo_pipetype.SelectedValue;

            // Make sure it really changed, so UpdateSizes will not reset the size input
            if (data.drip.pipetype == pt) return;
            data.drip.pipetype = pt;
            UpdateSizes();
            if (pt != null) Properties.Settings.Default.PreviousPipeType = pt.Name;
        }



        private void Combo_transportsystem_SelectedValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            PipingSystemType pst = (PipingSystemType)combo_transportsystem.SelectedValue;
            data.drip.transportSystemType = pst;
            if(pst != null) Properties.Settings.Default.PreviousTransportSystem = pst.Name;
        }

        private void Combo_distributionsystem_SelectedValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            PipingSystemType pst = (PipingSystemType)combo_distributionsystem.SelectedValue;
            data.drip.distributionSystemType = pst;
            if (pst != null) Properties.Settings.Default.PreviousDistributionSystem = pst.Name;
        }

        private void Combo_valvefamily_SelectedValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            FamilySymbol valve = (FamilySymbol)combo_valvefamily.SelectedValue;
            data.drip.valvefamily = valve;
            if (valve != null) Properties.Settings.Default.PreviousValveFamily = valve.Name;
        }

        private void Num_interdistance_ValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            data.drip.intermediateDistance = (int)num_interdistance.Value;
            ReloadPreview();
            Properties.Settings.Default.PreviousIntermediateDistance = (int)num_interdistance.Value;
        }

        private void Num_backwalldistance_ValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            data.drip.backwallDistance = (int)num_backwalldistance.Value;
            ReloadPreview();
            Properties.Settings.Default.PreviousBackwallDistance = (int)num_backwalldistance.Value;
        }

        private void Num_valvecolumndistance_ValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            data.drip.valvecolumnDistance = (int)num_valvecolumndistance.Value;
            ReloadPreview();
            Properties.Settings.Default.PreviousValveColumnDistance = (int)num_valvecolumndistance.Value;
        }

        private void Num_pipecolumndistance_ValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            data.drip.pipecolumnDistance = (int)num_pipecolumndistance.Value;
            ReloadPreview();
            Properties.Settings.Default.PreviousPipeColumnDistance = (int)num_pipecolumndistance.Value;
        }

        private void num_valveheight_ValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            data.drip.valveheight = (int)num_valveheight.Value;
            Properties.Settings.Default.PreviousValveHeight = (int)num_valveheight.Value;
        }

        private void Num_transportheight_ValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            data.drip.transportlineheight = (int)num_transportheight.Value;
            Properties.Settings.Default.PreviousTransportHeight = (int)num_transportheight.Value;
        }

        private void Combo_transportdiameter_SelectedValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            if (this.combo_transportdiameter.SelectedValue == null) return;
            double size = (double)combo_transportdiameter.SelectedValue;
            data.drip.transport_diameter = size;
            Properties.Settings.Default.PreviousTransportDiameter = size;

        }

        private void Num_distributionheight_ValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            data.drip.distributionlineheight = (int)num_distributionheight.Value;
            Properties.Settings.Default.PreviousDistributionHeight = (int)num_distributionheight.Value;
        }

        private void Combo_distributiondiameter_SelectedValueChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            if (this.combo_distributiondiameter.SelectedValue == null) return;
            double size = (double)combo_distributiondiameter.SelectedValue;
            data.drip.distribution_diameter = size;
            Properties.Settings.Default.PreviousDistributionDiameter = size;
        }
        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            data.convertPlaceholders = button_convertplaceholders.Checked;
            Properties.Settings.Default.PreviousDoConvertPlaceholders = button_convertplaceholders.Checked;
        }

        #endregion

        private void ToggleDrainCollector(object sender, System.EventArgs e)
        {
            Button button = (Button)sender;
            if (button.Text.Equals("Add"))
            {
                button.Text = "Remove";
                data.drain.enabled = true;
                data.drain.collectorPoint = GeomUtils.RectangleGetCenter(bounds);
            }
            else
            {
                data.drain.enabled = false;
                button.Text = "Add";
            }
            canvas.Invalidate();
        }


        #region Button click events

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            bool validRun = dripGenerator.GenerateModel();
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
