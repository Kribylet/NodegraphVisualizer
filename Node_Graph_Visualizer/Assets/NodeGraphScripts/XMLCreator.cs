using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Nodegraph_Generator
{
    /**
     * Static class containing functions for writing and reading to/from XML files.
     * Can be used for any object class (But will be used in this project mostly for serializing NodeGraph).
     */
    public static class XMLCreator
    {
        /**
         * Translates the given object into its XML-representation and writes the content into a file at the given path.
         * Creates the file if no such file is pressent, otherwise overrides it.
         */
        public static void writeXML(object objectToSerialize, String path)
        {
            FileStream fileStream = new FileStream(path, FileMode.Create);

            // Format XML with newlines and indentation to improve readability.
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.NewLineHandling = NewLineHandling.Entitize;
            settings.Indent = true;

            // Object used to serialize given object into XML.
            XmlWriter writer = XmlWriter.Create(fileStream, settings);

            // Interprets object DataContract when serializing.
            DataContractSerializer ser = new DataContractSerializer(objectToSerialize.GetType());
            ser.WriteObject(writer, objectToSerialize);
            writer.Close();
            fileStream.Close();
        }

        /**
         * Reads the XML-file at the given path and restores the object (of type T) represented in the file back into its object form.
         * Returns the restored object.
         * This function is used for testing purposes only.
         */
        public static T readXML<T>(String path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            XmlReader reader = XmlReader.Create(fs);

            // Interprets object DataContract when deserializing.
            DataContractSerializer ser = new DataContractSerializer(typeof(T));

            // Deserialize the data and read it from the instance.
            T readItem = (T)ser.ReadObject(reader, true);
            reader.Close();
            fs.Close();

            return readItem;
        }
    }
}
