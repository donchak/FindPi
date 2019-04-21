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
            Stopwatch initializeTimer = new Stopwatch();
            Stopwatch calcTimer = new Stopwatch();
            for(int i = 0; i < 22; i++) {
                initializeTimer.Restart();
                PointD[] points = new PointD[prevPoints.Length * 2];
                //for(int j = 0; j < prevPoints.Length; j++) {
                Parallel.For(0, prevPoints.Length, (j) => {
                    var supportPoint = prevPoints[j];
                    var nextSupportPoint = prevPoints[j < prevPoints.Length - 1 ? j + 1 : 0];
                    var nextPoint = (supportPoint + nextSupportPoint) / 2;
                    var radiusToNextPoint = (decimal)Math.Sqrt((double)(nextPoint.X * nextPoint.X + nextPoint.Y * nextPoint.Y));
                    var normalizedNextPoint = nextPoint / radiusToNextPoint;
                    points[j * 2] = supportPoint;
                    points[(j * 2) + 1] = normalizedNextPoint;
                });
                prevPoints = points;
                initializeTimer.Stop();
                var initTime = initializeTimer.Elapsed;
                SpinLock spinLock = new SpinLock();
                decimal length = 0;
                calcTimer.Restart();
                //for(int j = 0; j < points.Length; j++) {
                Parallel.For(0, points.Length, (j) => {
                    decimal prevX = points[j].X;
                    decimal prevY = points[j].Y;
                    decimal x = (j < points.Length - 1 ? points[j + 1] : points[0]).X;
                    decimal y = (j < points.Length - 1 ? points[j + 1] : points[0]).Y;
                    decimal currentLength = (decimal)Math.Sqrt((double)((x - prevX) * (x - prevX) + (y - prevY) * (y - prevY)));
                    bool lockTaken = false;
                    do {
                        spinLock.Enter(ref lockTaken);
                    } while(!lockTaken);
                    try {
                        length += currentLength;
                    } finally {
                        spinLock.Exit();
                    }
                });
                calcTimer.Stop();
                var calcTime = calcTimer.Elapsed;
                Console.WriteLine($"Interation: {i}");
                Console.WriteLine($"Point count: {points.Length}");
                Console.WriteLine($"Init points time: {initTime}");
                Console.WriteLine($"Calc time: {calcTime}");
                Console.WriteLine($"Calc Pi: {length / 2}");
                Console.WriteLine($"Real Pi: {Math.PI}");
                Console.ReadKey();
            }
        }
    }
    public struct PointD {
        public static readonly PointD Empty;
        public decimal X;
        public decimal Y;
        public bool IsEmpty {
            get {
                if(X == 0M) {
                    return Y == 0M;
                }
                return false;
            }
        }
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
