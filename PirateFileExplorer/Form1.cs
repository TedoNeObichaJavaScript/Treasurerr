using System;
using System.IO;        
using System.Windows.Forms;

namespace PirateFileExplorer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent(); // Initializes UI components
            LoadDrives(); // Calls the function to load drives into TreeView
        }

        private void LoadDrives()
        {
            treeView1.Nodes.Clear(); // Clears TreeView before adding drives

            foreach (var drive in DriveInfo.GetDrives()) // Get all drives
            {
                // Create a TreeNode for each drive (C:, D:, etc.)
                TreeNode node = new TreeNode(drive.Name) { Tag = drive.Name };

                // Add a dummy "Loading..." node (to expand later)
                node.Nodes.Add("Loading...");

                // Add drive node to TreeView
                treeView1.Nodes.Add(node);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            e.Node.Nodes.Clear();
            string path = e.Node.Tag.ToString();

            try
            {
                foreach (string dir in Directory.GetDirectories(path))
                {
                    TreeNode node = new TreeNode(Path.GetFileName(dir)) { Tag = dir };
                    node.Nodes.Add("Loading...");
                    e.Node.Nodes.Add(node);
                }
            }
            catch { }
        }
    }
}
