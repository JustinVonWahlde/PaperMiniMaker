using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace PaperMiniMaker
{
	/// <summary>
	/// Interaction logic for AddOrEditMiniature.xaml
	/// </summary>
	public partial class AddOrEditMiniature : Window
	{
		public Miniature MiniatureToAddOrEdit { get; set; }

		// passed-in variables
		private readonly MainWindow mainWindow;
		private BitmapImage uploadedImage;
		private int maxQty;
		private string miniName;
		private int miniQuantity = 0;

		private double zoomRatio = 1;
		private int imgRectX;
		private int imgRectY;

		// cropping variables
		private Rectangle imageRectangle;
		private Rectangle croppingRectangle;
		double scaleRatio;
		int origImgWidth;
		int origImgHeight;
		int cropRectHeight;
		int cropRectWidth;
		int cropRectX;
		int cropRectY;

		// validation variables
		private bool initializing = false;
		private bool nameHasValue = true;
		private bool quantityHasValue = true;

		/// <summary>
		/// Open the AddOrEditMiniature Window to Add a Miniature.
		/// </summary>
		/// <param name="_mainWindow"></param>
		/// <param name="_uploadedImage"></param>
		/// <param name="uploadedImageName"></param>
		public AddOrEditMiniature(MainWindow _mainWindow, BitmapImage _uploadedImage, string uploadedImageName)
		{
			initializing = true;
			InitializeComponent();
			initializing = false;

			Title = "Add Miniature";
			addOrEdit.Content = "Add";
			nameTextBox.Text = uploadedImageName;
			quantityTextBox.Text = "1";

			mainWindow = _mainWindow;
			maxQty = mainWindow.GetMaxQuantity();

			miniName = uploadedImageName;
			miniQuantity = 1;
			uploadedImage = _uploadedImage;

			InitializeCanvas("ADD");

			UpdateCroppingControls();
		}

		/// <summary>
		/// Open the AddOrEditMiniature Window to Edit a Miniature.
		/// </summary>
		/// <param name="_mainWindow"></param>
		/// <param name="_miniatureToEdit"></param>
		public AddOrEditMiniature(MainWindow _mainWindow, Miniature _miniatureToEdit)
		{
			initializing = true;
			InitializeComponent();
			initializing = false;

			Title = "Edit Miniature";
			addOrEdit.Content = "Edit";
			nameTextBox.Text = _miniatureToEdit.Name;
			quantityTextBox.Text = _miniatureToEdit.Quantity.ToString();

			mainWindow = _mainWindow;
			maxQty = mainWindow.GetMaxQuantity() + _miniatureToEdit.Quantity;
			
			miniName = _miniatureToEdit.Name;
			miniQuantity = _miniatureToEdit.Quantity;
			uploadedImage = _miniatureToEdit.UploadedImage;
			zoomRatio = _miniatureToEdit.ZoomRatio;
			imgRectX = _miniatureToEdit.XCoordinate;
			imgRectY = _miniatureToEdit.YCoordinate;

			InitializeCanvas("EDIT");

			// Adjust image rectangle size and position
			imageRectangle.Width = origImgWidth * zoomRatio;
			imageRectangle.Height = origImgHeight * zoomRatio;
			Canvas.SetLeft(imageRectangle, imgRectX);
			Canvas.SetTop(imageRectangle, imgRectY);

			UpdateCroppingControls();
		}

		/// <summary>
		/// Draw two rectangles: one to display the uploaded image and one to show where the cropping will take place.
		/// </summary>
		private void InitializeCanvas(string mode)
		{
			// First declare the image rectangle, filling it with the uploaded image
			imageRectangle = new Rectangle { Fill = new ImageBrush { ImageSource = uploadedImage }, };

			// Determine how the image must be scaled dow in order for the rectangle to fit on the canvas
			scaleRatio = croppingCanvas.Width / uploadedImage.PixelWidth < croppingCanvas.Height / uploadedImage.PixelHeight ?
				croppingCanvas.Width / uploadedImage.PixelWidth :
				croppingCanvas.Height / uploadedImage.PixelHeight;

			// If the image is smaller than the canvas, do not scale it
			if (scaleRatio > 1)
			{
				imageRectangle.Width = origImgWidth = uploadedImage.PixelWidth;
				imageRectangle.Height = origImgHeight = uploadedImage.PixelHeight;
			}
			else
			{
				imageRectangle.Width = origImgWidth = Convert.ToInt32(uploadedImage.PixelWidth * scaleRatio);
				imageRectangle.Height = origImgHeight = Convert.ToInt32(uploadedImage.PixelHeight * scaleRatio);
			}

			// Add the rectangle to the canvase, and center it both vertially and horizontally if this miniature is being added 
			// (if it is being edited it's position will be adjusted to it's previous settings in the constructor function)
			croppingCanvas.Children.Add(imageRectangle);

			if (mode == "ADD")
			{
				imgRectX = Convert.ToInt32((croppingCanvas.Width - imageRectangle.Width) / 2);
				imgRectY = Convert.ToInt32((croppingCanvas.Height - imageRectangle.Height) / 2);

				Canvas.SetLeft(imageRectangle, imgRectX);
				Canvas.SetTop(imageRectangle, imgRectY);
			}

			// second, create and place the cropping rectangle on the canvas. In order to create the rectangle
			// it's dimensions need to be figured out
			cropRectHeight = 0;
			cropRectWidth = 0;

			for (int i = 1; i <= croppingCanvas.Width && Math.Round(i * mainWindow.aspectRatio, 0) <= croppingCanvas.Height; i++)
			{
				cropRectWidth = i;
				cropRectHeight = Convert.ToInt32(Math.Round(i * mainWindow.aspectRatio));
			}

			cropRectX = Convert.ToInt32(croppingCanvas.Width - cropRectWidth) / 2;
			cropRectY = Convert.ToInt32(croppingCanvas.Height - cropRectHeight) / 2;

			croppingRectangle = new Rectangle()
			{
				Width = cropRectWidth,
				Height = cropRectHeight,
				Stroke = new SolidColorBrush { Color = Colors.Red},
				StrokeThickness = 2,
				StrokeDashArray = new DoubleCollection() { 2 },
			};

			// add the rectangle to the canvas
			croppingCanvas.Children.Add(croppingRectangle);

			Canvas.SetLeft(croppingRectangle, cropRectX);
			Canvas.SetTop(croppingRectangle, cropRectY);
		}

		#region Control updates
		/// <summary>
		/// Whenever the user types into the quantityTextBox, make sure the text is an integer. 
		/// If it isn't, ignore the keystroke. If it is, make sure it's less than or equal to
		/// the max quantity.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void QuantityTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			string temp = quantityTextBox.SelectedText != string.Empty ?
				quantityTextBox.Text.Replace(quantityTextBox.SelectedText, "") + e.Text :
				quantityTextBox.Text + e.Text;

			// if the value in the textbox cannont be parsed as an integer, don't allow the input.
			if (int.TryParse(temp, out int qty))
			{
				// if the value in the textbox is greater than the max quantity, reset it to the
				// max quantity and warn the user.
				if (qty > maxQty)
				{
					quantityTextBox.Text = maxQty.ToString();
					e.Handled = true;
					MessageBox.Show(string.Format("Quantity must be less than or equal to {0}", maxQty), "Invalid Quantity");
				}

				// enable add button if both textboxes have values
				quantityHasValue = true;
				addOrEdit.IsEnabled = nameHasValue && quantityHasValue;
			}
			else
			{
				e.Handled = true;
			}
		}

		/// <summary>
		/// Whenever the user pastes text into the quantityTextBox, make sure the text is an 
		/// integer. If it isn't, cancel the paste event. If the result of the pasting event
		/// is greater than the max quantity, set the textbox text to the max quantity, warn 
		/// the user, and cancel the pasting command. If the user pastes into the nameTextBox,
		/// update the add button accordingly.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
		{
			switch (((FrameworkElement)sender).Name)
			{
				case "nameTextBox":
					// enable add button if both textboxes have values
					nameHasValue = true;
					addOrEdit.IsEnabled = nameHasValue && quantityHasValue;
					break;

				case "quantityTextBox":
					// if there was any selected text when the pasting event occured, remove it from
					// the temp string before adding the pasted text.
					string temp = quantityTextBox.SelectedText != string.Empty ?
						quantityTextBox.Text.Replace(quantityTextBox.SelectedText, "") + e.DataObject.GetData(typeof(string)).ToString() :
						quantityTextBox.Text + e.DataObject.GetData(typeof(string)).ToString();


					// if the result of the pasting can't be parsed into an integer, 
					// cancel the pasting command.
					if (int.TryParse(temp, out int qty))
					{
						// if the result of the pasting is greater than the max quantity, 
						// set the textbox text to the max quantity, warn the user, and 
						// cancel the pasting command.
						if (qty > maxQty)
						{
							quantityTextBox.Text = maxQty.ToString();
							Dispatcher.BeginInvoke(new Action(() => MessageBox.Show(string.Format("Quantity must be less than or equal to {0}", maxQty), "Invalid Quantity")));
							e.CancelCommand();
						}

						// enable add button if both textboxes have values
						quantityHasValue = true;
						addOrEdit.IsEnabled = nameHasValue && quantityHasValue;
					}
					else
					{
						e.CancelCommand();
					}
					break;
			}
		}

		/// <summary>
		/// Whenever one of the textboxes lose focus, validate their values (just in case).
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			switch (((FrameworkElement)sender).Name)
			{
				case "nameTextBox":
					bool unique = true;

					// check to make sure the named entered is unique. If it isn't, undo the change and warn the user.
					foreach (Miniature m in mainWindow.miniatures)
					{
						if (m.Name == nameTextBox.Text)
						{
							unique = false;
							MessageBox.Show("Name must be unique.", "Invalid Name");
						}
					}

					if (unique)
					{
						miniName = nameTextBox.Text;
					}
					else
					{
						nameTextBox.Text = string.Empty;
						nameTextBox.Focus();
					}

					// enable add button if both textboxes have values
					nameHasValue = nameTextBox.Text != string.Empty;
					addOrEdit.IsEnabled = nameHasValue && quantityHasValue;

					break;

				case "quantityTextBox":
					int.TryParse(quantityTextBox.Text, out int qty);

					if (qty > maxQty)
					{
						quantityTextBox.Text = maxQty.ToString();
						miniQuantity = maxQty;
						Dispatcher.BeginInvoke(new Action(() => MessageBox.Show(string.Format("Quantity must be less than or equal to {0}", maxQty), "Invalid Quantity")));
					}
					else
					{
						miniQuantity = qty;
					}

					// enable add button if both textboxes have values
					quantityHasValue = quantityTextBox.Text != string.Empty;
					addOrEdit.IsEnabled = nameHasValue && quantityHasValue;

					break;
			}
		}

		/// <summary>
		/// If the user deletes everything from a textbox, update the add button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TextBox_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.X)
			{
				switch(((FrameworkElement)sender).Name)
				{
					case "nameTextBox":
						if (nameTextBox.Text == string.Empty)
						{
							nameHasValue = false;
							addOrEdit.IsEnabled = nameHasValue && quantityHasValue;
						}
						break;

					case "quantityTextBox":
						if (quantityTextBox.Text == string.Empty)
						{
							quantityHasValue = false;
							addOrEdit.IsEnabled = nameHasValue && quantityHasValue;
						}
						break;
				}
			}
		}

		private void ZoomOut_Click(object sender, RoutedEventArgs e)
		{
			if (zoomRatio - (Increment.Value / 100) < 0)
			{
				zoomRatio = 0.01;
			}
			else
			{
				zoomRatio -= (Increment.Value / 100);
			}

			imageRectangle.Width = origImgWidth * zoomRatio;
			imageRectangle.Height = origImgHeight * zoomRatio;

			// Check to make sure the image wasn't moved off of the canvas
			if (imgRectY < Convert.ToInt32(imageRectangle.Height) * -1)
			{
				imgRectY = Convert.ToInt32(imageRectangle.Height) * -1;
				Canvas.SetTop(imageRectangle, imgRectY);
			}
			if (imgRectX < Convert.ToInt32(imageRectangle.Width) * -1)
			{
				imgRectX = Convert.ToInt32(imageRectangle.Width) * -1;
				Canvas.SetLeft(imageRectangle, imgRectX);
			}

			UpdateCroppingControls();
		}

		private void ZoomIn_Click(object sender, RoutedEventArgs e)
		{
			if (zoomRatio + (Increment.Value / 100) > 2)
			{
				zoomRatio = 2;
			}
			else
			{
				zoomRatio += (Increment.Value / 100);
			}

			imageRectangle.Width = origImgWidth * zoomRatio;
			imageRectangle.Height = origImgHeight * zoomRatio;

			UpdateCroppingControls();
		}

		private void MoveUp_Click(object sender, RoutedEventArgs e)
		{
			if (imgRectY - Convert.ToInt32(Increment.Value) < Convert.ToInt32(imageRectangle.Height) * -1)
			{
				imgRectY = Convert.ToInt32(imageRectangle.Height) * -1;
			}
			else
			{
				imgRectY -= Convert.ToInt32(Increment.Value);
			}

			Canvas.SetTop(imageRectangle, imgRectY);

			UpdateCroppingControls();
		}

		private void MoveDown_Click(object sender, RoutedEventArgs e)
		{
			if (imgRectY + Convert.ToInt32(Increment.Value) > Convert.ToInt32(croppingCanvas.Height))
			{
				imgRectY = Convert.ToInt32(croppingCanvas.Height);
			}
			else
			{
				imgRectY += Convert.ToInt32(Increment.Value);
			}

			Canvas.SetTop(imageRectangle, imgRectY);

			UpdateCroppingControls();
		}

		private void MoveLeft_Click(object sender, RoutedEventArgs e)
		{
			if (imgRectX - Convert.ToInt32(Increment.Value) < Convert.ToInt32(imageRectangle.Width) * -1)
			{
				imgRectX = Convert.ToInt32(imageRectangle.Width) * -1;
			}
			else
			{
				imgRectX -= Convert.ToInt32(Increment.Value);
			}

			Canvas.SetLeft(imageRectangle, imgRectX);

			UpdateCroppingControls();
		}

		private void MoveRight_Click(object sender, RoutedEventArgs e)
		{
			if (imgRectX + Convert.ToInt32(Increment.Value) > Convert.ToInt32(croppingCanvas.Width))
			{
				imgRectX = Convert.ToInt32(croppingCanvas.Width);
			}
			else
			{
				imgRectX += Convert.ToInt32(Increment.Value);
			}

			Canvas.SetLeft(imageRectangle, imgRectX);

			UpdateCroppingControls();
		}

		private void Increment_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!initializing)
				UpdateCroppingControls();
		}

		/// <summary>
		/// Returns an add or edited miniature to the MainWindow.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AddOrEdit_Click(object sender, RoutedEventArgs e)
		{
			Bitmap uploadedBmp = null;
			int whiteSpaceX = 0;
			int whiteSpaceY = 0;

			// First convert the uploaded image to a Bitmap (for easier manipulation)
			using (MemoryStream ms = new MemoryStream())
			{
				PngBitmapEncoder encoder = new PngBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(uploadedImage));
				encoder.Save(ms);

				using (Bitmap temp = new Bitmap(ms))
				{
					temp.SetResolution(100, 100);
					uploadedBmp = new Bitmap(temp);
				}
			}

			// now crop and scale the image

			// First figure out where the cropping rectangle is in relation to the uploaded 
			// image by figuring out it's size and location scaled up to the uploaded image
			double compoundRatio = scaleRatio > 1 ? 1 * zoomRatio : scaleRatio * zoomRatio;

			int x = Convert.ToInt32(Math.Floor((cropRectX - imgRectX) / compoundRatio));
			int y = Convert.ToInt32(Math.Floor((cropRectY - imgRectY) / compoundRatio));
			int width = Convert.ToInt32(Math.Floor(cropRectWidth / compoundRatio));
			int height = Convert.ToInt32(Math.Floor(cropRectHeight / compoundRatio));

			// this Bitmap is the image for the miniature that is being added. It's size
			// is being set here because the width and height variables are about to be
			// adjusted as needed.
			Bitmap bmp = new Bitmap(width, height);

			// Now adjust the x, y, width, and height variables so that they represent 
			// a the rectangle where the uploaded image rectangle and cropping recangle
			// intersect. 

			// If x is negative, the cropping recangle is too far to the right. Set x to zero 
			// (so that the cropping recangle is within the bounds of the uploaded image rectangle), add
			// whitespace to the left of bmp, and reduce the width accordingly
			if (x < 0)
			{
				whiteSpaceX = Math.Abs(x);
				width -= whiteSpaceX;
				x = 0;
			}
			// if x plus the width of the cropping recangle is greater than the width of the uploaded image rectangle, 
			// the cropping recangle is too wide. Shorten it to fit within the bounds of the uploaded image rectangle.
			else if (x + width > uploadedBmp.Width)
			{ width = uploadedBmp.Width - x; }

			// If y is negative, the cropping recangle is too far down. Set y to zero 
			// (so that the cropping recangle is within the bounds of the uploaded image rectangle), add
			// whitespace to the top of bmp, and reduce the height accordingly
			if (y < 0)
			{
				whiteSpaceY = Math.Abs(y);
				height -= whiteSpaceY;
				y = 0;
			}
			// if y plus the height of the cropping recangle is greater than the height of the uploaded image rectangle, 
			// the cropping recangle is too tall. Shorten it to fit within the bounds of the uploaded image rectangle.
			else if (y + height > uploadedBmp.Height)
			{ height = uploadedBmp.Height - y; }

			// One last check for peace of mind
			if (width > uploadedBmp.Width) { width = uploadedBmp.Width; }
			if (height > uploadedBmp.Height) { height = uploadedBmp.Height; }


			// Using all of the adjusted variables, create the intersection rectangle
			System.Drawing.Rectangle intersectionRect = new System.Drawing.Rectangle(x, y, width, height);

			// Use the intersection rectangle to crop the image out of the uploaded image.
			Bitmap croppedBmp = uploadedBmp.Clone(intersectionRect, uploadedBmp.PixelFormat);

			// Draw the cropped image onto the miniture image, adding whitespace as needed.
			using (Graphics g = Graphics.FromImage(bmp))
			{
				g.DrawImage(croppedBmp, whiteSpaceX, whiteSpaceY);
			}

			// Scale the miniature image down to the required size
			bmp = ScaleBitmapToHeight(bmp, mainWindow.miniWidth, mainWindow.miniHeight);

			MiniatureToAddOrEdit = new Miniature();

			MiniatureToAddOrEdit.Name = miniName;
			MiniatureToAddOrEdit.Quantity = miniQuantity;
			MiniatureToAddOrEdit.FrontImage = bmp;
			MiniatureToAddOrEdit.UploadedImage = uploadedImage;
			MiniatureToAddOrEdit.ZoomRatio = zoomRatio;
			MiniatureToAddOrEdit.XCoordinate = imgRectX;
			MiniatureToAddOrEdit.YCoordinate = imgRectY;

			Close();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			MiniatureToAddOrEdit = null;
			Close();
		}
		#endregion

		private void UpdateCroppingControls()
		{
			SizeLabel.Content = "Size: " + zoomRatio * 100 + "%";
			LocationLabel.Content = "Location: (" + imgRectX + "," + imgRectY + ")";

			ZoomIn.IsEnabled = zoomRatio < 2;
			ZoomOut.IsEnabled = zoomRatio > 0.01;
			MoveLeft.IsEnabled = imgRectX > Convert.ToInt32(imageRectangle.Width) * -1;
			MoveUp.IsEnabled = imgRectY > Convert.ToInt32(imageRectangle.Height) * -1;
			MoveRight.IsEnabled = imgRectX < Convert.ToInt32(croppingCanvas.Width);
			MoveDown.IsEnabled = imgRectY < Convert.ToInt32(croppingCanvas.Height);
		}

		private Bitmap ScaleBitmapToHeight(Bitmap imageToScale, int width, int height)
		{
			Bitmap result = new Bitmap(width, height);

			using (Graphics graphics = Graphics.FromImage(result))
			{
				graphics.DrawImage(imageToScale, 0, 0, width, height);
			}

			return result;
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
	}
}
