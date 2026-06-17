namespace FacialRecognitionSystem.FormScreen
{
    partial class InputPlayerList
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
            panel1 = new Panel();
            txtPlayerName = new TextBox();
            label3 = new Label();
            grdPlayerList = new DataGridView();
            groupBox1 = new GroupBox();
            rdoRecently = new RadioButton();
            rdoAll = new RadioButton();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grdPlayerList).BeginInit();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(txtPlayerName);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(grdPlayerList);
            panel1.Controls.Add(groupBox1);
            panel1.Location = new Point(4, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(463, 387);
            panel1.TabIndex = 2;
            // 
            // txtPlayerName
            // 
            txtPlayerName.Location = new Point(351, 44);
            txtPlayerName.Name = "txtPlayerName";
            txtPlayerName.Size = new Size(81, 23);
            txtPlayerName.TabIndex = 22;
            txtPlayerName.TextChanged += txtPlayerName_TextChanged;
            // 
            // label3
            // 
            label3.Location = new Point(245, 45);
            label3.Name = "label3";
            label3.Size = new Size(100, 23);
            label3.TabIndex = 21;
            label3.Text = "プレーヤー名で検索";
            label3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // grdPlayerList
            // 
            grdPlayerList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grdPlayerList.Location = new Point(20, 88);
            grdPlayerList.Name = "grdPlayerList";
            grdPlayerList.Size = new Size(412, 283);
            grdPlayerList.TabIndex = 3;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(rdoRecently);
            groupBox1.Controls.Add(rdoAll);
            groupBox1.Location = new Point(20, 20);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(219, 62);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "条件";
            // 
            // rdoRecently
            // 
            rdoRecently.AutoSize = true;
            rdoRecently.Checked = true;
            rdoRecently.Location = new Point(92, 25);
            rdoRecently.Name = "rdoRecently";
            rdoRecently.Size = new Size(115, 19);
            rdoRecently.TabIndex = 1;
            rdoRecently.TabStop = true;
            rdoRecently.Text = "直近（画像あり）";
            rdoRecently.UseVisualStyleBackColor = true;
            rdoRecently.CheckedChanged += rdoRecently_CheckedChanged;
            // 
            // rdoAll
            // 
            rdoAll.AutoSize = true;
            rdoAll.Location = new Point(13, 25);
            rdoAll.Name = "rdoAll";
            rdoAll.Size = new Size(73, 19);
            rdoAll.TabIndex = 0;
            rdoAll.Text = "全件表示";
            rdoAll.UseVisualStyleBackColor = true;
            rdoAll.CheckedChanged += rdoAll_CheckedChanged;
            // 
            // InputPlayerList
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(475, 402);
            Controls.Add(panel1);
            Name = "InputPlayerList";
            Text = "PlayerList";
            Shown += InputPlayerList_Shown;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)grdPlayerList).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private DataGridView grdPlayerList;
        private GroupBox groupBox1;
        private RadioButton rdoRecently;
        private RadioButton rdoAll;
        private TextBox txtPlayerName;
        private Label label3;
    }
}