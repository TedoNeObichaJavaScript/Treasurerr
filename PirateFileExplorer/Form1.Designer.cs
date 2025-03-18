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
            btnDecrypt = new Button();
            btnRename = new Button();
            btnEncrypt = new Button();
            panel2 = new Panel();
            panel3 = new Panel();
            btnRedirect = new Button();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // treeView1
            // 
            treeView1.BackColor = Color.BurlyWood;
            treeView1.Location = new Point(203, 32);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(280, 641);
            treeView1.TabIndex = 0;
            treeView1.BeforeExpand += treeView1_BeforeExpand;
            treeView1.AfterSelect += treeView1_AfterSelect;
            // 
            // listView1
            // 
            listView1.BackColor = Color.PapayaWhip;
            listView1.BackgroundImageTiled = true;
            listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1 });
            listView1.Font = new Font("Comic Sans MS", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            listView1.Location = new Point(481, 35);
            listView1.Name = "listView1";
            listView1.Size = new Size(409, 572);
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
            txtSearch.Location = new Point(11, 5);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Search for files..";
            txtSearch.Size = new Size(1118, 24);
            txtSearch.TabIndex = 2;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // btnSearch
            // 
            btnSearch.Font = new Font("Comic Sans MS", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            btnSearch.Location = new Point(1125, 5);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(74, 24);
            btnSearch.TabIndex = 3;
            btnSearch.Text = "🔎 Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // btnDelete
            // 
            btnDelete.BackColor = Color.SandyBrown;
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.Font = new Font("Comic Sans MS", 12F, FontStyle.Italic, GraphicsUnit.Point, 204);
            btnDelete.Location = new Point(3, 60);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(194, 39);
            btnDelete.TabIndex = 4;
            btnDelete.Text = "☠️ Delete";
            btnDelete.UseVisualStyleBackColor = false;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnCopy
            // 
            btnCopy.BackColor = Color.SandyBrown;
            btnCopy.FlatAppearance.BorderSize = 0;
            btnCopy.FlatStyle = FlatStyle.Flat;
            btnCopy.Font = new Font("Comic Sans MS", 12F, FontStyle.Italic, GraphicsUnit.Point, 204);
            btnCopy.Location = new Point(3, 105);
            btnCopy.Name = "btnCopy";
            btnCopy.Size = new Size(194, 39);
            btnCopy.TabIndex = 5;
            btnCopy.Text = "📄 Copy";
            btnCopy.UseVisualStyleBackColor = false;
            btnCopy.Click += btnCopy_Click;
            // 
            // btnMove
            // 
            btnMove.BackColor = Color.SandyBrown;
            btnMove.FlatAppearance.BorderSize = 0;
            btnMove.FlatStyle = FlatStyle.Flat;
            btnMove.Font = new Font("Comic Sans MS", 12F, FontStyle.Italic, GraphicsUnit.Point, 204);
            btnMove.ForeColor = SystemColors.ActiveCaptionText;
            btnMove.Location = new Point(3, 15);
            btnMove.Name = "btnMove";
            btnMove.Size = new Size(194, 39);
            btnMove.TabIndex = 6;
            btnMove.Text = "📦 Move";
            btnMove.UseVisualStyleBackColor = false;
            btnMove.Click += btnMove_Click;
            // 
            // btnZip
            // 
            btnZip.BackColor = Color.SandyBrown;
            btnZip.FlatAppearance.BorderSize = 0;
            btnZip.FlatStyle = FlatStyle.Flat;
            btnZip.Font = new Font("Comic Sans MS", 12F, FontStyle.Italic, GraphicsUnit.Point, 204);
            btnZip.Location = new Point(3, 195);
            btnZip.Name = "btnZip";
            btnZip.Size = new Size(194, 39);
            btnZip.TabIndex = 7;
            btnZip.Text = "🗜️ Zip";
            btnZip.UseVisualStyleBackColor = false;
            btnZip.Click += btnZip_Click;
            // 
            // btnUnzip
            // 
            btnUnzip.BackColor = Color.SandyBrown;
            btnUnzip.FlatAppearance.BorderSize = 0;
            btnUnzip.FlatStyle = FlatStyle.Flat;
            btnUnzip.Font = new Font("Comic Sans MS", 12F, FontStyle.Italic, GraphicsUnit.Point, 204);
            btnUnzip.Location = new Point(3, 240);
            btnUnzip.Name = "btnUnzip";
            btnUnzip.Size = new Size(194, 39);
            btnUnzip.TabIndex = 8;
            btnUnzip.Text = "📂 Unzip";
            btnUnzip.UseVisualStyleBackColor = false;
            btnUnzip.Click += btnUnzip_Click;
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(txtSearch);
            panel1.Controls.Add(btnSearch);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1258, 35);
            panel1.TabIndex = 9;
            panel1.Paint += panel1_Paint;
            // 
            // btnDecrypt
            // 
            btnDecrypt.BackColor = Color.SandyBrown;
            btnDecrypt.FlatAppearance.BorderSize = 0;
            btnDecrypt.FlatStyle = FlatStyle.Flat;
            btnDecrypt.Font = new Font("Comic Sans MS", 12F, FontStyle.Italic, GraphicsUnit.Point, 204);
            btnDecrypt.Location = new Point(3, 330);
            btnDecrypt.Name = "btnDecrypt";
            btnDecrypt.Size = new Size(194, 39);
            btnDecrypt.TabIndex = 13;
            btnDecrypt.Text = "🗝 Decrypt";
            btnDecrypt.UseVisualStyleBackColor = false;
            btnDecrypt.Click += btnDecrypt_Click;
            // 
            // btnRename
            // 
            btnRename.BackColor = Color.SandyBrown;
            btnRename.FlatAppearance.BorderSize = 0;
            btnRename.FlatStyle = FlatStyle.Flat;
            btnRename.Font = new Font("Comic Sans MS", 12F, FontStyle.Italic, GraphicsUnit.Point, 204);
            btnRename.Location = new Point(3, 150);
            btnRename.Name = "btnRename";
            btnRename.Size = new Size(194, 39);
            btnRename.TabIndex = 12;
            btnRename.Text = "✏️ Rename";
            btnRename.UseVisualStyleBackColor = false;
            btnRename.Click += btnRename_Click;
            // 
            // btnEncrypt
            // 
            btnEncrypt.BackColor = Color.SandyBrown;
            btnEncrypt.FlatAppearance.BorderSize = 0;
            btnEncrypt.FlatStyle = FlatStyle.Flat;
            btnEncrypt.Font = new Font("Comic Sans MS", 12F, FontStyle.Italic, GraphicsUnit.Point, 204);
            btnEncrypt.Location = new Point(3, 285);
            btnEncrypt.Name = "btnEncrypt";
            btnEncrypt.Size = new Size(194, 39);
            btnEncrypt.TabIndex = 10;
            btnEncrypt.Text = "🔒 Encrypt";
            btnEncrypt.UseVisualStyleBackColor = false;
            btnEncrypt.Click += btnEncrypt_Click;
            // 
            // panel2
            // 
            panel2.BackColor = Color.Chocolate;
            panel2.Controls.Add(btnRedirect);
            panel2.Controls.Add(panel3);
            panel2.Controls.Add(btnDecrypt);
            panel2.Controls.Add(btnMove);
            panel2.Controls.Add(btnRename);
            panel2.Controls.Add(btnCopy);
            panel2.Controls.Add(btnEncrypt);
            panel2.Controls.Add(btnDelete);
            panel2.Controls.Add(btnZip);
            panel2.Controls.Add(btnUnzip);
            panel2.Dock = DockStyle.Left;
            panel2.Location = new Point(0, 35);
            panel2.Name = "panel2";
            panel2.Size = new Size(205, 641);
            panel2.TabIndex = 10;
            // 
            // panel3
            // 
            panel3.BackgroundImage = (Image)resources.GetObject("panel3.BackgroundImage");
            panel3.BackgroundImageLayout = ImageLayout.Stretch;
            panel3.Location = new Point(3, 419);
            panel3.Name = "panel3";
            panel3.Size = new Size(194, 174);
            panel3.TabIndex = 11;
            panel3.Paint += panel3_Paint;
            // 
            // btnRedirect
            // 
            btnRedirect.BackColor = Color.SandyBrown;
            btnRedirect.FlatAppearance.BorderSize = 0;
            btnRedirect.FlatStyle = FlatStyle.Flat;
            btnRedirect.Font = new Font("Comic Sans MS", 12F, FontStyle.Italic, GraphicsUnit.Point, 204);
            btnRedirect.Location = new Point(3, 599);
            btnRedirect.Name = "btnRedirect";
            btnRedirect.Size = new Size(194, 39);
            btnRedirect.TabIndex = 11;
            btnRedirect.Text = "🌐 Treasurerr";
            btnRedirect.UseVisualStyleBackColor = false;
            btnRedirect.Click += btnRedirect_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(92, 64, 51);
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            ClientSize = new Size(1258, 676);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Controls.Add(listView1);
            Controls.Add(treeView1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "Treasurer";
            Load += Form1_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
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
        private Button btnEncrypt;
        private Button btnRename;
        private Button btnDecrypt;
        private Panel panel2;
        private Panel panel3;
        private Button btnRedirect;
    }
}
