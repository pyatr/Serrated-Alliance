using System;
using System.Xml;
using System.Collections.Generic;
using XRL;
using XRL.Core;
using XRL.Messages;
using XRL.World;
using UnityEngine;

namespace XRL
{
    [HasGameBasedStaticCache]
    public class WWA_AddPopulationsFromXML
    {
        public static void Reset()
        {
            string modName = "Serrated Alliance";
            string path = "";
            foreach (ModInfo modInfo in ModManager.Mods)
            {
                if (modInfo.Manifest.Title == modName)
                {
                    path = modInfo.Path + "/PopulationTablesMerge.xml";
                    break;
                }
            }
            if (path != "")
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(path);
                XmlElement tables = xDoc.DocumentElement;
                foreach (XmlNode table in tables)
                {
                    if (table.Attributes != null && table.Name == "population")//Comments count as nodes but don't have attributes
                    {
                        string populationName = table.Attributes.GetNamedItem("Name").Value;
                        foreach (XmlNode entry in table.ChildNodes)
                        {
                            if (entry.Name == "group" && entry.Attributes != null)
                            {
                                string groupName = entry.Attributes.GetNamedItem("Name").Value;
                                string groupStyle = entry.Attributes.GetNamedItem("Style").Value;
                                PopulationGroup popGroup = new PopulationGroup();
                                popGroup.Name = groupName;
                                popGroup.Style = groupStyle;
                                List<PopulationObject> populationObjects = new List<PopulationObject>();
                                List<PopulationTable> populationTables = new List<PopulationTable>();
                                foreach (XmlNode groupElement in entry.ChildNodes)
                                {
                                    if (groupElement.Name == "object")
                                    {
                                        PopulationObject popObject = new PopulationObject();
                                        foreach (XmlAttribute attribute in groupElement.Attributes)
                                        {
                                            switch (attribute.Name)
                                            {
                                                case "Blueprint": popObject.Blueprint = attribute.Value; break;
                                                case "Number": popObject.Number = attribute.Value; break;
                                                case "Weight": popObject.Weight = Convert.ToUInt32(attribute.Value); break;
                                                default: break;
                                            }
                                        }
                                        populationObjects.Add(popObject);
                                    }
                                    else if (groupElement.Name == "table")
                                    {
                                        PopulationTable popTable = new PopulationTable();
                                        foreach (XmlAttribute attribute in groupElement.Attributes)
                                        {
                                            switch (attribute.Name)
                                            {
                                                case "Name": popTable.Name = attribute.Value; break;
                                                case "Number": popTable.Number = attribute.Value; break;
                                                case "Chance": popTable.Chance = attribute.Value; break;
                                                default: break;
                                            }
                                        }
                                        populationTables.Add(popTable);
                                    }
                                }
                                popGroup.Items.AddRange(populationObjects);
                                popGroup.Items.AddRange(populationTables);
                                foreach (KeyValuePair<string, PopulationInfo> pop in PopulationManager.Populations)
                                {
                                    if (pop.Value.Name == populationName)
                                    {
                                        pop.Value.Items.AddRange(popGroup.Items);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                Debug.Log("Populations from " + modName + " merged succesfully");
                //MessageQueue.AddPlayerMessage("Populations from " + modName + " merged succesfully");
            }
            else
                Debug.Log("Could not find path to mod " + modName);
            //MessageQueue.AddPlayerMessage("Could not find path to mod " + modName);
        }
    }
}