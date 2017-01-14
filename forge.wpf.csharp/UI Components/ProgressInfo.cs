using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodesk.Forge.WpfCsharp {

	public class ProgressInfo {

		public int pct { get; set; }
		public string msg { get; set; }

		public ProgressInfo (int pctValue, string msgValue) {
			pct =pctValue ;
			msg =msgValue ;
		}

	}

}
