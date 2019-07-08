using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Windows.Forms;
using System.Text;

namespace DayZLibraryFix
{
    class Program
    {
        static bool detectDuplicatePath(List<XmlDocument> allDocuments, string pathFile, string nameTemplate)
        {
            foreach (XmlDocument document in allDocuments)
            {
                XmlNodeList nodes = document.DocumentElement.SelectNodes("/Library/Template");
                foreach (XmlNode node in nodes)
                {
                    string filePath = node.SelectSingleNode("File").InnerText;

                    if (node.SelectSingleNode("Name").InnerText != nameTemplate && filePath == pathFile )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static StringBuilder LogString = new StringBuilder();
        public static void Out(string str)
        {
            Console.WriteLine(str);
            LogString.Append(str).Append(Environment.NewLine);
        }

        [STAThread]
        static void Main(string[] args)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if ( fbd.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                string pathFolder = fbd.SelectedPath;

                if (!Directory.Exists(pathFolder))
                {
                    Console.WriteLine("The path is not a existing folder");
                    return;
                }

                IEnumerable<string> files = Directory.EnumerateFiles(pathFolder, "*.tml");

                List<XmlDocument> allDocuments = new List<XmlDocument>();

                foreach (string file in files)
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(file);
                    allDocuments.Add(xmlDoc);
                }

                List<string> allPathsDuplicated = new List<string>();

                foreach (XmlDocument document in allDocuments)
                {
                    XmlNodeList nodes = document.DocumentElement.SelectNodes("/Library/Template");
                    foreach (XmlNode node in nodes)
                    {
                        string filePath = node.SelectSingleNode("File").InnerText;

                        if (detectDuplicatePath(allDocuments, filePath, node.SelectSingleNode("Name").InnerText))
                        {
                            allPathsDuplicated.Add(filePath);
                            continue;
                        }
                    }
                }

                if (allPathsDuplicated.Count() > 0)
                {
                    allPathsDuplicated = new HashSet<string>(allPathsDuplicated).ToList();
                    foreach (string currentPath in allPathsDuplicated)
                    {
                        Out("Path duplicated for " + currentPath);
                    }

                    using (System.IO.StreamWriter file = new System.IO.StreamWriter("./DayZLibraryFix.log"))
                    {
                        file.Write(LogString.ToString());
                        file.Close();
                        file.Dispose();
                    }

                    Console.WriteLine("PRESS any key to exit");
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("All path is valid, PRESS any key to exit");
                    Console.ReadKey();
                }
            }
        }
    }
}
