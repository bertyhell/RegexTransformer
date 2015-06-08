using System.ComponentModel;
using RegexTransformer.Annotations;

namespace RegexTransformer
{
	public class RegExItem : INotifyPropertyChanged
	{
		private string _search;
		private string _replace;
		private bool _capitalSensitive;
		private bool _useRegex;

		public string Search
		{
			get { return _search; }
			set
			{
				if (value == _search) return;
				_search = value;
				OnPropertyChanged("Search");
			}
		}

		public string Replace
		{
			get { return _replace ?? ""; }
			set
			{
				if (value == _replace) return;
				_replace = value;
				OnPropertyChanged("Replace");
			}
		}

		public bool CapitalSensitive
		{
			get { return _capitalSensitive; }
			set
			{
				if (value.Equals(_capitalSensitive)) return;
				_capitalSensitive = value;
				OnPropertyChanged("CapitalSensitive");
			}
		}

		public bool UseRegex
		{
			get { return _useRegex; }
			set
			{
				if (value.Equals(_useRegex)) return;
				_useRegex = value;
				OnPropertyChanged("UseRegex");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}