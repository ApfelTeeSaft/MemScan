using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MemScan
{
    public partial class ResultsWindow : Window
    {
        private List<(string Value, IntPtr Address)> allResults;
        private Process process;

        public ResultsWindow(Process process)
        {
            InitializeComponent();
            this.process = process;
        }

        public void DisplayResults(List<(string Value, IntPtr Address)> results)
        {
            allResults = results.OrderBy(r => r.Value).ToList();
            ResultsListBox.ItemsSource = allResults.Select(r => r.Value).ToList();
        }

        private void ResultsSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = ResultsSearchBox.Text.ToLower();
            ResultsListBox.ItemsSource = allResults
                .Where(r => r.Value.ToLower().Contains(searchText))
                .Select(r => r.Value)
                .ToList();
        }

        private void ResultsListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ResultsListBox.SelectedItem is string selectedString)
            {
                var result = allResults.FirstOrDefault(r => r.Value == selectedString);
                if (result.Value != null)
                {
                    var editWindow = new EditStringWindow(result.Value, result.Address, process);
                    editWindow.Show();
                }
            }
        }
    }
}