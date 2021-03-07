using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.XPath;

namespace Ecrion
{
    class Program
    {
        static void Main(string[] args)
        {

            // example C:\ /ns:Statements/ns:Statement/ns:Transactions/ns:Transaction/@Id 5 E:\
            // bad example order C:\ /ns:Statements/ns:Statement/ns:Transactions/ns:Transaction/@Id 5 E:\
            // bad example xpathExpression C:\ /ns:Statements/ns:Statement/ns:Transactions/ns:Transaction/@invalidId 5 E:\
         
            //Get commands
            String command = Console.ReadLine();

         //validate commands
            var commandsList = ParseCommands(command);

            string[] files=GetFiles(commandsList[0]);
            int threads = Int32.Parse(commandsList[2]);
            string outputPath = commandsList[3];
            String strExpression = commandsList[1];
            string attribute = strExpression.Substring(strExpression.LastIndexOf("@")+1, strExpression.Length-strExpression.IndexOf("@")-1);
            //declare necessary objects for xpath
            XPathNavigator nav;
            XPathDocument docNav;
            XPathNodeIterator NodeIter;

            //set number of threads
            ThreadPool.SetMinThreads(threads, 0);
            ThreadPool.SetMaxThreads(threads, 0);

            //used to write to the file
            var csv = new StringBuilder();

            //loop through each file
            foreach (String file in files)
            {
                //create necessary objects for the xpath value extraction to work
                docNav = new XPathDocument(@"" + file);
                nav = docNav.CreateNavigator();
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(nav.NameTable);
                nsmgr.AddNamespace("ns", @"http://www.tempuri.org/XML");
                NodeIter = nav.Select(strExpression, nsmgr);
                //allocate each thread 1 file
                ThreadPool.QueueUserWorkItem(state => ThreadProc(NodeIter.Clone(), outputPath, csv,attribute));
            }

            //keep console open
            Console.ReadLine();
            while (true)
            {

            }
        }
        public static void ThreadProc(XPathNodeIterator NodeIter, String outputPath, StringBuilder csv,String attribute)
        {
            if (NodeIter.Count == 0)
            {
                ErrorLogger.Log("no xpath attribute was found");
            }
            //iterate through the xml
            while (NodeIter.MoveNext())
            {
                var second = NodeIter.Current.ToString();
                var first = attribute;
                var newLine = string.Format("{0},{1}", first, second);
                //stringbuilder is not thread safe
                lock (csv)
                {
                    csv.AppendLine(newLine);
                }

            }
            //writting is not thread safe so we lock it
            lock (csv)
            {
                File.WriteAllText(outputPath + "output.csv", csv.ToString());
            }


        }
        public static string[] ParseCommands(string command)
        {

            var commandsList = command.Split().Select(x => x).ToArray();
            if (commandsList.Length > 4)
            {
                ErrorLogger.Log("not enough arguments");
            }
            try
            {
                String inputPath = commandsList[0];
                int threads = Int32.Parse(commandsList[2]);
                String outputPath = commandsList[3];
                String strExpression = commandsList[1];

            }
            catch (IndexOutOfRangeException ex)
            {
                ErrorLogger.Log(ex.ToString());
                throw new IndexOutOfRangeException("argument is missing");
            }
            catch (FormatException ex)
            {
                ErrorLogger.Log(ex.ToString());
                throw new FormatException("formating is incorrect");
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex.ToString());
                throw new Exception("unexpected error occured");
            }


            return commandsList;
        }
        public static string[] GetFiles(string inputPath)
        {
            //this should ignore the files that are not part of the required ones
            //logging a message if there are files that don't need to be used seems useless and inefficient 

            string[] allFiles = Directory.GetFiles(inputPath, "*.*", SearchOption.TopDirectoryOnly);
            string[] files = Directory.GetFiles(inputPath, "*.xml", SearchOption.TopDirectoryOnly);
            if (allFiles.Length > files.Length)
            {
                ErrorLogger.Log("files that are not xml inside directory");
            }
            return files;
        }

    }
}
