using System;
using System.Security;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Collections.Generic;

namespace OracleDB_Access_Console
{
    class Program
    {
        private const string structure  = "----------------------------------------------------";
        private const string structure2 = "----------------------------------------------------\n";
        static void Main(string[] args)
        {
            string userId = "hr";
            string password = "<password>";
            string hostname = "localhost";
            string portNum = "1521";
            string serviceName = "ORCL.HAYWORKS";
            string termcode = "201940";
            //OCObject oC = new OCObject(userId, password, "localhost", "1521", "ORCL.HAYWORKS", termcode);
            bool again = true;
            Reporting report = new Reporting();
            OCObject oC = new OCObject();

            Console.WriteLine(structure);
            Console.WriteLine("Welcome to the THECR Program");
            Console.WriteLine(structure2);

            Console.WriteLine(structure);
            while (!oC.isConnect)
            {
                Console.Write("Do you need the manual login? (y/n): ");
                string manual = Console.ReadLine();

                Console.Write("Input username: ");
                userId = Console.ReadLine();
                Console.Write("Input password: ");
                password = report.passGiver(password);

                if (manual.ToLower() == "y")
                {
                    Console.Write("Input hostname: ");
                    hostname = Console.ReadLine();
                    Console.Write("Input port number: ");
                    portNum = Console.ReadLine();
                    Console.Write("Input service name: ");
                    serviceName = Console.ReadLine();
                    Console.WriteLine(structure2);
                    try { oC = new OCObject(userId, password, hostname, portNum, serviceName); }
                    catch (Exception e) 
                    {
                        Console.WriteLine(report.connectionError(e.Message));
                        Console.WriteLine(e.Message);
                    }
                }
                else
                {
                    Console.WriteLine(structure2);
                    try { oC = new OCObject(userId, password); }
                    catch (Oracle.ManagedDataAccess.Client.OracleException e) 
                    {
                        Console.WriteLine(report.connectionError(e.Message));
                        Console.WriteLine(e.Message); 
                    }
                }

                Console.WriteLine(structure2);

                Console.Write("Input query file (default if empty): ");
                string mFile = Console.ReadLine();
                Console.Write("Input PIDM file (default if empty): ");
                string sFile = Console.ReadLine();
                Console.Write("Input termcode: ");
                string term = Console.ReadLine();
                if (mFile.Length > 0) { oC.masterFile = mFile; }
                if (sFile.Length > 0) { oC.sqlFile = sFile; }
                oC.termcode = term;
            }


            while (again)
            {
                menu();
                string choice = "8";
                Console.Write("What to do?: ");
                choice = Console.ReadLine();
                Console.WriteLine(structure2);
                switch (choice)
                {
                    case "1":
                        List<List<string>> results = new List<List<string>>();
                        Console.WriteLine(structure2);
                        try { results = report.runReport(oC); }
                        catch (Exception) { Console.WriteLine("Problem Running the Report"); }
                        if (results.Count > 0)
                        {
                            Console.WriteLine("Successfully made THEC Report");
                            Console.Write("Would you like to save the report to a *.csv file? (y/n): ");
                            string saving = Console.ReadLine().ToLower();
                            if (saving == "y")
                            {
                                Console.Write("What file name would like save the report: ");
                                string saveName = Console.ReadLine();
                                report.csvBuilder(results, saveName, oC.shortHands);
                            }

                            Console.Write("Would you like save the last used queries? (y/n): ");
                            saving = Console.ReadLine().ToLower();
                            if (saving == "y")
                            {
                                Console.Write("What file name would like save the queries under: ");
                                string saveName = Console.ReadLine();
                                report.saveReport(oC.ReportBlock, saveName);
                            }
                        }

                        else { Console.WriteLine("Report was empty"); }
                        break;
                    case "2":
                        Console.Write("Would you like to list the current THEC list (y/n): ");
                        if (Console.ReadLine().ToLower() == "y")
                        {
                            Console.WriteLine();
                            report.shortReport(oC);
                            Console.WriteLine();
                        }
                        report.modifyList(oC);

                        break;
                    case "3":
                        report.checkCol(oC);
                        break;
                    case "T":
                    case "t":
                        report.sqlChoice(oC);
                        break;
                    case "L":
                    case "l":
                        report.listTables(oC);
                        break;
                    case "F":
                    case "f":
                        report.runSQLFile(oC);
                        break;
                    default:
                        Console.WriteLine("default");
                        break;
                }

                Console.WriteLine(structure2);
                Console.WriteLine("Again? (y/n): ");
                string a = Console.ReadLine();
                if (a.ToLower() == "y") { again = true; }
                else { again = false; }
            }

            Console.WriteLine("Ending Program: Hit the Enter key to end");
            Console.ReadLine();
        }

        static void menu()
        {
            Console.WriteLine(structure);
            Console.WriteLine("Choices:");
            Console.WriteLine("1. Run THEC");
            Console.WriteLine("2. Modify THEC");
            Console.WriteLine("3. See if Field Exists");
            Console.WriteLine("T. Test Sql");
            Console.WriteLine("L. List Tables");
            Console.WriteLine("F. Run sql file");
        }
    }
}
