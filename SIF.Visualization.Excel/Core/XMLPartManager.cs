﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Office.Core;
using SIF.Visualization.Excel.Properties;

namespace SIF.Visualization.Excel.Core
{
    public class XMLPartManager
    {
        #region Singleton

        private static volatile XMLPartManager instance;
        private static readonly object syncRoot = new object();
        private XmlSchemaSet report, request;

        private XMLPartManager()
        {
        }

        /// <summary>
        ///     Gets the current XML part manager instance.
        /// </summary>
        public static XMLPartManager Instance
        {
            get
            {
                if (instance == null)
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new XMLPartManager();
                    }

                return instance;
            }
        }

        #endregion

        #region Methods

        public XElement LoadXMLPart(WorkbookModel workbook, string id)
        {
            var part = GetCustomXMLPart(workbook, id);
            if (part != null)
            {
                var result = XElement.Parse(part.XML);
                Debug.WriteLine("Loaded from the customXMLParts with ID = '" + id + "'");
                //Debug.WriteLine(result);
                return result;
            }
            return null;
        }

        public void SaveXMLPart(WorkbookModel workbook, XElement root, string id)
        {
            if (root == null) return;

            //clear old
            var oldPart = GetCustomXMLPart(workbook, id);
            if (oldPart != null) oldPart.Delete();

            //save
            var scenarioXMLPart = workbook.Workbook.CustomXMLParts.Add(root.ToString());
            Debug.WriteLine("Saved customXMLPart with ID = '" + id + "'");
            //Debug.WriteLine(root.ToString());
        }

        private CustomXMLPart GetCustomXMLPart(WorkbookModel workbook, string id)
        {
            CustomXMLPart customPart = null;
            foreach (CustomXMLPart part in workbook.Workbook.CustomXMLParts)
                try
                {
                    var xml = XElement.Parse(part.XML);
                    if (xml.Name.LocalName.Equals(id))
                    {
                        customPart = part;
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

            return customPart;
        }

        public string Serialize<T>(T value)
        {
            if (value == null) return null;
            try
            {
                var xmlserializer = new XmlSerializer(typeof(T));
                var stringWriter = new StringWriter();
                var writer = XmlWriter.Create(stringWriter);

                xmlserializer.Serialize(writer, value);
                var serializeXml = stringWriter.ToString();
                writer.Close();

                return serializeXml;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public T Deserialize<T>(string xml)
        {
            if (xml == null) return default(T);
            try
            {
                var xmlserializer = new XmlSerializer(typeof(T));
                var stringReader = new StringReader(xml);
                var reader = XmlReader.Create(stringReader);

                var myObject = xmlserializer.Deserialize(reader);
                reader.Close();
                return (T) myObject;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return default(T);
            }
        }

        public XmlSchema ReadXMLSchemaFromFile(string filename)
        {
            try
            {
                var reader = new XmlTextReader(filename);
                var myschema = XmlSchema.Read(reader, ValidationCallback);
                return myschema;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        ///     Creates and returns the XML Schema definition for the SpRuDeL requests
        ///     with a ValidationCallback which reports errors to the UI.
        /// </summary>
        /// <returns>The corresponding XmlSchemaSet</returns>
        public XmlSchemaSet GetRequestSchema()
        {
            if (request == null)
            {
                var sprudel = XmlReader.Create(new StringReader(SchemaStrings.getRequestXSD()));
                request = new XmlSchemaSet();
                request.Add(string.Empty, sprudel);
                request.ValidationEventHandler += ValidationCallback;
            }
            return request;
        }

        /// <summary>
        ///     Creates and returns the XML Schema definition for the SpRuDeL reports
        ///     with a ValidationCallback which reports errors to the UI.
        /// </summary>
        /// <returns>The corresponding XmlSchemaSet</returns>
        public XmlSchemaSet getReportSchema()
        {
            if (report == null)
            {
                var sprudel = XmlReader.Create(new StringReader(SchemaStrings.getReportXSD()));
                report = new XmlSchemaSet();
                report.Add(string.Empty, sprudel);
                report.ValidationEventHandler += ValidationCallback;
            }
            return report;
        }

        public void ValidationCallback(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Warning)
            {
                Debug.Write("WARNING ValidationCallback: ");
            }
            else if (e.Severity == XmlSeverityType.Error)
            {
                Debug.Write("ERROR ValidationCallback: ");
                MessageBox.Show(Resources.tl_ValidationError + e.Message, Resources.tl_MessageBox_Error);
            }
            Debug.WriteLine(e.Message);
        }

        #endregion
    }
}