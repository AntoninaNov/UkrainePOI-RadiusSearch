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

        string filePath = @"/Users/antoninanovak/RiderProjects/UkrainePOI-RadiusSearch/ukraine_poi.csv";

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