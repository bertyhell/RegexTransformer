using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RegexTransformer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly MainController _controller;

		public MainWindow()
		{
			InitializeComponent();

			_controller = new MainController(this);
			DataContext = _controller;
		}

		private void ExecuteButtonClick(object sender, RoutedEventArgs e)
		{
			_controller.Execute();
		}

		private void SaveRegexList(object sender, RoutedEventArgs routedEventArgs)
		{
			_controller.SaveRegexFile();
		}

		private void LoadRegexList(object sender, RoutedEventArgs routedEventArgs)
		{
			_controller.LoadRegexFile();
		}

		/// <summary>
		/// DraggedItem Dependency Property
		/// </summary>
		public static readonly DependencyProperty DRAGGED_ITEM_PROPERTY =
			DependencyProperty.Register("DraggedItem", typeof(RegExItem), typeof(MainWindow));

		/// <summary>
		/// Gets or sets the DraggedItem property.  This dependency property
		/// indicates ....
		/// </summary>
		public RegExItem DraggedItem
		{
			get { return (RegExItem)GetValue(DRAGGED_ITEM_PROPERTY); }
			set { SetValue(DRAGGED_ITEM_PROPERTY, value); }
		}


		public bool IsEditing { get; set; }

		private void OnBeginEdit(object sender, DataGridBeginningEditEventArgs e)
		{
			IsEditing = true;
			//in case we are in the middle of a drag/drop operation, cancel it...
			if (IsDragging) ResetDragDrop();
		}

		private void OnEndEdit(object sender, DataGridCellEditEndingEventArgs e)
		{
			IsEditing = false;
		}

		private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			//exit if in edit mode
			if (IsEditing) return;

			//find the clicked row
			var row = UIHelpers.TryFindFromPoint<DataGridRow>((UIElement)sender, e.GetPosition(RegexDataGrid));
			if (row == null || !(row.Item is RegExItem)) return;

			//set flag that indicates we're capturing mouse movements
			IsDragging = true;
			DraggedItem = (RegExItem)row.Item;

		}

		/// <summary>
		/// Moves the popup icon.
		/// </summary>
		private void OnMouseMove(object sender, MouseEventArgs e)
		{
			if (!IsDragging || e.LeftButton != MouseButtonState.Pressed) return;

			//display the popup if it hasn't been opened yet
			if (!DragPopup.IsOpen)
			{
				//switch to read-only mode
				RegexDataGrid.IsReadOnly = true;

				//make sure the popup is visible
				DragPopup.IsOpen = true;
			}


			Size popupSize = new Size(DragPopup.ActualWidth, DragPopup.ActualHeight);
			DragPopup.PlacementRectangle = new Rect(e.GetPosition(this), popupSize);

			//make sure the row under the grid is being selected
			Point position = e.GetPosition(RegexDataGrid);
			var row = UIHelpers.TryFindFromPoint<DataGridRow>(RegexDataGrid, position);
			if (row != null) RegexDataGrid.SelectedItem = row.Item;
		}

		/// <summary>
		/// Completes a drag/drop operation.
		/// </summary>
		private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (!IsDragging || IsEditing)
			{
				return;
			}

			//get the target item
			if (!(RegexDataGrid.SelectedItem is RegExItem))
			{
				return;
			}

			RegExItem targetItem = (RegExItem)RegexDataGrid.SelectedItem;

			if (targetItem == null || !ReferenceEquals(DraggedItem, targetItem))
			{
				//remove the source from the list
				_controller.RegularExpressions.Remove(DraggedItem);

				//get target index
				var targetIndex = _controller.RegularExpressions.IndexOf(targetItem);

				//move source at the target's location
				_controller.RegularExpressions.Insert(targetIndex, DraggedItem);

				//select the dropped item
				RegexDataGrid.SelectedItem = DraggedItem;
			}

			//reset
			ResetDragDrop();
		}

		/// <summary>
		/// Closes the popup and resets the
		/// grid to read-enabled mode.
		/// </summary>
		private void ResetDragDrop()
		{
			IsDragging = false;
			DragPopup.IsOpen = false;
			RegexDataGrid.IsReadOnly = false;
		}

		public bool IsDragging { get; set; }

		private void RegexTutorialClick(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start("http://regexone.com/");
		}

		private void ReferenceClick(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start("http://msdn.microsoft.com/en-us/library/az24scfc(v=vs.110).aspx");
		}
	}
}
