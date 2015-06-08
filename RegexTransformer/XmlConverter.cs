using System;
using System.IO;
using System.Xml.Serialization;

namespace RegexTransformer
{
	public static class XmlConverter
	{
		public static void Serialize(string filename, object data)
		{
			StreamWriter fileWriter = null;
			try
			{
				fileWriter = new StreamWriter(filename);
				XmlSerializer ser = new XmlSerializer(data.GetType());
				ser.Serialize(fileWriter, data);

				fileWriter.Close();
			}
			finally
			{
				if (fileWriter != null) fileWriter.Close();
			}
		}


		public static object Deserialize(string filename, Type datatype)
		{
			object result;
			StreamReader fileReader = null;
			try
			{
				XmlSerializer ser = new XmlSerializer(datatype);
				fileReader = new StreamReader(filename);
				result = ser.Deserialize(fileReader);
			}
			finally
			{
				if (fileReader != null) fileReader.Close();
			}
			return result;
		}
	}
}
