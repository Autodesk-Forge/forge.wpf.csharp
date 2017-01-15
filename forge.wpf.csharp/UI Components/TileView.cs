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
using System.Windows;
using System.Windows.Controls;

namespace Autodesk.Forge.WpfCsharp {

	public class TileView : ViewBase {

		public static readonly DependencyProperty ItemContainerStyleProperty =ItemsControl.ItemContainerStyleProperty.AddOwner (typeof (TileView)) ;

		public Style ItemContainerStyle {
			get { return ((Style)GetValue (ItemContainerStyleProperty)) ; }
			set { SetValue (ItemContainerStyleProperty, value) ; }
		}

		public static readonly DependencyProperty ItemTemplateProperty =ItemsControl.ItemTemplateProperty.AddOwner (typeof (TileView)) ;

		public DataTemplate ItemTemplate {
			get { return ((DataTemplate)GetValue (ItemTemplateProperty)) ; }
			set { SetValue (ItemTemplateProperty, value) ; }
		}

		public static readonly DependencyProperty ItemWidthProperty =WrapPanel.ItemWidthProperty.AddOwner (typeof (TileView)) ;

		public double ItemWidth {
			get { return ((double)GetValue (ItemWidthProperty)) ; }
			set { SetValue (ItemWidthProperty, value) ; }
		}

		public static readonly DependencyProperty ItemHeightProperty =WrapPanel.ItemHeightProperty.AddOwner (typeof (TileView)) ;

		public double ItemHeight {
			get { return ((double)GetValue (ItemHeightProperty)) ; }
			set { SetValue (ItemHeightProperty, value) ; }
		}

		protected override object DefaultStyleKey {
			get { return (new ComponentResourceKey (GetType (), "myTileView")) ; }
		}

	}

}
