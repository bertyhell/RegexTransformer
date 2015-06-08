using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using IvanAkcheurov.Commons;
using Microsoft.Win32;
using RegexTransformer.Annotations;

namespace RegexTransformer
{
	class MainController : INotifyPropertyChanged
	{
		private readonly MainWindow _mainWindow;
		private readonly IHighlightingDefinition _pythonDefinition;

		readonly Dictionary<string, string> _languageMappings = new Dictionary<string, string>
			                       {
				                       {"c", "C++"},
				                       {"c++", "C++"},
				                       {"csharp", "C#"},
				                       {"css", "CSS"},
				                       {"html", "HTML"},
				                       {"javascript", "Javascript"},
				                       {"python", "Python"}
			                       };

		public MainController(MainWindow mainWindow)
		{
			_mainWindow = mainWindow;
			RegularExpressions = new ObservableCollection<RegExItem>();

			_mainWindow.OrignalTextEditor.TextChanged += OrignalTextEditorTextChanged;

			// Load python syntax highlighting
			string projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if (projectPath == null)
			{
				throw new DirectoryNotFoundException("Could not find the training-set folder.");
			}

			string templatePath = Path.Combine(projectPath, "AvalonEdit\\Templates");

			_pythonDefinition = HighlightingLoader.Load(
				new XmlTextReader(templatePath + "\\ICSharpCode.PythonBinding.Resources.Python.xshd"),
				HighlightingManager.Instance);

#if DEBUG
			OriginalText =
				"A robot may not injure a human being or, through inaction, allow a human being to come to harm.\r\n" +
				"A robt must obey the orders given to it by human beings, except where such orders would conflict with the First Law.\r\n" +
				"A rbot must protect its own existence as long as such protection does not conflict with the First or Second Law.";

			RegularExpressions.Add(new RegExItem { Search = "[robot]{4,}", Replace = "robot", UseRegex = false, CapitalSensitive = true });
			RegularExpressions.Add(new RegExItem { Search = ",", Replace = ";", UseRegex = false, CapitalSensitive = true });
			RegularExpressions.Add(new RegExItem { Search = "^", Replace = "System.Write(\"", UseRegex = true, CapitalSensitive = true });
			RegularExpressions.Add(new RegExItem { Search = "$", Replace = "\");", UseRegex = true, CapitalSensitive = true });
			RegularExpressions.Add(new RegExItem { Search = ".", Replace = "<br>", UseRegex = false, CapitalSensitive = true });
#endif
		}

		private void OrignalTextEditorTextChanged(object sender, EventArgs e)
		{
			if (OriginalText.IsNullOrEmpty())
			{
				SetEditorLanguage("text");
			}
			else
			{
				// avoid parsing too many characters
				SetEditorLanguage(
					CodeClassifier.CodeClassifier.Classify(OriginalText.Substring(0, Math.Min(OriginalText.Length, 5000))));
			}
		}

		private void SetEditorLanguage(string language)
		{
			if (language == null)
			{
				_mainWindow.OrignalTextEditor.SyntaxHighlighting = null;
				return;
			}

			Console.WriteLine("best language: " + language);

			// Editor supports: ASP.NET, Boo, Coco/R grammars, C++, C#, HTML, Java, JavaScript, Patch files, PHP, TeX, VB, XML
			if (language == "python")
			{
				_mainWindow.OrignalTextEditor.SyntaxHighlighting = _pythonDefinition;
			}
			else if (_languageMappings.ContainsKey(language))
			{
				_mainWindow.OrignalTextEditor.SyntaxHighlighting =
					HighlightingManager.Instance.GetDefinition(_languageMappings[language]);
			}
			else
			{
				_mainWindow.OrignalTextEditor.SyntaxHighlighting = null;
			}
		}

		public string OriginalText
		{
			get { return _mainWindow.OrignalTextEditor.Text; }
			set
			{
				_mainWindow.OrignalTextEditor.Clear();
				_mainWindow.OrignalTextEditor.AppendText(value);
			}
		}

		public string TransformedText
		{
			get { return _mainWindow.TransformedTextEditor.Text; }
			set
			{
				_mainWindow.TransformedTextEditor.Clear();
				_mainWindow.TransformedTextEditor.AppendText(value);
			}
		}

		public ObservableCollection<RegExItem> RegularExpressions { get; set; }


		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		public void Execute()
		{
			//TransformedText = Regex.Replace(OriginalText, "(\n|\r|\r\n)", "\n", RegexOptions.Singleline);
			var transformedText = OriginalText;
			foreach (RegExItem regularExpression in RegularExpressions)
			{
				string search = regularExpression.UseRegex ? regularExpression.Search : Regex.Escape(regularExpression.Search);
				if (search.EndsWith("$") && regularExpression.UseRegex)
				{
					search = search.Substring(0, search.Length - 1) + "[\r\n]*$";
				}
				string replace = regularExpression.UseRegex ? regularExpression.Replace : Regex.Escape(regularExpression.Replace);

				transformedText = Regex.Replace(transformedText, search, replace, (regularExpression.CapitalSensitive ? RegexOptions.None : RegexOptions.IgnoreCase) | RegexOptions.Multiline);
			}
			TransformedText = transformedText;
		}

		public void SaveRegexFile()
		{
			SaveFileDialog dlg = new SaveFileDialog
								 {
									 FileName = "regular_expressions_" + DateTime.Today.ToString("d").Replace("/", "-"),
									 DefaultExt = ".ret",
									 Filter = "Regex Transform Files (.ret)|*.ret"
								 };
			bool? result = dlg.ShowDialog();
			if (result == true)
			{
				XmlConverter.Serialize(dlg.FileName, RegularExpressions.ToArray());
			}
		}

		public void LoadRegexFile()
		{
			OpenFileDialog dlg = new OpenFileDialog
			{
				DefaultExt = ".ret",
				Filter = "Regex Transform Files (.ret)|*.ret"
			};
			bool? result = dlg.ShowDialog();
			if (result == true)
			{
				RegularExpressions.Clear();
				foreach (RegExItem regExItem in (RegExItem[])XmlConverter.Deserialize(dlg.FileName, RegularExpressions.ToArray().GetType()))
				{
					RegularExpressions.Add(regExItem);
				}
			}
		}
	}
}
