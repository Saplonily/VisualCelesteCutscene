using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CelesteDialog;

namespace VisualCelesteCutscene;

public sealed class DialogFileService
{
	public DialogFileService()
	{

	}

	public DialogDocument ReadFrom(string file)
	{
		using FileStream fs = new(file, FileMode.Open, FileAccess.Read);
		return new(fs);
	}
}
