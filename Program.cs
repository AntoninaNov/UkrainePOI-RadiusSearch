/*
using System.Diagnostics;
using System.Globalization;

class Program
{
    static void Main()
    {
        Console.WriteLine("Enter latitude: ");
        string latInput = Console.ReadLine().Replace(',', '.');
        double lat1 = Convert.ToDouble(latInput, CultureInfo.InvariantCulture);
        
        
        Console.WriteLine("Enter longitude: ");
        string lonInput = Console.ReadLine().Replace(',', '.');
        double lon1 = Convert.ToDouble(lonInput, CultureInfo.InvariantCulture);
        
        Console.WriteLine("Enter radius: ");
        string radiusInput = Console.ReadLine().Replace(',', '.');
        double radius = Convert.ToDouble(radiusInput, CultureInfo.InvariantCulture);

        string filePath = @"/Users/vladshcherbyna/RiderProjects/UkrainePOI-RadiusSearch/ukraine_poi.csv";

        List<string[]> points = new List<string[]>();
        using (var sr = new StreamReader(filePath))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                string[] record = line.Split(';');
                points.Add(record);
            }
        }

        Stopwatch sw = new Stopwatch();
        sw.Start();

        int index = 1;
        foreach (var record in points)
        {
            if (record.Length == 0 || record[0].Trim() == "" || record[1].Trim() == "")
                continue;
            
            double recordLat = double.Parse(record[0].Replace(',', '.'), CultureInfo.InvariantCulture);
            double recordLon = double.Parse(record[1].Replace(',', '.'), CultureInfo.InvariantCulture);
            double distance = HaversineDistance(lat1, lon1, recordLat, recordLon);
            if (distance <= radius)
            {
                Console.WriteLine("{0}. Type: {1}, Subtype: {2}, Name: {3}, Details: {4}, Distance: {5:F2} km", index, record[2], record[3], record[4], record.Length > 5 ? record[5] : "", distance);
                index++;
            }
        }
        
        sw.Stop();
        Console.WriteLine($"Elapsed time: {sw.Elapsed}");
    }

    static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        double R = 6371;
        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);
        lat1 = ToRadians(lat1);
        lat2 = ToRadians(lat2);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
        double d = 2 * R * Math.Asin(Math.Sqrt(a));
        return d;
    }

    static double ToRadians(double angle)
    {
        return Math.PI * angle / 180.0;
    }
}

// KSE: 50,45836535137246, 30,42989186016101
*/


using System.Globalization;


class Program
{
    static void Main()
    {
        string filePath = @"/Users/vladshcherbyna/RiderProjects/UkrainePOI-RadiusSearch/ukraine_poi.csv";

        List<Point> points = ReadPointsFromFile(filePath);

        double minLatitude = double.MaxValue;
        double maxLatitude = double.MinValue;
        double minLongitude = double.MaxValue;
        double maxLongitude = double.MinValue;

        foreach (Point point in points)
        {
            if (point.Latitude < minLatitude)
                minLatitude = point.Latitude;
            if (point.Latitude > maxLatitude)
                maxLatitude = point.Latitude;

            if (point.Longitude < minLongitude)
                minLongitude = point.Longitude;
            if (point.Longitude > maxLongitude)
                maxLongitude = point.Longitude;
        }

        Rectangle boundingRectangle = new Rectangle(minLatitude, maxLatitude, minLongitude, maxLongitude);
        Node root = BuildTree(points, boundingRectangle, SplitAxis.Latitude);
        Console.WriteLine("Binary Tree Construction Completed");
        
        Point newPoint = new Point(48.56847574, 30.1485768439);
        root.Insert(newPoint);
        Console.WriteLine("New point inserted: Latitude={0}, Longitude={1}", newPoint.Latitude, newPoint.Longitude);

        // Видалення точки
        bool removed = root.Remove(newPoint);
        if (removed)
            Console.WriteLine("Point removed: Latitude={0}, Longitude={1}", newPoint.Latitude, newPoint.Longitude);
        else
            Console.WriteLine("Point not found: Latitude={0}, Longitude={1}", newPoint.Latitude, newPoint.Longitude);

        Console.ReadLine();
    }

    static List<Point> ReadPointsFromFile(string filePath)
    {
        List<Point> points = new List<Point>();

        using (var sr = new StreamReader(filePath))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                string[] record = line.Split(';');
                if (record.Length >= 2)
                {
                    double latitude;
                    if (double.TryParse(record[0].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out latitude))
                    {
                        double longitude;
                        if (double.TryParse(record[1].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out longitude))
                        {
                            Point point = new Point(latitude, longitude);
                            points.Add(point);
                        }
                    }
                }
            }
        }

        return points;
    }

    static Node BuildTree(List<Point> points, Rectangle boundingRectangle, SplitAxis splitAxis)
    {
        if (points.Count <= 100)
        {
            // Create a leaf node
            return new Node(boundingRectangle, points);
        }

        List<Point> sortedPoints;
        Rectangle leftRectangle;
        Rectangle rightRectangle;

        if (splitAxis == SplitAxis.Latitude)
        {
            sortedPoints = SortPoints(points, PointComparer.ByLatitude);
            double medianLatitude = sortedPoints[sortedPoints.Count / 2].Latitude;
            leftRectangle = new Rectangle(boundingRectangle.MinLatitude, medianLatitude, boundingRectangle.MinLongitude, boundingRectangle.MaxLongitude);
            rightRectangle = new Rectangle(medianLatitude, boundingRectangle.MaxLatitude, boundingRectangle.MinLongitude, boundingRectangle.MaxLongitude);
        }
        else
        {
            sortedPoints = SortPoints(points, PointComparer.ByLongitude);
            double medianLongitude = sortedPoints[sortedPoints.Count / 2].Longitude;
            leftRectangle = new Rectangle(boundingRectangle.MinLatitude, boundingRectangle.MaxLatitude, boundingRectangle.MinLongitude, medianLongitude);
            rightRectangle = new Rectangle(boundingRectangle.MinLatitude, boundingRectangle.MaxLatitude, medianLongitude, boundingRectangle.MaxLongitude);
        }

        SplitAxis nextSplitAxis = (splitAxis == SplitAxis.Latitude) ? SplitAxis.Longitude : SplitAxis.Latitude;

        Node node = new Node(boundingRectangle);
        node.Left = BuildTree(sortedPoints.GetRange(0, sortedPoints.Count / 2), leftRectangle, nextSplitAxis);
        node.Right = BuildTree(sortedPoints.GetRange(sortedPoints.Count / 2, sortedPoints.Count - sortedPoints.Count / 2), rightRectangle, nextSplitAxis);

        return node;
    }

    static List<Point> SortPoints(List<Point> points, IComparer<Point> comparer)
    {
        List<Point> sortedPoints = new List<Point>(points);
        sortedPoints.Sort(comparer);
        return sortedPoints;
    }
}

enum SplitAxis
{
    Latitude,
    Longitude
}

class Point
{
    public double Latitude { get; }
    public double Longitude { get; }

    public Point(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
}

class Rectangle
{
    public double MinLatitude { get; }
    public double MaxLatitude { get; }
    public double MinLongitude { get; }
    public double MaxLongitude { get; }

    public Rectangle(double minLatitude, double maxLatitude, double minLongitude, double maxLongitude)
    {
        MinLatitude = minLatitude;
        MaxLatitude = maxLatitude;
        MinLongitude = minLongitude;
        MaxLongitude = maxLongitude;
    }
    
    public bool Contains(Point point)
    {
        return point.Latitude >= MinLatitude && point.Latitude <= MaxLatitude &&
               point.Longitude >= MinLongitude && point.Longitude <= MaxLongitude;
    }
}

class Node
{
    public Rectangle BoundingRectangle { get; }
    public List<Point> Points { get; }
    public Node Left { get; set; }
    public Node Right { get; set; }

    public Node(Rectangle boundingRectangle)
    {
        BoundingRectangle = boundingRectangle;
        Points = new List<Point>();
    }

    public Node(Rectangle boundingRectangle, List<Point> points)
    {
        BoundingRectangle = boundingRectangle;
        Points = points;
    }
    
    public Node Insert(Point point)
    {
        // Якщо поточний вузол є листом, просто додаємо нову точку до списку точок у вузлі
        if (Left == null && Right == null)
        {
            Points.Add(point);
            return this;
        }

        // Визначаємо в який підвузол має бути вставлена точка
        if (Left != null && Left.BoundingRectangle.Contains(point))
        {
            Left = Left.Insert(point);
        }
        else if (Right != null && Right.BoundingRectangle.Contains(point))
        {
            Right = Right.Insert(point);
        }
        else
        {
            // Якщо точка не підпадає під жоден підвузол, вона залишається у поточному вузлі
            Points.Add(point);
        }

        return this;
    }
    
    public bool Remove(Point point)
    {
        bool removed = Points.Remove(point);

        if (removed)
            return true;

        if (Left != null && Left.BoundingRectangle.Contains(point))
        {
            removed = Left.Remove(point);
            if (removed && Left.Points.Count == 0)
                Left = null;
        }
        else if (Right != null && Right.BoundingRectangle.Contains(point))
        {
            removed = Right.Remove(point);
            if (removed && Right.Points.Count == 0)
                Right = null;
        }

        return removed;
    }
}

class PointComparer : IComparer<Point>
{
    private readonly SplitAxis splitAxis;

    private PointComparer(SplitAxis splitAxis)
    {
        this.splitAxis = splitAxis;
    }

    public int Compare(Point x, Point y)
    {
        return (splitAxis == SplitAxis.Latitude) ? x.Latitude.CompareTo(y.Latitude) : x.Longitude.CompareTo(y.Longitude);
    }

    public static readonly PointComparer ByLatitude = new PointComparer(SplitAxis.Latitude);
    public static readonly PointComparer ByLongitude = new PointComparer(SplitAxis.Longitude);
}
