using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Shapes;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace PaperMiniMaker
{
	/// <summary>
	/// Interaction logic for ImageCropper.xaml
	/// </summary>
	public partial class ImageCropper : Window
	{
		public Miniature MiniatureToAdd { get; set; }

		public ImageCropper(double aspectRatio, bool heightByWidth, int miniHeight, List<Miniature> miniatures)
		{
			InitializeComponent();
		}
	}

	/// <summary>
	/// Provides a Canvas where a rectangle will be drawn
	/// that matches the selection area that the user drew
	/// on the canvas using the mouse
	/// </summary>
	public partial class SelectionCanvas : Canvas
	{
		#region Instance Fields
		private Point mouseLeftDownPoint;
		private Style cropperStyle;
		public Shape rubberBand = null;
		public readonly RoutedEvent CropImageEvent;
		#endregion

		#region Events
		/// <summary>
		/// Raised when the user has drawn a selection area
		/// </summary>
		public event RoutedEventHandler CropImage
		{
			add { AddHandler(this.CropImageEvent, value); }
			remove { RemoveHandler(this.CropImageEvent, value); }
		}
		#endregion

		#region Ctor
		/// <summary>
		/// Constructs a new SelectionCanvas, and registers the
		/// CropImage event
		/// </summary>
		public SelectionCanvas()
		{
			this.CropImageEvent = EventManager.RegisterRoutedEvent("CropImage",
				RoutingStrategy.Bubble,
				typeof(RoutedEventHandler),
				typeof(SelectionCanvas));
		}
		#endregion

		#region Public Properties
		public Style CropperStyle
		{
			get { return cropperStyle; }
			set { cropperStyle = value; }
		}
		#endregion

		#region Overrides
		/// <summary>
		/// Captures the mouse
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			if (!IsMouseCaptured)
			{
				mouseLeftDownPoint = e.GetPosition(this);
				CaptureMouse();
			}
		}

		/// <summary>
		/// Releases the mouse, and raises the CropImageEvent
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonUp(e);
			if (IsMouseCaptured && rubberBand != null)
			{
				ReleaseMouseCapture();
				RaiseEvent(new RoutedEventArgs(CropImageEvent, this));
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (IsMouseCaptured)
			{
				Point currentPoint = e.GetPosition(this);

				if (rubberBand == null)
				{
					rubberBand = new Rectangle();

					if (cropperStyle != null)
						rubberBand.Style = cropperStyle;

					Children.Add(rubberBand);
				}

				double width = Math.Abs(mouseLeftDownPoint.X - currentPoint.X);
				double height = Math.Abs(mouseLeftDownPoint.Y - currentPoint.Y);
				double left = Math.Min(mouseLeftDownPoint.X, currentPoint.X);
				double top = Math.Min(mouseLeftDownPoint.Y, currentPoint.Y);

				rubberBand.Width = width;
				rubberBand.Height = height;
				SetLeft(rubberBand, left);
				SetTop(rubberBand, top);
			}
		}
		#endregion
	}
}
