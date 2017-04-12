using System;
using DBreeze;
using System.IO;
using System.Diagnostics;

namespace XOFF.DBreeze
{

	public class DBReezeConnectionProvider : IDBreezeConnectionProvider, IDisposable
	{
		DBreezeEngine _engine;
		readonly string _path;

		public DBReezeConnectionProvider()
		{

			var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			 var libraryPath = Path.Combine(documentsPath, "..", "Library");
			_path = Path.Combine(libraryPath, "DBreezeFiles");
		}

		//public DBReezeConnectionProvider(string path = null)
		//{
		//	if (path == null)
		//	{
		//		var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
		//		var libraryPath = Path.Combine(documentsPath, "..", "Library");
		//		_path = Path.Combine(libraryPath, "DBreezeFiles");
		//	}

		//	_path = path;
		//}




		public DBreezeEngine Engine
		{
			get
			{
				if (_engine == null)
				{
					

					_engine = new DBreezeEngine(_path);
				}
				return _engine;
			}
		}

		public void Dispose()
		{
			Debug.WriteLine("Dispose Of Engine");
			_engine.Dispose();
		}
	}
}