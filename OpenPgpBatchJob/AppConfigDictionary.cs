using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OpenPgpBatchJob
{

    /// <summary>
    /// Helper Class to load custom app configurations
    /// </summary>
    internal class AppConfigDictionary
    {
        public static Dictionary<string, string> ParseXmlFragmentFromFile(string filePath)
        {
            Dictionary<string, string> dictionary = new();


            if (!File.Exists(filePath)) { throw new FileNotFoundException(string.Format("[{0}] NOT FOUND!", filePath)); }

            XmlDocument xmlDoc = new();
            xmlDoc.Load(filePath);

            if (xmlDoc.DocumentElement != null)
            {
                XmlNodeList addNodes = xmlDoc.GetElementsByTagName("add");
                foreach (XmlNode addNode in addNodes)
                {
                    XmlAttribute keyAttribute = addNode.Attributes["key"];
                    XmlAttribute valueAttribute = addNode.Attributes["value"];

                    if (keyAttribute != null && valueAttribute != null)
                    {
                        string key = keyAttribute.Value;
                        string value = valueAttribute.Value;

                        dictionary[key] = value;
                    }
                }
            }
            else { throw new FileLoadException(string.Format("[{0}] - Invalid Configurations!", filePath)); }

            return dictionary;
        }
    }

}
