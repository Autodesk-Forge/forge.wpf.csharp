using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Autodesk.Forge.WpfCsharp {

	// http://www.codearsenal.net/2013/12/wpf-multibinding-example.html#.WHn-dbYrKEI
	// http://stackoverflow.com/questions/905932/how-can-i-provide-multiple-conditions-for-data-trigger-in-wpf

	public class MultipleBusy : IMultiValueConverter {

		public object Convert (object [] values, Type targetType, object parameter, CultureInfo culture) {
			StateEnum result =StateEnum.Idle ;
			foreach ( StateEnum one in values ) {
				if ( one == StateEnum.Busy )
					result =one ;
			}
			return (result) ;
		}

		public object [] ConvertBack (object value, Type [] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException () ;
		}

	}

}
