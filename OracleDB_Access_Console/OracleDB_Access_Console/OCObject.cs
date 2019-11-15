using System;
using System.Collections.Generic;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.IO;

namespace OracleDB_Access_Console
{
    class OCObject
    {
        //Holds the current connection to Oracle
        public OracleConnection con;
        public List<string> shortHands;
        public List<string> queries;

        private List<string> insertSH;
        private List<string> pidms;
        private string _masterFile = "THEC_QUERIES.pls";
        public string masterFile { set { _masterFile = value; } }
        private string _sqlFile = "PIDM_ORGANIZER.sql";
        public string sqlFile { set { _sqlFile = value; } }
        private string _termcode;
        public string termcode { set { _termcode = value; } }
        
        public string ReportBlock = "begin \n";

        public bool isConnect = false;

        /* Need to do:
         * change passwords to System.Security.SecureString
         * error checking for reader
         */

        public OCObject() { isConnect = false; }

        public OCObject(string userId, string password)
        {
            string[] args = new string[] { userId,password,"","",""};
            Console.WriteLine("Creating New Oracle Connection");
            
            con = createConnection(true, args);
            //Opening connection to Oracle DBMS
            Console.WriteLine("Openning Oracle Connection");
            isConnect = true;
            try { con.Open(); }
            catch (Oracle.ManagedDataAccess.Client.OracleException e)
            {
                isConnect = false;
                Console.WriteLine(e.Message);
            }
            //Grabbing object variables
            try { PopulateQSP(_masterFile); }
            catch (Exception e)
            {
                Console.WriteLine("Problem Populating Lists from Files\n");
                Console.WriteLine(e.Message);
            }
        }

        public OCObject(string userId, string password, string hostname, string portNum, string serviceName)
        {
            string[] args   = new string[] { userId, password, hostname, portNum, serviceName };
            Console.WriteLine("Creating Custom Oracle Connection");

            con = createConnection(false, args);            
            //Opening connection to Oracle DBMS
            Console.WriteLine("Opening Oracle Connection");
            isConnect = true;
            try { con.Open(); }
            catch (Oracle.ManagedDataAccess.Client.OracleException e)
            {
                isConnect = false;
                Console.WriteLine(e.Message);
            }
            //Grabbing object variables
            try { PopulateQSP(_masterFile); }
            catch (Exception e)
            {
                Console.WriteLine("Problem Populating Lists from Files\n");
                Console.WriteLine(e.Message);
            }            
        }

        private OracleConnection createConnection(bool hasConfig, string[] args)
        {

            string conString = "User Id = " + args[0] + "; Password = " + args[1] + "; " + "Data Source =";
            //How to connect to an Oracle DB without SQL*Net configuration file, also known as tnsnames.ora.
            if (hasConfig) { conString += "orclpdb;"; }
            //String below is for SQL*Net config file.
            else { conString += args[2] + ":" + args[3] + "/" + args[4] + ";"; }

            OracleConnection con = new OracleConnection();
            con.ConnectionString = conString;
            return con;
        }

        //reads, runs, and returns a list from a sql command
        public List<string> generalSqlCommand(string sqlquery)
        {
            //Create command within context of the connection
            //The CommandText will be a string of the actual SQL Query
            OracleCommand cmd = new OracleCommand(sqlquery, con);
            cmd.CommandText     = sqlquery;
            List<string> output = new List<string>();
            try { output = generalReader(cmd); }
            catch (Exception e)
            {
                Console.WriteLine("Problem with Reader");
                Console.WriteLine(e.Message);
                output = new List<string>();
            }

            cmd.Dispose();
            return output;
        }

        //Helper of generalSqlCommand reads in command
        private List<string> generalReader(OracleCommand cmd)
        {
            //Execute the command and use DataReader grab return statement
            OracleDataReader reader = cmd.ExecuteReader();
            List<string> output = new List<string>();
            try
            {
                while (reader.Read())
                {
                    //for each entry checking type of object given then translates it to string and adds it to list
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        //Console.WriteLine(reader.GetDataTypeName(i));
                        //if entry is null return "NIL"
                        if (reader.IsDBNull(i)) { output.Add("NIL"); }
                        else
                        {
                            if (reader.GetDataTypeName(i) == "Decimal") { output.Add(reader.GetDecimal(i).ToString()); }
                            else if (reader.GetDataTypeName(i).StartsWith("Int")) { output.Add(reader.GetDecimal(i).ToString()); }
                            else { output.Add(reader.GetString(i)); }
                        }
                    }
                }
            }
            finally { reader.Close(); }
            return output;
        }

        //Checks if a column exiists
        public bool columnExists(string col_name, string tab_name)
        {
            bool trfa = false;
            List<string> output = new List<string>();
            string sql = "SELECT COLUMN_NAME AS FOUND FROM USER_TAB_COLS WHERE TABLE_NAME = '" + tab_name + "' AND COLUMN_NAME = '" + col_name + "'";

            output = generalSqlCommand(sql);
            
            if (output.Count > 0) { trfa = true; }
            return trfa;
        }

        //Helper Function for PopulateQSP and ScriptExec to create the list of commands
        public List<string> ScriptToQuery(string fileName) {
            List<string> queries = new List<string>();
            string text, newText = "";
            bool ifPL = true;

            try
            {
                using (StreamReader sr = new StreamReader(fileName)) { text = sr.ReadToEnd(); }
                //beginning script at PL/SQL BEGIN:
                int beginInt = text.IndexOf("BEGIN");
                if (beginInt == -1) { beginInt = text.IndexOf("begin"); }
                if (beginInt == -1 ) { ifPL = false; }
                else { text = text.Substring(beginInt + 5); }

                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] == '/')
                    {
                        if (text[i+1] == '*')
                        {
                            i += 2;
                            while (text[i] != '/')
                            {
                                while (text[i] != '*') { i++; }
                                i++;
                            }
                            i++;
                        }
                    }
                    if (text[i] == '-')
                    {
                        if (text[i+1] == '-')
                        {
                            i += 2;
                            while ((text[i] != '\n')) { i++; }
                            i++;
                        }
                    }

                    if (text[i] ==';')
                    {
                        if (ifPL) { newText += text[i]; }

                        queries.Add(newText);
                        //Console.WriteLine(newText);
                        newText = "";
                        i++;

                    }
                    if (i < text.Length) { newText += text[i]; }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("File Loading Problem\n");
                Console.WriteLine(e.Message);
            }

            //for (int i = 0; i < queries.Count; i++) { queries[i] = queries[i].Replace(Environment.NewLine, " "); }
            if(ifPL) { queries.RemoveAt(queries.Count - 1); }
            return queries;
        }

        //Helper Function for PopulateQSP to create the shorthands/variable names list
        public List<string>[] shorting(List<string> allQueries) {
            List<string>[] shorts = new List<string>[2];
            shorts[0] = new List<string>();
            shorts[1] = new List<string>();

            foreach (string item in allQueries)
            {
                string itemMod = item;
                int iStart = itemMod.IndexOf(":INPUT_");
                while (iStart != -1)
                {
                    iStart += 1;
                    int iLen = ((itemMod.IndexOf(' ', iStart)) - iStart);
                    string input = itemMod.Substring(iStart, iLen);
                    if (!shorts[1].Contains(input)) { shorts[1].Add(itemMod.Substring(iStart, iLen)); }
                    itemMod = itemMod.Remove(iStart - 1, iLen + 1);
                    iStart = itemMod.IndexOf(":INPUT_");
                }

                iStart = itemMod.IndexOf("into :");
                while (iStart != -1)
                {
                    iStart += 6;
                    int len = ((itemMod.IndexOf(' ', iStart)) - iStart);
                    string output = itemMod.Substring(iStart, len);
                    if (!shorts[0].Contains(output)) { shorts[0].Add(output); }
                    itemMod = itemMod.Remove(iStart - 6, len + 6);
                    iStart = itemMod.IndexOf(':');
                }    
            }
            return shorts;
        }

        //Function to set the Objects Lists of command queries and shorthands for them        
        public void PopulateQSP(string mFileName) {
            //loading queries and shorthands to lists
            try
            {
                queries = ScriptToQuery(mFileName);
                List<string>[] temp = shorting(queries);
                shortHands = temp[0];
                insertSH = temp[1];

            }
            catch (Exception e)
            {
                Console.WriteLine("Problem Loading Master File\n");
                Console.WriteLine(e.Message);
            }
        }
        
        //Executes a base SQL Scipt file and returns a list for each response in an outer list
        public List< List<string> > ScriptExec(string fileName)
        {
            //Reading in Script File
            List<string> script = new List<string>();
            try { script = ScriptToQuery(fileName); }
            catch (Exception e)
            {
                Console.WriteLine("Problem loading Script File\n");
                Console.WriteLine(e.Message);
            }

            //Running Script
            List< List<string> > output = new List< List<string> >();
            foreach (string i in script) { output.Add(generalSqlCommand(i)); }

            if (output.Count <= 0) { Console.WriteLine("No Scripts Loaded"); }
            else { if (output[0].Count <= 0) { Console.WriteLine("Problem Running Script\n"); } }
            return output;
        }

        //function to create the parameters for the plsql commands on single pidm
        private List<string> ExecuteSingle(string pidm, string Report) {
            List<string> responseEntry = new List<string>();
            List<OracleParameter> parameters = new List<OracleParameter>();
            List<OracleParameter> inputParameters = new List<OracleParameter>();
            OracleCommand cmd = con.CreateCommand();
            cmd.BindByName = true;

            //Making List of all parameters
            foreach (string name in shortHands)
            {
                OracleParameter p = new OracleParameter(name, OracleDbType.Varchar2,200);
                p.Direction = System.Data.ParameterDirection.Output;
                parameters.Add(p);
            }

            foreach (string iName in insertSH)
            {
                OracleParameter p = new OracleParameter(iName, OracleDbType.Varchar2, System.Data.ParameterDirection.Input);
                if (iName == "INPUT_INSERT_PIDM") { p.Value = pidm; }
                else if (iName == "INPUT_INSERT_TERMCODE") { p.Value = _termcode; }
                else
                {
                    Console.WriteLine("Enter input for variable \'" + iName + "\': ");
                    p.Value = Console.ReadLine();
                }
                //OracleParameter p = new OracleParameter(iName, OracleDbType.Varchar2, pidm, System.Data.ParameterDirection.Input);
                inputParameters.Add(p);
            }

            //example:
            //string testAnonyBlock = "Begin " + " select a.name, a.pid " + " into :p_namer, " + " :p_pidm " + "from test_table a  " + "where a.pid = 53; " + "end;";
            //Console.WriteLine(ReportBlock);
            cmd.CommandText = Report;
            //Adding all Parameters to Command from list
            foreach (OracleParameter parameter in parameters) { cmd.Parameters.Add(parameter); }
            foreach (OracleParameter input in inputParameters) { cmd.Parameters.Add(input); }

            try
            {
                //Executing
                cmd.ExecuteNonQuery();

                //Adding Entry as List of strings
                for (int i = 0; i < parameters.Count; i++)
                {
                    string temp = parameters[i].Value.ToString();
                    if (temp != " ") { responseEntry.Add(temp); }
                    else { responseEntry.Add(""); }
                }
                // how to pull vales from params = p_1.Value.ToString(), p_2.Value.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem Executing Program: ");
                Console.WriteLine(e.Message);
            }
            //Ending Command and returning Entry List
            finally { cmd.Dispose(); }
            return responseEntry;
        }

        //Function to run parameters over all pidms
        public List< List<string> > fullExecute() {
            List< List<string> > allEntries = new List<List<string>>();
            //loading pidms to use queries on
            try { pidms = generalSqlCommand((ScriptToQuery(_sqlFile)[0]).Replace("<insert_term>", _termcode)); }
            catch (Exception e)
            {
                Console.WriteLine("Problem Loading PIDM File\n");
                Console.WriteLine(e.Message);
            }
            if (ReportBlock.Length > 10) { ReportBlock = "begin \n"; }
            //Creating Command and adding block string to it
            for (int i = 0; i < queries.Count; i++)
            {
                //tempQueries.Add((queries[i].Replace("<insert_pidm>", pidm)).Replace("<insert_term>", termcode));
                ReportBlock += queries[i] + "\n";
            }
            ReportBlock += "\n end;";

            try { 
                foreach (string pidm in pidms)
                {
                    allEntries.Add(ExecuteSingle(pidm,ReportBlock));
                } 
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
            
            return allEntries;
        }

        //Function



    }
}
