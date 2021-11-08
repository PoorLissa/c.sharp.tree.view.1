using System;
using System.Collections.Generic;



public class ini_file_base
{
	// map to hold the data
	Dictionary<string, iniMapData> _dic = null;

	static string iniFileName = null;
	static string exeFileName = null;

	// -------------------------------------------------------------------------------------------------------------------

	public struct iniMapData
	{
		public string origParamName;
		public string origSectionName;
		public string paramData;
		public string comment;
	};

	// -------------------------------------------------------------------------------------------------------------------

	public ini_file_base()
	{
	}

	// -------------------------------------------------------------------------------------------------------------------

	public void read()
	{
		read_dic();
	}

	// -------------------------------------------------------------------------------------------------------------------

	// Get map with all the ini-data
	public Dictionary<string, iniMapData> getDic()
	{
		return null;
	}

	// -------------------------------------------------------------------------------------------------------------------

	// Get parameter by its name
	public string getParam(string name)
	{
		//const std::wstring& operator [] (const std::wstring);

		return "";
	}

	// -------------------------------------------------------------------------------------------------------------------

	// Read ini file as a string, as-is
	private void read_raw(ref string res)
	{
		string fileName = get_ini_path();

		if (fileName.Length > 0)
		{
			res = System.IO.File.ReadAllText(fileName);
		}

		return;
	}

	// -------------------------------------------------------------------------------------------------------------------

	// Read ini file into map
	private void read_dic()
	{
		// Get line bounded by '\n' symbols
		int getLine(ref string src, ref string res, ref int pos)
		{
			res = "";

			while (pos < src.Length)
			{
				if (src[pos] == '\n' || src[pos] == '\r')
					break;
				res += src[pos++];
			}

			pos = (pos == src.Length) ? 0 : pos + 1;

			return pos;
		};

		// Put the data into map
		void mapData(string sectionName, string paramName, string paramData, string comment)
		{
			//trim(paramData, L" \n\r\t");

			string pName = paramName.ToLower();

			var data = new iniMapData();

			data.origSectionName = sectionName;
			data.origParamName = paramName;
			data.paramData = paramData;
			data.comment = comment;

			_dic[pName] = data;
		};

		// ------------------------------------------------------------

		string rawFile = Environment.NewLine;
		read_raw(ref rawFile);

		if (rawFile.Length > 0)
		{
			string line = null;
			int pos = 0;

			string sectionName = null, paramName = null, paramData = null, commentary = null;

			while (getLine(ref rawFile, ref line, ref pos) != 0)
			{
				int len = line.Length;

				// Empty line indicates the end of current record
				if (len == 0)
				{
					mapData(sectionName, paramName, paramData, commentary);

					sectionName = "";
					paramName = "";
					paramData = "";
					commentary = "";
					continue;
				}

				// Commentary
				if (line[0] == '#' || line[0] == ';')
				{
					commentary = (commentary == null || commentary.Length == 0) ? line : commentary + "\n" + line;
					continue;
				}

				// [ marks the beginning of section
				if (line[0] == '[')
				{
					int Pos = line.IndexOf(']');
					sectionName = line.Substring(1, Pos - 1);
					sectionName += ".";
					continue;
				}

				// Text before '=' marks parameter name
				// This is also a point where current record might end
				if (line.IndexOf('=') >= 0)
				{
					// Save current record
					if (paramData.Length >= 0)
					{
						mapData(sectionName, paramName, paramData, commentary);
					}

					// Start the new one
					int Pos = line.IndexOf('=') + 1;

					paramName = line.Substring(0, Pos - 1);
					//trim(paramName, L" \n\r\t");
					paramName = sectionName + paramName;

					paramData = line.Substring(Pos);
					continue;
				}

				// Multiline parameter data:
				{
					paramData += ' ';
					paramData += line;
				}
			}

			if (paramName.Length > 0)
			{
				mapData(sectionName, paramName, paramData, commentary);
			}
		}

		return;
	}

	// -------------------------------------------------------------------------------------------------------------------

	// Get full path to this program's .exe file
	string get_exe_path()
	{
		return get_file_path('e');
	}

	// -------------------------------------------------------------------------------------------------------------------

	// Get full path to this program's .ini file
	string get_ini_path()
	{
		return get_file_path('i');
	}

	// -------------------------------------------------------------------------------------------------------------------

	// Get .exe or .ini fileName
	private string get_file_path(char file)
	{
		string res = null;

		if (iniFileName == null && exeFileName == null)
		{
			exeFileName = AppDomain.CurrentDomain.BaseDirectory + "\\__ini.exe";
			iniFileName = AppDomain.CurrentDomain.BaseDirectory + "\\__ini.ini";
		}

		switch (file)
		{
			case 'i':
				res = iniFileName;
				break;

			case 'e':
				res = exeFileName;
				break;
		}

		return res;
	}

	// -------------------------------------------------------------------------------------------------------------------

	// Trim string on the right and left
	//private void trim(string str, const char*)	{	}
};


#if false

            var ini = new ini_file_base();

            ini.read();

            var aaa = ini.getDic();

            Console.WriteLine(ini.getParam(""));

            Console.ReadKey();

#endif

#if false

 include "stdafx.h"
 include "__ini_file.h"

// -----------------------------------------------------------------------------------------------


// --- INI_FILE_BASE ---


ini_file_base::ini_file_base()
{
}
// -----------------------------------------------------------------------------------------------

ini_file_base::~ini_file_base()
{

}
// -----------------------------------------------------------------------------------------------

// Get full path to this program's .exe file
std::string* ini_file_base::get_exe_path()
{
	return get_file_path('e');
}
// -----------------------------------------------------------------------------------------------

// Get full path to this program's .ini file
std::string* ini_file_base::get_ini_path()
{
	return get_file_path('i');
}
// -----------------------------------------------------------------------------------------------

// Get .exe or .ini fileName
std::string* ini_file_base::get_file_path(const char file)
{
	std::string *res = nullptr;

	static std::string iniFileName("");
	static std::string exeFileName("");

	if (iniFileName.empty() && exeFileName.empty())
	{
		TCHAR path[MAX_PATH];

		int len = GetModuleFileName(NULL, path, MAX_PATH);

		if (len)
		{
			exeFileName = std::string(&path[0], &path[len]);

			std::transform(exeFileName.begin(), exeFileName.end(), exeFileName.begin(), ::tolower);

			int pos = exeFileName.find_last_of('.');

			iniFileName = exeFileName.substr(0, pos + 1) + "ini";
		}
	}

	switch (file)
	{
		case 'i':
			res = &iniFileName;
			break;

		case 'e':
			res = &exeFileName;
			break;
	}

	return res;
}
// -----------------------------------------------------------------------------------------------

// Make file NOT hidden, system or readOnly, because in Windows 7 fstream::open fails for hidden files
// OR:
// Make file hidden
void ini_file_base::makeVisible(const char *file, bool visible)
{
	if (file)
	{
		DWORD attr = GetFileAttributesA(file);

		if (visible)
		{
			if (attr & FILE_ATTRIBUTE_HIDDEN || attr & FILE_ATTRIBUTE_SYSTEM || attr & FILE_ATTRIBUTE_READONLY)
			{
				DWORD removeAttr = FILE_ATTRIBUTE_HIDDEN | FILE_ATTRIBUTE_SYSTEM | FILE_ATTRIBUTE_READONLY;
				SetFileAttributesA(file, attr & ~removeAttr);
			}
		}
		else
		{
			SetFileAttributesA(file, attr | FILE_ATTRIBUTE_HIDDEN);
		}
	}

	return;
}
// -----------------------------------------------------------------------------------------------

// Trim string on the right and left
void ini_file_base::trim(std::wstring &str, const wchar_t *symbols)
{
	size_t pos1 = str.find_first_not_of(symbols, 0);
	size_t pos2 = str.find_last_not_of(symbols);

	if (pos1 == std::string::npos)
		str.clear();

	str = str.substr(pos1, pos2 - pos1 + 1);

	return;
}
// -----------------------------------------------------------------------------------------------

// Read ini file as a string, as-is
void ini_file_base::read_raw(std::wstring &res)
{
	res.clear();

	std::string *fileName = get_ini_path();

	if (fileName->length())
	{
		std::wfstream file;
		std::wstring  line;

		// Make .ini NOT hidden, system or readOnly, because in Windows 7 fstream::open fails for hidden files:
		makeVisible(fileName->c_str(), true);

		// If the file exists, we read data from it. Otherwise, we create it.
		file.open(*fileName, std::wfstream::in | std::wfstream::app);

		if (file.is_open())
		{
			//file.imbue(std::locale("rus_rus.866"));
			file.imbue(std::locale("rus_rus.1251"));

			while (std::getline(file, line))
			{
				res += line;
				res += '\n';
			}

			file.close();

			// Make .ini hidden
			makeVisible(fileName->c_str(), false);
		}
	}

	return;
}
// -----------------------------------------------------------------------------------------------

// Read ini file into map
void ini_file_base::read_map()
{
	// Get line bounded by '\n' symbols
	auto getLine = [](std::wstring &src, std::wstring &res, size_t &pos) -> size_t
	{
		res.clear();

		while(pos < src.length())
		{
			if (src[pos] == '\n')
				break;
			res += src[pos++];
		}

		pos = (pos == src.length()) ? 0u : pos + 1;

		return pos;
	};

	// Put the data into map
	auto mapData = [&](std::wstring sectionName, std::wstring paramName, std::wstring paramData, std::wstring comment)
	{
		trim(paramData, L" \n\r\t");

		std::wstring pName(paramName);

		std::transform(pName.begin(), pName.end(), pName.begin(),
			[](unsigned char c) {
				return ::tolower(c); 
			}
		);

		iniMapData data;

		data.origSectionName = sectionName;
		data.origParamName	 = paramName;
		data.paramData		 = paramData;
		data.comment		 = comment;

		// wtf?..
		if (!_map.count(pName))
			_map[pName].paramData += paramData;

		_map[pName] = data;
	};

	// ---------------------------------------

	std::wstring rawFile;
	read_raw(rawFile);

	if (rawFile.length())
	{
		std::wstring line;
		size_t pos = 0u;

		std::wstring sectionName, paramName, paramData, commentary;

		while (getLine(rawFile, line, pos))
		{
			size_t len = line.length();

			// Empty line indicates the end of current record
			if (!len)
			{
				mapData(sectionName, paramName, paramData, commentary);

				sectionName.clear();
				paramName.clear();
				paramData.clear();
				commentary.clear();
				continue;
			}

			// Commentary
			if (line[0] == '#' || line[0] == ';')
			{
				commentary = commentary.empty() ? line : commentary + L"\n" + line;
				//commentary = line;
				continue;
			}

			// [ marks the beginning of section
			if (line[0] == '[')
			{
				size_t Pos = line.find_first_of(']');
				sectionName = line.substr(1, Pos-1);
				sectionName += L".";
				continue;
			}

			// Text before '=' marks parameter name
			// This is also a point where current record might end
			if (line.find('=') != std::string::npos)
			{
				// Save current record
				if (paramData.length())
				{
					mapData(sectionName, paramName, paramData, commentary);
				}

				// Start the new one
				size_t Pos = line.find_first_of('=') + 1;

				paramName = line.substr(0, Pos - 1);
				trim(paramName, L" \n\r\t");
				paramName = sectionName + paramName;

				paramData = line.substr(Pos);
				continue;
			}

			// Multiline parameter data:
			{
				paramData += ' ';
				paramData += line;
			}
		}

		if (!paramName.empty())
		{
			mapData(sectionName, paramName, paramData, commentary);
		}
	}

	return;
}
// -----------------------------------------------------------------------------------------------

// Get the map with all the ini-data
const std::map<std::wstring, ini_file_base::iniMapData> & ini_file_base::getMap() const
{
	return _map;
}
// -----------------------------------------------------------------------------------------------

// Get parameter by its name
const std::wstring& ini_file_base::operator[](const std::wstring paramName)
{
	std::wstring param(paramName);

	std::transform(param.begin(), param.end(), param.begin(),
		[](unsigned char c) {
			return ::tolower(c);
		}
	);

	return _map[param].paramData;
}
// -----------------------------------------------------------------------------------------------

void ini_file_base::read()
{
	read_map();

	return;
}
// -----------------------------------------------------------------------------------------------
// -----------------------------------------------------------------------------------------------
// -----------------------------------------------------------------------------------------------


// --- INI_FILE ---


// Read data from the .ini-file
int ini_file::read_ini_file(bool doReadProfiles /*default=false*/)
{
	return 1;
}
// -----------------------------------------------------------------------------------------------

ini_file::ini_file() : ini_file_base()
{

}
// -----------------------------------------------------------------------------------------------

ini_file::~ini_file()
{

}
// -----------------------------------------------------------------------------------------------


#endif