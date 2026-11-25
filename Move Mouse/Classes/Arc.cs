using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;

namespace ellabi.Classes
{
    public class Arc : Control
    {
        public static readonly StyledProperty<double> StartAngleProperty =
            AvaloniaProperty.Register<Arc, double>(nameof(StartAngle), 0.0);

        public static readonly StyledProperty<double> EndAngleProperty =
            AvaloniaProperty.Register<Arc, double>(nameof(EndAngle), 90.0);

        public static readonly StyledProperty<SweepDirection> DirectionProperty =
            AvaloniaProperty.Register<Arc, SweepDirection>(nameof(Direction), SweepDirection.Clockwise);

        public static readonly StyledProperty<double> OriginRotationDegreesProperty =
            AvaloniaProperty.Register<Arc, double>(nameof(OriginRotationDegrees), 270.0);

        public static readonly StyledProperty<IBrush> StrokeProperty =
            AvaloniaProperty.Register<Arc, IBrush>(nameof(Stroke));

        public static readonly StyledProperty<double> StrokeThicknessProperty =
            AvaloniaProperty.Register<Arc, double>(nameof(StrokeThickness), 1.0);

        static Arc()
        {
            AffectsRender<Arc>(
                StartAngleProperty,
                EndAngleProperty,
                DirectionProperty,
                OriginRotationDegreesProperty,
                StrokeProperty,
                StrokeThicknessProperty);
        }

        public double StartAngle
        {
            get => GetValue(StartAngleProperty);
            set => SetValue(StartAngleProperty, value);
        }

        public double EndAngle
        {
            get => GetValue(EndAngleProperty);
            set => SetValue(EndAngleProperty, value);
        }

        public SweepDirection Direction
        {
            get => GetValue(DirectionProperty);
            set => SetValue(DirectionProperty, value);
        }

        public double OriginRotationDegrees
        {
            get => GetValue(OriginRotationDegreesProperty);
            set => SetValue(OriginRotationDegreesProperty, value);
        }

        public IBrush Stroke
        {
            get => GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        public double StrokeThickness
        {
            get => GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (Stroke == null || StrokeThickness <= 0 || Bounds.Width <= 0 || Bounds.Height <= 0)
            {
                return;
            }

            try
            {
                var geom = GetArcGeometry();
                if (geom != null)
                {
                    context.DrawGeometry(null, new Pen(Stroke, StrokeThickness), geom);
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private Geometry GetArcGeometry()
        {
            double minAngle = Math.Min(StartAngle, EndAngle);
            double maxAngle = Math.Max(StartAngle, EndAngle);
            if (Math.Abs(maxAngle - minAngle) < 0.01) return null;

            double halfStroke = StrokeThickness / 2.0;
            double xr = Math.Max(0, (Bounds.Width - StrokeThickness) / 2.0);
            double yr = Math.Max(0, (Bounds.Height - StrokeThickness) / 2.0);
            if (xr <= 0 || yr <= 0) return null;

            Point startPoint = PointAtAngle(minAngle, Direction, xr, yr, halfStroke);
            Point endPoint = PointAtAngle(maxAngle, Direction, xr, yr, halfStroke);
            Size arcSize = new Size(xr, yr);
            bool isLargeArc = Math.Abs(EndAngle - StartAngle) > 180.0;

            var geom = new StreamGeometry();
            using (var ctx = geom.Open())
            {
                ctx.BeginFigure(startPoint, false);
                ctx.ArcTo(endPoint, arcSize, 0, isLargeArc, Direction);
                ctx.EndFigure(false);
            }

            return geom;
        }

        private Point PointAtAngle(double angle, SweepDirection sweep, double xr, double yr, double offset)
        {
            double translatedAngle = angle + OriginRotationDegrees;
            double radAngle = translatedAngle * (Math.PI / 180.0);
            double x = offset + xr + xr * Math.Cos(radAngle);
            double y = offset + (sweep == SweepDirection.CounterClockwise
                ? yr - yr * Math.Sin(radAngle)
                : yr + yr * Math.Sin(radAngle));

            return new Point(x, y);
        }
    }
}
