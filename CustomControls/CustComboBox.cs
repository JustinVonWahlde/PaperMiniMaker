using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Drawing.Design;

namespace PaperMiniMaker.CustomControls
{
	[DefaultEvent("OnSelectedIndexChanged")]
	class CustComboBox : UserControl
	{
		// Fields
		private Color backColor = Color.WhiteSmoke;
		private Color iconColor = Color.MediumSlateBlue;
		private Color listBackColor = Color.FromArgb(230, 228, 245);
		private Color listTextColor = Color.DimGray;
		private Color borderColor = Color.MediumSlateBlue;
		private int borderSize = 1;

		// Items
		private ComboBox cmbList;
		private Label lblText;
		private Button btnIcon;

		#region Properties
		[Category("CustomComboBox - Appearance")]
		public new Color BackColor
		{
			get => backColor;
			set
			{
				backColor = value;
				lblText.BackColor = backColor;
				btnIcon.BackColor = backColor;
			}
		}

		[Category("CustomComboBox - Appearance")]
		public Color IconColor
		{
			get => iconColor;
			set
			{
				iconColor = value;
				btnIcon.Invalidate(); // Redraw icon
			}
		}

		[Category("CustomComboBox - Appearance")]
		public Color ListBackColor
		{
			get => listBackColor;
			set
			{
				listBackColor = value;
				cmbList.BackColor = listBackColor;
			}
		}

		[Category("CustomComboBox - Appearance")]
		public Color ListTextColor
		{
			get => listTextColor;
			set
			{
				listTextColor = value;
				cmbList.ForeColor = listTextColor;
			}
		}

		[Category("CustomComboBox - Appearance")]
		public Color BorderColor
		{
			get => borderColor;
			set
			{
				borderColor = value;
				base.BackColor = borderColor;
			}
		}

		[Category("CustomComboBox - Appearance")]
		public int BorderSize
		{
			get => borderSize;
			set
			{
				borderSize = value;
				Padding = new Padding(borderSize);
				AdjustComboBoxDimensions();
			}
		}

		[Category("CustomComboBox - Appearance")]
		public override Color ForeColor
		{
			get => base.ForeColor;
			set
			{
				base.ForeColor = value;
				lblText.ForeColor = value;
			}

		}

		[Category("CustomComboBox - Appearance")]
		public override Font Font
		{
			get => base.Font;
			set
			{
				base.Font = value;
				lblText.Font = value;
				cmbList.Font = value;
			}
		}

		[Category("CustomComboBox - Appearance")]
		public string Texts
		{
			get { return lblText.Text; }
			set { lblText.Text = value; }
		}

		[Category("CustomComboBox - Appearance")]
		public ComboBoxStyle DropDownStyle
		{
			get { return cmbList.DropDownStyle; }
			set
			{
				if (cmbList.DropDownStyle != ComboBoxStyle.Simple)
					cmbList.DropDownStyle = value;
			}
		}

		[Category("CustomComboBox - Data")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
		[Localizable(true)]
		[MergableProperty(false)]
		public ComboBox.ObjectCollection Items { get { return cmbList.Items; } }

		[Category("CustomComboBox - Data")]
		[AttributeProvider(typeof(IListSource))]
		[DefaultValue(null)]
		[RefreshProperties(RefreshProperties.Repaint)]
		public object DataSource
		{
			get { return cmbList.DataSource; }
			set { cmbList.DataSource = value; }
		}

		[Category("CustomComboBox - Data")]
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
		[EditorBrowsable(EditorBrowsableState.Always)]
		[Localizable(true)]
		public AutoCompleteStringCollection AutoCompleteCustomSource
		{
			get { return cmbList.AutoCompleteCustomSource; }
			set { cmbList.AutoCompleteCustomSource = value; }
		}

		[Category("CustomComboBox - Data")]
		[Browsable(true)]
		[DefaultValue(AutoCompleteSource.None)]
		[EditorBrowsable(EditorBrowsableState.Always)]
		public AutoCompleteSource AutoCompleteSource
		{
			get { return cmbList.AutoCompleteSource; }
			set { cmbList.AutoCompleteSource = value; }
		}

		[Category("CustomComboBox - Data")]
		[Browsable(true)]
		[DefaultValue(AutoCompleteMode.None)]
		[EditorBrowsable(EditorBrowsableState.Always)]
		public AutoCompleteMode AutoCompleteMode
		{
			get { return cmbList.AutoCompleteMode; }
			set { cmbList.AutoCompleteMode = value; }
		}

		[Category("CustomComboBox - Data")]
		[Bindable(true)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object SelectedItem
		{
			get { return cmbList.SelectedItem; }
			set { cmbList.SelectedItem = value; }
		}

		[Category("CustomComboBox - Data")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectedIndex
		{
			get { return cmbList.SelectedIndex; }
			set { cmbList.SelectedIndex = value; }
		}
		#endregion

		// Events
		public event EventHandler OnSelectedIndexChanged; // Default event

		// Constructor
		public CustComboBox()
		{
			cmbList = new ComboBox();
			lblText = new Label();
			btnIcon = new Button();
			SuspendLayout();

			// ComboBox: Dropdown list
			cmbList.BackColor = listBackColor;
			cmbList.ForeColor = listTextColor;
			cmbList.Font = new Font(Font.Name,10F);
			cmbList.SelectedIndexChanged += new EventHandler(ComboBox_SelectedIndexChanged); // Default event
			cmbList.TextChanged += new EventHandler(ComboBox_TextChanged); // Refresh text

			// Button: Icon
			btnIcon.Dock = DockStyle.Right;
			btnIcon.FlatStyle = FlatStyle.Flat;
			btnIcon.FlatAppearance.BorderSize = 0;
			btnIcon.BackColor = backColor;
			btnIcon.Size = new Size(30, 30);
			btnIcon.Cursor = Cursors.Hand;
			btnIcon.Click += new EventHandler(Icon_Click); // Open Dropdown list
			btnIcon.Paint += new PaintEventHandler(Icon_Paint); // Draw Icon

			// Label: Text
			lblText.Dock = DockStyle.Fill;
			lblText.AutoSize = false;
			lblText.BackColor = backColor;
			lblText.TextAlign = ContentAlignment.MiddleLeft;
			lblText.Padding = new Padding(8, 0, 0, 0);
			lblText.Font = new Font(Font.Name, 10F);
			lblText.Click += new EventHandler(Surface_Click);

			// User Control
			Controls.Add(lblText); // 2
			Controls.Add(btnIcon); // 1
			Controls.Add(cmbList); // 0

			MinimumSize = new Size(75, 20);
			Size = new Size(75, 20);
			ForeColor = listTextColor;
			Padding = new Padding(borderSize);
			base.BackColor = borderColor;
			ResumeLayout();
			AdjustComboBoxDimensions();

		}

		// Private Methods
		private void AdjustComboBoxDimensions()
		{
			cmbList.Width = lblText.Width;
			cmbList.Location = new Point()
			{
				X = Width - Padding.Right - cmbList.Width,
				Y = lblText.Bottom - cmbList.Height
			};
		}

		#region Event Methods
		// Default Event
		private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (OnSelectedIndexChanged != null)
				OnSelectedIndexChanged.Invoke(sender, e);

			// Refresh text
			lblText.Text = cmbList.Text;
		}

		private void Icon_Paint(object sender, PaintEventArgs e)
		{
			// Fields
			int iconWidth = 14;
			int iconHeight = 6;
			var rectIcon = new Rectangle((btnIcon.Width - iconWidth) / 2, (btnIcon.Height - iconHeight) / 2, iconWidth, iconHeight);
			Graphics graph = e.Graphics;

			// Draw arrow down icon
			using (GraphicsPath path = new GraphicsPath())
			{
				using (Pen pen = new Pen(iconColor, 2))
				{
					graph.SmoothingMode = SmoothingMode.AntiAlias;
					path.AddLine(rectIcon.X, rectIcon.Y, rectIcon.X + (iconWidth / 2), rectIcon.Bottom);
					path.AddLine(rectIcon.X + (iconWidth / 2), rectIcon.Bottom, rectIcon.Right, rectIcon.Y);
					graph.DrawPath(pen, path);
				}
			}
		}

		private void Icon_Click(object sender, EventArgs e)
		{
			// Open dropdown list
			cmbList.Select();
			cmbList.DroppedDown = true;
		}

		private void Surface_Click(object sender, EventArgs e)
		{
			// Select combo box
			cmbList.Select();
			if (cmbList.DropDownStyle == ComboBoxStyle.DropDownList)
				cmbList.DroppedDown = true; // Open dropdown list
		}

		private void ComboBox_TextChanged(object sender, EventArgs e)
		{
			// Refresh text
			lblText.Text = cmbList.Text;
		}
		#endregion
	}
}
