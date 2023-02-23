using System.Drawing;

namespace Avant.WTI.Util
{
    partial class ErrorDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ErrorDialog));
            this.text_msgbox = new System.Windows.Forms.TextBox();
            this.label_title = new System.Windows.Forms.Label();
            this.button_prev = new System.Windows.Forms.Button();
            this.button_next = new System.Windows.Forms.Button();
            this.button_close = new System.Windows.Forms.Button();
            this.label_list = new System.Windows.Forms.Label();
            this.error_icon = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.error_icon)).BeginInit();
            this.SuspendLayout();
            // 
            // text_msgbox
            // 
            this.text_msgbox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.text_msgbox.BackColor = System.Drawing.SystemColors.Window;
            this.text_msgbox.Location = new System.Drawing.Point(20, 56);
            this.text_msgbox.Margin = new System.Windows.Forms.Padding(5, 10, 5, 5);
            this.text_msgbox.Multiline = true;
            this.text_msgbox.Name = "text_msgbox";
            this.text_msgbox.ReadOnly = true;
            this.text_msgbox.ShortcutsEnabled = false;
            this.text_msgbox.Size = new System.Drawing.Size(468, 171);
            this.text_msgbox.TabIndex = 3;
            this.text_msgbox.TabStop = false;
            this.text_msgbox.Text = "Dummy warning message";
            // 
            // label_title
            // 
            this.label_title.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_title.Location = new System.Drawing.Point(60, 14);
            this.label_title.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label_title.Name = "label_title";
            this.label_title.Size = new System.Drawing.Size(572, 32);
            this.label_title.TabIndex = 4;
            this.label_title.Text = "Dialog Title";
            this.label_title.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // button_prev
            // 
            this.button_prev.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_prev.Location = new System.Drawing.Point(18, 244);
            this.button_prev.Margin = new System.Windows.Forms.Padding(5);
            this.button_prev.Name = "button_prev";
            this.button_prev.Size = new System.Drawing.Size(150, 35);
            this.button_prev.TabIndex = 2;
            this.button_prev.Text = "Previous";
            this.button_prev.UseVisualStyleBackColor = true;
            this.button_prev.Click += new System.EventHandler(this.button_prev_Click);
            // 
            // button_next
            // 
            this.button_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_next.Location = new System.Drawing.Point(178, 244);
            this.button_next.Margin = new System.Windows.Forms.Padding(5);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(150, 35);
            this.button_next.TabIndex = 0;
            this.button_next.Text = "Next";
            this.button_next.UseVisualStyleBackColor = true;
            this.button_next.Click += new System.EventHandler(this.button_next_Click);
            // 
            // button_close
            // 
            this.button_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_close.Location = new System.Drawing.Point(338, 244);
            this.button_close.Margin = new System.Windows.Forms.Padding(5);
            this.button_close.Name = "button_close";
            this.button_close.Size = new System.Drawing.Size(150, 35);
            this.button_close.TabIndex = 1;
            this.button_close.Text = "Close";
            this.button_close.UseVisualStyleBackColor = true;
            this.button_close.Click += new System.EventHandler(this.button_close_Click);
            // 
            // label_list
            // 
            this.label_list.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label_list.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_list.Location = new System.Drawing.Point(401, 14);
            this.label_list.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label_list.Name = "label_list";
            this.label_list.Size = new System.Drawing.Size(87, 32);
            this.label_list.TabIndex = 7;
            this.label_list.Text = "(1 of 1)";
            this.label_list.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // error_icon
            // 
            this.error_icon.BackColor = System.Drawing.Color.Transparent;
            this.error_icon.Image = ((System.Drawing.Image)(resources.GetObject("error_icon.Image")));
            this.error_icon.Location = new System.Drawing.Point(20, 14);
            this.error_icon.Name = "error_icon";
            this.error_icon.Size = new System.Drawing.Size(32, 32);
            this.error_icon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.error_icon.TabIndex = 8;
            this.error_icon.TabStop = false;
            // 
            // ErrorDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(511, 293);
            this.Controls.Add(this.error_icon);
            this.Controls.Add(this.label_list);
            this.Controls.Add(this.button_prev);
            this.Controls.Add(this.button_next);
            this.Controls.Add(this.label_title);
            this.Controls.Add(this.text_msgbox);
            this.Controls.Add(this.button_close);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MinimumSize = new System.Drawing.Size(529, 340);
            this.Name = "ErrorDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AvantWTI";
            ((System.ComponentModel.ISupportInitialize)(this.error_icon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox text_msgbox;
        private System.Windows.Forms.Label label_title;
        private System.Windows.Forms.Button button_prev;
        private System.Windows.Forms.Button button_next;
        private System.Windows.Forms.Button button_close;
        private System.Windows.Forms.Label label_list;
        private System.Windows.Forms.PictureBox error_icon;
    }
}