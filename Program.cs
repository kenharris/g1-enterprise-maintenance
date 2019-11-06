using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace G1EnterpriseMaintenance
{
    class Metric
    {
        public string DeviceId { get; set; }
        public DateTime CreateDate { get; set; }
        public string OsVersion { get; set; }
        public string G1AppVersion { get; set; }
        public DateTime LastCheckDate { get; set; }
        public DateTime LastInstallDate { get; set; }
    }

    class Location
    {
        public int LocationId { get; set; }
        public string Name { get; set; }
        public List<Metric> Metrics { get; set; }
    }

    class LocationResponse
    {
        public List<Location> Results { get; set; }
    }

    class Program
    {       
        static void Main(string[] args)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://g1-enterprise.kenna.io");

            client.DefaultRequestHeaders.Add("Username", "sa");
            client.DefaultRequestHeaders.Add("Password", "Jjd?~e%up._Q+Rjr");

            var resp = client.GetAsync("/api/locations");
            var body = resp.Result.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            var response = JsonSerializer.Deserialize<LocationResponse>(body.Result, options);

            //var flatMetrics = response.Results.SelectMany(l => l.Metrics, ())
            var locationVersions = response.Results.SelectMany(l => l.Metrics ?? new List<Metric>(), (l, m) => (l.LocationId, l.Name, m.G1AppVersion)).ToList();
            var distinctLocationVersions = locationVersions.Distinct().OrderBy(lv => lv.LocationId).ThenBy(lv => lv.G1AppVersion).ToList();
            //var totals = locationVersions.Select(lv => (lv.LocationId, lv.Name, lv.G1AppVersion, locationVersions.Where(v => v.G1AppVersion == lv.G1AppVersion && v.LocationId == lv.LocationId).Count()))
            //    .Distinct();
            foreach ((int LocationId, string Name, string G1AppVersion) lv in distinctLocationVersions)
            {
                var count = locationVersions.Where(locVer => locVer.G1AppVersion == lv.G1AppVersion && locVer.LocationId == lv.LocationId).Count();
                //Console.WriteLine($"{lv.LocationId, -10}\t{lv.Name, -50}\t{lv.G1AppVersion, 10}\t{count, 5}");
                Console.WriteLine($"\"{lv.LocationId}\",\"{lv.Name}\",\"{lv.G1AppVersion}\",\"{count}\"");
            }

            //var offendingLocations = response.Results.Where(l => l.Metrics?.Any(m => m.G1AppVersion.Equals("2.1.0") || m.G1AppVersion.Equals("2.1.1")) ?? false).ToList();

            //List<(int LocationId, string DeviceId)> metrics = new List<(int LocationId, string DeviceId)>();
            //foreach (Location loc in offendingLocations)
            //{
            //    Console.WriteLine($"Location: {loc.LocationId}");
            //    var offendingMetrics = loc.Metrics.Where(m => m.G1AppVersion.Equals("2.1.0") || m.G1AppVersion.Equals("2.1.1")).ToList();
            //    foreach (Metric metric in offendingMetrics)
            //    {
            //        Console.WriteLine($"\tDeviceId: {metric.DeviceId}\tG1 App Version: {metric.G1AppVersion}");
            //        metrics.Add((LocationId: loc.LocationId, DeviceId: metric.DeviceId));
            //    }
            //}

            //foreach ((int LocationId, string DeviceId) metric in metrics)
            //{
            //    Console.WriteLine($"https://stage-g1-enterprise.kenna.io/api/Metrics/{metric.DeviceId}/locationid/{metric.LocationId}");
            //}

            Console.WriteLine();
        }
    }
}