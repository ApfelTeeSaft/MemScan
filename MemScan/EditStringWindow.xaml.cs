using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace MemScan
{
    public partial class EditStringWindow : Window
    {
        private IntPtr address;
        private Process process;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        public EditStringWindow(string value, IntPtr address, Process process)
        {
            InitializeComponent();
            StringTextBox.Text = value;
            this.address = address;
            this.process = process;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string newValue = StringTextBox.Text;

            byte[] buffer = Encoding.UTF8.GetBytes(newValue + "\0");

            IntPtr processHandle = process.Handle;
            if (WriteProcessMemory(processHandle, address, buffer, (uint)buffer.Length, out int bytesWritten))
            {
                MessageBox.Show("String successfully written to memory.");
                Close();
            }
            else
            {
                MessageBox.Show("Failed to write string to memory.");
            }
        }
    }
}