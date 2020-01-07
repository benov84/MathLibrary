﻿using System;

namespace Benov.MathLib
{
    public partial class Geometry
    {
        public static Point[] ConvexHull(Point[] pts)
        {
            // Sort points lexicographically by increasing (x, y)
            int N = pts.Length;
            Polysort.Quicksort<Point>(pts);
            Point left = pts[0], right = pts[N - 1];
            // Partition into lower hull and upper hull
            CDLL<Point> lower = new CDLL<Point>(left), upper = new CDLL<Point>(left);
            for (int i = 0; i < N; i++)
            {
                double det = Point.Area2(left, right, pts[i]);
                if (det > 0)
                    upper = upper.Append(new CDLL<Point>(pts[i]));
                else if (det < 0)
                    lower = lower.Prepend(new CDLL<Point>(pts[i]));
            }
            lower = lower.Prepend(new CDLL<Point>(right));
            upper = upper.Append(new CDLL<Point>(right)).Next;
            // Eliminate points not on the hull
            eliminate(lower);
            eliminate(upper);
            // Eliminate duplicate endpoints
            if (lower.Prev.val.Equals(upper.val))
                lower.Prev.Delete();
            if (upper.Prev.val.Equals(lower.val))
                upper.Prev.Delete();
            // Join the lower and upper hull
            Point[] res = new Point[lower.Size() + upper.Size()];
            lower.CopyInto(res, 0);
            upper.CopyInto(res, lower.Size());
            return res;
        }

        // Graham's scan
        private static void eliminate(CDLL<Point> start)
        {
            CDLL<Point> v = start, w = start.Prev;
            bool fwd = false;
            while (v.Next != start || !fwd)
            {
                if (v.Next == w)
                    fwd = true;
                if (Point.Area2(v.val, v.Next.val, v.Next.Next.val) < 0) // right turn
                    v = v.Next;
                else
                {                                       // left turn or straight
                    v.Next.Delete();
                    v = v.Prev;
                }
            }
        }
    }

    // ------------------------------------------------------------

    // Points in the plane

    public class Point : Ordered<Point>
    {
        private static readonly Random rnd = new Random();

        public double x, y;

        //For compatibility - you can you x and y case insensitive
        public double X
        {
            get { return x; }
            set { x = value; }
        }

        public double Y
        {
            get { return y; }
            set { y = value; }
        }

        public Point(double x, double y)
        {
            this.x = x; this.y = y;
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }

        public static Point Random(int w, int h)
        {
            return new Point(rnd.Next(w), rnd.Next(h));
        }

        public bool Equals(Point p2)
        {
            return x == p2.x && y == p2.y;
        }

        public override bool Less(Ordered<Point> o2)
        {
            Point p2 = (Point)o2;
            return x < p2.x || x == p2.x && y < p2.y;
        }

        // Twice the signed area of the triangle (p0, p1, p2)
        public static double Area2(Point p0, Point p1, Point p2)
        {
            return p0.x * (p1.y - p2.y) + p1.x * (p2.y - p0.y) + p2.x * (p0.y - p1.y);
        }

        public double DistanceTo(Point pt2)
        {
            return Core.Dist(this.x, this.y, pt2.x, pt2.y);
        }
    }

    // ------------------------------------------------------------

    // Circular doubly linked lists of T

    class CDLL<T>
    {
        private CDLL<T> prev, next;     // not null, except in deleted elements
        public T val;

        // A new CDLL node is a one-element circular list
        public CDLL(T val)
        {
            this.val = val; next = prev = this;
        }

        public CDLL<T> Prev
        {
            get { return prev; }
        }

        public CDLL<T> Next
        {
            get { return next; }
        }

        // Delete: adjust the remaining elements, make this one point nowhere
        public void Delete()
        {
            next.prev = prev; prev.next = next;
            next = prev = null;
        }

        public CDLL<T> Prepend(CDLL<T> elt)
        {
            elt.next = this; elt.prev = prev; prev.next = elt; prev = elt;
            return elt;
        }

        public CDLL<T> Append(CDLL<T> elt)
        {
            elt.prev = this; elt.next = next; next.prev = elt; next = elt;
            return elt;
        }

        public int Size()
        {
            int count = 0;
            CDLL<T> node = this;
            do
            {
                count++;
                node = node.next;
            } while (node != this);
            return count;
        }

        public void PrintFwd()
        {
            CDLL<T> node = this;
            do
            {
                Console.WriteLine(node.val);
                node = node.next;
            } while (node != this);
            Console.WriteLine();
        }

        public void CopyInto(T[] vals, int i)
        {
            CDLL<T> node = this;
            do
            {
                vals[i++] = node.val;   // still, implicit checkcasts at runtime 
                node = node.next;
            } while (node != this);
        }
    }

    // ------------------------------------------------------------

    class Polysort
    {
        private static void swap<T>(T[] arr, int s, int t)
        {
            T tmp = arr[s]; arr[s] = arr[t]; arr[t] = tmp;
        }

        // Typed OO-style quicksort a la Hoare/Wirth

        private static void qsort<T>(Ordered<T>[] arr, int a, int b)
        {
            // sort arr[a..b]
            if (a < b)
            {
                int i = a, j = b;
                Ordered<T> x = arr[(i + j) / 2];
                do
                {
                    while (arr[i].Less(x)) i++;
                    while (x.Less(arr[j])) j--;
                    if (i <= j)
                    {
                        swap<Ordered<T>>(arr, i, j);
                        i++; j--;
                    }
                } while (i <= j);
                qsort<T>(arr, a, j);
                qsort<T>(arr, i, b);
            }
        }

        public static void Quicksort<T>(Ordered<T>[] arr)
        {
            qsort<T>(arr, 0, arr.Length - 1);
        }
    }

    public abstract class Ordered<T>
    {
        public abstract bool Less(Ordered<T> that);
    }

    // ------------------------------------------------------------

    class TestCH
    {
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                int N = int.Parse(args[0]);
                Point[] pts = new Point[N];
                for (int i = 0; i < N; i++)
                    pts[i] = Point.Random(500, 500);
                Point[] chpts = Geometry.ConvexHull(pts);
                Console.WriteLine("Area is " + area(chpts));
                print(chpts);
            }
            else
                Console.WriteLine("Usage: TestCH <pointcount>\n");
        }

        // The centroid of a point set
        public static Point centroid(Point[] pts)
        {
            int N = pts.Length;
            double sumx = 0, sumy = 0;
            for (int i = 0; i < N; i++)
            {
                sumx += pts[i].x;
                sumy += pts[i].y;
            }
            return new Point(sumx / N, sumy / N);
        }

        // The area of a polygon (represented by an array of ordered vertices)
        public static double area(Point[] pts)
        {
            int N = pts.Length;
            Point centr = centroid(pts);
            double area2 = 0;
            for (int i = 0; i < N; i++)
                area2 += Point.Area2(centr, pts[i], pts[(i + 1) % N]);
            return Math.Abs(area2 / 2);
        }

        public static void print(Point[] pts)
        {
            int N = pts.Length;
            for (int i = 0; i < N; i++)
                Console.WriteLine(pts[i]);
        }
    }
}