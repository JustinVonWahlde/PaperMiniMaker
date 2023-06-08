using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Point = System.Drawing.Point;
//using Pen = System.Drawing.Pen;
//using Color = System.Drawing.Color;

namespace PaperMiniMaker
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		#region Global variables
		// hard coded values, for now...
		public const int resolution = 300;
		public const int pageHeight = 3300;
		public const int pageWidth = 2550;

		// global variables 
		public List<Miniature> miniatures = new List<Miniature>();
		private Bitmap frontPage;
		private Bitmap backPage;
		public double aspectRatio;
		public int miniHeight;
		public int baseHeight;
		public int miniWidth;

		public int leftRightMargin;
		public int topBottomMargin;

		public int effectivePageHeight;
		public int effectivePageWidth;

		public int numOfRows;
		public int numOfColumns;

		private int currentDisplay = 0;

		// validation variables
		public bool arSelected;
		public bool initializing = false;
		#endregion

		/// <summary>
		/// Since sliders run their ValueChanged Event Handler as soon as they're
		/// initialized, added a boolean to check if they're just being initialized
		/// or if they're actually being called. Also since the miniHeight and baseHeight
		/// sliders begin with a value, update their variables respecively
		/// </summary>
		public MainWindow()
		{
			initializing = true;
			InitializeComponent();
			initializing = false;

			miniHeight = Convert.ToInt32(Math.Round(MiniHeightSlider.Value * resolution));
			baseHeight = Convert.ToInt32(Math.Round(BaseHeightSlider.Value * resolution));
		}

		#region Control updates
		/// <summary>
		/// When a new aspect ratio is selected, use the value in the combo box to calculate the actual aspect ratio.
		/// Then update the arSelected boolean and redraw the Pages.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RatioComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			string value = e.AddedItems[0].ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "").Replace(" ", "");
			string[] vs = value.Split(':');
			if (vs.Length == 2)
			{
				if (double.TryParse(vs[0], out double width) && double.TryParse(vs[1], out double height))
				{
					aspectRatio = Math.Round(height / width, 3);
				}
			}
			arSelected = true;

			add.IsEnabled = true;
			GeneratePages();
		}

		/// <summary>
		/// When a new Miniature Height is selected, use the value from the slider to update the miniHeight 
		/// variable. Then, if an aspect ratio has been seleced as well, redraw the pages.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MiniHeightSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (initializing) { return; }

			miniHeight = Convert.ToInt32(Math.Round(MiniHeightSlider.Value * resolution));

			if (arSelected)
			{
				add.IsEnabled = true;
				GeneratePages();
			}
		}

		/// <summary>
		/// When a new Base Height is selected, use the value from the slider to update the baseHeight 
		/// variable. Then, if an aspect ratio has been seleced as well, redraw the pages.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BaseHeightSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (initializing) { return; }

			baseHeight = Convert.ToInt32(Math.Round(BaseHeightSlider.Value * resolution));

			if (arSelected)
			{
				add.IsEnabled = true;
				GeneratePages();
			}
		}

		/// <summary>
		/// Time to add a miniature. First open a dialog box to allow the user to upload an image. Then pass 
		/// that image, along with the file's name and the this instance of MainWindow, on to a newly created 
		/// instance of the AddOrEditMiniature window. If the AddOrEditMiniature window returns a Miniature, add it to the 
		/// miniatures list, disable the ratio combo box, mini height slider, and base height slder, update the 
		/// ListView, and redraw the page.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Add_Click(object sender, RoutedEventArgs e)
		{
			// open a file dialog so that the user can upload an image
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.InitialDirectory = "c://";
			openFileDialog.Filter = "Image files (*.jpg;*.png)|*.jpg;*.png|All Files (*.*)|*.*";

			if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				string selectedFileName = openFileDialog.FileName;
				BitmapImage uploadedImage = new BitmapImage();
				uploadedImage.BeginInit();
				uploadedImage.UriSource = new Uri(selectedFileName);
				uploadedImage.EndInit();

				// if the user successfully uploaded an image, start a new instance of the AddOrEditMiniature window
				AddOrEditMiniature addMini = new AddOrEditMiniature(this, uploadedImage, openFileDialog.SafeFileName);
				addMini.ShowDialog();

				// if the addMini.MiniatureToAdd object isn't null, add the miniature to the list of miniatures,
				// update the listview, and redraw the page with the added minis. Also disable the combo boxes
				// since their values can't be changed once minis are on the page.
				if (addMini.MiniatureToAddOrEdit != null)
				{
					miniatures.Add(addMini.MiniatureToAddOrEdit);

					// as long as there is an image on the pages, these controls cannot be changed.
					ratioComboBox.IsEnabled = false;
					MiniHeightSlider.IsEnabled = false;
					BaseHeightSlider.IsEnabled = false;

					// update the ListView
					listView.Items.Clear();
					foreach (Miniature m in miniatures)
					{
						listView.Items.Add(m);
					}

					GeneratePages();
				}
			}
		}

		/// <summary>
		/// Time to edit a miniature. First determine which miniature is selected in the 
		/// listView. Then create a new instance of the EditMiniature window, passing 
		/// along the selected mini, the list of all of the minis (so that the user can't 
		/// duplicate names), and the max quantity. If the EditMiniature window returns a
		/// miniature, edit the selected mini, refreash the listView, and redraw the page.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Edit_Click(object sender, RoutedEventArgs e)
		{
			Miniature selectedMini = (Miniature)listView.SelectedItem;

			// since in this case the "maxQty" represents the maximum number that the
			// mini's quantity could be updated to, it needs to include the mini's 
			// current quantity as well.
			int maxQty = GetMaxQuantity() + selectedMini.Quantity;
			int miniIndex = listView.SelectedIndex;

			AddOrEditMiniature editMini = new AddOrEditMiniature(this, selectedMini);
			//EditMiniature editMini = new EditMiniature(selectedMini, miniatures, maxQty);
			editMini.ShowDialog();

			// if the editMini.editedMiniature object isn't null, edit the selected miniature,
			// update the listView, and redraw the page.
			if (editMini.MiniatureToAddOrEdit != null)
			{
				miniatures[miniIndex] = editMini.MiniatureToAddOrEdit;

				listView.Items.Clear();
				foreach (Miniature m in miniatures)
				{
					listView.Items.Add(m);
				}

				GeneratePages();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Remove_Click(object sender, RoutedEventArgs e)
		{
			Miniature selectedMini = (Miniature)listView.SelectedItem;
			miniatures.Remove(selectedMini);

			edit.IsEnabled = false;
			remove.IsEnabled = false;
			listView.SelectedIndex = -1;
			listView.Items.Clear();

			foreach (Miniature m in miniatures)
			{
				listView.Items.Add(m);
			}

			if (miniatures.Count == 0)
			{
				ratioComboBox.IsEnabled = true;
				MiniHeightSlider.IsEnabled = true;
				BaseHeightSlider.IsEnabled = true;
			}

			GeneratePages();
		}

		/// <summary>
		/// If an item in the ListView is selected, enable the edit and delete buttons
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			object selectedMini = ((System.Windows.Controls.ListBox)sender).SelectedItem;

			edit.IsEnabled = selectedMini != null;
			remove.IsEnabled = selectedMini != null;
		}

		/// <summary>
		/// It's time to save the pdf! First open a SaveFileDialog to allow the user to choose where the pdf is saved,
		/// then create the pdf using the pages generated by the GeneratePages function.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Generate_Click(object sender, RoutedEventArgs e)
		{
			//page.Save("test.png");

			SaveFileDialog sfd = new SaveFileDialog();

			sfd.InitialDirectory = "c:\\";
			sfd.Filter = "pdf files (*.pdf)|*.pdf|All files (*.*)|*.*";
			sfd.FilterIndex = 1;
			sfd.RestoreDirectory = true;

			if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				try
				{
					using (FileStream fs = File.Create(sfd.FileName))
					{
						Document document = new Document(PageSize.LETTER, 0, 0, 0, 0);
						PdfWriter writer = PdfWriter.GetInstance(document, fs);

						document.AddTitle("Paper Miniatures");
						document.AddAuthor("PaperMiniMaker");
						document.AddSubject("Paper Miniatures");
						document.AddCreator("PaperMiniMaker");
						document.AddCreationDate();
						document.AddProducer();

						document.Open();

						iTextSharp.text.Image frontPageImage = iTextSharp.text.Image.GetInstance(frontPage, BaseColor.WHITE);
						iTextSharp.text.Image backPageImage = iTextSharp.text.Image.GetInstance(backPage, BaseColor.WHITE);

						frontPageImage.ScaleToFit(document.PageSize);
						document.Add(frontPageImage);

						document.NewPage();

						backPageImage.ScaleToFit(document.PageSize);
						document.Add(backPageImage);

						document.Close();
						writer.Close();
						//fs.Close();
					}					
				}

				catch (UnauthorizedAccessException)
				{
					FileAttributes attributes = File.GetAttributes(sfd.FileName);
					if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
					{

					}
				}
			}
		}

		private void NextPage_Click(object sender, RoutedEventArgs e)
		{
			currentDisplay = 2;
			pageNofM.Content = "Page 2 of 2";
			preview.Source = BitmapToImageSource(backPage);
			nextPage.IsEnabled = false;
			previousPage.IsEnabled = true;
		}

		private void PreviousPage_Click(object sender, RoutedEventArgs e)
		{
			currentDisplay = 1;
			pageNofM.Content = "Page 1 of 2";
			preview.Source = BitmapToImageSource(frontPage);
			nextPage.IsEnabled = true;
			previousPage.IsEnabled = false;
		}
		#endregion Control updates

		/// <summary>
		/// Create two bitmaps with the dimensions of a page, add the images of each miniature to the pages,
		/// add the bases, then add the grid lines. Finally, display the first bitmap on the MainWindow screen.
		/// </summary>
		private void GeneratePages()
		{
			#region establish variables
			miniWidth = Convert.ToInt32(Math.Round(miniHeight / aspectRatio));

			// determine the page margins, setting their minimum to the resolution (this will need to be changed once resolution is no longer hardcoded)
			leftRightMargin = (resolution + ((pageWidth - resolution) % miniWidth)) / 2;
			topBottomMargin = (resolution + ((pageHeight - resolution) % (miniHeight + baseHeight))) / 2;

			effectivePageHeight = pageHeight - (topBottomMargin * 2);
			effectivePageWidth = pageWidth - (leftRightMargin * 2);

			numOfRows = Convert.ToInt32(Math.Floor((double)effectivePageHeight / (miniHeight + baseHeight)));
			numOfColumns = Convert.ToInt32(Math.Floor((double)effectivePageWidth / miniWidth));
			#endregion

			// generate the pages
			frontPage = new Bitmap(pageWidth, pageHeight);
			backPage = new Bitmap(pageWidth, pageHeight);

			Graphics fpGraphics = Graphics.FromImage(frontPage);
			Graphics bpGraphics = Graphics.FromImage(backPage);

			// set the background color of the pages
			fpGraphics.FillRectangle(new SolidBrush(Color.White), 0, 0, pageWidth, pageHeight);
			bpGraphics.FillRectangle(new SolidBrush(Color.White), 0, 0, pageWidth, pageHeight);

			// if there are miniatures to add to the pages, do it first
			if (miniatures.Count > 0)
			{
				// populate back images
				foreach (Miniature m in miniatures)
				{
					m.BackImage = new Bitmap(m.FrontImage);
					m.BackImage.RotateFlip(RotateFlipType.RotateNoneFlipX);

					// <ADD SILHOUETTE, GREYSCALE, AND BLUR FUNCTIONS HERE>
					// if greyscale control is checked
					m.BackImage = ConverToGrayscale(m.BackImage);
					
				}

				// first get all of the points where the miniature images need to be added. These points are the same for both pages
				List<Point> points = new List<Point>();
				for (int i = 0; i < numOfRows; i++)
				{
					for (int j = 0; j < numOfColumns; j++)
					{
						points.Add(new Point(leftRightMargin + (miniWidth * j), topBottomMargin + (miniHeight * i) + (baseHeight * i)));
					}
				}

				// Add the images to the pages
				int counter = 0;

				foreach (Miniature m in miniatures)
				{
					for (int k = 0; k < m.Quantity; k++)
					{
						// calculate the current miniNumber
						int miniNumber = counter + k;

						// double check to make sure there are enough points to add the miniature's image.
						if (points.Count >= counter + k)
						{
							// add miniature images to the front page first.
							fpGraphics.DrawImage(m.FrontImage, points[miniNumber]);
						}

						// calculate the current row (0-based)
						int rowNumber = Convert.ToInt32(Math.Floor((double)miniNumber / numOfColumns));
						// find the point in the row from right to left given the miniNumber
						int backPoint = (rowNumber * numOfColumns) + (numOfColumns - (miniNumber % numOfColumns) - 1);

						// double check to make sure there are enough points to add the miniature's image.
						if (points.Count >= backPoint)
						{
							// now add miniature images to the back page. The miniature images are added from
							// right to left in each row so that they line up with the images on the front page.
							bpGraphics.DrawImage(m.BackImage, points[backPoint]);
						}
					}

					counter += m.Quantity;
				}
			}

			// setup pens for the base lines and grid lines
			Pen baseLine = new Pen(Color.LightGray, baseHeight);
			Pen gridLine = new Pen(Color.Black, 2);

			// determine the y-coordinate for each base line 
			List<int> baseYCoords = new List<int>();
			for (int i = 1; i <= numOfRows; i++)
			{
				baseYCoords.Add(topBottomMargin + (miniHeight * i) + (baseHeight * (i - 1)) + (baseHeight / 2));
			}

			// draw each base line on the pages
			foreach (int baseYCoord in baseYCoords)
			{
				fpGraphics.DrawLine(baseLine,
					new Point(leftRightMargin, baseYCoord),
					new Point(leftRightMargin + effectivePageWidth, baseYCoord));
				bpGraphics.DrawLine(baseLine,
					new Point(leftRightMargin, baseYCoord),
					new Point(leftRightMargin + effectivePageWidth, baseYCoord));
			}

			// determine the y-coordinate for each horizontal grid line
			List<int> hGridLineYCoords = new List<int>();
			for (int i = 0; i <= numOfRows; i++)
			{
				hGridLineYCoords.Add(topBottomMargin + ((miniHeight + baseHeight) * i));
			}

			// draw each horizontal grid line on the pages
			foreach (int hGridLineYCoord in hGridLineYCoords)
			{
				fpGraphics.DrawLine(gridLine,
					new Point(leftRightMargin - 1, hGridLineYCoord),
					new Point(leftRightMargin + effectivePageWidth + 1, hGridLineYCoord));
				bpGraphics.DrawLine(gridLine,
					new Point(leftRightMargin - 1, hGridLineYCoord),
					new Point(leftRightMargin + effectivePageWidth + 1, hGridLineYCoord));
			}

			// determine the x-coordinates for each vertical grid line
			List<int> vGridLineXCoords = new List<int>();
			for (int i = 0; i <= numOfColumns; i++)
			{
				vGridLineXCoords.Add(leftRightMargin + (miniWidth * i));
			}

			// draw each vertical grid line on the pages
			foreach (int vGridLineXCoord in vGridLineXCoords)
			{
				fpGraphics.DrawLine(gridLine,
					new Point(vGridLineXCoord, topBottomMargin - 1),
					new Point(vGridLineXCoord, topBottomMargin + effectivePageHeight + 1));
				bpGraphics.DrawLine(gridLine,
					new Point(vGridLineXCoord, topBottomMargin - 1),
					new Point(vGridLineXCoord, topBottomMargin + effectivePageHeight + 1));
			}


			// update the screen with the page and basic stats about it
			ColumnNumberLabel.Content = numOfColumns;
			RowNumberLabel.Content = numOfRows;
			MiniNumberLabel.Content = numOfColumns * numOfRows;

			if (currentDisplay != 2)
			{
				pageNofM.Content = "Page 1 of 2";
				preview.Source = BitmapToImageSource(frontPage);
				nextPage.IsEnabled = true;
			}
			else
			{
				pageNofM.Content = "Page 2 of 2";
				preview.Source = BitmapToImageSource(backPage);
				previousPage.IsEnabled = true;
			}
				

		}

		public int GetMaxQuantity()
		{
			int temp = numOfColumns * numOfRows;

			foreach (Miniature m in miniatures)
			{
				temp -= m.Quantity;
			}

			return temp;
		}

		private BitmapImage BitmapToImageSource(Bitmap bitmap)
		{
			using (MemoryStream memory = new MemoryStream())
			{
				bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
				memory.Position = 0;
				BitmapImage bitmapimage = new BitmapImage();
				bitmapimage.BeginInit();
				bitmapimage.StreamSource = memory;
				bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapimage.EndInit();

				return bitmapimage;
			}
		}

		private Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				BitmapEncoder encoder = new BmpBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
				encoder.Save(memoryStream);
				Bitmap bitmap = new Bitmap(memoryStream);

				return new Bitmap(bitmap);
			}
		}

		/// <summary>
		/// This was pulled directly off of the internet: https://stackoverflow.com/questions/2265910/convert-an-image-to-grayscale
		/// I don't fully understand the ColorMatrix bit, but it looks fascinating.
		/// </summary>
		/// <param name="original"></param>
		/// <returns></returns>
		public static Bitmap ConverToGrayscale(Bitmap original)
		{
			//create a blank bitmap the same size as original
			Bitmap newBitmap = new Bitmap(original.Width, original.Height);

			//get a graphics object from the new image
			using (Graphics g = Graphics.FromImage(newBitmap))
			{

				//create the grayscale ColorMatrix
				ColorMatrix colorMatrix = new ColorMatrix(
				   new float[][]
				   {
					   new float[] {.3f, .3f, .3f, 0, 0},
					   new float[] {.59f, .59f, .59f, 0, 0},
					   new float[] {.11f, .11f, .11f, 0, 0},
					   new float[] {0, 0, 0, 1, 0},
					   new float[] {0, 0, 0, 0, 1}
				   });

				//create some image attributes
				using (ImageAttributes attributes = new ImageAttributes())
				{

					//set the color matrix attribute
					attributes.SetColorMatrix(colorMatrix);

					//draw the original image on the new image
					//using the grayscale color matrix
					g.DrawImage(original, new System.Drawing.Rectangle(0, 0, original.Width, original.Height),
								0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
				}
			}
			return newBitmap;
		}

	}

	public class Miniature
	{
		public string Name { get; set; }
		public int Quantity { get; set; }
		public BitmapImage UploadedImage { get; set; }
		public Bitmap FrontImage { get; set; }
		public Bitmap BackImage { get; set; }
		public double ZoomRatio { get; set; }
		public int XCoordinate { get; set; }
		public int YCoordinate { get; set; }
	}
}
