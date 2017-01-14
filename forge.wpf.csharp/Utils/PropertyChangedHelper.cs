using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Autodesk.Forge.WpfCsharp {

	public abstract class PropertyChangedHelper : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged ;

		public void OnPropertyChanged (string name) {
			PropertyChangedEventArgs args =new PropertyChangedEventArgs (name) ;
			OnPropertyChanged (args) ;
		}

		public void OnPropertyChanged (PropertyChangedEventArgs args) {
			if ( PropertyChanged != null )
				PropertyChanged (this, args) ;
		}

		public static PropertyChangedEventArgs CreateArgs<T> (Expression<Func<T, Object>> propertyExpression) {
			return (new PropertyChangedEventArgs (GetNameFromLambda (propertyExpression))) ;
		}

		private static string GetNameFromLambda<T> (Expression<Func<T, object>> propertyExpression) {
			var expr =propertyExpression as LambdaExpression ;
			MemberExpression member =expr.Body is UnaryExpression ? ((UnaryExpression)expr.Body).Operand as MemberExpression : expr.Body as MemberExpression;
			var propertyInfo =member.Member as PropertyInfo ;
			return (propertyInfo.Name) ;
		}

	}

}

// class Test : PropertyChangedHelper {
//	public static readonly PropertyChangedEventArgs StateArgs =PropertyChangedHelper.CreateArgs<Test> (c => c.State) ;
//	private int _State =0 ;
// 	public int State {
// 		get { return (_State) ; }
// 		set {
// 			var oldValue =State ;
// 			_State =value ;
// 			if ( oldValue != value )
// 				OnPropertyChanged (StateArgs) ;
// 		}
// 	}
