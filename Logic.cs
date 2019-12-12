using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neo4j.Driver.V1;
using System.Threading.Tasks;
using System.IO;

namespace Neo4J
{
    public class Airports
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }

    public class Flights
    {
        public string Company { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public int Price { get; set; }
        public int Duration { get; set; }
    }

    public class Flights2
    {
        public string Company { get; set; }
        public int FlightNum { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Departs { get; set; }
        public string Arrives { get; set; }
        public bool ArrOnNextDay { get; set; }
        public int Price { get; set; }
    }

    public class Countries
    {
        public string Name { get; set; }

        public string Code { get; set; }
    }

    public class Cities
    {
        public string Name { get; set; }
        public string Country { get; set; }
    }


    class Logic
    {
        public IStatementResult Run(string command)
        {
            using (ISession session = Program.driver.Session())
            {
                IStatementResult result = session.Run(command);
                return result;
            }
            //Program.driver.Dispose();
        }

        public void ResetDatabase()
        {
            DeleteAll();
            CreateCountries();
            CreateCities();
            CreateAirports();
            CreateAirportDays();
            CreateFlights2();

            Run("CREATE (:test{name:'test'})");

            Console.WriteLine("Database deleted and created!");
        }

        public void DeleteAll()
        {
            Run("MATCH (n) DETACH DELETE n");
        }

        public void CreateCountries()
        {
            List<Countries> countries = ReadCountries();
            foreach (var i in countries)
            {
                Run("create (:country{name:'" + i.Name + "', code:'" + i.Code + "'})");
            }
        }

        public void CreateCities()
        {
            List<Cities> cities = ReadCities();
            foreach (var i in cities)
            {
                Run("create (:city{name:'" + i.Name + "', country:'" + i.Country + "'})");
                Run("match (a:city{name:'" + i.Name + "'}), (b:country{name:'" + i.Country + "'}) merge (a)-[:inCountry]->(b)");
            }
        }

        public void CreateAirports()
        {
            List<Airports> airports = ReadAirports();
            foreach(var i in airports)
            {
                Run("create (:airport{name:'" + i.Code + "', fullName:'" + i.Name + "', city:'" + i.City + "', country:'" + i.Country + "'})");
                Run("match (a:airport{name:'" + i.Code + "'}), (b:city{name:'" + i.City + "'}) merge (a)-[:inCity]->(b)");
            }
        }

        private void CreateAirportDays()
        {
            List<Airports> airports = ReadAirports();
            foreach (var i in airports)
            {
                DateTime begin = new DateTime(2019, 10, 1);
                DateTime end = new DateTime(2019, 10, 30);

                for (DateTime date = begin; date <= end; date = date.AddDays(1))
                {
                    string dateStr = date.ToString("yyyy-M-d", System.Globalization.CultureInfo.InvariantCulture);

                    Run("create (:airportDay{name:'" + i.Code + ":" + dateStr + "'})");
                    Run("match (a:airportDay{name:'" + i.Code + ":" + dateStr + "'}), (b:airport{name:'" + i.Code + "'}) merge (a)<-[:hasDay]-(b)");
                }
            }
        }

        public void CreateFlights()
        {
            List<Flights> flights = ReadFlights();
            foreach (var i in flights)
            {
                Run("match (n { name: '" + i.From + "' }), (m { name: '" + i.To + "' }) merge (n)-[r:" + i.Company + "{price:" + i.Price + ", duration:" + i.Duration + "}]->(m)");
                Run("match (n { name: '" + i.From + "' }), (m { name: '" + i.To + "' }) merge (n)<-[r:" + i.Company + "{price:" + i.Price + ", duration:" + i.Duration + "}]-(m)");
            }
        }

        public void CreateFlights2()
        {
            List<Flights2> flights = ReadFlights2();
            foreach (var i in flights)
            {

                DateTime begin = new DateTime(2019, 10, 1);
                DateTime end = new DateTime(2019, 10, 30);

                int num = 1;

                for (DateTime date = begin; date <= end; date = date.AddDays(1))
                {
                    string dateStr = date.ToString("yyyy-M-d", System.Globalization.CultureInfo.InvariantCulture);
                    string dateNextStr = date.AddDays(1).ToString("yyyy-M-d", System.Globalization.CultureInfo.InvariantCulture);

                    if (i.ArrOnNextDay == false)
                    {
                        Run("create (:flight{name:'" + i.Company + "-" + num + i.FlightNum + "', departs: time('" + i.Departs + "'), arrives: time('" + i.Arrives + "'), price: " + i.Price + "})");
                        Run("match (a:flight{name:'" + i.Company + "-" + num + i.FlightNum + "'}), (b:airportDay{name:'" + i.From + ":" + dateStr + "'}) merge (b)-[:" + i.To + "flight]->(a)");
                        Run("match (a:flight{name:'" + i.Company + "-" + num + i.FlightNum + "'}), (b:airportDay{name:'" + i.To + ":" + dateStr + "'}) merge (a)-[:" + i.To + "flight]->(b)");

                        Run("create (:flight{name:'" + i.Company + "-" + num + (i.FlightNum + 1) + "', departs: time('" + i.Departs + "'), arrives: time('" + i.Arrives + "'), price: " + i.Price + "})");
                        Run("match (a:flight{name:'" + i.Company + "-" + num + (i.FlightNum + 1) + "'}), (b:airportDay{name:'" + i.From + ":" + dateStr + "'}) merge (b)<-[:" + i.To + "flight]-(a)");
                        Run("match (a:flight{name:'" + i.Company + "-" + num + (i.FlightNum + 1) + "'}), (b:airportDay{name:'" + i.To + ":" + dateStr + "'}) merge (a)<-[:" + i.To + "flight]-(b)");
                    }
                    else
                    {
                        Run("create (:flight{name:'" + i.Company + "-" + num + i.FlightNum + "', departs: time('" + i.Departs + "'), arrives: time('" + i.Arrives + "'), price: " + i.Price + "})");
                        Run("match (a:flight{name:'" + i.Company + "-" + num + i.FlightNum + "'}), (b:airportDay{name:'" + i.From + ":" + dateStr + "'}) merge (b)-[:" + i.To + "flight]->(a)");
                        Run("match (a:flight{name:'" + i.Company + "-" + num + i.FlightNum + "'}), (b:airportDay{name:'" + i.To + ":" + dateNextStr + "'}) merge (a)-[:" + i.To + "flight]->(b)");

                        Run("create (:flight{name:'" + i.Company + "-" + num + (i.FlightNum + 1) + "', departs: time('" + i.Departs + "'), arrives: time('" + i.Arrives + "'), price: " + i.Price + "})");
                        Run("match (a:flight{name:'" + i.Company + "-" + num + (i.FlightNum + 1) + "'}), (b:airportDay{name:'" + i.From + ":" + dateStr + "'}) merge (b)<-[:" + i.To + "flight]-(a)");
                        Run("match (a:flight{name:'" + i.Company + "-" + num + (i.FlightNum + 1) + "'}), (b:airportDay{name:'" + i.To + ":" + dateNextStr + "'}) merge (a)<-[:" + i.To + "flight]-(b)");
                    }

                    num++;

                }

                //                Run("create (:flight{name:'" + i.Code + "', departs:'" + i.Departs + "', arrives:'" + i.Arrives + "', price:'" + i.Price + "'})");
                //                Run("match (a:flight{name:'" + i.Code + "'}), (b:airportDay{name:'" + i.From + ":2019-10-2'}) merge (b)-[:" + i.To + "flight]->(a)");
                //                Run("match (a:flight{name:'" + i.Code + "'}), (b:airportDay{name:'" + i.To + ":2019-10-2'}) merge (a)-[:" + i.To + "flight]->(b)");
                //
                //                Run("create (:flight{name:'" + i.Code + "', departs:'" + i.Departs + "', arrives:'" + i.Arrives + "', price:'" + i.Price + "'})");
                //                Run("match (a:flight{name:'" + i.Code + "'}), (b:airportDay{name:'" + i.From + ":2019-10-3'}) merge (b)-[:" + i.To + "flight]->(a)");
                //                Run("match (a:flight{name:'" + i.Code + "'}), (b:airportDay{name:'" + i.To + ":2019-10-3'}) merge (a)-[:" + i.To + "flight]->(b)");
            }
        }

        public List<Countries> ReadCountries()
        {
            List<Countries> retList = new List<Countries>();

            using (var reader = new StreamReader(@"C:\Users\Paulius\Desktop\Countries.csv"))
            {
                int i = 0;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    if (i != 0)
                    {
                        Countries country = new Countries()
                        {
                            Name = values[0],
                            Code = values[1]
                        };
                        retList.Add(country);
                    }
                    i++;
                }
            }
            return retList;
        }

        public List<Cities> ReadCities()
        {
            List<Cities> retList = new List<Cities>();

            using (var reader = new StreamReader(@"C:\Users\Paulius\Desktop\Cities.csv"))
            {
                int i = 0;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    if (i != 0)
                    {
                        Cities city = new Cities()
                        {
                            Name = values[0],
                            Country = values[1]
                        };
                        retList.Add(city);
                    }
                    i++;
                }
            }

            return retList;
        }

        public List<Airports> ReadAirports()
        {
            List<Airports> retList = new List<Airports>();

            using (var reader = new StreamReader(@"C:\Users\Paulius\Desktop\Airports.csv"))
            {
                int i = 0;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    if (i != 0)
                    {
                        Airports airport = new Airports()
                        {
                            Code = values[0],
                            Name = values[1],
                            City = values[2],
                            Country = values[3]
                        };
                        retList.Add(airport);
                    }
                    i++;
                }
            }

            return retList;
        }

        public List<Flights> ReadFlights()
        {
            List<Flights> retList = new List<Flights>();

            using (var reader = new StreamReader(@"C:\Users\Paulius\Desktop\Flights.csv"))
            {
                int i = 0;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    if (i != 0)
                    {
                        Flights flight = new Flights()
                        {
                            Company = values[0],
                            From = values[1],
                            To = values[2],
                            Price = Int32.Parse(values[3]),
                            Duration = Int32.Parse(values[4]),
                        };
                        retList.Add(flight);
                    }
                    i++;
                }
            }

            return retList;
        }

        public List<Flights2> ReadFlights2()
        {
            List<Flights2> retList = new List<Flights2>();

            using (var reader = new StreamReader(@"C:\Users\Paulius\Desktop\Flights2.csv"))
            {
                var i = 0;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    if (i != 0)
                    {
                        var flight = new Flights2()
                        {
                            Company = values[0].Substring(0, 2),
                            FlightNum = int.Parse(values[0].Substring(3)),
                            From = values[1],
                            To = values[2],
                            Departs = values[3],
                            Arrives = values[4],
                            Price = int.Parse(values[5]),
                            ArrOnNextDay = bool.Parse(values[6])
                        };
                        retList.Add(flight);
                    }
                    i++;
                }
            }

            return retList;
        }

        public void FindAirportsByCountry()
        {
            string input;
            input = Console.ReadLine();
            var result = Run("MATCH (n:airport{country:'" + input + "'}) RETURN n LIMIT 25");

            Console.WriteLine("Airports in " + input + ":");
            Console.WriteLine("------------------------------------------------------------------------------------------");
            Console.WriteLine("Airport                                            | City                 | Country       ");
            Console.WriteLine("------------------------------------------------------------------------------------------");
            foreach (var r in result)
            {
                var node = r["n"].As<INode>();

                var name = node["name"].As<string>();
                var city = node["city"].As<string>();
                var country = node["country"].As<string>();

                Console.WriteLine(String.Format("{0,-50} | {1,-20} | {2,-20}", name, city, country));
            }
            Console.WriteLine("-------------------------------------------------------------------------------------------");
            Console.WriteLine();
        }

        public void FindConnectedAirports()
        {
            string input;
            input = Console.ReadLine();
            var result = Run("match (n:airport{name:'" + input + "'})-[]->(m:airport) return m");

            Console.WriteLine("Direct Flights:");
            Console.WriteLine("------------------------------------------------------------------------------------------");
            Console.WriteLine("Airport                                            | City                 | Country       ");
            Console.WriteLine("------------------------------------------------------------------------------------------");

            foreach (var r in result)
            {
                var node = r["m"].As<INode>();

                var name = node["name"].As<string>();
                var city = node["city"].As<string>();
                var country = node["country"].As<string>();

                Console.WriteLine(String.Format("{0,-50} | {1,-20} | {2,-20}", name, city, country));
            }
            Console.WriteLine("-------------------------------------------------------------------------------------------");
            Console.WriteLine();
        }

        public void FindConnectedCountries()
        {
            string input;
            input = Console.ReadLine();
            var result = Run("match (a:country{name:'" + input + "'})-[:inCountry]-()-[:inCity]-()-[]-()-[:inCity]-()-[:inCountry]-(b) where b.name <> '" + input + "' return distinct b");

            Console.WriteLine("Direct flight to:");

            foreach (var r in result)
            {
                var node = r["b"].As<INode>();

                var country = node["name"].As<string>();

                Console.WriteLine(country);
            }
            Console.WriteLine("-----------------");
            Console.WriteLine();
        }

        public void FindNotConnectedCountries()
        {
            string input;
            input = Console.ReadLine();
            var result = Run("match (result:country), (a:country{name:'" + input + "'}) where not (a)-[:inCountry]-()-[:inCity]-()-[]-()-[:inCity]-()-[:inCountry]-(result) and result.name <> '" + input + "' return distinct result");

            Console.WriteLine("No direct flight to:");

            foreach (var r in result)
            {
                var node = r["result"].As<INode>();

                var country = node["name"].As<string>();

                Console.WriteLine(country);
            }
            Console.WriteLine("-----------------");
            Console.WriteLine();
        }

        public void FindCheapAndFast()
        {
            string input1;
            input1 = Console.ReadLine();
            string input2;
            input2 = Console.ReadLine();
            var result = Run("match (start:airport{name:'" + input1 + "'}), (end:airport{name:'" + input2 + "'})CALL algo.shortestPath.stream(start, end, 'price', { nodeQuery: 'match (n:airport) RETURN id(n) as id', relationshipQuery: 'MATCH(n:airport)-[r]->(m:airport) RETURN id(n) as source, id(m) as target, r.price as weight',graph: 'cypher'})YIELD nodeId, cost RETURN algo.getNodeById(nodeId).name AS name, cost");
            String cost = "nan";
            Console.WriteLine("Cheapest flight route: ");
            foreach (var r in result)
            {
                var name = r["name"].As<string>();
                cost = r["cost"].As<string>();

                Console.Write(name + " --- ");
            }
            Console.WriteLine("Total cost: " + cost);

            result = Run("match (start:airport{name:'" + input1 + "'}), (end:airport{name:'" + input2 + "'})CALL algo.shortestPath.stream(start, end, 'duration', { nodeQuery: 'match (n:airport) RETURN id(n) as id', relationshipQuery: 'MATCH(n:airport)-[r]->(m:airport) RETURN id(n) as source, id(m) as target, r.duration as weight',graph: 'cypher'})YIELD nodeId, cost RETURN algo.getNodeById(nodeId).name AS name, cost as duration");
            String dur = "nan";
            Console.WriteLine("Shortest flight route: ");
            foreach (var r in result)
            {
                var name = r["name"].As<string>();
                dur = r["duration"].As<string>();

                Console.Write(name + " --- ");
            }
            Console.WriteLine("Total duration: " + dur);
        }

        public void CountConn()
        {
            string input1;
            input1 = Console.ReadLine();
            string input2;
            input2 = Console.ReadLine();
            Console.WriteLine();
            var result = Run("match (c:airport) where (:airport{name:'" + input1 + "'})-[*.." + input2 + "]->(c) and c.name <> '" + input1  + "' return distinct count(c.name)");
            foreach (var r in result)
            {
                var name = r["count(c.name)"].As<string>();

                Console.WriteLine(name);
            }

            result = Run("match (c:airport) where (:airport{name:'" + input1 + "'})-[*.." + input2 + "]->(c) and c.name <> '" + input1 + "' return distinct c");
            Console.WriteLine("------------------------------------------------------------------------------------------");
            Console.WriteLine("Airport                                            | City                 | Country       ");
            Console.WriteLine("------------------------------------------------------------------------------------------");

            foreach (var r in result)
            {
                var node = r["c"].As<INode>();

                var name = node["name"].As<string>();
                var city = node["city"].As<string>();
                var country = node["country"].As<string>();

                Console.WriteLine(String.Format("{0,-50} | {1,-20} | {2,-20}", name, city, country));
            }
            Console.WriteLine("-------------------------------------------------------------------------------------------");
            Console.WriteLine();

            Console.WriteLine("No Connection:");
            result = Run("match (c:airport) where not (:airport{name:'" + input1 + "'})-[*.." + input2 + "]->(c) and c.name <> '" + input1 + "' return distinct c");
            Console.WriteLine("------------------------------------------------------------------------------------------");
            Console.WriteLine("Airport                                            | City                 | Country       ");
            Console.WriteLine("------------------------------------------------------------------------------------------");

            foreach (var r in result)
            {
                var node = r["c"].As<INode>();

                var name = node["name"].As<string>();
                var city = node["city"].As<string>();
                var country = node["country"].As<string>();

                Console.WriteLine(String.Format("{0,-50} | {1,-20} | {2,-20}", name, city, country));
            }
            Console.WriteLine("-------------------------------------------------------------------------------------------");
            Console.WriteLine();
        }
    }
}
