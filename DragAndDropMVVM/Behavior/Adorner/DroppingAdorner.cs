﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace DragAndDropMVVM.Behavior
{
    public class DroppingAdorner : Adorner
    {
        private AdornerLayer adornerLayer;

        public DroppingAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            this.adornerLayer = AdornerLayer.GetAdornerLayer(this.AdornedElement);
            this.adornerLayer.Add(this);
        }

        internal void Update()
        {
            this.adornerLayer.Update(this.AdornedElement);
            this.Visibility = System.Windows.Visibility.Visible;
        }

        public void Remove()
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect adornedElementRect = new Rect(this.AdornedElement.DesiredSize);

            SolidColorBrush renderBrush = (SolidColorBrush)Application.Current.Resources["AdornerPanelBrush"];
            renderBrush.Opacity = 0.5;
            Pen renderPen = new Pen((SolidColorBrush)Application.Current.Resources["WhiteBrush"], 1.5);
            double renderRadius = 5.0;

            //TODO: want to draw the
            // Draw a circle at each corner.
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopLeft, renderRadius, renderRadius);
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopRight, renderRadius, renderRadius);
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomLeft, renderRadius, renderRadius);
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomRight, renderRadius, renderRadius);
        }

    }
}