using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Security.Cryptography;

namespace VRCScraperWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            InitState();
        }

        private void RunClick(object sender, RoutedEventArgs e)
        {
            CheckFiles();
        }

        private void ChangeFolder(object sender, RoutedEventArgs e)
        {
            Console.Clear();
            ChangeTheFolder();
        }

        static void ChangeTheFolder()
        {
            CommonOpenFileDialog dialog = new()
            {
                InitialDirectory = Environment.ExpandEnvironmentVariables(@"%UserProfile%\Desktop\"),
                IsFolderPicker = true
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                DataState.writePATH = dialog.FileName;
                ResetState();
            }
        }

        static void ResetState()
        {
            DataState.FileNameList.Clear();
            DataState.FileSizeList.Clear();
            string[] filePaths = Directory.GetFiles(DataState.writePATH, "*.vrca",
                                         SearchOption.TopDirectoryOnly);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Building HashList please wait...");
            //Creating a List of sizes for all files in the designated folder
            Parallel.ForEach(filePaths, currentFilePath => {
                //Check filesize
                var FileSize = CheckMD5(currentFilePath);
                DataState.FileSizeList.Add(FileSize);
                //Get File name
                var FileName = System.IO.Path.GetFileNameWithoutExtension(currentFilePath);
                int intFileName = Convert.ToInt32(FileName);
                DataState.FileNameList.Add(intFileName);
            });
            Console.WriteLine("All done! Please press run to check avatars!");
        }

        static void CheckFiles()
        {
            Console.Clear();
            string readPATHwithEnv = @"%UserProfile%/AppData/LocalLow/VRChat/VRChat\Cache-WindowsPlayer";
            var readPATH = Environment.ExpandEnvironmentVariables(readPATHwithEnv);

            string[] filePaths = Directory.GetFiles(readPATH, "__data",
                                         SearchOption.AllDirectories);
            
            Parallel.ForEach(filePaths, (currentFilePath, state, index) =>
            {
                // Check filesize
                var FileSize = CheckMD5(currentFilePath);
                // If file.hash is in HashArray, skip
                if (DataState.FileSizeList.Contains(FileSize))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"FOUND DUPLICATE {index}.vrca... SKIPPING");
                }
                else
                {
                    // Convert __data file to .vrca and save to designated folder
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"COPYING VRCA to {index}.vrca");
                    DataState.FileSizeList.Add(FileSize);
                    string destFile = System.IO.Path.Combine(DataState.writePATH, $"{index}.vrca");
                    System.IO.File.Copy(currentFilePath, destFile, true);
                }
            });
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n=================");
            Console.WriteLine("Checked all VRCAs");
            Console.WriteLine("=================");
        }

        static void InitState()
        {
            AllocConsole();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(string.Concat(Enumerable.Repeat("DO NOT CLOSE THIS CONSOLE UNLESS YOU WANT TO CLOSE THE APP!\n", 3)));
            ChangeTheFolder();
        }

        private void OpenFolder(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", DataState.writePATH);
        }

        static string CheckMD5(string path)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    byte[] checksum = md5.ComputeHash(stream);
                    return BitConverter.ToString(checksum).Replace("-", String.Empty).ToLower();
                }
            }
        }

        [DllImport("Kernel32")]
        private static extern void AllocConsole();

        public static class DataState
        {
            public static List<int> FileNameList { get; set; } = new List<int>();
            public static List<string> FileSizeList { get; set; } = new List<string>();

            public static string writePATH = "";
        }
    }
}
