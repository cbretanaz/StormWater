using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using lib = CoP.Enterprise.Utilities;

namespace CoP.Enterprise
{

    public static class Serialization
    {
        private static readonly string sNL = Environment.NewLine;
        public enum Formatter { Binary, Xml, DataContract }

        #region Serialization methods
        /// <summary>
        /// Serializes provided object of Type T and converts
        /// serialized stream to byte[] 
        /// </summary>
        /// <typeparam name="T">Type of object graph</typeparam>
        /// <param name="obj">object to serialize</param>
        /// <returns>serialized byte array</returns>
        public static byte[] SerializeToByteArray<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                (new BinaryFormatter()).Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Serializes provided object of Type T and persists 
        /// serialized byte stream to File specified by Path Specificiation  
        /// </summary>
        /// <typeparam name="T">.Net CLR Type of object</typeparam>
        /// <param name="obj">The object to be serialized and persisted</param>
        /// <param name="pathSpec">Path specification of file to create 
        /// (Will over-write existing file with same name)</param>
        /// <param name="formatter">Which Serializer to use 
        /// (Binary, Xml, or Daat Contract Serializer)</param>
        /// <param name="ns">Namespace to be included in header</param>
        /// <param name="createDirectoryIfMissing">Create directory if it does not exist. (Defaults to false)</param>
        /// <param name="resetCurDir2aPath"></param>
        public static void Serialize2File<T>(T obj, 
            string pathSpec, Formatter formatter,
            XmlSerializerNamespaces ns = null,
            bool createDirectoryIfMissing = false,
            bool resetCurDir2aPath = true)
        {
            if(resetCurDir2aPath) Directory.SetCurrentDirectory(lib.ApplicationPath);
            if (createDirectoryIfMissing && !Directory.Exists(lib.ExtractPath(pathSpec)))
                Directory.CreateDirectory(lib.ExtractPath(pathSpec));
            try
            {
                switch (formatter)
                {
                    case (Formatter.Binary):
                        using (var fs = new FileStream(pathSpec, FileMode.Create,
                                            FileAccess.Write, FileShare.Write))
                            (new BinaryFormatter()).Serialize(fs, obj);
                        break;

                    case (Formatter.Xml):
                        if (ns == null)
                        {
                            ns = new XmlSerializerNamespaces();
                            ns.Add("", "");
                        }
                        var xmlSrlzr = new XmlSerializer(typeof(T));
                        using (var tw = new StreamWriter(pathSpec) as TextWriter)
                            xmlSrlzr.Serialize(tw, obj, ns);
                        break;

                    case (Formatter.DataContract):
                        var wcfSrlzr = new DataContractSerializer(typeof(T));
                        using (var filStrm = File.Open(pathSpec, FileMode.Create))
                            wcfSrlzr.WriteObject(filStrm, obj);
                        break;

                    default:
                        throw new CoPException("Invalid Formatter option");
                }
            }
            catch (SerializationException sX)
            {
                var errMsg = $"Unable to serialize {obj} into file {pathSpec}";
                DALLog.Write(DALLog.Level.Error, errMsg, sX);
                throw new CoPException(errMsg, sX);
            }
        }

        #region SerializeToString overloads
        /// <summary>
        /// Serializes provided object of Type T using default [Xml] Serializer
        /// </summary>
        /// <typeparam name="T">.Net CLR Type of object</typeparam>
        /// <param name="obj">The object to be serialized and persisted</param>
        /// <returns>Xml serialized representation of object, as a string</returns>
        /// <param name="ns">Namespace to be included in header</param>
        public static string SerializeToString<T>(T obj,
            XmlSerializerNamespaces ns = null)
        { return (SerializeToStringWriter(obj, Formatter.Xml, ns).ToString()); }
        /// <summary>
        /// Serializes provided object of Type T using specified [Xml or DataContract] Serializer
        /// </summary>
        /// <typeparam name="T">.Net CLR Type of object</typeparam>
        /// <param name="obj">The object to be serialized and persisted</param>
        /// <param name="formatter">Whether to use Xml or Data Contract Serializer</param>
        /// <returns>Xml serialized representation of object, as a string</returns>
        /// <param name="ns">Namespace to be included in header</param>
        public static string SerializeToString<T>(T obj, Formatter formatter,
            XmlSerializerNamespaces ns = null)
        { return SerializeToStringWriter(obj, formatter, ns).ToString(); }
        #endregion SerializeToString overloads

        #region SerializeToStringWriter overloads
        /// <summary>
        /// Serializes provided object of Type T using specified [Xml or DataContract] Serializer
        /// </summary>
        /// <typeparam name="T">.Net CLR Type of object</typeparam>
        /// <param name="obj">The object to be serialized and persisted</param>
        /// <param name="formatter">Whether to use Xml or Data Contract Serializer</param>
        /// <returns>character stream as String Writer, 
        /// suitable for writing to another stream, to disk, 
        /// a WCF, web sservice or remoting communication channel, 
        /// or to a remote socket/port.</returns>
        /// <param name="ns">Namespace to be included in header</param>
        public static StringWriter SerializeToStringWriter<T>(T obj,
            Formatter formatter = Formatter.Xml,
            XmlSerializerNamespaces ns = null)
        {
            if (formatter == Formatter.Binary)
                throw new InvalidOperationException(
                    "Cannot use Binary serializaion with SerializeToStringWriter.");
            var wrtr = new StringWriter();
            SerializeToTextWriter(obj, wrtr, formatter, ns);
            return wrtr;
        }

        /// <summary>
        /// Serializes provided object of Type T into TextWriter 
        ///  using specified [Xml or DataContract] Serializer
        /// </summary>
        /// <typeparam name="T">.Net CLR Type of object</typeparam>
        /// <param name="obj">The object to be serialized and persisted</param>
        /// <param name="txtWrtr">TextWriter to put the serialized xml into</param>
        /// <param name="formatter">Whether to use Xml[Default] or Data Contract Serializer</param>
        /// <param name="ns">Namespace to be included in header</param>
        public static void SerializeToTextWriter<T>(T obj,
            TextWriter txtWrtr, Formatter formatter,
            XmlSerializerNamespaces ns = null)
        {
            try
            {
                switch (formatter)
                {
                    case (Formatter.Xml):
                        if (ns == null)
                        {
                            ns = new XmlSerializerNamespaces();
                            ns.Add("", "");
                        }
                        var xmlSrzlr = new XmlSerializer(obj.GetType());
                        xmlSrzlr.Serialize(txtWrtr, obj, ns);
                        break;

                    case (Formatter.DataContract):
                        var dcSrzlr = new DataContractSerializer(obj.GetType());
                        using (var xmlWrtr = new XmlTextWriter(txtWrtr))
                            dcSrzlr.WriteObject(xmlWrtr, obj);
                        break;

                    default: throw new CoPException(
                        "Invalid Formatter option");
                }
            }
            catch (SerializationException sX)
            {
                var errMsg = String.Format(
                    "Unable to serialize {0}",
                    typeof(T));
                DALLog.Write(DALLog.Level.Error, errMsg, sX);
                throw new CoPException(errMsg, sX);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="txtWrtr">TextWriter to put the serialized xml into</param>
        /// <param name="formatter">Whether to use Xml or Data Contract Serializer</param>
        /// <param name="objects">collection of object items to serialize</param>
        public static void SerializeToTextWriter(TextWriter txtWrtr, 
                Formatter formatter, params object[] objects)
        { txtWrtr.Write(SerializeToString(formatter, objects)); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formatter">Whether to use Xml or Data Contract Serializer</param>
        /// <param name="objects">collection of object items to serialize</param>
        /// <returns>string xml document containing serialized objects </returns>
        public static string SerializeToString(Formatter formatter, params object[] objects) 
        { return SerializeToString(formatter, null, objects); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formatter">Whether to use Xml or Data Contract Serializer</param>
        /// <param name="rootElmName">Xml root element name</param>
        /// <param name="objects">collection of object items to serialize</param>
        /// <returns>string xml document containing serialized objects </returns>
        public static string SerializeToString(Formatter formatter, 
            string rootElmName, params object[] objects)
        {
            if(formatter != Formatter.Xml && formatter != Formatter.DataContract)
                throw new CoPException("Invalid Formatter option");
            // ------------------------------------------------------
            var xDoc = new XmlDocument();
            if (xDoc == null) 
                throw new CoPException("Could not create Xml Document");
            // ------------------------------------------------------
            xDoc.LoadXml(string.Format("<{0}/>", rootElmName?? "xml"));
            if (xDoc.DocumentElement==null)
                throw new CoPException("Could not load Xml Document root element");
            // ------------------------------------------------------
            var root=xDoc.DocumentElement;
            foreach(var obj in objects)
            {
                var elm=xDoc.CreateElement(obj.GetType().Name);
                elm.InnerXml = SerializeToString(obj, formatter);
                root.AppendChild(elm);
            }
            return xDoc.OuterXml;
        }
        #endregion SerializeToStringWriter overloads
        #endregion Serialization methods

        #region DeSerialization methods
        /// <summary>
        /// Deserializes byte array into object of generic type T
        /// </summary>
        /// <typeparam name="T">Type to deserialize into</typeparam>
        /// <param name="bytes">byte array to deserialize</param>
        /// <returns>Object of type T</returns>
        public static T DeserializeFromByteArray<T>(byte[] bytes)
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Seek(0, SeekOrigin.Begin);
                var obj = (new BinaryFormatter()).Deserialize(ms);
                if (obj is T) return (T) obj;
                throw new SerializationException(string.Format(
                    "Unable to deserialize Byte arrray as type {0}",
                    typeof (T)));
            }
        }

        /// <summary>
        /// Attempts to Deserialize contents of specified file
        ///  using specified Serializer (Binary, Xml or DataContract)
        /// </summary>
        /// <typeparam name="T">Type of object to attempt to instantiate</typeparam>
        /// <param name="pathSpec">PathSpecification to 
        /// file on disk containing serialized byte stream</param>
        /// <param name="formatter">Specifies which Serializer to use:
        ///  (Binary, Xml. or Data Contract Serializer)</param>
        /// <returns>.Net object of type T</returns>
        public static T DeSerializeFromFile<T>(string pathSpec,
            Formatter formatter)where T : class
        { return DeSerializeFromFile<T>(pathSpec, formatter, null, null); }

        /// <summary>
        /// Attempts to Deserialize contents of specified file
        ///  using specified Serializer (Binary, Xml or DataContract)
        /// </summary>
        /// <typeparam name="T">Type of object to attempt to instantiate</typeparam>
        /// <param name="pathSpec">PathSpecification to 
        /// file on disk containing serialized byte stream</param>
        /// <param name="formatter">Specifies which Serializer to use:
        ///  (Binary, Xml. or Data Contract Serializer)</param>
        /// <param name="rootName">If using the DataContract Serializer, what the neame of ther root element is</param>
        /// <param name="rootNamespace">when using the DataContract Serializer, what the root element namepsace is</param>
        /// <param name="resetCurDir2aPath"></param>
        /// <returns>.Net object of type T</returns>
        public static T DeSerializeFromFile<T>(
            string pathSpec, Formatter formatter,
            string rootName, string rootNamespace,
            bool resetCurDir2aPath = true) 
            where T: class
        {
            if (resetCurDir2aPath) Directory.SetCurrentDirectory(lib.ApplicationPath);
            try
            {
                switch (formatter)
                {
                    case (Formatter.Binary):
                        using (var strm = new FileStream(pathSpec,
                                  FileMode.Open, FileAccess.Read))
                        {
                            IFormatter fmt = new BinaryFormatter();
                            var obj = fmt.Deserialize(strm);
                            if (!(obj is T)) throw new ArgumentException(string.Format(
                                    "Cannot Deserialize {1} from file:{0}" +
                                    " [{2}]. Bad or missing Data File", sNL,
                                    typeof(T), pathSpec));
                            return obj as T;
                        }

                    case (Formatter.Xml):
                        var serializer = new XmlSerializer(typeof(T));
                        TextReader rdr = new StreamReader(pathSpec);
                        return (T)serializer.Deserialize(rdr);

                    case (Formatter.DataContract):
                        var wcfSrlzr = rootName == null && rootNamespace == null ?
                            new DataContractSerializer(typeof(T)):
                            new DataContractSerializer(typeof(T), rootName?? "", rootNamespace);
                        using (var filStrm = File.Open(pathSpec, FileMode.Open, FileAccess.Read))
                        {
                            var obj = wcfSrlzr.ReadObject(filStrm);
                            if (!(obj is T)) throw new ArgumentException(string.Format(
                                    "Cannot Deserialize {1} from file:{0}" +
                                    " [{2}]. Bad or missing Data File", sNL,
                                    typeof(T), pathSpec));
                            return obj as T;
                        }

                    default:
                        throw new CoPException("Invalid Formatter option");
                }
            }
            catch(IOException ioX)
            {
                var errMsg = $"Unable to deserialize {typeof(T)} from file {pathSpec}";
                DALLog.Write(DALLog.Level.Error, errMsg, ioX);
                throw new CoPException(errMsg, ioX);
            }
            catch (SerializationException sX)
            {
                var errMsg = $"Unable to deserialize {typeof(T)} from file {pathSpec}";
                DALLog.Write(DALLog.Level.Error, errMsg, sX);
                throw new CoPException(errMsg, sX);
            }
        }
           
        #region Deserialize From Stream Overloads 
        /// <summary>
        /// Attempts to Deserialize a string (text-stream of characters)
        ///  using default Xml Deserializer
        /// </summary>
        /// <typeparam name="T">Type of object to attempt to instantiate</typeparam>
        /// <param name="rdr">Stream of characters as Stringreader</param>
        /// <returns>.Net object of type T</returns>
        public static T DeSerialize<T>(TextReader rdr)
        {
            var srzr = new XmlSerializer(typeof(T));
            return (T)srzr.Deserialize(rdr);
        }
        /// <summary>
        /// Attempts to Deserialize a string (text-stream of characters)
        ///  using default Xml Deserializer
        /// </summary>
        /// <typeparam name="T">Type of object to attempt to instantiate</typeparam>
        /// <param name="rdr">Stream of characters as Stringreader</param>
        /// <param name="serializer"></param>
        /// <returns>.Net object of type T</returns>
        public static T DeSerialize<T>(TextReader rdr, Formatter serializer)
        { return DeSerialize<T>(rdr, serializer, null, null); }

        /// <summary>
        /// Attempts to Deserialize a string (text-stream of characters)
        ///  using default Xml Deserializer
        /// </summary>
        /// <typeparam name="T">Type of object to attempt to instantiate</typeparam>
        /// <param name="rdr">Stream of characters as Stringreader</param>
        /// <param name="serializer">which Deserializer to use</param>
        /// <param name="rootName">If using the DataContract Serializer, what the neame of ther root element is</param>
        /// <param name="rootNamespace">when using the DataContract Serializer, what the root element namepsace is</param>
        /// <returns>.Net object of type T</returns>
        public static T DeSerialize<T>(TextReader rdr, Formatter serializer,
            string rootName, string rootNamespace)
        {
            switch(serializer)
            {
                case (Formatter.Xml):
                    var xmlSrzr = new XmlSerializer(typeof(T));
                    return (T)xmlSrzr.Deserialize(rdr);

                case (Formatter.DataContract):
                    var dcSrlzr = rootName == null && rootNamespace == null ?
                            new DataContractSerializer(typeof(T)) :
                            new DataContractSerializer(typeof(T), rootName ?? "", rootNamespace);
                    using (var xmlRdr = new XmlTextReader(rdr))
                        return (T)dcSrlzr.ReadObject(xmlRdr);

                default: throw new CoPException(
                    "Invalid Serializer option");
            }
        } 
        #endregion Deserialize From Stream Overloads

        #region Deserialize From String Overloads
        /// <summary>
        /// Attempts to Deserialize a string 
        ///  using default Xml Deserializer
        /// </summary>
        /// <typeparam name="T">Type of object to attempt to instantiate</typeparam>
        /// <param name="s">string containing  xml serialization of object T </param>
        /// <returns>.Net object of type T</returns>
        public static T DeSerialize<T>(string s)
        {
            using(var rdr = new StringReader(s))
                return DeSerialize<T>(rdr, Formatter.Xml);
        }
        /// <summary>
        /// Attempts to Deserialize a string 
        ///  using default Xml Deserializer
        /// </summary>
        /// <typeparam name="T">Type of object to attempt to instantiate</typeparam>
        /// <param name="s">string containing  xml serialization of object T </param>
        /// <param name="formatter"></param>
        /// <returns>.Net object of type T</returns>
        public static T DeSerialize<T>(string s, Formatter formatter)
        {
            using (var rdr = new StringReader(s))
                return DeSerialize<T>(rdr, formatter);
        }

        /// <summary>
        /// Attempts to Deserialize a string 
        ///  using default Xml Deserializer
        /// </summary>
        /// <typeparam name="T">Type of object to attempt to instantiate</typeparam>
        /// <param name="s">string containing  xml serialization of object T </param>
        /// <param name="formatter"></param>
        /// <param name="rootName"></param>
        /// <param name="roorNamespace"></param>
        /// <returns>.Net object of type T</returns>
        public static T DeSerialize<T>(string s, Formatter formatter,
            string rootName, string rootNamespace)
        {
            using (var rdr = new StringReader(s))
                return DeSerialize<T>(rdr, formatter);
        }
        #endregion Deserialize From String Overloads
        #endregion DeSerialization methods 
    }

    [Serializable]
    public class SerializableDictionary<K, V> : Dictionary<K, V>, IXmlSerializable
    {
        public SerializableDictionary() { }

        public static SerializableDictionary<K, V> Make()
        { return new SerializableDictionary<K, V>(); }

        public SerializableDictionary<K, V> Clone()
        {
            var newSD = Make();
            foreach(var k in Keys)
                newSD.Add(k, this[k]);
            return newSD;
        }

        #region IXmlSerializable Members
        public XmlSchema GetSchema() { return null; }
        public void ReadXml(XmlReader reader)
        {
            //var keySerializer = new XmlSerializer(typeof(K));
            //var valueSerializer = new XmlSerializer(typeof(V));
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty) return;

            var baseKeyType = typeof(K).AssemblyQualifiedName;
            var baseValueType = typeof(V).AssemblyQualifiedName;

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                // Start
                reader.ReadStartElement("item");

                // Start Key
                var keySerializer = GetTypeSerializer(reader["type"] ?? baseKeyType);
                reader.ReadStartElement("key");
                var key = (K)keySerializer.Deserialize(reader);
                reader.ReadEndElement();
                // End Key

                // Start Value
                var valueSerializer = GetTypeSerializer(reader["type"] ?? baseValueType);
                reader.ReadStartElement("value");
                var value = (V)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();
                // End Value

                Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }
        public void WriteXml(XmlWriter writer)
        {
            //var keySerializer = new XmlSerializer(typeof(K));
            var baseValueSerializer = new XmlSerializer(typeof(V));
            foreach (var key in Keys)
            {
                //Start
                writer.WriteStartElement("item");

                // Key -----------------------------------------
                var keyType = key.GetType();
                var keySerializer = GetTypeSerializer(keyType.AssemblyQualifiedName);
                writer.WriteStartElement("key");
                if (keyType != typeof(K))
                    { writer.WriteAttributeString("type", 
                        keyType.AssemblyQualifiedName); }
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();
                // End Key  --------------------------------------


                // Value -----------------------------------------
                var value = this[key];
                var valueType = value?.GetType();
                var valueSerializer = valueType == null ? baseValueSerializer :
                    GetTypeSerializer(valueType.AssemblyQualifiedName);
                writer.WriteStartElement("value");
                if (valueType != null && valueType != typeof(V))
                {
                    writer.WriteAttributeString("type",
                      valueType.AssemblyQualifiedName);
                }

                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();
                // End Value ------------------------------------

                writer.WriteEndElement();
                //End
            }
        }
        #endregion

        #region " GetTypeSerializer "

        private static readonly Dictionary<string, XmlSerializer> serlzrs 
            = new Dictionary<string, XmlSerializer>();
        private static readonly object locker = new object();
        private XmlSerializer GetTypeSerializer(string type)
        {
            if (!serlzrs.ContainsKey(type))
            {
                lock (locker)
                {
                    if (!serlzrs.ContainsKey(type))
                    {
                        var ser = new XmlSerializer(Type.GetType(type));
                        serlzrs.Add(type, ser);
                    }
                }
            }
            return serlzrs[type];
        }

        #endregion

    }

    public class SerializableTimeSeries<V> : Dictionary<HourInterval, V>, IXmlSerializable
    {
        #region IXmlSerializable Members
        public XmlSchema GetSchema() { return null; }
        public void ReadXml(XmlReader reader)
        {
            var strSerializer = new XmlSerializer(typeof(string));
            var valueSerializer = new XmlSerializer(typeof(V));
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty) return;
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadAttributeValue();
                var itvl = HourInterval.Make((string)strSerializer.Deserialize(reader));
                reader.ReadAttributeValue(); 
                var value = (V)valueSerializer.Deserialize(reader);
                Add(itvl, value);
                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }
        public void WriteXml(XmlWriter writer)
        {
            var strSerializer = new XmlSerializer(typeof(string));
            var valueSerializer = new XmlSerializer(typeof(V));
            foreach (var key in Keys)
            {
                writer.WriteStartElement("item");
                writer.WriteStartElement("key");
                strSerializer.Serialize(writer, key);
                writer.WriteEndElement();
                writer.WriteStartElement("value");
                var value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }
        #endregion
    }

    public class DataContractSerializer<T>: XmlObjectSerializer
    {
        private DataContractSerializer dcSerzr;

        #region ctor/Factorys
        public DataContractSerializer()
        { dcSerzr = new DataContractSerializer(typeof(T)); }
        #endregion ctor/Factorys


        public new T ReadObject(Stream stream) { return (T)dcSerzr.ReadObject(stream); }
        public new T ReadObject(XmlReader rdr) { return (T)dcSerzr.ReadObject(rdr); }
        public void WriteObject(Stream stream, T graph) { dcSerzr.WriteObject(stream, graph); }
        public void WriteObject(XmlWriter wrtr, T graph) { dcSerzr.WriteObject(wrtr, graph); }
        
        public override void WriteStartObject(XmlDictionaryWriter writer, object graph)
        {
            throw new NotImplementedException();
        }

        public override void WriteObjectContent(XmlDictionaryWriter writer, object graph)
        {
            throw new NotImplementedException();
        }

        public override void WriteEndObject(XmlDictionaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public override object ReadObject(XmlDictionaryReader reader, bool verifyObjectName)
        {
            throw new NotImplementedException();
        }

        public override bool IsStartObject(XmlDictionaryReader reader)
        {
            throw new NotImplementedException();
        }
    }
}
