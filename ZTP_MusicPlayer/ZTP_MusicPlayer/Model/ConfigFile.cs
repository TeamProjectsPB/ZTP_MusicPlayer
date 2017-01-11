using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ZTP_MusicPlayer.Model
{
    public static class ConfigFile
    {
        private static readonly string fileUrl;

        static ConfigFile()
        {
            fileUrl = Directory.GetCurrentDirectory() + "\\config.dat";
        }

        public static bool ConfigFileExists()
        {
            return File.Exists(fileUrl);
        }

        public static void CreateNewFile()
        {
            var writer = new XmlTextWriter(fileUrl, null);
            writer.WriteStartDocument();
            writer.WriteWhitespace("\n");
            {
                writer.WriteStartElement("player");
                {
                    writer.WriteWhitespace("\n\t");
                    writer.WriteStartElement("head");
                    {
                    }
                    writer.WriteEndElement();
                    writer.WriteWhitespace("\n\t");
                    writer.WriteStartElement("playlists");
                    {
                    }
                    writer.WriteEndElement();
                    writer.WriteWhitespace("\n\t");
                    writer.WriteStartElement("libraries");
                    {
                    }
                    writer.WriteEndElement();
                }
                writer.WriteWhitespace("\n");
                writer.WriteEndElement();
            }
            writer.WriteEndDocument();
            writer.Close();
        }

        private static void SaveNewFile(int volume, List<string> playlists, List<Library> libraries)
        {
            var writer = new XmlTextWriter(fileUrl, null);
            writer.WriteStartDocument();
            writer.WriteWhitespace("\n");
            {
                writer.WriteStartElement("player");
                {
                    writer.WriteWhitespace("\n\t");
                    writer.WriteStartElement("head");
                    {
                        writer.WriteWhitespace("\n\t\t");
                        writer.WriteAttributeString("volume", volume.ToString());
                    }
                    writer.WriteEndElement();
                    writer.WriteWhitespace("\n\t");
                    writer.WriteStartElement("playlists");
                    {
                        playlists.ForEach(x =>
                        {
                            writer.WriteWhitespace("\n\t\t");
                            writer.WriteElementString("playlist", x);
                        });
                    }
                    writer.WriteEndElement();
                    writer.WriteWhitespace("\n\t\t");
                    writer.WriteStartElement("libraries");
                    {
                        libraries.ForEach(x =>
                        {
                            writer.WriteWhitespace("\n\t\t");
                            writer.WriteAttributeString("name", x.Name);
                            writer.WriteAttributeString("url", x.Url);
                        });
                    }
                    writer.WriteEndElement();
                }
            }
        }

        #region Getters

        public static int GetVolume()
        {
            var volume = 50;
            if (File.Exists(fileUrl))
            {
                try
                {
                    var document = new XmlDocument();
                    document.Load(fileUrl);
                    var xmlNodeList = document.GetElementsByTagName("head");
                    foreach (XmlNode xmlNode in xmlNodeList)
                    {
                        var xmlNodeList2 = xmlNode.ChildNodes;
                        foreach (XmlNode xmlNode2 in xmlNodeList2)
                        {
                            if (xmlNode2.Name.Equals("volume"))
                            {
                                int.TryParse(xmlNode2.InnerText, out volume);
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            return volume;
        }

        public static List<Tuple<string, string>> GetLibraries()
        {
            var tuples = new List<Tuple<string, string>>();
            //List<string> libraries = new List<string>();
            try
            {
                var document = new XmlDocument();
                document.Load(fileUrl);

                var xmlNodeList = document.GetElementsByTagName("libraries");
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    var xmlNodeList2 = xmlNode.ChildNodes;
                    foreach (XmlNode xmlNode2 in xmlNodeList2)
                    {
                        if (xmlNode2.Name.Equals("library"))
                        {
                            if (xmlNode2.Attributes["name"] != null && xmlNode2.Attributes["url"] != null)
                            {
                                var name = xmlNode2.Attributes["name"].Value;
                                var libUrl = xmlNode2.Attributes["url"].Value;
                                tuples.Add(Tuple.Create(name, libUrl));
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return tuples;
        }

        public static List<string> GetPlaylists()
        {
            var playlists = new List<string>();
            try
            {
                var document = new XmlDocument();
                document.Load(fileUrl);

                var xmlNodeList = document.GetElementsByTagName("playlists");
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    var xmlNodeList2 = xmlNode.ChildNodes;
                    foreach (XmlNode xmlNode2 in xmlNodeList2)
                    {
                        if (xmlNode2.Name.Equals("playlist"))
                        {
                            playlists.Add(xmlNode2.InnerText);
                        }
                    }
                }
            }
            catch
            {
            }
            return playlists;
        }

        #endregion

        #region Save

        public static void SaveVolume(int volume)
        {
            //XDocument xDocument = XDocument.Load(fileUrl);
            //XElement root = xDocument.Element("player");
            //IEnumerable<XElement> rows = root.Descendants("head");
            if (File.Exists(fileUrl))
            {
                try
                {
                    var document = new XmlDocument();
                    document.Load(fileUrl);

                    var xmlNodeList = document.GetElementsByTagName("head");
                    foreach (XmlNode xmlNode in xmlNodeList)
                    {
                        var xmlNodeList2 = xmlNode.ChildNodes;
                        var exists = false;
                        foreach (XmlNode xmlNode2 in xmlNodeList2)
                        {
                            if (xmlNode2.Name.Equals("volume"))
                            {
                                xmlNode2.InnerText = volume.ToString();
                                exists = true;
                            }
                        }
                        if (!exists)
                        {
                            var volumeNode = document.CreateNode(XmlNodeType.Element, "volume", "");
                            volumeNode.InnerText = volume.ToString();
                            xmlNode.AppendChild(volumeNode);
                        }
                    }
                    document.Save(fileUrl);
                }
                catch
                {
                }
            }
        }

        public static void SaveNewPlaylist(string name)
        {
            //XDocument xDocument = XDocument.Load(fileUrl);
            //XElement root = xDocument.Element("player");
            //IEnumerable<XElement> rows = root.Descendants("head");
            if (File.Exists(fileUrl))
            {
                try
                {
                    var document = new XmlDocument();
                    document.Load(fileUrl);

                    var playlists = document.GetElementsByTagName("playlists")[0];

                    var node = document.CreateNode(XmlNodeType.Element, "playlist", "");
                    node.InnerText = name;
                    playlists.AppendChild(node);
                    document.Save(fileUrl);
                }
                catch
                {
                }
            }
        }

        public static void SaveNewLibrary(string name, string libUrl)
        {
            //XDocument xDocument = XDocument.Load(fileUrl);
            //XElement root = xDocument.Element("player");
            //IEnumerable<XElement> rows = root.Descendants("head");
            if (File.Exists(fileUrl))
            {
                try
                {
                    var document = new XmlDocument();
                    document.Load(fileUrl);

                    var playlists = document.GetElementsByTagName("libraries")[0];

                    var node = document.CreateNode(XmlNodeType.Element, "library", "");
                    {
                        var nameAttribute = document.CreateAttribute("", "name", "");
                        nameAttribute.Value = name;
                        node.Attributes.Append(nameAttribute);

                        var urlAttribute = document.CreateAttribute("", "url", "");
                        urlAttribute.Value = libUrl;
                        node.Attributes.Append(urlAttribute);
                    }

                    playlists.AppendChild(node);
                    document.Save(fileUrl);
                }
                catch
                {
                }
            }
        }

        #endregion

        #region Remove

        public static void RemoveLibrary(string libName)
        {
            try
            {
                var document = new XmlDocument();
                document.Load(fileUrl);

                var xmlNodeList = document.GetElementsByTagName("libraries");
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    var xmlNodeList2 = xmlNode.ChildNodes;
                    foreach (XmlNode xmlNode2 in xmlNodeList2)
                    {
                        if (xmlNode2.Name.Equals("library"))
                        {
                            if (xmlNode2.Attributes["name"].Value.Equals(libName))
                            {
                                xmlNode.RemoveChild(xmlNode2);
                            }
                        }
                    }
                }
                document.Save(fileUrl);
            }
            catch
            {
            }
        }

        public static void RemovePlaylist(string playlistName)
        {
            try
            {
                var document = new XmlDocument();
                document.Load(fileUrl);

                var xmlNodeList = document.GetElementsByTagName("playlists");
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    var xmlNodeList2 = xmlNode.ChildNodes;
                    foreach (XmlNode xmlNode2 in xmlNodeList2)
                    {
                        if (xmlNode2.Name.Equals("playlist"))
                        {
                            if (xmlNode2.Attributes["name"].Value.Equals(playlistName))
                            {
                                xmlNode.RemoveChild(xmlNode2);
                            }
                        }
                    }
                }
                document.Save(fileUrl);
            }
            catch
            {
            }
        }

        #endregion
    }
}