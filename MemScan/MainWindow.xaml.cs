using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace MemScan
{
    public partial class MainWindow : Window
    {
        private List<ProcessInfo> allProcesses;

        public MainWindow()
        {
            InitializeComponent();
            LoadProcesses();
        }

        private void LoadProcesses()
        {
            allProcesses = new List<ProcessInfo>();

            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    allProcesses.Add(new ProcessInfo
                    {
                        Name = process.ProcessName,
                        PID = process.Id,
                        Icon = GetProcessIcon(process)
                    });
                }
                catch
                {
                }
            }

            ProcessListView.ItemsSource = allProcesses;
        }

        private BitmapSource GetProcessIcon(Process process)
        {
            try
            {
                return IconExtractor.Extract(process.MainModule.FileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting icon for process {process.ProcessName}: {ex.Message}");
                return null;
            }
        }

        private void ProcessSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = ProcessSearchBox.Text.ToLower();

            ProcessListView.ItemsSource = allProcesses
                .Where(p => p.Name.ToLower().Contains(searchText) || p.PID.ToString().Contains(searchText))
                .ToList();
        }

        private void ProcessListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScanButton.IsEnabled = ProcessListView.SelectedItem != null;
        }

        private async void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessListView.SelectedItem is ProcessInfo selectedProcess)
            {
                // var indexingWindow = new IndexingWindow();
                // indexingWindow.ShowInTaskbar = false;
                // indexingWindow.Show();
                // indexingWindow.StartAnimation("Scanning process for strings");

                try
                {
                    var results = await Task.Run(() =>
                    {
                        var process = Process.GetProcessById(selectedProcess.PID);
                        return (process, MemoryScanner.ScanProcessForStrings(process));
                    });

                    var resultsWindow = new ResultsWindow(results.process);
                    resultsWindow.DisplayResults(results.Item2);
                    resultsWindow.Show();
                }
                finally
                {
                    // indexingWindow.StopAnimation("Scan complete");
                    // indexingWindow.Close();
                }
            }
        }
    }

    public class ProcessInfo
    {
        public string Name { get; set; }
        public int PID { get; set; }
        public BitmapSource Icon { get; set; }
    }

    public static class IconExtractor
    {
        [DllImport("Shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

        public static BitmapSource Extract(string path)
        {
            IntPtr hIcon = ExtractIcon(IntPtr.Zero, path, 0);
            if (hIcon == IntPtr.Zero)
            {
                return null;
            }

            using (var icon = System.Drawing.Icon.FromHandle(hIcon))
            {
                using (var bmp = icon.ToBitmap())
                {
                    var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                        bmp.GetHbitmap(),
                        IntPtr.Zero,
                        System.Windows.Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());

                    DestroyIcon(hIcon);

                    return bitmapSource;
                }
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyIcon(IntPtr hIcon);
    }
}