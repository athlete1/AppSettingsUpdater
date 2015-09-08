using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace AppSettingsUpdater
{
    internal class Program
    {
        public static string SettingName { get; set; }
        public static string NewValue { get; set; }
        public static string ConfigFile { get; set; }

        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                DisplayHelp();
                PauseForKeyPress();
                return;
            }

            if (args.Contains("-h") || args.Contains("-help"))
            {
                DisplayHelp();
                PauseForKeyPress();  
                return;
            }

            foreach (string arg in args)
            {
                string argValue = arg.Substring(0, arg.IndexOf(':') + 1);
                switch (argValue.ToLower())
                {
                    case "-n:":
                    case "-name:":
                        SettingName = arg.Substring(argValue.Length);
                        break;
                    case "-v:":
                    case "-value:":
                        NewValue = arg.Substring(argValue.Length);
                        break;
                    case "-f:":
                    case "-file:":
                        ConfigFile = arg.Substring(argValue.Length);
                        break;
                }
            }

            UpdateAppSetting();

            if (!args.Contains("-s") && !args.Contains("-silent"))
            {
                PauseForKeyPress();
            }
        }

        private static void PauseForKeyPress()
        {
            Console.WriteLine("Press any key to close...");
            Console.ReadLine();
        }

        private static void DisplayHelp()
        {
            DisplayFormat();
            Console.WriteLine("Paramaters:");
            Console.WriteLine("  [-n:,-name: 'The AppSetting Name (key)']");
            Console.WriteLine("  [-v:,-value: 'The New AppSetting Value']");
            Console.WriteLine("  [-f:,-file: 'Config File to modify']");
            Console.WriteLine("  [-s,-silent 'Close when finished']");
            Console.WriteLine("  [-h,-help 'Displays Argument Order and Options']");
            Console.WriteLine("");
        }

        private static void DisplayFormat()
        {
            Console.WriteLine("Example: AppSettingsUpdater.exe -s -n:[SettingName] -v:[NewValue] -f:[ConfigFile]");
        }

        private static void UpdateAppSetting()
        {
            bool foundMatch = false;

            if (!File.Exists(ConfigFile))
            {
                Console.WriteLine("File not found {0}!", ConfigFile);
            }

            try
            {
                XDocument xdoc = XDocument.Load(ConfigFile);
                XElement xElement = xdoc.Element("configuration");
                if (xElement != null)
                {
                    XElement appSettingsElement = xElement.Element("appSettings");

                    //get the list of all elements in appsettings
                    if (appSettingsElement != null)
                    {
                        IEnumerable<XElement> list = appSettingsElement.Descendants();
                        IList<XElement> xElements = list as IList<XElement> ?? list.ToList();
                        if (xElements.Any())
                        {
                            //iterate through the list for a match
                            foreach (XElement item in xElements)
                            {
                                if (item.Name == "add")
                                {
                                    //if we find the match for the key we are looking, simple update the value
                                    if (item.Attributes("key") != null && item.Attributes("key").Any()
                                        && item.Attributes("key").First().Value == SettingName)
                                    {
                                        item.SetAttributeValue("value", NewValue);
                                        foundMatch = true;
                                    }
                                }
                            }
                        }
                    }
                    //if we did not find a match for the key we were looking create a new entry
                    if (!foundMatch)
                    {
                        var newElement = new XElement("add");
                        newElement.SetAttributeValue("key", SettingName);
                        newElement.SetAttributeValue("value", NewValue);
                        if (appSettingsElement != null) appSettingsElement.Add(newElement);
                    }
                }
                //create a write object to persist contents back to configuration file
                var writer = new XmlTextWriter(ConfigFile, null) {Formatting = Formatting.Indented};

                //write the contents the xml document to the writer
                xdoc.WriteTo(writer);

                //save changes
                writer.Flush();
                writer.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid XML Document!");
            }
        }
    }
}