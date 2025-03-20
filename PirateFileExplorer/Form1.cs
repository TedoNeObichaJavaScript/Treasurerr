using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PirateFileExplorer
{
    public partial class Form1 : Form
    {

        private ImageList imageList1;

        private Dictionary<string, int> extIconCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private string currentDirectory = "";

        public Form1()
        {
            InitializeComponent();
            InitializeImageList();
            LoadDrives();
            treeView1.ItemDrag += treeView1_ItemDrag;
            listView1.DragEnter += listView1_DragEnter;
            listView1.DragDrop += listView1_DragDrop;
            listView1.DoubleClick += listView1_DoubleClick;
        }
        private void InitializeImageList()
        {
            imageList1 = new ImageList { ImageSize = new Size(32, 32) };
            listView1.SmallImageList = imageList1;
        }

        // ----------------------------------------------------------------
        // 1. Зареждане на устройства и дървовидната структура на папките с размер
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
                        // Създаване на възел за всяко устройство (например "C:\")
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
                    // Създаване на възел с временен текст
                    TreeNode node = new TreeNode($"{Path.GetFileName(dir)} (Calculating...)") { Tag = dir };
                    node.Nodes.Add("Loading...");  // Временен възел за разширяване
                    e.Node.Nodes.Add(node);

                    // Асинхронно изчисляване на размера на папката за отзивчивост на UI
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
        // 2. Зареждане на файлове и папки в ListView
        // ----------------------------------------------------------------
        private void LoadFiles(string path)
        {
            listView1.Items.Clear();
            extIconCache.Clear(); // Изчистване на кеша при зареждане на нова папка
            currentDirectory = path; // Актуализиране на текущата директория

            try
            {
                // Зареждане първо на папките
                foreach (var dir in Directory.GetDirectories(path))
                {
                    ListViewItem item = new ListViewItem(Path.GetFileName(dir), GetFileIconIndex(dir));
                    item.Tag = dir;
                    item.SubItems.Add("Folder");
                    listView1.Items.Add(item);
                }
                // Зареждане на файловете
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
        // 3. Търсене на файлове (с филтриране според най-близко съвпадение)
        // ----------------------------------------------------------------
        private async void btnSearch_Click(object sender, EventArgs e)
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

            btnSearch.Enabled = false;
            btnSearch.Text = "Searching...";

            try
            {
                var matchedFiles = new List<Tuple<string, int>>();
                await Task.Run(() => SearchFilesSafe(currentPath, query, matchedFiles));

                listView1.Invoke((MethodInvoker)delegate
                {
                    foreach (var tuple in matchedFiles.OrderBy(t => t.Item2))
                    {
                        string file = tuple.Item1;
                        ListViewItem item = new ListViewItem(Path.GetFileName(file), GetFileIconIndex(file))
                        {
                            Tag = file
                        };
                        listView1.Items.Add(item);
                    }

                    if (matchedFiles.Count == 0)
                    {
                        MessageBox.Show("No files found matching your search.");
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during search: {ex.Message}");
            }
            finally
            {
                btnSearch.Enabled = true;
                btnSearch.Text = "Search";
            }
        }

        private void SearchFilesSafe(string directory, string query, List<Tuple<string, int>> matchedFiles)
        {
            try
            {
                var files = Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly);

                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file).ToLower();
                    if (fileName.Contains(query))
                    {
                        int distance = LevenshteinDistance(fileName, query);
                        matchedFiles.Add(new Tuple<string, int>(file, distance));
                    }
                }

                foreach (var subDir in Directory.GetDirectories(directory))
                {
                    try
                    {
                        SearchFilesSafe(subDir, query, matchedFiles);
                    }
                    catch (UnauthorizedAccessException) { }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Skipping folder {subDir}: {ex.Message}");
                    }
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in directory {directory}: {ex.Message}");
            }
        }

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
        // 4. Файлови операции (Delete, Copy, Move, Zip, Unzip)
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
            if (filesToDelete.Count == 0)
                return;

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
            if (filesToCopy.Count == 0)
                return;

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
            if (filesToMove.Count == 0)
                return;

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
            if (filesToZip.Count == 0)
                return;

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
            if (filesToUnzip.Count == 0)
                return;

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
        // 5. Контекстно меню: Open, Properties, Rename
        // ----------------------------------------------------------------
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string filePath = listView1.SelectedItems[0].Tag.ToString();
                if (File.Exists(filePath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
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
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
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
        // 6. Drag & Drop: От TreeView към ListView (Зареждане на съдържанието на папка)
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
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
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
        // 7. Помощни методи: GetFolderSize, FormatBytes, Get File/Folder Icon
        // ----------------------------------------------------------------
        private long GetFolderSize(string folderPath)
        {
            long size = 0;
            try
            {
                foreach (string file in Directory.EnumerateFiles(folderPath))
                {
                    try
                    {
                        FileInfo info = new FileInfo(file);
                        size += info.Length;
                    }
                    catch (UnauthorizedAccessException) { }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error getting file size: {ex.Message}");
                    }
                }
                foreach (string dir in Directory.EnumerateDirectories(folderPath))
                {
                    try
                    {
                        size += GetFolderSize(dir);
                    }
                    catch (UnauthorizedAccessException) { }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error getting folder size: {ex.Message}");
                    }
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetFolderSize: {ex.Message}");
            }
            return size;
        }

        private string FormatBytes(long bytes)
        {
            if (bytes >= 1_073_741_824)
                return $"{bytes / 1_073_741_824.0:F2} GB";
            if (bytes >= 1_048_576)
                return $"{bytes / 1_048_576.0:F2} MB";
            if (bytes >= 1_024)
                return $"{bytes / 1_024.0:F2} KB";
            return $"{bytes} bytes";
        }

        // Метод за получаване на индекс на икона за файл или папка с кеширане
        private int GetFileIconIndex(string path)
        {
            if (imageList1 == null)
                imageList1 = new ImageList() { ImageSize = new Size(32, 32) };

            // Ако е папка
            if (Directory.Exists(path))
            {
                if (!extIconCache.ContainsKey("folder"))
                {
                    try
                    {
                        // Извличане на икона за папка чрез SHGetFileInfo
                        Icon folderIcon = GetFolderIcon(path);
                        imageList1.Images.Add(folderIcon.ToBitmap());
                        extIconCache["folder"] = imageList1.Images.Count - 1;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading folder icon: {ex.Message}");
                    }
                }
                return extIconCache["folder"];
            }

            // Ако е файл – обработка на разширението
            string ext = Path.GetExtension(path).ToLower();
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
                        imageList1.Images.Add(icon.ToBitmap());
                        int index = imageList1.Images.Count - 1;
                        extIconCache[ext] = index;
                        return index;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error extracting file icon: {ex.Message}");
                }
            }
            return -1;
        }

        // Метод за извличане на икона за папка чрез SHGetFileInfo
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(
            string pszPath,
            uint dwFileAttributes,
            ref SHFILEINFO psfi,
            uint cbFileInfo,
            uint uFlags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_SMALLICON = 0x000000001;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;

        private Icon GetFolderIcon(string folderPath)
        {
            SHFILEINFO shfi = new SHFILEINFO();
            IntPtr retVal = SHGetFileInfo(folderPath, FILE_ATTRIBUTE_DIRECTORY, ref shfi, (uint)Marshal.SizeOf(shfi), SHGFI_ICON | SHGFI_SMALLICON | SHGFI_USEFILEATTRIBUTES);
            if (retVal != IntPtr.Zero)
            {
                return Icon.FromHandle(shfi.hIcon);
            }
            return SystemIcons.WinLogo; // Връща стандартна икона, ако не е намерена друга
        }

        // ----------------------------------------------------------------
        // 8. Двойно щракване в ListView: Отваряне на папка или файл
        // ----------------------------------------------------------------
        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string selectedPath = listView1.SelectedItems[0].Tag.ToString();
                if (Directory.Exists(selectedPath))
                {
                    // Ако е папка, зареждане на съдържанието ѝ
                    LoadFiles(selectedPath);
                }
                else if (File.Exists(selectedPath))
                {
                    // Ако е файл, отваряне с подразбиращото се приложение
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = selectedPath,
                        UseShellExecute = true
                    });
                }
            }
        }

        // ----------------------------------------------------------------
        // 9. Събития на формата (допълнителна обработка, ако е необходимо)
        // ----------------------------------------------------------------
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void txtSearch_TextChanged(object sender, EventArgs e) { }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }

        // ----------------------------------------------------------------
        // ОПЦИОНАЛНО: Бутон за избиране на файлове (ако е необходимо)
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

        // Encrypt Button Click
        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            // Check if a file is selected in the ListView
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a file to encrypt.");
                return;
            }

            string inputFile = listView1.SelectedItems[0].Tag.ToString();  // Get the file path from the selected item

            try
            {
                EncryptFile(inputFile);  // Encrypt and overwrite the file
                MessageBox.Show("File encrypted successfully:\n" + inputFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Encryption error: " + ex.Message);
            }
        }

        // Encrypt File with AES (this will overwrite the original file)
        private void EncryptFile(string inputFile)
        {
            byte[] fileBytes = File.ReadAllBytes(inputFile);  // Read the file content as bytes

            // Encrypt the content using AES
            byte[] encryptedBytes = AesEncrypt(fileBytes);

            // Overwrite the original file with the encrypted data
            File.WriteAllBytes(inputFile, encryptedBytes);  // Save the encrypted content back to the same file
        }

        // Decrypt Button Click
        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            // Check if a file is selected in the ListView
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a file to decrypt.");
                return;
            }

            string inputFile = listView1.SelectedItems[0].Tag.ToString();  // Get the file path from the selected item

            try
            {
                DecryptFile(inputFile);  // Decrypt and overwrite the file
                MessageBox.Show("File decrypted successfully:\n" + inputFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Decryption error: " + ex.Message);
            }
        }

        // Decrypt File with AES (this will overwrite the original file)
        private void DecryptFile(string inputFile)
        {
            byte[] encryptedBytes = File.ReadAllBytes(inputFile);  // Read the encrypted file content as bytes

            // Decrypt the byte array to the original content
            byte[] decryptedBytes = AesDecrypt(encryptedBytes);

            // Overwrite the original file with the decrypted content
            File.WriteAllBytes(inputFile, decryptedBytes);  // Save the decrypted content back to the same file

            // Ensure the file system has properly flushed the changes before trying to access the file again
            System.Threading.Thread.Sleep(500);  // Small delay to ensure file is properly written and available
        }

        // AES Decryption Logic
        private byte[] AesDecrypt(byte[] inputBytes)
        {
            byte[] keyAndIvBytes = UTF8Encoding.UTF8.GetBytes("tR7nR6wZHGjYMCuV");  // Fixed 16-byte key (128-bit)

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyAndIvBytes;

                // Extract the IV from the input data (the first 16 bytes in this case, since we use AES with a 128-bit block)
                byte[] iv = new byte[16];
                Buffer.BlockCopy(inputBytes, 0, iv, 0, iv.Length);

                aesAlg.IV = iv;  // Set the IV for decryption

                // Extract the actual encrypted data (everything after the IV)
                byte[] encryptedData = new byte[inputBytes.Length - iv.Length];
                Buffer.BlockCopy(inputBytes, iv.Length, encryptedData, 0, encryptedData.Length);

                using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, aesAlg.CreateDecryptor(), CryptoStreamMode.Read))
                using (MemoryStream msOutput = new MemoryStream())
                {
                    csDecrypt.CopyTo(msOutput);
                    return msOutput.ToArray();  // Return the decrypted binary data
                }
            }
        }

        // AES Encryption Logic
        private byte[] AesEncrypt(byte[] inputBytes)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = UTF8Encoding.UTF8.GetBytes("tR7nR6wZHGjYMCuV");

                // Generate a random IV
                aesAlg.GenerateIV();
                byte[] iv = aesAlg.IV;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // Write the IV to the memory stream first
                    memoryStream.Write(iv, 0, iv.Length);

                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesAlg.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(inputBytes, 0, inputBytes.Length);
                        cryptoStream.FlushFinalBlock();
                    }

                    return memoryStream.ToArray();  // Return the encrypted data (IV + encrypted binary data)
                }
            }
        }




        // Rename Button
        private void btnRename_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a file to rename.");
                return;
            }

            string oldPath = listView1.SelectedItems[0].Tag.ToString();
            if (!File.Exists(oldPath))
            {
                MessageBox.Show("The selected file does not exist.");
                return;
            }

            string oldName = Path.GetFileName(oldPath);
            string newName = Microsoft.VisualBasic.Interaction.InputBox("Enter new file name:", "Rename", oldName);
            if (string.IsNullOrWhiteSpace(newName) || newName == oldName)
            {
                MessageBox.Show("Invalid new name.");
                return;
            }

            string newPath = Path.Combine(Path.GetDirectoryName(oldPath), newName);
            if (File.Exists(newPath))
            {
                MessageBox.Show("A file with this name already exists.");
                return;
            }

            try
            {
                File.Move(oldPath, newPath);
                // Update the selected ListViewItem to reflect the new file name and path.
                ListViewItem selectedItem = listView1.SelectedItems[0];
                selectedItem.Text = newName;
                selectedItem.Tag = newPath;
                MessageBox.Show("File renamed successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error renaming file: {ex.Message}");
            }
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        //btnredirect
        private void btnRedirect_Click(object sender, EventArgs e)
        { 
            string htmlPath = System.IO.Path.Combine(Application.StartupPath, "index.html");  
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = htmlPath,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }
    }
}