using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace PaperMiniMaker
{
	/// <summary>
	/// Interaction logic for EditMiniature.xaml
	/// </summary>
	public partial class EditMiniature : Window
	{
		public Miniature editedMiniature { get; set; }
		private Miniature miniatureToEdit;
		private readonly List<Miniature> miniatures;
		private int maxQty;

		// validation variables
		private bool nameHasValue = true;
		private bool quantityHasValue;

		public EditMiniature(Miniature _miniatureToEdit, List<Miniature> _miniatures, int _maxQty)
		{
			miniatureToEdit = _miniatureToEdit;
			miniatures = _miniatures;
			maxQty = _maxQty;

			InitializeComponent();

			nameTextBox.Text = miniatureToEdit.Name;
			quantityTextBox.Text = miniatureToEdit.Quantity.ToString();
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
				edit.IsEnabled = nameHasValue && quantityHasValue;
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
					edit.IsEnabled = nameHasValue && quantityHasValue;
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
						edit.IsEnabled = nameHasValue && quantityHasValue;
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
					foreach (Miniature m in miniatures)
					{
						if (m.Name == nameTextBox.Text)
						{
							unique = false;
							MessageBox.Show("Name must be unique.", "Invalid Name");
						}
					}

					if (unique)
					{
						miniatureToEdit.Name = nameTextBox.Text;
					}
					else
					{
						nameTextBox.Text = string.Empty;
						nameTextBox.Focus();
					}

					// enable add button if both textboxes have values
					nameHasValue = nameTextBox.Text != string.Empty;
					edit.IsEnabled = nameHasValue && quantityHasValue;

					break;

				case "quantityTextBox":
					int.TryParse(quantityTextBox.Text, out int qty);

					if (qty > maxQty)
					{
						quantityTextBox.Text = maxQty.ToString();
						miniatureToEdit.Quantity = maxQty;
						Dispatcher.BeginInvoke(new Action(() => MessageBox.Show(string.Format("Quantity must be less than or equal to {0}", maxQty), "Invalid Quantity")));
					}
					else
					{
						miniatureToEdit.Quantity = qty;
					}

					// enable add button if both textboxes have values
					quantityHasValue = quantityTextBox.Text != string.Empty;
					edit.IsEnabled = nameHasValue && quantityHasValue;

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
				switch (((FrameworkElement)sender).Name)
				{
					case "nameTextBox":
						if (nameTextBox.Text == string.Empty)
						{
							nameHasValue = false;
							edit.IsEnabled = nameHasValue && quantityHasValue;
						}
						break;

					case "quantityTextBox":
						if (quantityTextBox.Text == string.Empty)
						{
							quantityHasValue = false;
							edit.IsEnabled = nameHasValue && quantityHasValue;
						}
						break;
				}
			}
		}

		/// <summary>
		/// Returns a miniature to the MainWindow.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Edit_Click(object sender, RoutedEventArgs e)
		{
			editedMiniature = new Miniature();

			editedMiniature.Name = miniatureToEdit.Name;
			editedMiniature.Quantity = miniatureToEdit.Quantity;
			editedMiniature.FrontImage = miniatureToEdit.FrontImage;

			Close();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			miniatureToEdit = null;
			Close();
		}
		#endregion
	}
}
