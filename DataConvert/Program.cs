using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DataConvert
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                PrintUsage();
                return;
            }
            Console.WriteLine("Begin to retrieve data from indeed.com...");
            Console.WriteLine("Parameters:");
            String fullQuery = "";
            String fullQuery2 = "";
            int n = 0;
            foreach (var lang in args)
            {
                if (n < 5)
                {
                    fullQuery += lang;
                    fullQuery += "\t";
                }
                else
                {
                    fullQuery2 += lang;
                    fullQuery2 += "\t";
                }
                Console.WriteLine("\t" + lang);
                n++;
            }
            Console.WriteLine("Now downloading data...");
            fullQuery = fullQuery.TrimEnd("\t".ToCharArray());
            fullQuery2 = fullQuery2.TrimEnd("\t".ToCharArray());
            String json = DownloadDataFile(fullQuery);
            String json2 = DownloadDataFile(fullQuery2);
            Console.WriteLine("Now saving data to CSV...");
            ConvertToCsv(json, json2);
        }

        static String DownloadDataFile(String language)
        {
            String languageEncoded = WebUtility.UrlEncode(language);

            WebClient client = new WebClient();
            String json = client.DownloadString("http://www.indeed.com/jobtrends/trends/data?q=" + languageEncoded + "&t=1&subfrom=jobtrends-init");
            return json;
        }

        static void ConvertToCsv(String json, String json2)
        {
            //String fileName = args[0];

            Dictionary<String, ArrayList> dataDict = new Dictionary<string, ArrayList>();

            JObject results = JObject.Parse(json);
            JToken response = results["response"];
            int numDays = 0;
            DateTime dtStart = DateTime.Now;
            String fileName = "indeed.csv";
            foreach (var v in response)
            {

                ArrayList langData = new ArrayList();
                dataDict[v["term"].ToString()] = langData;

                dtStart = DateTime.Parse(v["startDate"].ToString());
                numDays = Convert.ToInt32(v["numDays"].ToString());

                
                foreach (var da in v["data"])
                {
                    langData.Add(da.ToString());
                }        
            }

            JObject results2 = JObject.Parse(json2);
            JToken response2 = results2["response"];
            
            foreach (var v in response2)
            {

                ArrayList langData = new ArrayList();
                dataDict[v["term"].ToString()] = langData;

                foreach (var da in v["data"])
                {
                    langData.Add(da.ToString());
                }
            }

            var export = new CsvExport();
            for (int i = 0; i < numDays; i++)
            {
                export.AddRow();
                export["Date"] = dtStart.ToShortDateString();
                foreach (var lang in dataDict.Keys)
                {
                    export[lang] = dataDict[lang][i];
                }

                dtStart = dtStart.AddDays(1);
            }
            export.ExportToFile(fileName);
            Console.WriteLine(fileName + " generated!");
        }
        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("\tDataConvert.exe [keyword1] [keyword2]...");
            Console.WriteLine("\nExample:");
            Console.WriteLine("\tDataConvert.exe JAVA C++ Python");
            Console.WriteLine("\n\nOutput file will be indeed.csv.");
        }
    }

}
