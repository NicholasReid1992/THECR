using System;
using System.Text;
using System.IO;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Collections.Generic;

namespace OracleDB_Access_Console
{
    class Reporting
    {


        public Reporting()
        {

        }

        public string passGiver(string pass) 
        {
            pass = "";
            while (true)
            {
                var key = System.Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter) { break; }
                else if (key.Key == ConsoleKey.Backspace) { if (pass.Length > 0) { pass = pass.Remove(pass.Length - 1); } }
                else if (((int)key.Key) >= 65 && ((int)key.Key <= 90)) { pass += key.KeyChar; }
                else { }
            }
            Console.WriteLine("\n");
            return pass; 
        }

        //used as error checking for main when connection object throws error
        public string connectionError(string err)
        {
            string errType = err.Substring(4, 5);
            if (errType == "12541")         { return "Check Port Number."; }
            else if (errType == "12154")    { return "Check Hostname."; }
            else if (errType == "12514")    { return "Check Service Name."; }
            else                            { return "No problem with connector found."; }
        }

        public void shortReport(OCObject oC)
        {
            for (int i = 0; i < oC.shortHands.Count; i++)
            {
                if ((i % 3 == 2) || (i == oC.shortHands.Count - 1)) { Console.WriteLine(oC.shortHands[i]); }
                else { Console.Write(oC.shortHands[i] + ", "); }
            }
            //foreach (string item in oC.shortHands) { Console.WriteLine(item); } 

        }

        public List<List<string>> runReport(OCObject oC)
        {
            List<List<string>> results = new List<List<string>>();
            List<string> nullLocations = new List<string>();
            string ok = "n";

            Console.WriteLine("This is the current number of queries: " + oC.shortHands.Count);
            Console.Write("Would you like to list the current THEC list (y/n): ");
            if (Console.ReadLine().ToLower() == "y")
            {
                shortReport(oC);
                Console.WriteLine();
            }

            Console.Write("\nIs This OK? (y/n): ");
            ok = Console.ReadLine();
            if ((ok.ToLower() == "y") || (ok.ToLower() == "yes"))
            {
                try { results = oC.fullExecute(); }
                catch (Exception e)
                {
                    Console.WriteLine("Problem with main Execution\n");
                    throw e;
                }


                //TO be changed to function to make into a .csv file later
                for (int i = 0; i < results.Count; i++)
                {
                    //Console.WriteLine("Entry: ");
                    for (int j = 0; j < results[i].Count; j++)
                    { if (results[i][j].ToLower() == "null") { nullLocations.Add((i+1)+", "+(j + 1)); } }
                }

                //List all Nulls found and their location
                foreach (string item in nullLocations) 
                {
                    string entry = item.Substring(0, item.IndexOf(','));
                    string loc = item.Substring(entry.Length + 2);
                    Console.Write("Null at row: " + entry);
                    Console.WriteLine(" and column: " + loc);
                }
            }
            else { Console.WriteLine("Confirmed: No report ran."); }

            return results;
        }

        private string baseTemplate()
        {
            StringBuilder query = new StringBuilder();
            Console.Write("What shorthand should be made: ");
            string sh = Console.ReadLine();
            query.Append("select ");
            Console.Write("What column are you selecting from? ex.(TABLE_FIRST_NAME): ");
            query.Append( Console.ReadLine().ToUpper() );
            Console.Write("What tables do you need? ex.(table1, table2): ");
            string tables = Console.ReadLine().ToUpper();
            query.Append(" into :" + sh + " from ");
            query.Append(tables);
            query.Append(" where ");

            //adding pidm conditional
            if (tables.Contains(',')) { tables = tables.Substring(0, tables.IndexOf(',')); }
            if (tables.IndexOf(' ') == 0) { tables = tables.Substring(1,tables.Length-1); }
            query.Append(" " + tables + "_PIDM = ( :INPUT_INSERT_PIDM );");
            return query.ToString();
        }

        private string ConditionTemplate()
        {
            StringBuilder query = new StringBuilder();
            Console.Write("What THEC Shorthand should be made: ");
            string sh = Console.ReadLine();
            query.Append("select ");
            Console.Write("What column are you selecting from? ex.(TABLE_FIRST_NAME): ");
            query.Append(Console.ReadLine().ToUpper());
            Console.Write("What tables do you need? ex.(table1, table2): ");
            string tables = Console.ReadLine().ToUpper();
            query.Append(" into :" + sh + " from ");
            query.Append(tables);
            query.Append(" where ");

            //where statements
            Console.WriteLine("what are the conditionals after the where ex.(... NAME IS NULL)?: ");
            query.Append(Console.ReadLine());
            query.Append(" and ");

            //adding pidm conditional
            if (tables.Contains(',')) { tables = tables.Substring(0, tables.IndexOf(',')); }
            if (tables.IndexOf(' ') == 0) { tables = tables.Substring(1, tables.Length - 1); }
            query.Append(" " + tables + "_PIDM = ( :INPUT_INSERT_PIDM );");
            return query.ToString();
        }

        private void templateType()
        {
            Console.WriteLine("1. Simple Template (No conditionals/where)");
            Console.WriteLine("2. Conditional Template");
            Console.Write("Then which template do you want: ");
        }

        private void addReport(OCObject oC)
        {
            string manual = "n";
            Console.Write("Do you want to add the query manually? (y/n): ");
            manual = Console.ReadLine().ToLower();
            string newQuery = "";
            if (manual == "y")
            {
                Console.WriteLine("Input the new query to add: ex. (select <Column> into :<VARI> from <Table> where ...)");
                newQuery = Console.ReadLine();
            }
            else 
            {
                templateType();
                string ans = Console.ReadLine();
                if (ans == "1") { newQuery = baseTemplate(); }
                else if (ans == "2") { newQuery = ConditionTemplate(); }
                else { Console.WriteLine("Not a reconized choice"); }
            }

            if (newQuery.Length > 5) 
            {
                oC.queries.Add(newQuery);
                oC.shortHands = oC.shorting(oC.queries)[0];
            }
            else { Console.WriteLine("No query added"); }
        }

        private void delReport(OCObject oC)
        {
            Console.WriteLine("Input column to be Deleted: ex. (THEC_SSN)");
            string delShorthand = Console.ReadLine().ToUpper();
            int loc = oC.shortHands.IndexOf(delShorthand);
            if (loc == -1) { Console.WriteLine("Column not found"); }
            else
            {
                if (oC.queries[loc].Contains(":" + delShorthand))
                {
                    oC.shortHands.RemoveAt(loc);
                    oC.queries.RemoveAt(loc);
                    Console.WriteLine("Removed Column");
                }
                else
                {
                    for (int i = 0; i < oC.queries.Count; i++)
                    {
                        if (oC.queries[i].Contains(":" + delShorthand))
                        {
                            oC.shortHands.RemoveAt(i);
                            oC.queries.RemoveAt(i+1);
                            Console.WriteLine("Removed Column");
                            break;
                        }
                    }
                }
            }
        }

        private void reorderReport(OCObject oC)
        {
            Console.WriteLine("Input column to be Moved: ex. (SSN)");
            string movShorthand = Console.ReadLine().ToUpper();
            int loc = oC.shortHands.IndexOf(movShorthand);
            int pos;
            //check if real shorthand
            if (loc == -1) { Console.WriteLine("Column not found"); }
            //if so
            else
            {
                //what position to move to
                Console.WriteLine("The column is in order: " + (loc+1).ToString() + " out of " + oC.shortHands.Count.ToString());
                Console.WriteLine("What position should it be moved to: ex. (" + oC.shortHands.Count.ToString() + ") for last");
                //check if position is number
                if (int.TryParse(Console.ReadLine(), out pos))
                {
                    //if so, find shorthand in queries and remove, the replace in the list
                    if (oC.queries[loc].Contains(":" + movShorthand))
                    {
                        //remove and replace shorthands
                        string tempShorthand = oC.shortHands[loc];
                        oC.shortHands.RemoveAt(loc);
                        //insert needs to be checked
                        oC.shortHands.Insert(pos-1, tempShorthand);

                        //remove and replace query
                        string tempQuery = oC.queries[loc];                        
                        oC.queries.RemoveAt(loc);
                        oC.queries.Insert(pos-1, tempQuery);

                        Console.WriteLine("Moved Column");
                    }
                    //if so, but not in same order as the shorthand list find the remove/replace
                    else
                    {
                        for (int i = 0; i < oC.queries.Count; i++)
                        {
                            if (oC.queries[i].Contains(":" + movShorthand))
                            {
                                //remove and replace shorthands
                                string tempShorthand = oC.shortHands[loc];
                                oC.shortHands.RemoveAt(loc);
                                oC.shortHands.Insert(pos-1, tempShorthand);

                                //remove and replace query
                                string tempQuery = oC.queries[loc];
                                oC.queries.RemoveAt(loc);
                                oC.queries.Insert(pos-1, tempQuery);

                                Console.WriteLine("Moved Column");
                                break;
                            }
                        }
                    }
                }
                //if not a number
                else { Console.WriteLine("That is not a Number."); }
            }
        }

        //what to do with the modify
        public void modifyList(OCObject oC)
        {
            string input = "";
            Console.WriteLine("Choices:");
            Console.WriteLine("1. Add to the Report");
            Console.WriteLine("2. Delete field from the Report");
            Console.WriteLine("3. Reorder current Report");
            Console.WriteLine("What kind of modification is to be done:");
            input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    addReport(oC);
                    break;
                case "2":
                    delReport(oC);
                    break;
                case "3":
                    reorderReport(oC);
                    break;

                default:
                    Console.WriteLine("Not a choice");
                    break;
            }

        }


        /*
        public List<string> directGrab(string column, string table)
        {
            string sqlSelect = "SELECT " + column + " FROM " + table + ";";
            List<string> output = oCObject.generalSqlCommand(sqlSelect);
            return output;
        }

        public List<string> quickSelect(List<string> columns , string table)
        {
            string sqlSelect = "SELECT ";
            for (int i=0; i < (columns.Count-1); i++)
            {
                sqlSelect += columns[i] + ", ";
            }
            sqlSelect += columns[(columns.Count - 1)] + " FROM " + table + ";";

            List<string> output = oCObject.generalSqlCommand(sqlSelect);
            return output;
        }
        */
        public void listTables(OCObject oC)
        {
            Console.WriteLine("Tables:");
            string owner = "C##GHAY";
            string tableSQL1 = "SELECT owner FROM all_tables WHERE owner IN ('" + owner + "')";
            string tableSQL2 = "SELECT table_name FROM all_tables WHERE owner IN ('" + owner + "')";
            List<string> testResults1 = oC.generalSqlCommand(tableSQL1);
            List<string> testResults2 = oC.generalSqlCommand(tableSQL2);
            for (int i = 0; i < testResults1.Count; i++) { Console.WriteLine(testResults1[i] + "/" + testResults2[i]); }
        }

        public void sqlChoice(OCObject oC)
        {
            Console.Write("Type Test SQL funct (w/o ;): ");
            string testSql = Console.ReadLine();
            List<string> testResults = oC.generalSqlCommand(testSql);
            foreach (string tt in testResults) { Console.WriteLine("Test result: " + tt.ToString()); }
        }

        public void runSQLFile(OCObject oC)
        {
            Console.Write("\nFileName: ");
            string sqlFile = Console.ReadLine();
            List<List<string>> testResults = oC.ScriptExec(sqlFile);
            foreach (var i in testResults)
            {
                Console.WriteLine("------- Query ------");
                foreach (string tt in i) { Console.WriteLine("Script Result: " + tt);
                }
            }
        }

        public void runPLSQLFile(OCObject oC)
        {
            List<List<string>> Test = oC.fullExecute();
            foreach (List<string> item in Test)
            {
                Console.WriteLine("Entry: ");
                foreach (string i in item) { Console.WriteLine("Row: " + i); }
            }

        }

        //Function to check if a column in table
        public void checkCol(OCObject oC)
        {
            Console.Write("\nTable Name: ");
            string table = Console.ReadLine();
            Console.Write("Column Name: ");
            string column = Console.ReadLine();

            bool result = oC.columnExists(column, table);
            if (result) { Console.WriteLine(column + " Exists in " + table); }
            else { Console.WriteLine("Does Not Exist"); }
        }

        //Function to make 2D into .csv file
        public void csvBuilder(List<List<string>> results, string fileName, List<string> shorts)
        {
            var csv = new StringBuilder();
            //marking title of each column to be in the csv
            //the shorthands will be used as column titles
            for (int i = 0; i < shorts.Count; i++)
            {
                if (i == shorts.Count - 1) { csv.Append(shorts[i]); }
                else { csv.Append(shorts[i] + ","); }
            }
            csv.AppendLine();

            //adding each entry as a new row in the csv
            for (int i = 0; i < results.Count; i++)
            {
                for (int j = 0; j < results[i].Count; j++) { csv.Append("\"" + results[i][j] + "\"" + ","); }
                csv.AppendLine();
            }

            //saving the file
            //making new file
            if (!File.Exists(fileName)) { File.WriteAllText(fileName + ".csv", csv.ToString()); }
            //appending if file exists
            else { File.AppendAllText(fileName + ".csv", csv.ToString()); }
        }

        public void saveReport(string report, string fileName)
        {
            //saving the file
            //making new file
            try { File.WriteAllText(fileName, report); }
            catch (Exception e)
            {
                Console.WriteLine("Problem Saving File.");
                throw e;
            }
        }
    }
}
