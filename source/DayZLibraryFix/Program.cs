using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Windows.Forms;
using System.Text;

namespace DayZLibraryFix
{
    class CustomDictionary
    {
        public Dictionary<string, List<string>> internalDictionary = new Dictionary<string, List<string>>();

        public void Add(string key, string value)
        {
            if (this.internalDictionary.ContainsKey(key))
            {
                List<string> list = this.internalDictionary[key];
                if (!list.Contains(value))
                {
                    list.Add(value);
                }
            }
            else
            {
                List<string> list = new List<string>();
                list.Add(value);
                this.internalDictionary.Add(key, list);
            }
        }
    }

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

                CustomDictionary allPathsDuplicated = new CustomDictionary();

                foreach (XmlDocument document in allDocuments)
                {
                    XmlNodeList nodes = document.DocumentElement.SelectNodes("/Library/Template");
                    foreach (XmlNode node in nodes)
                    {
                        string filePath = node.SelectSingleNode("File").InnerText;
                        string templateName = node.SelectSingleNode("Name").InnerText;

                        if (detectDuplicatePath(allDocuments, filePath, templateName))
                        {
                            allPathsDuplicated.Add(filePath, templateName);
                            continue;
                        }
                    }
                }

                if (allPathsDuplicated.internalDictionary.Count() > 0)
                {
                    foreach (KeyValuePair<string, List<string>> item in allPathsDuplicated.internalDictionary)
                    {
                        string text = "Path duplicated for " + item.Key + " template(s) : ";

                        for (int i = 0; i < item.Value.Count; ++i)
                        {
                            if (i == item.Value.Count - 1)
                            {
                                text += item.Value[i];
                            }
                            else
                            {
                                text += item.Value[i] + ", ";
                            }
                        }

                        Out(text);
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
