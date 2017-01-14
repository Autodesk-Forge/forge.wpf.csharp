using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Autodesk.Forge.WpfCsharp {

	public class WaitCursor : IDisposable {
		private Cursor _previousCursor;

		public WaitCursor () {
			_previousCursor =Mouse.OverrideCursor ;
			Mouse.OverrideCursor =Cursors.Wait ;
		}

		#region IDisposable Members

		public void Dispose () {
			Mouse.OverrideCursor =_previousCursor ;
		}

		#endregion
	}

	//using ( new WaitCursor () ) {
	//	// very long task
	//}
}
