using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PirateFileExplorer
{
    public partial class Form1 : Form
    {
        // ImageList for file icons
        private ImageList imageList1;
        // Cache to store file extension to ImageList index mapping
        private Dictionary<string, int> extIconCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public Form1()
        {
            InitializeComponent();
            InitializeImageList();
            LoadDrives();
        }

        // ----------------------------------------------------------------
        // INITIALIZATION METHODS
        // ----------------------------------------------------------------
        private void InitializeImageList()
        {
            imageList1 = new ImageList();
            imageList1.ImageSize = new Size(32, 32);
            listView1.SmallImageList = imageList1;
        }

        // ----------------------------------------------------------------
        // 1. LOAD DRIVES + TREEVIEW FOLDER SIZE
        // ----------------------------------------------------------------
        private void LoadDrives()
        {
            treeView1.Nodes.Clear();

            foreach (var drive in DriveInfo.GetDrives())
            {
                // Create a node for each drive (e.g., "C:\")
                TreeNode node = new TreeNode(drive.Name) { Tag = drive.Name };
                node.Nodes.Add("Loading...");
                treeView1.Nodes.Add(node);
            }
        }

        private async void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            e.Node.Nodes.Clear();
            string path = e.Node.Tag.ToString();

            try
            {
                string[] dirs = Directory.GetDirectories(path);
                foreach (string dir in dirs)
                {
                    // Create a node with temporary text
                    TreeNode node = new TreeNode($"{Path.GetFileName(dir)} (Calculating...)") { Tag = dir };
                    node.Nodes.Add("Loading...");  // dummy node for further expansion
                    e.Node.Nodes.Add(node);

                    // Calculate folder size asynchronously to keep UI responsive
                    long folderSize = await Task.Run(() => GetFolderSize(dir));
                    string sizeText = FormatBytes(folderSize);
                    node.Text = $"{Path.GetFileName(dir)} ({sizeText})";
                }
            }
            catch
            {
                // Optionally handle errors (e.g., access denied)
            }
        }

        // ----------------------------------------------------------------
        // 2. LOAD FILES IN LISTVIEW WHEN A FOLDER IS SELECTED
        // ----------------------------------------------------------------
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string selectedPath = e.Node.Tag.ToString();
            if (Directory.Exists(selectedPath))
            {
                LoadFiles(selectedPath);
            }
        }

        private void LoadFiles(string path)
        {
            listView1.Items.Clear();
            extIconCache.Clear(); // Optionally clear cache when loading a new folder

            try
            {
                long totalSize = 0;
                string[] files = Directory.GetFiles(path);

                foreach (var file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    totalSize += fi.Length;

                    ListViewItem item = new ListViewItem(Path.GetFileName(file));
                    item.Tag = file;
                    item.ImageIndex = GetFileIconIndex(file);
                    listView1.Items.Add(item);
                }
                // Optionally, update status (e.g., a TextBox or StatusStrip) with the folder path and file stats.
                // Example: txtCurrentPath.Text = path;
                // Example: toolStripStatusLabel1.Text = $"Files: {files.Length}, Total Size: {FormatBytes(totalSize)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading files: {ex.Message}");
            }
        }

        // ----------------------------------------------------------------
        // 3. SEARCH FILES (with closest match filtering)
        // ----------------------------------------------------------------
        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                MessageBox.Show("Please select a folder in the TreeView first.");
                return;
            }

            string currentPath = treeView1.SelectedNode.Tag.ToString();
            string query = txtSearch.Text.Trim().ToLower();

            if (!Directory.Exists(currentPath))
            {
                MessageBox.Show("Current folder does not exist.");
                return;
            }

            listView1.Items.Clear();
            extIconCache.Clear();

            try
            {
                // Get all files (non-recursive search)
                var files = Directory.GetFiles(currentPath, "*", SearchOption.TopDirectoryOnly);
                var matchedFiles = new List<Tuple<string, int>>();

                // Calculate similarity using Levenshtein distance for each file name that contains the query
                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file).ToLower();
                    if (fileName.Contains(query))
                    {
                        int distance = LevenshteinDistance(fileName, query);
                        matchedFiles.Add(new Tuple<string, int>(file, distance));
                    }
                }

                // Order files by distance (closer match = lower distance)
                var ordered = matchedFiles.OrderBy(t => t.Item2).ToList();

                foreach (var tuple in ordered)
                {
                    string file = tuple.Item1;
                    ListViewItem item = new ListViewItem(Path.GetFileName(file));
                    item.Tag = file;
                    item.ImageIndex = GetFileIconIndex(file);
                    listView1.Items.Add(item);
                }

                if (ordered.Count == 0)
                {
                    MessageBox.Show("No files found matching your search.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during search: {ex.Message}");
            }
        }

        // Helper method to compute Levenshtein distance
        private int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
                return string.IsNullOrEmpty(t) ? 0 : t.Length;
            if (string.IsNullOrEmpty(t))
                return s.Length;

            int[,] d = new int[s.Length + 1, t.Length + 1];

            for (int i = 0; i <= s.Length; i++)
                d[i, 0] = i;
            for (int j = 0; j <= t.Length; j++)
                d[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            return d[s.Length, t.Length];
        }

        // ----------------------------------------------------------------
        // 4. FILE OPERATIONS (DELETE, COPY, MOVE, ZIP, UNZIP)
        // ----------------------------------------------------------------
        private List<string> GetSelectedFiles()
        {
            List<string> selectedFiles = new List<string>();
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                selectedFiles.Add(item.Tag.ToString());
            }
            return selectedFiles;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var filesToDelete = GetSelectedFiles();
            if (filesToDelete.Count == 0) return;

            DialogResult dialog = MessageBox.Show("Are you sure you want to delete these files?",
                                                  "Confirm Delete",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Warning);

            if (dialog == DialogResult.Yes)
            {
                foreach (var file in filesToDelete)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting {file}: {ex.Message}");
                    }
                }
                if (treeView1.SelectedNode != null)
                    LoadFiles(treeView1.SelectedNode.Tag.ToString());
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            var filesToCopy = GetSelectedFiles();
            if (filesToCopy.Count == 0) return;

            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string destFolder = fbd.SelectedPath;
                    foreach (var file in filesToCopy)
                    {
                        try
                        {
                            string fileName = Path.GetFileName(file);
                            string destPath = Path.Combine(destFolder, fileName);
                            File.Copy(file, destPath, true);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error copying {file}: {ex.Message}");
                        }
                    }
                    MessageBox.Show("Files copied successfully!");
                }
            }
        }

        private void btnMove_Click(object sender, EventArgs e)
        {
            var filesToMove = GetSelectedFiles();
            if (filesToMove.Count == 0) return;

            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string destFolder = fbd.SelectedPath;
                    foreach (var file in filesToMove)
                    {
                        try
                        {
                            string fileName = Path.GetFileName(file);
                            string destPath = Path.Combine(destFolder, fileName);
                            File.Move(file, destPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error moving {file}: {ex.Message}");
                        }
                    }
                    MessageBox.Show("Files moved successfully!");
                    if (treeView1.SelectedNode != null)
                        LoadFiles(treeView1.SelectedNode.Tag.ToString());
                }
            }
        }

        private void btnZip_Click(object sender, EventArgs e)
        {
            var filesToZip = GetSelectedFiles();
            if (filesToZip.Count == 0) return;

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Zip files|*.zip";
                sfd.Title = "Save Zip File";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string zipPath = sfd.FileName;
                    using (FileStream zipStream = new FileStream(zipPath, FileMode.Create))
                    using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                    {
                        foreach (var file in filesToZip)
                        {
                            try
                            {
                                archive.CreateEntryFromFile(file, Path.GetFileName(file));
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error zipping {file}: {ex.Message}");
                            }
                        }
                    }
                    MessageBox.Show("Files zipped successfully!");
                }
            }
        }

        private void btnUnzip_Click(object sender, EventArgs e)
        {
            var filesToUnzip = GetSelectedFiles();
            if (filesToUnzip.Count == 0) return;

            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string extractPath = fbd.SelectedPath;
                    foreach (var zipFile in filesToUnzip)
                    {
                        if (Path.GetExtension(zipFile).ToLower() != ".zip")
                        {
                            MessageBox.Show($"Not a zip file: {zipFile}");
                            continue;
                        }
                        try
                        {
                            ZipFile.ExtractToDirectory(zipFile, extractPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error unzipping {zipFile}: {ex.Message}");
                        }
                    }
                    MessageBox.Show("Files unzipped successfully!");
                }
            }
        }

        // ----------------------------------------------------------------
        // 5. CONTEXT MENU: OPEN, PROPERTIES, RENAME
        // ----------------------------------------------------------------
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string filePath = listView1.SelectedItems[0].Tag.ToString();
                if (File.Exists(filePath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
            }
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string filePath = listView1.SelectedItems[0].Tag.ToString();
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = filePath,
                    Verb = "properties",
                    UseShellExecute = true
                });
            }
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string oldPath = listView1.SelectedItems[0].Tag.ToString();
                string oldName = Path.GetFileName(oldPath);

                string newName = Microsoft.VisualBasic.Interaction.InputBox("Enter new file name:", "Rename", oldName);

                if (!string.IsNullOrWhiteSpace(newName) && newName != oldName)
                {
                    string newPath = Path.Combine(Path.GetDirectoryName(oldPath), newName);
                    try
                    {
                        File.Move(oldPath, newPath);
                        if (treeView1.SelectedNode != null)
                            LoadFiles(treeView1.SelectedNode.Tag.ToString());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error renaming file: {ex.Message}");
                    }
                }
            }
        }

        // ----------------------------------------------------------------
        // 6. DRAG & DROP FROM LISTVIEW TO TREEVIEW
        // ----------------------------------------------------------------
        private void listView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            List<string> files = new List<string>();
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                files.Add(item.Tag.ToString());
            }
            if (files.Count > 0)
            {
                DoDragDrop(new DataObject(DataFormats.FileDrop, files.ToArray()), DragDropEffects.Move);
            }
        }

        private void treeView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            TreeView tv = (TreeView)sender;
            Point pt = tv.PointToClient(new Point(e.X, e.Y));
            TreeNode targetNode = tv.GetNodeAt(pt);

            if (targetNode == null) return;

            string targetPath = targetNode.Tag.ToString();

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    try
                    {
                        string fileName = Path.GetFileName(file);
                        string destPath = Path.Combine(targetPath, fileName);
                        File.Move(file, destPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error moving file: {ex.Message}");
                    }
                }
            }
            // Optionally refresh the views
        }

        // ----------------------------------------------------------------
        // 7. HELPER METHODS (FOLDER SIZE, FORMAT BYTES, GET FILE ICON)
        // ----------------------------------------------------------------
        private long GetFolderSize(string folderPath)
        {
            long size = 0;
            try
            {
                foreach (string file in Directory.GetFiles(folderPath))
                {
                    try
                    {
                        FileInfo info = new FileInfo(file);
                        size += info.Length;
                    }
                    catch { }
                }
                foreach (string dir in Directory.GetDirectories(folderPath))
                {
                    size += GetFolderSize(dir);
                }
            }
            catch { }
            return size;
        }

        private string FormatBytes(long bytes)
        {
            if (bytes >= 1073741824)
                return $"{bytes / 1073741824.0:F2} GB";
            if (bytes >= 1048576)
                return $"{bytes / 1048576.0:F2} MB";
            if (bytes >= 1024)
                return $"{bytes / 1024.0:F2} KB";
            return $"{bytes} bytes";
        }

        // Retrieves the icon index for a file, using caching to avoid duplicate extractions.
        private int GetFileIconIndex(string filePath)
        {
            string ext = Path.GetExtension(filePath);
            if (extIconCache.ContainsKey(ext))
            {
                return extIconCache[ext];
            }
            else
            {
                try
                {
                    Icon icon = Icon.ExtractAssociatedIcon(filePath);
                    if (icon != null)
                    {
                        Bitmap bmp = icon.ToBitmap();
                        imageList1.Images.Add(bmp);
                        int index = imageList1.Images.Count - 1;
                        extIconCache[ext] = index;
                        return index;
                    }
                }
                catch { }
                return -1; // Return -1 if extraction fails.
            }
        }

        // ----------------------------------------------------------------
        // FORM EVENTS (Placeholders for additional handling)
        // ----------------------------------------------------------------
        private void Form1_Load(object sender, EventArgs e) { }
        private void txtSearch_TextChanged(object sender, EventArgs e) { }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
    }
}
