using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Windows.Forms;

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

                foreach (XmlDocument document in allDocuments)
                {
                    XmlNodeList nodes = document.DocumentElement.SelectNodes("/Library/Template");
                    foreach (XmlNode node in nodes)
                    {
                        string filePath = node.SelectSingleNode("File").InnerText;

                        if (detectDuplicatePath(allDocuments, filePath, node.SelectSingleNode("Name").InnerText))
                        {
                            Console.WriteLine("Path duplicated for {0}", filePath);
                            Console.WriteLine("PRESS any key to exit");
                            Console.ReadKey();
                            return;
                        }
                    }
                }

                Console.WriteLine("All path is valid, PRESS any key to exit");
                Console.ReadKey();
            }
        }
    }
}
