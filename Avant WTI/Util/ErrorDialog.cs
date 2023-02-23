﻿using Avant.WTI.Drip;
using Microsoft.Scripting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

namespace Avant.WTI.Util
{
    public partial class ErrorDialog : Form
    {

        private List<DripData.DripErrorMessage> errorMessages;
        public DripData.DripErrorMessage.Severity maxSeverity {
            get {
                if (errorMessages.Count == 0) return DripData.DripErrorMessage.Severity.NONE;
                return errorMessages.Select(m => m.severity).Max();
            }
        }

        private int currentIndex = 0;

        public ErrorDialog(List<DripData.DripErrorMessage> msgs)
        {
            if (msgs == null) throw new ArgumentNullException();
            InitializeComponent();
            errorMessages = msgs.Distinct().ToList();
        }

        public void ShowErrors()
        {
            List<DripData.DripErrorMessage> msgs = errorMessages;
            msgs = msgs.Distinct().ToList();
            if (msgs.Count == 0) return;

            UpdateMessage();
            ShowDialog();
        }

        private void UpdateMessage()
        {
            DripData.DripErrorMessage msg = errorMessages[currentIndex];

            string title;
            System.Drawing.Image icon;
            string listLabel = "";
            switch (msg.severity)
            {
                case DripData.DripErrorMessage.Severity.FATAL:
                    icon = SystemIcons.Error.ToBitmap();
                    title = "An error occurred!";
                    break;
                case DripData.DripErrorMessage.Severity.WARNING:
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
