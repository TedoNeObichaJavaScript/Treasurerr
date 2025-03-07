namespace PirateFileExplorer
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            treeView1 = new TreeView();
            listView1 = new ListView();
            columnHeader1 = new ColumnHeader();
            txtSearch = new TextBox();
            btnSearch = new Button();
            btnDelete = new Button();
            btnCopy = new Button();
            btnMove = new Button();
            btnZip = new Button();
            btnUnzip = new Button();
            panel1 = new Panel();
            btnSelectFile = new Button();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // treeView1
            // 
            treeView1.BackColor = Color.BurlyWood;
            treeView1.Location = new Point(0, 33);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(361, 592);
            treeView1.TabIndex = 0;
            treeView1.BeforeExpand += treeView1_BeforeExpand;
            treeView1.AfterSelect += treeView1_AfterSelect;
            // 
            // listView1
            // 
            listView1.BackColor = Color.Bisque;
            listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1 });
            listView1.Font = new Font("Comic Sans MS", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            listView1.GridLines = true;
            listView1.Location = new Point(385, 41);
            listView1.Name = "listView1";
            listView1.Size = new Size(324, 572);
            listView1.TabIndex = 1;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Treasurer";
            columnHeader1.Width = 250;
            // 
            // txtSearch
            // 
            txtSearch.Font = new Font("Comic Sans MS", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            txtSearch.Location = new Point(830, 5);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Search for files..";
            txtSearch.Size = new Size(346, 24);
            txtSearch.TabIndex = 2;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // btnSearch
            // 
            btnSearch.Font = new Font("Comic Sans MS", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            btnSearch.Location = new Point(1172, 5);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(74, 24);
            btnSearch.TabIndex = 3;
            btnSearch.Text = "🔎 Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // btnDelete
            // 
            btnDelete.BackColor = Color.Transparent;
            btnDelete.Font = new Font("Comic Sans MS", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            btnDelete.Location = new Point(242, 4);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(75, 23);
            btnDelete.TabIndex = 4;
            btnDelete.Text = "☠️ Delete";
            btnDelete.UseVisualStyleBackColor = false;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnCopy
            // 
            btnCopy.Font = new Font("Comic Sans MS", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            btnCopy.Location = new Point(161, 4);
            btnCopy.Name = "btnCopy";
            btnCopy.Size = new Size(75, 23);
            btnCopy.TabIndex = 5;
            btnCopy.Text = "📄 Copy";
            btnCopy.UseVisualStyleBackColor = true;
            btnCopy.Click += btnCopy_Click;
            // 
            // btnMove
            // 
            btnMove.Font = new Font("Comic Sans MS", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            btnMove.Location = new Point(80, 4);
            btnMove.Name = "btnMove";
            btnMove.Size = new Size(75, 23);
            btnMove.TabIndex = 6;
            btnMove.Text = "📦 Move";
            btnMove.UseVisualStyleBackColor = true;
            btnMove.Click += btnMove_Click;
            // 
            // btnZip
            // 
            btnZip.Font = new Font("Comic Sans MS", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            btnZip.Location = new Point(323, 4);
            btnZip.Name = "btnZip";
            btnZip.Size = new Size(75, 23);
            btnZip.TabIndex = 7;
            btnZip.Text = "🗜️ Zip";
            btnZip.UseVisualStyleBackColor = true;
            btnZip.Click += btnZip_Click;
            // 
            // btnUnzip
            // 
            btnUnzip.Font = new Font("Comic Sans MS", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            btnUnzip.Location = new Point(404, 4);
            btnUnzip.Name = "btnUnzip";
            btnUnzip.Size = new Size(75, 23);
            btnUnzip.TabIndex = 8;
            btnUnzip.Text = "📂 Unzip";
            btnUnzip.UseVisualStyleBackColor = true;
            btnUnzip.Click += btnUnzip_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnSelectFile);
            panel1.Controls.Add(txtSearch);
            panel1.Controls.Add(btnUnzip);
            panel1.Controls.Add(btnSearch);
            panel1.Controls.Add(btnMove);
            panel1.Controls.Add(btnZip);
            panel1.Controls.Add(btnCopy);
            panel1.Controls.Add(btnDelete);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1258, 35);
            panel1.TabIndex = 9;
            panel1.Paint += panel1_Paint;
            // 
            // btnSelectFile
            // 
            btnSelectFile.Location = new Point(3, 4);
            btnSelectFile.Name = "btnSelectFile";
            btnSelectFile.Size = new Size(75, 23);
            btnSelectFile.TabIndex = 9;
            btnSelectFile.Text = "Select File";
            btnSelectFile.UseVisualStyleBackColor = true;
            btnSelectFile.Click += btnSelectFile_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(92, 64, 51);
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1258, 676);
            Controls.Add(panel1);
            Controls.Add(listView1);
            Controls.Add(treeView1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "Form1";
            Text = "Treasurer";
            Load += Form1_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TreeView treeView1;
        private ListView listView1;
        private ColumnHeader columnHeader1;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnDelete;
        private Button btnCopy;
        private Button btnMove;
        private Button btnZip;
        private Button btnUnzip;
        private Panel panel1;
        private Button btnSelectFile;
    }
}
