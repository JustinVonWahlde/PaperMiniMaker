using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PaperMiniMaker
{
	/// <summary>
	/// Interaction logic for TestWindow.xaml
	/// </summary>
	public partial class TestWindow : Window
	{
		public TestWindow()
		{
			InitializeComponent();
		}

		private void BrowseButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.InitialDirectory = "c:\\";
			dlg.Filter = "Image files (*.jpg;*.png)|*.jpg;*.png|All Files (*.*)|*.*";

			if (dlg.ShowDialog() == true)
			{
				string selectedFileName = dlg.FileName;
				FileNameLabel.Content = selectedFileName;
				BitmapImage bitmap = new BitmapImage();
				bitmap.BeginInit();
				bitmap.UriSource = new Uri(selectedFileName);
				bitmap.EndInit();
				ImageViewer1.Source = bitmap;
			}
		}
	}
}
