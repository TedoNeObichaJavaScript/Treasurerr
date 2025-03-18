using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PirateFileExplorer; // Ensure correct namespace

namespace PirateFileExplorerTests
{
    [TestClass]
    public class Form1Tests
    {
        private string tempDir;

        [TestInitialize]
        public void Setup()
        {
            tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }

        private Form1 CreateForm1Instance()
        {
            return new Form1();
        }

        private object InvokePrivateMethod(object instance, string methodName, object[] parameters)
        {
            var method = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                throw new MissingMethodException($"Method '{methodName}' not found on type '{instance.GetType()}'.");
            }
            return method.Invoke(instance, parameters);
        }

        [TestMethod]
        public void TestLevenshteinDistance()
        {
            var form = CreateForm1Instance();
            string s = "kitten";
            string t = "sitting";
            var result = (int)InvokePrivateMethod(form, "LevenshteinDistance", new object[] { s, t });
            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void TestGetFolderSize()
        {
            var form = CreateForm1Instance();

            string filePath = Path.Combine(tempDir, "testfile.bin");
            byte[] data = new byte[1024];
            new Random().NextBytes(data);
            File.WriteAllBytes(filePath, data);

            string subDir = Path.Combine(tempDir, "subdir");
            Directory.CreateDirectory(subDir);
            string subFile = Path.Combine(subDir, "subfile.bin");
            byte[] subData = new byte[512];
            new Random().NextBytes(subData);
            File.WriteAllBytes(subFile, subData);

            var result = (long)InvokePrivateMethod(form, "GetFolderSize", new object[] { tempDir });
            Assert.AreEqual(1536, result);
        }

        [TestMethod]
        public void TestSearchFilesSafe()
        {
            var form = CreateForm1Instance();

            string searchDir = Path.Combine(tempDir, "searchFolder");
            Directory.CreateDirectory(searchDir);

            string matchingFile = Path.Combine(searchDir, "example_test.txt");
            string nonMatchingFile = Path.Combine(searchDir, "anotherfile.doc");
            File.WriteAllText(matchingFile, "dummy content");
            File.WriteAllText(nonMatchingFile, "dummy content");

            var matchedFiles = new List<Tuple<string, int>>();

            InvokePrivateMethod(form, "SearchFilesSafe", new object[] { searchDir, "test", matchedFiles });

            bool foundMatch = matchedFiles.Exists(t =>
                t.Item1.Equals(matchingFile, StringComparison.OrdinalIgnoreCase));
            bool foundNonMatch = matchedFiles.Exists(t =>
                t.Item1.Equals(nonMatchingFile, StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(foundMatch, "Expected to find the matching file.");
            Assert.IsFalse(foundNonMatch, "Did not expect to find the non-matching file.");
        }
    }
}
