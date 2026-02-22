using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ellabi.Classes
{
    public class Arc : Shape
    {
        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartAngle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartAngleProperty = DependencyProperty.Register("StartAngle", typeof(double), typeof(Arc), new UIPropertyMetadata(0.0, UpdateArc));

        public double EndAngle
        {
            get { return (double)GetValue(EndAngleProperty); }
            set { SetValue(EndAngleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EndAngle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndAngleProperty = DependencyProperty.Register("EndAngle", typeof(double), typeof(Arc), new UIPropertyMetadata(90.0, UpdateArc));

        // This controls whether or not the progress bar goes clockwise or counterclockwise
        public SweepDirection Direction
        {
            get { return (SweepDirection)GetValue(DirectionProperty); }
            set { SetValue(DirectionProperty, value); }
        }

        public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register("Direction", typeof(SweepDirection), typeof(Arc), new UIPropertyMetadata(SweepDirection.Clockwise));

        // Rotate the start/endpoint of the arc a certain number of degree in the direction
        // ie. if you wanted it to be at 12:00 that would be 270 Clockwise or 90 counterclockwise
        public double OriginRotationDegrees
        {
            get { return (double)GetValue(OriginRotationDegreesProperty); }
            set { SetValue(OriginRotationDegreesProperty, value); }
        }

        public static readonly DependencyProperty OriginRotationDegreesProperty = DependencyProperty.Register("OriginRotationDegrees", typeof(double), typeof(Arc), new UIPropertyMetadata(270.0, UpdateArc));

        protected static void UpdateArc(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                Arc arc = d as Arc;
                arc?.InvalidateVisual();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        protected override Geometry DefiningGeometry => GetArcGeometry();

        protected override void OnRender(DrawingContext drawingContext)
        {
            try
            {
                drawingContext.DrawGeometry(null, new Pen(Stroke, StrokeThickness), GetArcGeometry());
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private Geometry GetArcGeometry()
        {
            StreamGeometry geom = new StreamGeometry();

            try
            {
                Point startPoint = PointAtAngle(Math.Min(StartAngle, EndAngle), Direction);
                Point endPoint = PointAtAngle(Math.Max(StartAngle, EndAngle), Direction);
                Size arcSize = new Size(Math.Max(0, (RenderSize.Width - StrokeThickness) / 2), Math.Max(0, (RenderSize.Height - StrokeThickness) / 2));
                bool isLargeArc = Math.Abs(EndAngle - StartAngle) > 180;

                using (StreamGeometryContext context = geom.Open())
                {
                    context.BeginFigure(startPoint, false, false);
                    context.ArcTo(endPoint, arcSize, 0, isLargeArc, Direction, true, false);
                }

                geom.Transform = new TranslateTransform(StrokeThickness / 2, StrokeThickness / 2);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }

            return geom;
        }

        private Point PointAtAngle(double angle, SweepDirection sweep)
        {
            try
            {
                double translatedAngle = angle + OriginRotationDegrees;
                double radAngle = translatedAngle * (Math.PI / 180);
                double xr = (RenderSize.Width - StrokeThickness) / 2;
                double yr = (RenderSize.Height - StrokeThickness) / 2;
                double x = xr + xr * Math.Cos(radAngle);
                double y = yr * Math.Sin(radAngle);

                if (sweep == SweepDirection.Counterclockwise)
                {
                    y = yr - y;
                }
                else
                {
                    y = yr + y;
                }

                return new Point(x, y);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }

            return new Point();
        }
    }
}