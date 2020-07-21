namespace BES.SWMM.PAC.FormViews
{
    partial class FailuresForm
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgvFails = new System.Windows.Forms.DataGridView();
            this.timestepDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.typeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.messageDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bsFails = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dgvFails)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsFails)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvFails
            // 
            this.dgvFails.AllowUserToAddRows = false;
            this.dgvFails.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.dgvFails.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvFails.AutoGenerateColumns = false;
            this.dgvFails.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvFails.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvFails.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvFails.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Raised;
            this.dgvFails.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Sunken;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvFails.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvFails.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFails.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.timestepDataGridViewTextBoxColumn,
            this.typeDataGridViewTextBoxColumn,
            this.messageDataGridViewTextBoxColumn});
            this.dgvFails.DataSource = this.bsFails;
            this.dgvFails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvFails.Location = new System.Drawing.Point(0, 0);
            this.dgvFails.MultiSelect = false;
            this.dgvFails.Name = "dgvFails";
            this.dgvFails.ReadOnly = true;
            this.dgvFails.RowHeadersVisible = false;
            this.dgvFails.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgvFails.ShowCellErrors = false;
            this.dgvFails.Size = new System.Drawing.Size(547, 159);
            this.dgvFails.TabIndex = 0;
            // 
            // timestepDataGridViewTextBoxColumn
            // 
            this.timestepDataGridViewTextBoxColumn.DataPropertyName = "TS";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.timestepDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle3;
            this.timestepDataGridViewTextBoxColumn.HeaderText = "TS";
            this.timestepDataGridViewTextBoxColumn.MaxInputLength = 4;
            this.timestepDataGridViewTextBoxColumn.Name = "timestepDataGridViewTextBoxColumn";
            this.timestepDataGridViewTextBoxColumn.ReadOnly = true;
            this.timestepDataGridViewTextBoxColumn.Width = 46;
            // 
            // typeDataGridViewTextBoxColumn
            // 
            this.typeDataGridViewTextBoxColumn.DataPropertyName = "Type";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.typeDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle4;
            this.typeDataGridViewTextBoxColumn.HeaderText = "Type";
            this.typeDataGridViewTextBoxColumn.Name = "typeDataGridViewTextBoxColumn";
            this.typeDataGridViewTextBoxColumn.ReadOnly = true;
            this.typeDataGridViewTextBoxColumn.Width = 58;
            // 
            // messageDataGridViewTextBoxColumn
            // 
            this.messageDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.messageDataGridViewTextBoxColumn.DataPropertyName = "Message";
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.messageDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle5;
            this.messageDataGridViewTextBoxColumn.HeaderText = "Failure Message";
            this.messageDataGridViewTextBoxColumn.MinimumWidth = 50;
            this.messageDataGridViewTextBoxColumn.Name = "messageDataGridViewTextBoxColumn";
            this.messageDataGridViewTextBoxColumn.ReadOnly = true;
            this.messageDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // bsFails
            // 
            this.bsFails.DataSource = typeof(BES.SWMM.PAC.Failures);
            // 
            // FailuresForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(547, 159);
            this.Controls.Add(this.dgvFails);
            this.Name = "FailuresForm";
            this.Text = "Failures";
            ((System.ComponentModel.ISupportInitialize)(this.dgvFails)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsFails)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvFails;
        private System.Windows.Forms.BindingSource bsFails;
        private System.Windows.Forms.DataGridViewTextBoxColumn timestepDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn typeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn messageDataGridViewTextBoxColumn;
    }
}