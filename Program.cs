using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver.V1;

namespace Neo4J
{
    class Program
    {
        public static IDriver driver = GraphDatabase.Driver("bolt://localhost:11002", AuthTokens.Basic("neo4j", "labas"));
        static void Main(string[] args)
        {
            StartMenu();
            Console.ReadKey();
        }

        static public void StartMenu()
        {
            string input;

            Logic log = new Logic();

            InitialMessage();

        Menu:
            input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    log.FindAirportsByCountry();
                    break;
                case "2":
                    log.FindConnectedAirports();
                    break;
                case "3":
                    log.FindConnectedCountries();
                    break;
                case "4":
                    log.FindNotConnectedCountries();
                    break;
                case "5":
                    log.FindCheapAndFast();
                    break;
                case "6":
                    log.CountConn();
                    break;
                case "98":
                    log.ResetDatabase();
                    break;
                case "99":
                    Console.WriteLine("You chosen Exit");
                    goto ExitMenu;
                default:
                    Console.WriteLine("Incorrect input");
                    goto Menu;
            }
            goto Menu;
        ExitMenu:
            Console.WriteLine("Thank You for using our great program!");
            Console.ReadKey();
        }

        static private void InitialMessage()
        {
            Console.WriteLine("Welcome to Election System! Choose a function:");
            Console.WriteLine("1 - Find airport by Country");
            Console.WriteLine("2 - Find connected airports");
            Console.WriteLine("3 - Find connected countries");
            Console.WriteLine("4 - Find not connected countries");
            Console.WriteLine("5 - Find cheapest and fastest way between airports");
            Console.WriteLine("6 - Count n level connections");
            Console.WriteLine("98 - Reset Database");
            Console.WriteLine("99 - Exit");
        }
    }
}
