//
// Copyright (c) 2017 Autodesk, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// by Cyrille Fauvel
// Autodesk Forge Partner Development
//
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

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
