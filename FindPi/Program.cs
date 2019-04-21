using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace FindPi {
    class Program {
        static void Main(string[] args) {
            PointD[] prevPoints = new PointD[] {
                new PointD(1, 0),
                new PointD(0, -1),
                new PointD(-1, 0),
                new PointD(0, 1)
            };
            TimeSpan fullTime = TimeSpan.FromTicks(0);
            Stopwatch initializeTimer = new Stopwatch();
            Stopwatch calcTimer = new Stopwatch();
            decimal? error = null;
            for(int i = 0; i < 24; i++) {
                initializeTimer.Restart();
                PointD[] points = new PointD[prevPoints.Length * 2];
                for(int j = 0; j < prevPoints.Length; j++) {
                    var supportPoint = prevPoints[j];
                    var nextSupportPoint = prevPoints[j < prevPoints.Length - 1 ? j + 1 : 0];
                    var newPoint = (supportPoint + nextSupportPoint) / 2;
                    var radiusToNewPoint = (decimal)Math.Sqrt((double)(newPoint.X * newPoint.X + newPoint.Y * newPoint.Y));
                    var normalizedNewPoint = newPoint / radiusToNewPoint;
                    points[j * 2] = supportPoint;
                    points[(j * 2) + 1] = normalizedNewPoint;
                };
                prevPoints = points;
                initializeTimer.Stop();
                var initTime = initializeTimer.Elapsed;
                calcTimer.Restart();
                decimal length = 0;
                for(int j = 0; j < points.Length; j++) {
                    decimal prevX = points[j].X;
                    decimal prevY = points[j].Y;
                    decimal x = (j < points.Length - 1 ? points[j + 1] : points[0]).X;
                    decimal y = (j < points.Length - 1 ? points[j + 1] : points[0]).Y;
                    decimal currentLength = (decimal)Math.Sqrt((double)((x - prevX) * (x - prevX) + (y - prevY) * (y - prevY)));
                    length += currentLength;
                };
                calcTimer.Stop();
                var calcTime = calcTimer.Elapsed;
                var currentPi = length / 2;
                var currentError = Math.Abs((decimal)Math.PI - currentPi);
                if(error != null && !(error.Value > currentError)) {
                    break;
                }
                fullTime += initTime + calcTime;
                error = currentError;
                Console.WriteLine($"Interation: {i + 1}");
                Console.WriteLine($"Point count: {points.Length}");
                Console.WriteLine($"Init points time: {initTime}");
                Console.WriteLine($"Calc time: {calcTime}");
                Console.WriteLine($"Calc  Pi: {currentPi}");
                Console.WriteLine($"Const Pi: {Math.PI}");
                Console.WriteLine($"Error: {currentError}");

            }
            Console.WriteLine();
            Console.WriteLine($"Full time: {fullTime}");
            Console.WriteLine("The End");
        }
    }
    public struct PointD {
        public decimal X;
        public decimal Y;
        public PointD(decimal x, decimal y) {
            X = x;
            Y = y;
        }
        public static PointD operator +(PointD pt, PointD pt2) {
            return new PointD(pt.X + pt2.X, pt.Y + pt2.Y);
        }
        public static PointD operator -(PointD pt, PointD pt2) {
            return new PointD(pt.X - pt2.X, pt.Y - pt2.Y);
        }
        public static bool operator ==(PointD left, PointD right) {
            if(left.X == right.X) {
                return left.Y == right.Y;
            }
            return false;
        }
        public static PointD operator * (PointD left, decimal right) {
            return new PointD(left.X * right, left.Y * right);
        }
        public static PointD operator / (PointD left, decimal right) {
            return new PointD(left.X / right, left.Y / right);
        }
        public static PointD operator *(PointD left, int right) {
            return new PointD(left.X * right, left.Y * right);
        }
        public static PointD operator /(PointD left, int right) {
            return new PointD(left.X / right, left.Y / right);
        }
        public static bool operator !=(PointD left, PointD right) {
            return !(left == right);
        }
        public override bool Equals(object obj) {
            if(!(obj is PointD)) {
                return false;
            }
            PointD pointF = (PointD)obj;
            if(pointF.X == X && pointF.Y == Y) {
                return pointF.GetType().Equals(GetType());
            }
            return false;
        }
        public override int GetHashCode() {
            return ((ValueType)this).GetHashCode();
        }
        public override string ToString() {
            return string.Format(CultureInfo.CurrentCulture, "{{X={0}, Y={1}}}", new object[2] { X, Y });
        }
    }
}
