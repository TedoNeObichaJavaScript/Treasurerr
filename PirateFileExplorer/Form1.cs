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
        // Current directory tracker for navigation and search
        private string currentDirectory = "";

        public Form1()
        {
            InitializeComponent();
            InitializeImageList();
            LoadDrives();

            // Subscribe to drag-and-drop events for TreeView and ListView
            treeView1.ItemDrag += treeView1_ItemDrag;
            listView1.DragEnter += listView1_DragEnter;
            listView1.DragDrop += listView1_DragDrop;

            // Subscribe to the double-click event for ListView
            listView1.DoubleClick += listView1_DoubleClick;
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
            try
            {
                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady)
                    {
                        // Create a node for each drive (e.g., "C:\")
                        TreeNode node = new TreeNode(drive.Name) { Tag = drive.Name };
                        node.Nodes.Add("Loading...");
                        treeView1.Nodes.Add(node);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading drives: {ex.Message}");
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
                    node.Nodes.Add("Loading...");  // Dummy node for further expansion
                    e.Node.Nodes.Add(node);

                    // Calculate folder size asynchronously to keep UI responsive
                    long folderSize = await Task.Run(() => GetFolderSize(dir));
                    string sizeText = FormatBytes(folderSize);
                    node.Text = $"{Path.GetFileName(dir)} ({sizeText})";
                }
            }
            catch { }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string selectedPath = e.Node.Tag.ToString();
            if (Directory.Exists(selectedPath))
            {
                LoadFiles(selectedPath);
            }
        }

        // ----------------------------------------------------------------
        // 2. LOAD FILES AND FOLDERS IN LISTVIEW
        // ----------------------------------------------------------------
        private void LoadFiles(string path)
        {
            listView1.Items.Clear();
            extIconCache.Clear(); // Optionally clear cache when loading a new folder
            currentDirectory = path; // Update current directory

            try
            {
                // Load folders first
                foreach (var dir in Directory.GetDirectories(path))
                {
                    ListViewItem item = new ListViewItem(Path.GetFileName(dir), GetFileIconIndex(dir));
                    item.Tag = dir;
                    item.SubItems.Add("Folder");
                    listView1.Items.Add(item);
                }
                // Load files
                foreach (var file in Directory.GetFiles(path))
                {
                    FileInfo fi = new FileInfo(file);
                    ListViewItem item = new ListViewItem(Path.GetFileName(file), GetFileIconIndex(file));
                    item.Tag = file;
                    item.SubItems.Add(FormatBytes(fi.Length));
                    listView1.Items.Add(item);
                }
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
                var files = Directory.GetFiles(currentPath, "*", SearchOption.TopDirectoryOnly);
                var matchedFiles = new List<Tuple<string, int>>();

                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file).ToLower();
                    if (fileName.Contains(query))
                    {
                        int distance = LevenshteinDistance(fileName, query);
                        matchedFiles.Add(new Tuple<string, int>(file, distance));
                    }
                }

                var ordered = matchedFiles.OrderBy(t => t.Item2).ToList();

                foreach (var tuple in ordered)
                {
                    string file = tuple.Item1;
                    ListViewItem item = new ListViewItem(Path.GetFileName(file), GetFileIconIndex(file));
                    item.Tag = file;
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
        // 6. DRAG & DROP: FROM TREEVIEW TO LISTVIEW (Load Folder Contents)
        // ----------------------------------------------------------------
        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            TreeNode draggedNode = (TreeNode)e.Item;
            if (draggedNode.Tag != null && Directory.Exists(draggedNode.Tag.ToString()))
            {
                DoDragDrop(new DataObject(DataFormats.FileDrop, new string[] { draggedNode.Tag.ToString() }),
                           DragDropEffects.Copy);
            }
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedPaths = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string path in droppedPaths)
                {
                    if (Directory.Exists(path))
                    {
                        LoadFiles(path);
                    }
                    else
                    {
                        MessageBox.Show("Only folders can be dropped here.", "Invalid Drop", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        // ----------------------------------------------------------------
        // 7. HELPER METHODS: FOLDER SIZE, FORMAT BYTES, GET FILE ICON
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

        // Retrieves the icon index for a file or folder using caching.
        private int GetFileIconIndex(string path)
        {
            // Check if path is a folder.
            if (Directory.Exists(path))
            {
                if (!extIconCache.ContainsKey("folder"))
                {
                    // Use a placeholder folder icon; you can replace this with a custom icon.
                    imageList1.Images.Add(SystemIcons.WinLogo);
                    extIconCache["folder"] = imageList1.Images.Count - 1;
                }
                return extIconCache["folder"];
            }

            // Otherwise, it's a file. Use its extension as the key.
            string ext = Path.GetExtension(path);
            if (extIconCache.ContainsKey(ext))
            {
                return extIconCache[ext];
            }
            else
            {
                try
                {
                    Icon icon = Icon.ExtractAssociatedIcon(path);
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
        // LISTVIEW DOUBLE-CLICK EVENT: OPEN FOLDER OR FILE
        // ----------------------------------------------------------------
        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string selectedPath = listView1.SelectedItems[0].Tag.ToString();
                if (Directory.Exists(selectedPath))
                {
                    // If a folder is double-clicked, load its contents.
                    LoadFiles(selectedPath);
                }
                else if (File.Exists(selectedPath))
                {
                    // If a file is double-clicked, open it with its default application.
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = selectedPath,
                        UseShellExecute = true
                    });
                }
            }
        }

        // ----------------------------------------------------------------
        // FORM EVENTS (Placeholders for additional handling)
        // ----------------------------------------------------------------
        private void Form1_Load(object sender, EventArgs e)
        {
            // Any additional initialization
        }
        private void txtSearch_TextChanged(object sender, EventArgs e) { }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }

        // ----------------------------------------------------------------
        // OPTIONAL: Button to select files (if needed)
        // ----------------------------------------------------------------
        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "All Files (*.*)|*.*";
                openFileDialog.InitialDirectory = "C:\\";
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var file in openFileDialog.FileNames)
                    {
                        MessageBox.Show($"File Selected: {file}");
                    }
                }
            }
        }
    }
}
