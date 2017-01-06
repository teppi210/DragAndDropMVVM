﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace DragAndDropMVVM.Behavior
{
    public class FrameworkElementDropBehavior : Behavior<FrameworkElement>
    {


        private Type _dataType = typeof(DraggingAdorner); //the type of the data that can be dropped into this control
        private DroppingAdorner _adorner;

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.AllowDrop = true;
            this.AssociatedObject.DragEnter += AssociatedObject_DragEnter;
            this.AssociatedObject.DragOver += AssociatedObject_DragOver;
            this.AssociatedObject.DragLeave += AssociatedObject_DragLeave;
            this.AssociatedObject.Drop += AssociatedObject_Drop;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            this.AssociatedObject.DragEnter -= AssociatedObject_DragEnter;
            this.AssociatedObject.DragOver -= AssociatedObject_DragOver;
            this.AssociatedObject.DragLeave -= AssociatedObject_DragLeave;
            this.AssociatedObject.Drop -= AssociatedObject_Drop;
        }

        private void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            if (_dataType != null)
            {
                //if the data type can be dropped 
                if (e.Data.GetDataPresent(_dataType))
                {

                    ////drop the data
                    //IDropable target = this.AssociatedObject.DataContext as IDropable;
                    //target.Drop(source.DataContext);

                    ////remove the data from the source
                    //IDragable sourcevm = source.DataContext as IDragable;
                    //sourcevm.Drag(e.Data.GetData(_dataType));
                    UIElement element = sender as FrameworkElement;

                    ICommand dropcommand = GetDropCommand(element);

                    if (dropcommand != null)
                    {
                        object parameter = (GetDropCommandParameter(element) ??
                            (e.Data.GetDataPresent(DataFormats.Serializable) ? e.Data.GetData(DataFormats.Serializable) : null));
                            //??
                           // this.AssociatedObject.DataContext;

                        if(dropcommand.CanExecute(parameter))
                        {

                            Point point = e.GetPosition(element);

                            System.Diagnostics.Debug.WriteLine($"{nameof(AssociatedObject_Drop)} Current Point : X:{point.X} Y:{point.Y}");

                            ////TODO: Add the 
                            Canvas droppedcanvas = GetDroppedCanvas(element);

                            // if copy the 
                            if (!GetIsFixedPosition(element) && droppedcanvas != null)
                            {
                                var droppedcontroltype = GetDroppedControlType(element);


                                if (droppedcontroltype != null && !WPFUtil.IsCorrectType(droppedcontroltype, typeof(ContentControl)))
                                {
                                    throw new ArgumentException("DroppedControlType is base on ContentControl.");
                                }

                                if (e.Data.GetDataPresent(_dataType))
                                {
                                    var adn = e.Data.GetData(_dataType) as DraggingAdorner;
                                    //System.Diagnostics.Debug.WriteLine($"{nameof(DraggingAdorner)} Current Point : X:{adn.Position.X} Y:{adn.Position.Y}");
                                   var clnele = adn.GetGhostElement() as UIElement;

                                    if(droppedcontroltype != null)
                                    {
                                        clnele = CreateNewContentControl(droppedcontroltype, clnele);
                                    }

                                    //add the clone element
                                    if (clnele != null)
                                    {
                                        if (parameter != null)
                                        {
                                            dropcommand.Execute(parameter);
                                        }
                                        else
                                        {
                                            dropcommand.Execute((clnele as ContentControl).DataContext);
                                        }


                                        Canvas.SetRight(clnele, point.X - adn.CenterPoint.X);
                                        Canvas.SetLeft(clnele, point.X - adn.CenterPoint.X);
                                        Canvas.SetBottom(clnele, point.Y - adn.CenterPoint.Y);
                                        Canvas.SetTop(clnele, point.Y - adn.CenterPoint.Y);
                                        droppedcanvas.Children.Add(clnele);
                                    }

                                }

                            }
                        }
                    }

                }
            }

            if (this._adorner != null)
                this._adorner.Remove();


            e.Handled = true;
            return;
        }

        private  void AssociatedObject_DragLeave(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(_dataType))
            {
                if (this._adorner != null)
                    this._adorner.Remove();
                e.Handled = true;
            }
        }

        private void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            if (_dataType != null)
            {
                //if item can be dropped
                if (e.Data.GetDataPresent(_dataType))
                {
                    //give mouse effect
                    this.SetDragDropEffects(e);
                    //draw the dots
                    if (this._adorner != null)
                        this._adorner.Update();

                    e.Handled = true;
                }
            }
        }

        private void AssociatedObject_DragEnter(object sender, DragEventArgs e)
        {
            //if the DataContext implements IDropable, record the data type that can be dropped
            ////////////if (this._dataType == null)
            ////////////{
            ////////////    if (this.AssociatedObject.DataContext != null)
            ////////////    {
            ////////////        IDropable dropObject = this.AssociatedObject.DataContext as IDropable;
            ////////////        if (dropObject != null)
            ////////////        {
            ////////////            this._dataType = dropObject.DataType;
            ////////////        }
            ////////////    }
            ////////////}
            if (e.Data.GetDataPresent(_dataType))
            {
                if (this._adorner == null)
                    this._adorner = new DroppingAdorner(sender as UIElement);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Provides feedback on if the data can be dropped
        /// </summary>
        /// <param name="e"></param>
        private void SetDragDropEffects(DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;  //default to None

            //if the data type can be dropped 
            if (e.Data.GetDataPresent(_dataType))
            {
                e.Effects = DragDropEffects.Move;
            }
        }

        #region Dependency Property

        #region IsFixedPosition

        /// <summary>
        /// The IsFixedPosition attached property's name.
        /// </summary>
        public const string IsFixedPositionPropertyName = "IsFixedPosition";

        /// <summary>
        /// Gets the value of the IsFixedPosition attached property 
        /// for a given dependency object.
        /// </summary>
        /// <param name="obj">The object for which the property value
        /// is read.</param>
        /// <returns>The value of the IsFixedPosition property of the specified object.</returns>
        public static bool GetIsFixedPosition(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFixedPositionProperty);
        }

        /// <summary>
        /// Sets the value of the IsFixedPosition attached property
        /// for a given dependency object. 
        /// </summary>
        /// <param name="obj">The object to which the property value
        /// is written.</param>
        /// <param name="value">Sets the IsFixedPosition value of the specified object.</param>
        public static void SetIsFixedPosition(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFixedPositionProperty, value);
        }

        /// <summary>
        /// Identifies the IsFixedPosition attached property.
        /// </summary>
        public static readonly DependencyProperty IsFixedPositionProperty = DependencyProperty.RegisterAttached(
            IsFixedPositionPropertyName,
            typeof(bool),
            typeof(FrameworkElementDropBehavior),
            new UIPropertyMetadata(false));

        #endregion

        #region AdornerType

        /// <summary>
        /// The AdornerType attached property's name.
        /// </summary>
        public const string AdornerTypePropertyName = "AdornerType";

        /// <summary>
        /// Gets the value of the AdornerType attached property 
        /// for a given dependency object.
        /// </summary>
        /// <param name="obj">The object for which the property value
        /// is read.</param>
        /// <returns>The value of the AdornerType property of the specified object.</returns>
        public static FrameworkElementAdornerType GetAdornerType(DependencyObject obj)
        {
            return (FrameworkElementAdornerType)obj.GetValue(AdornerTypeProperty);
        }

        /// <summary>
        /// Sets the value of the AdornerType attached property
        /// for a given dependency object. 
        /// </summary>
        /// <param name="obj">The object to which the property value
        /// is written.</param>
        /// <param name="value">Sets the AdornerType value of the specified object.</param>
        public static void SetAdornerType(DependencyObject obj, FrameworkElementAdornerType value)
        {
            obj.SetValue(AdornerTypeProperty, value);
        }

        /// <summary>
        /// Identifies the AdornerType attached property.
        /// </summary>
        public static readonly DependencyProperty AdornerTypeProperty = DependencyProperty.RegisterAttached(
            AdornerTypePropertyName,
            typeof(FrameworkElementAdornerType),
            typeof(FrameworkElementDropBehavior),
            new UIPropertyMetadata(FrameworkElementAdornerType.DrawEllipse));


        #endregion

        #region DropCommand

        /// <summary>
        /// The DropCommand attached property's name.
        /// </summary>
        public const string DropCommandPropertyName = "DropCommand";

        /// <summary>
        /// Gets the value of the DropCommand attached property 
        /// for a given dependency object.
        /// </summary>
        /// <param name="obj">The object for which the property value
        /// is read.</param>
        /// <returns>The value of the DropCommand property of the specified object.</returns>
        public static ICommand GetDropCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(DropCommandProperty);
        }

        /// <summary>
        /// Sets the value of the DropCommand attached property
        /// for a given dependency object. 
        /// </summary>
        /// <param name="obj">The object to which the property value
        /// is written.</param>
        /// <param name="value">Sets the DropCommand value of the specified object.</param>
        public static void SetDropCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(DropCommandProperty, value);
        }

        /// <summary>
        /// Identifies the DropCommand attached property.
        /// </summary>
        public static readonly DependencyProperty DropCommandProperty = DependencyProperty.RegisterAttached(
            DropCommandPropertyName,
            typeof(ICommand),
            typeof(FrameworkElementDropBehavior),
            new UIPropertyMetadata(null));
        #endregion

        #region DropCommandParameter

        /// <summary>
        /// The DropCommandParameter attached property's name.
        /// </summary>
        public const string DropCommandParameterPropertyName = "DropCommandParameter";

        /// <summary>
        /// Gets the value of the DropCommandParameter attached property 
        /// for a given dependency object.
        /// </summary>
        /// <param name="obj">The object for which the property value
        /// is read.</param>
        /// <returns>The value of the DropCommandParameter property of the specified object.</returns>
        public static object GetDropCommandParameter(DependencyObject obj)
        {
            return (object)obj.GetValue(DropCommandParameterProperty);
        }

        /// <summary>
        /// Sets the value of the DropCommandParameter attached property
        /// for a given dependency object. 
        /// </summary>
        /// <param name="obj">The object to which the property value
        /// is written.</param>
        /// <param name="value">Sets the DropCommandParameter value of the specified object.</param>
        public static void SetDropCommandParameter(DependencyObject obj, object value)
        {
            obj.SetValue(DropCommandParameterProperty, value);
        }

        /// <summary>
        /// Identifies the DropCommandParameter attached property.
        /// </summary>
        public static readonly DependencyProperty DropCommandParameterProperty = DependencyProperty.RegisterAttached(
            DropCommandParameterPropertyName,
            typeof(object),
            typeof(FrameworkElementDropBehavior),
            new UIPropertyMetadata(null));
        #endregion

        #region DroppedCanvas
        /// <summary>
        /// The DroppedCanvas attached property's name.
        /// </summary>
        public const string DroppedCanvasPropertyName = "DroppedCanvas";

        /// <summary>
        /// Gets the value of the DroppedCanvas attached property 
        /// for a given dependency object.
        /// </summary>
        /// <param name="obj">The object for which the property value
        /// is read.</param>
        /// <returns>The value of the DroppedCanvas property of the specified object.</returns>
        public static Canvas GetDroppedCanvas(DependencyObject obj)
        {
            return (Canvas)obj.GetValue(DroppedCanvasProperty);
        }

        /// <summary>
        /// Sets the value of the DroppedCanvas attached property
        /// for a given dependency object. 
        /// </summary>
        /// <param name="obj">The object to which the property value
        /// is written.</param>
        /// <param name="value">Sets the DroppedCanvas value of the specified object.</param>
        public static void SetDroppedCanvas(DependencyObject obj, Canvas value)
        {
            obj.SetValue(DroppedCanvasProperty, value);
        }

        /// <summary>
        /// Identifies the DroppedCanvas attached property.
        /// </summary>
        public static readonly DependencyProperty DroppedCanvasProperty = DependencyProperty.RegisterAttached(
            DroppedCanvasPropertyName,
            typeof(Canvas),
            typeof(FrameworkElementDropBehavior),
            new UIPropertyMetadata(null));
        #endregion

        #region DroppedControlType
        /// <summary>
        /// The DroppedControlType attached property's name.
        /// </summary>
        public const string DroppedControlTypePropertyName = "DroppedControlType";

        /// <summary>
        /// Gets the value of the DroppedControlType attached property 
        /// for a given dependency object.
        /// </summary>
        /// <param name="obj">The object for which the property value
        /// is read.</param>
        /// <returns>The value of the DroppedControlType property of the specified object.</returns>
        public static Type GetDroppedControlType(DependencyObject obj)
        {
            return (Type)obj.GetValue(DroppedControlTypeProperty);
        }

        /// <summary>
        /// Sets the value of the DroppedControlType attached property
        /// for a given dependency object. 
        /// </summary>
        /// <param name="obj">The object to which the property value
        /// is written.</param>
        /// <param name="value">Sets the DroppedControlType value of the specified object.</param>
        public static void SetDroppedControlType(DependencyObject obj, Type value)
        {
            obj.SetValue(DroppedControlTypeProperty, value);
        }

        /// <summary>
        /// Identifies the DroppedControlType attached property.
        /// </summary>
        public static readonly DependencyProperty DroppedControlTypeProperty = DependencyProperty.RegisterAttached(
            DroppedControlTypePropertyName,
            typeof(Type),
            typeof(FrameworkElementDropBehavior),
            new UIPropertyMetadata(null));
        #endregion

        #endregion


        #region Private Method




        public ContentControl CreateNewContentControl(Type contenttype, object content)
        {
            var newobj = contenttype.GetConstructors()[0].Invoke(null);

            PropertyInfo contentpro = contenttype.GetProperty("Content");

            contentpro.SetValue(newobj, content);

            return newobj as ContentControl;
        }

        #endregion

    }
}
