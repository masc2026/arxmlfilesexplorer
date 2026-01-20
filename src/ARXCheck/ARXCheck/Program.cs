//
// Program.cs
//
// Author:
//  Markus Schmid
//
using System;
using System.Resources;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Utils;
using System.CommandLine;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using System.Reflection;

#pragma warning disable 1998

namespace APCheck
{
    class Program
    {
        static List<(string XSDFile, string XML)> xsdResources;
        static XmlDocument ConfigFile = null;

        public static XmlDocument XmlDocFromFile(string file)
        {
            XmlDocument xdoc = null;

            string fullPath = Path.GetFullPath(file);
            // Console.WriteLine("\nfullpath: \n{0}", fullPath);

            if (file == null || File.Exists(file) == false)
            {
                Console.WriteLine("\nError: Path not valid: \n{0}", (file == null ? "" : file));
                return null;
            }
            try
            {
                xdoc = new XmlDocument();
                XmlTextReader reader = new XmlTextReader(file);
                reader.WhitespaceHandling = WhitespaceHandling.None;
                reader.MoveToContent();
                reader.Read();
                xdoc.Load(reader);
            }
            catch (FileNotFoundException fnfe)
            {
                Console.WriteLine("\nError: File not found: \n{0}", file);
                Console.WriteLine("\nMessage: \n{0}", fnfe.Message);
            }
            catch (DirectoryNotFoundException dnfe)
            {
                Console.WriteLine("\nError: Path not found or not accessible: \n{0}", file);
                Console.WriteLine("\nMessage: \n{0}", dnfe.Message);
            }
            catch (UnauthorizedAccessException uae)
            {
                Console.WriteLine("\nError: Path not accessible: \n{0}", file);
                Console.WriteLine("\nMessage: \n{0}", uae.Message);
            }
            catch (InvalidOperationException ioe)
            {
                Console.WriteLine("\nError: While reading XML File: \n{0}", file);
                Console.WriteLine("\nMessage: \n{0}", ioe.Message);
            }
            catch (XmlException xe)
            {
                Console.WriteLine("\nError: XML Error while reading XML File: \n{0}", file);
                Console.WriteLine("\nMessage: \n{0}", xe.Message);
            }
            return xdoc;
        }

        public static XDocument XDocFromFile(string file)
        {
            string fullPath = Path.GetFullPath(file);
            // Console.WriteLine("\nfullpath: \n{0}", fullPath);

            XDocument xdoc = null;
            if (file == null || File.Exists(file) == false)
            {
                Console.WriteLine("\nError: Path not valid: \n{0}", (file == null ? "" : file));
                return null;
            }
            try
            {
                xdoc = XDocument.Load(file);
            }
            catch (FileNotFoundException fnfe)
            {
                Console.WriteLine("\nError: File not found: \n{0}", file);
                Console.WriteLine("\nMessage: \n{0}", fnfe.Message);
            }
            catch (DirectoryNotFoundException dnfe)
            {
                Console.WriteLine("\nError: Path not found or not accessible: \n{0}", file);
                Console.WriteLine("\nMessage: \n{0}", dnfe.Message);
            }
            catch (UnauthorizedAccessException uae)
            {
                Console.WriteLine("\nError: Path not accessible: \n{0}", file);
                Console.WriteLine("\nMessage: \n{0}", uae.Message);
            }
            catch (XmlException xe)
            {
                Console.WriteLine("\nError: XML Error while reading XML File: \n{0}", file);
                Console.WriteLine("\nMessage: \n{0}", xe.Message);
            }
            return xdoc;
        }

        public static XmlDocument GetConfig(string configfile)
        {
            if (ConfigFile == null && !string.IsNullOrEmpty(configfile))
            {
                ConfigFile = XmlDocFromFile(configfile);
            }
            return ConfigFile;
        }

        public static string GetXSDAliasMapping(string XSD)
        {
            string res = XSD;
            if (ConfigFile != null && !string.IsNullOrEmpty(XSD))
            {
                try
                {
                    XmlNamespaceManager ns = new XmlNamespaceManager(ConfigFile.NameTable);
                    ns.AddNamespace("conf", "ARXCheck.config");
                    string xpath = "//conf:xsdmap/conf:mapping//conf:xsd[conf:xsd/@name = \"" + XSD.ToUpper() + "\"]/@alias";
                    XmlNode alias = ConfigFile.SelectSingleNode(xpath, ns);
                    if (!string.IsNullOrEmpty(alias.Value))
                    {
                        res = alias.Value;
                    }
                }
                catch (NullReferenceException)
                {
                    // res = XSD;
                }
                catch (XPathException)
                {
                    // res = XSD;
                }
            }
            return res;
        }

        public static string GetXSDFromSchemaLocation(string schemalocation, bool configmappingon)
        {
            if (string.IsNullOrEmpty(schemalocation))
            {
                return string.Empty;
            }
            else
            {
                string res = Regex.Match(schemalocation, @"([a-zA-Z0123456789_-]*.[xX][sS][dD])$").Value.ToUpper();
                if (res == "AUTOSAR.XSD")
                {
                    try
                    {
                        res = string.Empty;
                        Match match = Regex.Match(schemalocation, @"http:\/\/autosar\.org\/([a-zA-Z0123456789.-]*)");
                        if (match.Groups.Count > 0)
                        {
                            res = "AUTOSAR_" + match.Groups[1].Value.ToUpper() + ".XSD";
                        }
                    }
                    catch (System.ArgumentNullException)
                    {
                        res = string.Empty;
                    }
                    catch (System.ArgumentException)
                    {
                        res = string.Empty;
                    }
                    catch (RegexMatchTimeoutException)
                    {
                        res = string.Empty;
                    }
                }
                if (!(string.IsNullOrEmpty(res)) && configmappingon)
                {
                    res = GetXSDAliasMapping(res);
                    return res;
                }
                else
                {
                    return res;
                }
            }
        }

        static List<(string XSDFile, string XML)> GetXSDInfoRessources()
        {
            ResourceSet rsrcSet = ARXCheck.Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

            var xsdResources = ARXCheck.Resources.ResourceManager
                       .GetResourceSet(CultureInfo.CurrentCulture, true, true)
                       .Cast<DictionaryEntry>()
                       .Where(x => x.Value.GetType() == typeof(string))
                       .Select(x => (XSDFile: (x.Key.ToString() + ".xsd"), XML: x.Value.ToString()))
                       .ToList();
            return xsdResources;
        }
        static string GetXSDFromARXML(string xmlFileName)
        {
            string fullPath = Path.GetFullPath(xmlFileName);
            // Console.WriteLine("\nfullpath: \n{0}", fullPath);
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Async = false;
            string res = string.Empty;
            try
            {
                using (XmlReader reader = XmlReader.Create(xmlFileName))
                {
                    bool resNotFound = true;
                    while (resNotFound && reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                if (reader.Name == "AUTOSAR")
                                {
                                    if (reader.MoveToAttribute("xsi:schemaLocation"))
                                    {
                                        res = GetXSDFromSchemaLocation(reader.Value.Trim(), true);
                                    }
                                    resNotFound = false;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (InvalidOperationException ioe)
            {
                Console.WriteLine("\nError: Invalid Operation");
                Console.WriteLine("\nMessage: \n{0}", ioe.Message);
            }
            catch (XmlException xe)
            {
                Console.WriteLine("\nError: XML Format in file: \n{0}", xmlFileName);
                Console.WriteLine("\nMessage: \n{0}", xe.Message);
            }
            catch (FileNotFoundException fnfe)
            {
                Console.WriteLine("\nError: File not found: \n{0}", xmlFileName);
                Console.WriteLine("\nMessage: \n{0}", fnfe.Message);
            }
            catch (DirectoryNotFoundException dnfe)
            {
                Console.WriteLine("\nError: Path not found or not accessible: \n{0}", xmlFileName);
                Console.WriteLine("\nMessage: \n{0}", dnfe.Message);
            }
            catch (UnauthorizedAccessException uae)
            {
                Console.WriteLine("\nError: Path not accessible: \n{0}", xmlFileName);
                Console.WriteLine("\nMessage: \n{0}", uae.Message);
            }

            return res.Replace(".xsd",".xsd", StringComparison.OrdinalIgnoreCase);
        }

        static void ShowHelp(RootCommand rc)
        {
            rc.Invoke("--help");

            Console.WriteLine("\nSupported XSDFILE values:\n");
            foreach (var elem in xsdResources)
            {
                string name = elem.XSDFile;
                Console.WriteLine("{0}", name);
            }
        }
        static bool ListValidationErrors = false;
        static async Task<int> Main(string[] args)
        {
            // Optionen definieren
            var arXmlFileOption = new Option<string>("--arxml", "Specify the ARXMLFILE.");
            arXmlFileOption.AddAlias("-x");
            var xsdFileOption = new Option<string>("--xsd", "If XSDFILE is specified, it is used instead of the schemaLocation in the ARXML file.");
            xsdFileOption.AddAlias("-s");
            var configFilePathOption = new Option<string>("--config", "Specify the CONFIGFILE.");
            configFilePathOption.AddAlias("-c");
            var listValidationErrorsOption = new Option<bool>("-v", "List schema validation warnings and errors.");
            var listXpathOption = new Option<int>("--xpath", "List XPath expressions with XPATHFORMAT:\nXPATHFORMAT = 0 : No XPath\nXPATHFORMAT = 1 : Valid XPath\nXPATHFORMAT = 2 : Human-readable 'Path'");
            listXpathOption.AddAlias("-p");

            // Root-Command erstellen
            var rootCommand = new RootCommand("Performs some checks on the ARXML File.")
            {
                arXmlFileOption,
                xsdFileOption,
                configFilePathOption,
                listValidationErrorsOption,
                listXpathOption
            };

            string[] env_args = Environment.GetCommandLineArgs();
            
            string path1 = Path.GetDirectoryName(Path.GetFullPath(env_args[0]));

            string path2 = Path.GetDirectoryName(Path.GetFullPath(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName));

            string path = path2;

            rootCommand.SetHandler(async (arXmlFile, xsdFile, configFilePath, listValidationErrors, listXpath) =>
            {
                ListValidationErrors = listValidationErrors;

                if (System.Diagnostics.Process.GetCurrentProcess().ProcessName != "ARXCheck")
                {
                    path = path1;
                }

                path = path.Replace(@"\", @"/");

                string xsdFilePath = path + @"/xsd/";

                string targetNS = @"http://autosar.org/schema/r4.0";

                // Console.WriteLine("\nFile = {0}", System.Diagnostics.Process.GetCurrentProcess().ProcessName);

                // string outputFile = @"..\..\..\output\output.xml";

                xsdResources = GetXSDInfoRessources();

                if (!string.IsNullOrEmpty(configFilePath))
                {

                    GetConfig(configFilePath);
                }

                SortedDictionary<string, string> complexTypeNamesAPOnly = new SortedDictionary<string, string>();
                SortedDictionary<string, string> simpleTypeNamesAPOnly = new SortedDictionary<string, string>();

                SortedDictionary<string, string> xsdFileNames = new SortedDictionary<string, string>();

                Dictionary<string, ArrayList> usedComplexTypeNamesAPOnly = new Dictionary<string, ArrayList>();

                Dictionary<string, ArrayList> usedComplexTypeNamesNotAPOnly = new Dictionary<string, ArrayList>();

                Dictionary<string, ArrayList> usedSimpleTypeNamesAPOnly = new Dictionary<string, ArrayList>();

                XDocument xmlFile = null;
                XmlDocument xsdDescription = new XmlDocument();

                if (arXmlFile == null || listXpath > 2 || listXpath < 0)
                {
                    ShowHelp(rootCommand);
                    Environment.Exit(0);
                }

                string fullPath = Path.GetFullPath(arXmlFile);
                // Console.WriteLine("\nfullpath: \n{0}", fullPath);

                if (File.Exists(arXmlFile) == false)
                {
                    Console.WriteLine("\nError: Path not valid: \n{0}", arXmlFile);
                    Environment.Exit(0);
                }

                // if xsdFile not set, try to read schema file name
                if (xsdFile == null)
                {
                    xsdFile = GetXSDFromARXML(arXmlFile);
                    if (xsdFile == string.Empty)
                    {
                        Console.WriteLine("\nXSD was not specified and no XSD was found in ARXML.", xsdFile);
                        Console.WriteLine("\nXSD must be specified with this ARXML.");
                        ShowHelp(rootCommand);
                        Environment.Exit(0);
                    }
                    else
                    {
                        Console.WriteLine("\nFound XSD {0} in ARXML", xsdFile);
                    }
                }

                // check if schema file is supported
                (string XSDFile, string XML) xsdRes = (null, null);

                try
                {
                    xsdRes = xsdResources.Find(x => x.XSDFile.ToUpper() == xsdFile.ToUpper());
                }
                catch (Exception)
                {
                    // s.
                }

                if (xsdRes == (null, null))
                {
                    Console.WriteLine("\nXSD file {0} is not supported!", xsdFile);
                    ShowHelp(rootCommand);
                    Environment.Exit(2);
                }
                else
                {
                    xsdDescription.LoadXml(xsdRes.XML);
                }

                xmlFile = XDocFromFile(arXmlFile);
                if (xmlFile == null)
                {
                    Environment.Exit(1);
                }

                // setup complexTypeNamesAPOnly and simpleTypeNamesAPOnly dicts
                XmlNamespaceManager ns = new XmlNamespaceManager(xsdDescription.NameTable);
                ns.AddNamespace("main", "main");
                XmlNodeList complexTypesAPOnly = xsdDescription.SelectNodes("descendant::main:xsd/main:restriction[@type='AP']/main:nodes/main:node[@category='complexType']/main:type", ns);
                XmlNodeList simpleTypesAPOnly = xsdDescription.SelectNodes("descendant::main:xsd/main:restriction[@type='AP']/main:nodes/main:node[@category='simpleType']/main:type", ns);
                XmlNodeList complexTypesCPOnly = xsdDescription.SelectNodes("descendant::main:xsd/main:restriction[@type='CP']/main:nodes/main:node[@category='complexType']/main:type", ns);
                XmlNodeList simpleTypesCPOnly = xsdDescription.SelectNodes("descendant::main:xsd/main:restriction[@type='CP']/main:nodes/main:node[@category='simpleType']/main:type", ns);

                foreach (XmlNode type in complexTypesAPOnly)
                {
                    string nameOfType = type.SelectSingleNode("@name").Value;
                    complexTypeNamesAPOnly.Add(nameOfType, nameOfType);
                }

                foreach (XmlNode type in simpleTypesAPOnly)
                {
                    string nameOfType = type.SelectSingleNode("@name").Value;
                    simpleTypeNamesAPOnly.Add(nameOfType, nameOfType);
                }

                // read schema file
                XmlSchemaSet xsdSet = new XmlSchemaSet();

                if (File.Exists(xsdFilePath + xsdFile))
                {
                    try
                    {
                        xsdSet.Add(targetNS, xsdFilePath + xsdFile);
                    }
                    catch (FileNotFoundException fnfe)
                    {
                        Console.WriteLine("\nXSD file {0} missing!", xsdFile);
                        Console.WriteLine("\nIn order to make this tool working the following XSD files must exist in the 'xsd' subdirectory:\n");
                        foreach (var elem in xsdResources)
                        {
                            string name = elem.XSDFile;
                            Console.WriteLine("{0}", name);
                        }

                        Console.WriteLine("\nError: File not found: \n{0}", xsdFile);
                        Console.WriteLine("\nMessage: \n{0}", fnfe.Message);
                        Environment.Exit(1);
                    }
                    catch (DirectoryNotFoundException dnfe)
                    {
                        Console.WriteLine("\nError: Path not found: \n{0}", xsdFile);
                        Console.WriteLine("\nMessage: \n{0}", dnfe.Message);
                        Console.WriteLine("\n'xsd' directory not found!");
                        Console.WriteLine("\nIn order to make this tool working the following XSD files must exist in the 'xsd' subdirectory:\n");
                        foreach (var elem in xsdResources)
                        {
                            string name = elem.XSDFile;
                            Console.WriteLine("{0}", name);
                        }
                        Environment.Exit(1);
                    }
                    catch (UnauthorizedAccessException uae)
                    {
                        Console.WriteLine("\nError: Path not accessible: \n{0}", xsdFile);
                        Console.WriteLine("\nMessage: \n{0}", uae.Message);
                        Environment.Exit(1);
                    }
                    catch (XmlSchemaException se)
                    {
                        Console.WriteLine("\nError: XSD Schema in file: \n{0}", xsdFile);
                        Console.WriteLine("\nMessage: \n{0}", se.Message);
                        Environment.Exit(1);
                    }
                    catch (XmlException xe)
                    {
                        Console.WriteLine("\nError: XML Format in file: \n{0}", xsdFile);
                        Console.WriteLine("\nMessage: \n{0}", xe.Message);
                        Environment.Exit(1);
                    }
                }
                else
                {
                    Console.WriteLine("\nError: Path not valid: \n{0}", xsdFilePath + xsdFile);
                    Console.WriteLine("\n'xsd' directory or file not found!");
                    Console.WriteLine("\nIn order to make this tool working the following XSD files must exist in the 'xsd' subdirectory:\n");
                    foreach (var elem in xsdResources)
                    {
                        string name = elem.XSDFile;
                        Console.WriteLine("{0}", name);
                    }

                    Environment.Exit(0);
                }

                try
                {
                    xsdSet.Add("http://www.w3.org/XML/1998/namespace", xsdFilePath + "xml.xsd");
                    xsdSet.Compile();
                }
                catch (XmlSchemaException se)
                {
                    Console.WriteLine("\nError while compiling XSD file: \n{0}:", xsdFile);
                    Console.WriteLine("\nMessage: \n{0}", se.Message);
                    Environment.Exit(1);
                }

                // validate arxml file with the schema given
                xmlFile.Validate(xsdSet, new ValidationEventHandler(arxmlValidationEventHandler), true);

                // traverse the arxml file
                IXmlSchemaInfo schemaInfo;

                int hundred = xmlFile.DescendantNodes().Count();
                int i = 0;
                XNamespace ar = "http://autosar.org/schema/r4.0";
                using (var progress = new ProgressBar())
                {
                    foreach (XNode aNode in xmlFile.DescendantNodes())
                    {
                        //Console.WriteLine(aNode.GetHashCode());

                        progress.Report((double)i / hundred);
                        i++;

                        switch (aNode)
                        {
                            case XElement element:
                                schemaInfo = element.GetSchemaInfo();
                                switch (schemaInfo.SchemaType)
                                {
                                    case XmlSchemaComplexType ct:
                                        ArrayList elements = null;
                                        if (ct.Name != null)
                                        {
                                            // Name of type available
                                            if (complexTypeNamesAPOnly.TryGetValue(ct.Name, out string found))
                                            {
                                                // found a complexType with restrict to AP
                                                if (usedComplexTypeNamesAPOnly.TryGetValue(ct.Name, out elements))
                                                {
                                                    // complexType available usedComplexTypeNamesAPOnly
                                                    // add element expression where it is used
                                                    if (listXpath > 0)
                                                    {
                                                        elements.Add(element);
                                                    }
                                                    usedComplexTypeNamesAPOnly[ct.Name] = elements;
                                                }
                                                else
                                                {
                                                    // complexType name not yet added to usedComplexTypeNamesAPOnly
                                                    elements = new ArrayList();
                                                    if (listXpath > 0)
                                                    {
                                                        // add xpath expression where it is used
                                                        elements.Add(element);
                                                    }
                                                    usedComplexTypeNamesAPOnly.Add(ct.Name, elements);
                                                }
                                            }
                                            else
                                            {
                                                // found a complexType with no restrict to AP
                                                if (usedComplexTypeNamesNotAPOnly.TryGetValue(ct.Name, out elements))
                                                {
                                                    // complexType available usedComplexTypeNamesNotAPOnly
                                                    if (listXpath > 0)
                                                    {
                                                        // add xpath expression where it is used
                                                        elements.Add(element);
                                                    }
                                                    usedComplexTypeNamesNotAPOnly[ct.Name] = elements;
                                                }
                                                else
                                                {
                                                    // complexType name not yet added to usedComplexTypeNamesNotAPOnly
                                                    elements = new ArrayList();
                                                    if (listXpath > 0)
                                                    {
                                                        // add xpath expression where it is used
                                                        elements.Add(element);
                                                    }
                                                    usedComplexTypeNamesNotAPOnly.Add(ct.Name, elements);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // not type name available
                                        }
                                        /*
                                        foreach (var attribute in element.Attributes())
                                        {
                                            if (attribute.GetSchemaInfo() == null || attribute.GetSchemaInfo().SchemaType == null)
                                                Console.WriteLine("attribute,{0},{1}", attribute.Name.LocalName, "");
                                            else
                                                Console.WriteLine("attribute,{0},{1}", attribute.Name.LocalName, attribute.GetSchemaInfo().SchemaType.Name);
                                        }
                                        */
                                        break;
                                    case XmlSchemaSimpleType st:
                                        // Console.WriteLine("element,simple,{0},{1},{2},{3}", element.Name.LocalName, st.Content.GetType().Name, st.Datatype.TypeCode, st.Datatype.ValueType.HasElementType);
                                        /*
                                        foreach (var attribute in element.Attributes())
                                        {
                                            if (attribute.GetSchemaInfo() == null || attribute.GetSchemaInfo().SchemaType == null)
                                                Console.WriteLine("attribute,{0},{1}", attribute.Name.LocalName, "");
                                            else
                                                Console.WriteLine("attribute,{0},{1}", attribute.Name.LocalName, attribute.GetSchemaInfo().SchemaType.Name);
                                        }
                                        */
                                        break;
                                }
                                break;
                            case XDocumentType docType:
                                break;
                            case XText text:
                                break;
                            case XComment comment:
                                break;
                        }
                    }
                }

                Console.WriteLine("\n");

                foreach (KeyValuePair<string, ArrayList> entry in usedComplexTypeNamesAPOnly)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("🔴 complexType: {0}", entry.Key);
                    Console.ResetColor();
                    if (listXpath > 0)
                    {
                        foreach (XElement element in entry.Value)
                        {
                            IEnumerable<XElement> ancestors;
                            ancestors = element.AncestorsAndSelf();
                            string xpath = "";
                            if (listXpath > 0)
                            {
                                if (listXpath == 1)
                                    xpath = ancestors.Aggregate(new StringBuilder(),
                                    (s, c) => s.Insert(0, "/*:" + c.Name.LocalName + "[" + (int)(c.ElementsBeforeSelf().Where(el => el.Name.LocalName == c.Name.LocalName).Count() + 1) + "]"),
                                    s => s.ToString());
                                else if (listXpath == 2)
                                    xpath = ancestors.Aggregate(new StringBuilder(),
                                    (s, c) => s.Insert(0, (c.Name.LocalName != "AR-PACKAGES" && c.Name.LocalName != "AUTOSAR" && c.Name.LocalName != "ELEMENTS" ? ((c.Name.LocalName == "AR-PACKAGE" ? "/" + c.Element(ar + "SHORT-NAME").Value : ("/" + c.Name.LocalName + "[" + (int)(c.ElementsBeforeSelf().Where(el => el.Name.LocalName == c.Name.LocalName).Count() + 1) + "]"))) : "")),
                                    s => s.ToString());
                                else if (listXpath == 3)
                                    xpath = GetAbsoluteXPath(element, 2);
                            }
                            Console.WriteLine("\t{0}", xpath);
                        }
                        Console.ResetColor();
                    }
                }
                foreach (KeyValuePair<string, ArrayList> entry in usedComplexTypeNamesNotAPOnly)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("🟢 complexType: {0}", entry.Key);
                    Console.ResetColor();
                    if (listXpath > 0)
                    {
                        foreach (XElement element in entry.Value)
                        {
                            IEnumerable<XElement> ancestors;
                            ancestors = element.AncestorsAndSelf();
                            string xpath = "";
                            if (listXpath > 0)
                            {
                                if (listXpath == 1)
                                    xpath = ancestors.Aggregate(new StringBuilder(),
                                    (s, c) => s.Insert(0, "/*:" + c.Name.LocalName + "[" + (int)(c.ElementsBeforeSelf().Where(el => el.Name.LocalName == c.Name.LocalName).Count() + 1) + "]"),
                                    s => s.ToString());
                                else if (listXpath == 2)
                                    xpath = ancestors.Aggregate(new StringBuilder(),
                                    (s, c) => s.Insert(0, (c.Name.LocalName != "AR-PACKAGES" && c.Name.LocalName != "AUTOSAR" && c.Name.LocalName != "ELEMENTS" ? ((c.Name.LocalName == "AR-PACKAGE" ? "/" + c.Element(ar + "SHORT-NAME").Value : ("/" + c.Name.LocalName + "[" + (int)(c.ElementsBeforeSelf().Where(el => el.Name.LocalName == c.Name.LocalName).Count() + 1) + "]"))) : "")),
                                    s => s.ToString());
                                else if (listXpath == 3)
                                    xpath = GetAbsoluteXPath(element, 2);
                            }
                            Console.WriteLine("\t{0}", xpath);
                        }
                        Console.ResetColor();
                    }
                }

                Environment.Exit(0);
            },
            arXmlFileOption, xsdFileOption, configFilePathOption, listValidationErrorsOption, listXpathOption);

            await rootCommand.InvokeAsync(args);

            return 0;
        }

        public static string GetAbsoluteXPath(XElement element, int xpversion)
        {
            IEnumerable<XElement> ancestors = element.AncestorsAndSelf();
            string xpath = ancestors.Aggregate(new StringBuilder(),
                                (str, elem) => str.Insert(0, (xpversion > 1 ? ("/*:" + elem.Name.LocalName) : ("/*[local-name(.) = '" + elem.Name.LocalName + "']")) + "[" + (int)(elem.ElementsBeforeSelf().Where(el => el.Name.LocalName == elem.Name.LocalName).Count() + 1) + "]"),
                                str => str.ToString());
            return xpath;
        }

        static void arxmlValidationEventHandler(object sender, ValidationEventArgs e)
        {
            if (ListValidationErrors && e.Severity == XmlSeverityType.Warning)
            {
                Console.WriteLine("\nWarning XML validation ");
                Console.WriteLine("\nMessage: {0}\n", e.Message);
            }
            else if (ListValidationErrors && e.Severity == XmlSeverityType.Error)
            {
                Console.WriteLine("\nError XML validation: ");
                Console.WriteLine("\nMessage: {0}\n", e.Message);
            }
        }
    }
}