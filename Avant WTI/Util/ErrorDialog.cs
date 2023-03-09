using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;

namespace Avant.WTI.Util
{
    public partial class ErrorDialog : System.Windows.Forms.Form
    {

        private List<WTIData.DripErrorMessage> errorMessages;
        public WTIData.DripErrorMessage.Severity maxSeverity {
            get {
                if (errorMessages.Count == 0) return WTIData.DripErrorMessage.Severity.NONE;
                return errorMessages.Select(m => m.severity).Max();
            }
        }

        private int currentIndex = 0;

        public ErrorDialog(List<WTIData.DripErrorMessage> msgs)
        {
            if (msgs == null) throw new ArgumentNullException();
            InitializeComponent();
            errorMessages = msgs.Distinct().ToList();
        }

        public void ShowErrors()
        {
            List<WTIData.DripErrorMessage> msgs = errorMessages;
            msgs = msgs.Distinct().ToList();
            if (msgs.Count == 0) return;

            UpdateMessage();
            ShowDialog();
        }

        private void UpdateMessage()
        {
            WTIData.DripErrorMessage msg = errorMessages[currentIndex];

            string title;
            System.Drawing.Image icon;
            string listLabel = "";
            switch (msg.severity)
            {
                case WTIData.DripErrorMessage.Severity.FATAL:
                    icon = SystemIcons.Error.ToBitmap();
                    title = "An error occurred!";
                    break;
                case WTIData.DripErrorMessage.Severity.WARNING:
                    icon = SystemIcons.Warning.ToBitmap();

                    title = "Warning!";
                    break;
                default:
                    icon = SystemIcons.Information.ToBitmap();
                    title = "Info";
                    break;
            }

            if (errorMessages.Count > 1)
            {
                listLabel = string.Format("({0} of {1})", currentIndex + 1, errorMessages.Count);
            }

            label_list.Text = listLabel;
            label_title.Text = title;
            text_msgbox.Text = msg.message;
            error_icon.Image = icon;

            button_next.Enabled = currentIndex < errorMessages.Count - 1;
            button_prev.Enabled = currentIndex > 0;

        }

        private void button_next_Click(object sender, EventArgs e)
        {
            if (currentIndex >= errorMessages.Count - 1) return;

            currentIndex++;
            UpdateMessage();
        }

        private void button_prev_Click(object sender, EventArgs e)
        {
            if (currentIndex <= 0) return;

            currentIndex--;
            UpdateMessage();
        }

        private void button_close_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
