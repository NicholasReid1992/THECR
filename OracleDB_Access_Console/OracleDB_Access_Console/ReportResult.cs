using System;
using System.Collections.Generic;
using System.Text;


namespace OracleDB_Access_Console
{
    class ReportResult
    {
        public List<List<string>> results;
        public List<string> nullLocations;
        public List<int> nullAt;

        public ReportResult()
        {
            results = new List<List<string>>();
            nullLocations = new List<string>();
            nullAt = new List<int>();

        }

        public void AddList(List<string> queryResult)
        {
            //fill list with any null variable locations
            for (int i = 0; i < queryResult.Count; i++) 
            {
                if (queryResult[i] == "null") { nullLocations.Add(results.Count + ", " + (i+1) ); }
            }

            //normal add for list to list of query results
            results.Add(queryResult);
        }

    }
}