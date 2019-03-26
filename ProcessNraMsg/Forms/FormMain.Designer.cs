namespace ProcessNRAmsg
{
    partial class FormMain
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
            this.dataGridViewLog = new System.Windows.Forms.DataGridView();
            this.eventDateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StationCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EventType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TaskAddInfo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ResponceCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ResponceStr = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelControls = new System.Windows.Forms.Panel();
            this.labelLoopCounter = new System.Windows.Forms.Label();
            this.labelMsg = new System.Windows.Forms.Label();
            this.checkBoxClose = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLog)).BeginInit();
            this.panelControls.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridViewLog
            // 
            this.dataGridViewLog.AllowUserToAddRows = false;
            this.dataGridViewLog.AllowUserToDeleteRows = false;
            this.dataGridViewLog.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dataGridViewLog.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewLog.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.eventDateTime,
            this.StationCode,
            this.EventType,
            this.TaskAddInfo,
            this.ResponceCode,
            this.ResponceStr});
            this.dataGridViewLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewLog.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewLog.MultiSelect = false;
            this.dataGridViewLog.Name = "dataGridViewLog";
            this.dataGridViewLog.ReadOnly = true;
            this.dataGridViewLog.RowHeadersVisible = false;
            this.dataGridViewLog.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewLog.Size = new System.Drawing.Size(814, 429);
            this.dataGridViewLog.TabIndex = 2;
            // 
            // eventDateTime
            // 
            this.eventDateTime.HeaderText = "Дата/Час";
            this.eventDateTime.Name = "eventDateTime";
            this.eventDateTime.ReadOnly = true;
            this.eventDateTime.Width = 83;
            // 
            // StationCode
            // 
            this.StationCode.HeaderText = "ПС";
            this.StationCode.Name = "StationCode";
            this.StationCode.ReadOnly = true;
            this.StationCode.Width = 47;
            // 
            // EventType
            // 
            this.EventType.HeaderText = "Задача";
            this.EventType.Name = "EventType";
            this.EventType.ReadOnly = true;
            this.EventType.Width = 68;
            // 
            // TaskAddInfo
            // 
            this.TaskAddInfo.HeaderText = "Доп. информация";
            this.TaskAddInfo.Name = "TaskAddInfo";
            this.TaskAddInfo.ReadOnly = true;
            this.TaskAddInfo.Width = 113;
            // 
            // ResponceCode
            // 
            this.ResponceCode.HeaderText = "Код от НАП";
            this.ResponceCode.Name = "ResponceCode";
            this.ResponceCode.ReadOnly = true;
            this.ResponceCode.Width = 84;
            // 
            // ResponceStr
            // 
            this.ResponceStr.HeaderText = "Текст от НАП";
            this.ResponceStr.Name = "ResponceStr";
            this.ResponceStr.ReadOnly = true;
            this.ResponceStr.Width = 73;
            // 
            // panelControls
            // 
            this.panelControls.Controls.Add(this.labelLoopCounter);
            this.panelControls.Controls.Add(this.labelMsg);
            this.panelControls.Controls.Add(this.checkBoxClose);
            this.panelControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelControls.Location = new System.Drawing.Point(0, 429);
            this.panelControls.Name = "panelControls";
            this.panelControls.Size = new System.Drawing.Size(814, 21);
            this.panelControls.TabIndex = 3;
            // 
            // labelLoopCounter
            // 
            this.labelLoopCounter.AutoSize = true;
            this.labelLoopCounter.Dock = System.Windows.Forms.DockStyle.Right;
            this.labelLoopCounter.Location = new System.Drawing.Point(801, 0);
            this.labelLoopCounter.Name = "labelLoopCounter";
            this.labelLoopCounter.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.labelLoopCounter.Size = new System.Drawing.Size(13, 16);
            this.labelLoopCounter.TabIndex = 2;
            this.labelLoopCounter.Text = "0";
            this.labelLoopCounter.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelMsg
            // 
            this.labelMsg.AutoSize = true;
            this.labelMsg.ForeColor = System.Drawing.SystemColors.Desktop;
            this.labelMsg.Location = new System.Drawing.Point(201, 4);
            this.labelMsg.Name = "labelMsg";
            this.labelMsg.Size = new System.Drawing.Size(369, 13);
            this.labelMsg.TabIndex = 1;
            this.labelMsg.Text = "Програмата се затваря с маркиране на \"Затваряне на приложението\"";
            this.labelMsg.Visible = false;
            // 
            // checkBoxClose
            // 
            this.checkBoxClose.AutoSize = true;
            this.checkBoxClose.Location = new System.Drawing.Point(3, 3);
            this.checkBoxClose.Name = "checkBoxClose";
            this.checkBoxClose.Size = new System.Drawing.Size(171, 17);
            this.checkBoxClose.TabIndex = 0;
            this.checkBoxClose.Text = "Затваряне на приложението";
            this.checkBoxClose.UseVisualStyleBackColor = true;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(814, 450);
            this.Controls.Add(this.dataGridViewLog);
            this.Controls.Add(this.panelControls);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Връзка с НАП";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLog)).EndInit();
            this.panelControls.ResumeLayout(false);
            this.panelControls.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewLog;
        private System.Windows.Forms.DataGridViewTextBoxColumn eventDateTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn StationCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn EventType;
        private System.Windows.Forms.DataGridViewTextBoxColumn TaskAddInfo;
        private System.Windows.Forms.DataGridViewTextBoxColumn ResponceCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn ResponceStr;
        private System.Windows.Forms.Panel panelControls;
        private System.Windows.Forms.CheckBox checkBoxClose;
        private System.Windows.Forms.Label labelMsg;
        private System.Windows.Forms.Label labelLoopCounter;
    }
}

