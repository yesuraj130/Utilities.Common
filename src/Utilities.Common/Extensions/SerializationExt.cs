using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Utilities.Common.Extensions
{
    public static class SerializationExt
    {



        //public static T DeSerialize<T>(this string Location)
        //{
        //    try
        //    {
        //        Test("8.X start: " + Location);
        //        using (var t = StreamExt.GetStreamCopy(Location))
        //        {
        //            if (t.IsZipStream())
        //            {
        //                using (var unZipStream = t.ToUnZipStream())
        //                {
        //                    return DeSerialize<T>(unZipStream);
        //                }
        //            }
        //            else
        //            {
        //                return DeSerialize<T>(t);
        //            }
        //        }
        //    }
        //    catch (InvalidOperationException Ex)
        //    {
        //        throw new Exception($"Error loading file \"{Location}\"", Ex);
        //    }

        //}
        //public static T DeSerialize<T>(this string Location)
        //{
        //    try
        //    {
        //        Test("8.X start: " + Location);
        //        using (var t = StreamExt.GetStreamCopy(Location))
        //        {
        //            if (t.IsZipStream())
        //            {
        //                using (var unZipStream = t.ToUnZipStream())
        //                {
        //                    return JsonDeSerialize<T>(unZipStream);
        //                }
        //            }
        //            else
        //            {
        //                return JsonDeSerialize<T>(t);
        //            }
        //        }
        //    }
        //    catch (InvalidOperationException Ex)
        //    {
        //        throw new Exception($"Error loading file \"{Location}\"", Ex);
        //    }

        //}



        //public static T DeSerialize<T>(this string Location)
        //{
        //    try
        //    {
        //        try
        //        {
        //            Test("8.X start: " + Location);

        //            using (var fileStream = new FileStream(Location, FileMode.Open, FileAccess.Read, FileShare.Read))
        //            {
        //                Test("8.X.1");
        //                using (var zipInputStream = new ZipInputStream(fileStream))
        //                {
        //                    Test("8.X.2");
        //                    zipInputStream.GetNextEntry();
        //                    Test("8.X.3");
        //                    return DeSerialize<T>(zipInputStream);
        //                }
        //            }
        //        }
        //        catch
        //        {
        //            Test("8.X.4");
        //            using (var fileStream = new FileStream(Location, FileMode.Open, FileAccess.Read, FileShare.Read))
        //            {
        //                Test("8.X.5");
        //                return DeSerialize<T>(fileStream);

        //            }
        //        }
        //    }
        //    catch (InvalidOperationException Ex)
        //    {
        //        Test("8.X.E1");
        //        throw new Exception($"Error loading file \"{Location}\"", Ex);
        //    }
        //    finally
        //    {
        //        Test("8.X end: ");
        //    }

        //}
        //private static void Test(string Message)
        //{
        //    //if (ThisApp.CurrerntUserName.IsEqualExt("YesurajS")) MessageBox.Show(Message);


        //    if (Environment.UserName.ContainsExt("Yesuraj"))
        //    {
        //        var testfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"Test1.txt");
        //        //if (File.Exists(testfile)) File.Delete(testfile);
        //        try
        //        {
        //            using (var Fs = new StreamWriter(testfile, true))
        //            {
        //                Fs.WriteLine($"{DateTime.Now:hh:mm:ss:ff tt}: {Message}");
        //            }
        //        }
        //        catch
        //        {
        //            try
        //            {
        //                var testfile2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"Test2.txt");
        //                using (var Fs = new StreamWriter(testfile2, true))
        //                {
        //                    Fs.WriteLine($"{DateTime.Now:hh:mm:ss:ff tt}: {Message}");
        //                }
        //            }
        //            catch
        //            {

        //                var testfile3 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"Test3.txt");
        //                using (var Fs = new StreamWriter(testfile3, true))
        //                {
        //                    Fs.WriteLine($"{DateTime.Now:hh:mm:ss:ff tt}: {Message}");
        //                }
        //            }
        //        }
        //    }

        //}









        //public static T DeSerialize<T>(this Stream stream)
        //{

        //    Test("8.X.3.1");
        //    using XmlReader xmlReader = XmlReader.Create(stream, new XmlReaderSettings() { DtdProcessing = DtdProcessing.Prohibit, /*IgnoreProcessingInstructions = true,*/ });
        //    Test("8.X.3.2");


        //    //Test("8.X.3.3");
        //    //stream.Seek(0, SeekOrigin.Begin);

        //    Test("8.X.3.4");
        //    //var xmldoc = new XmlDocument();
        //    //xmldoc.Load(stream);
        //    //var fromXml = JsonConvert.SerializeXmlNode(xmldoc);
        //    //var obj = JsonConvert.DeserializeObject(fromXml, typeof(T));

        //    //var serializer = new Polenter.Serialization.SharpSerializer();
        //    //var obj = serializer.Deserialize(stream);

        //    var ser = GetSerializer(typeof(T));
        //    Test("8.X.3.4.1 - Thread " + Thread.CurrentThread.ManagedThreadId + " - " + XmlSerializer.GetXmlSerializerAssemblyName(typeof(T)));


        //    var obj = ser?.Deserialize(xmlReader);


        //    //var serializer = new XSerializer.XmlSerializer<T>();
        //    //var obj = serializer.Deserialize(stream);

        //    //StreamReader reader = new StreamReader(stream);
        //    //string text = reader.ReadToEnd();

        //    Test("8.X.3.5" /*+ text*/);
        //    //XmlSerializerHelper xmlSerializerHelper = new XmlSerializerHelper();
        //    //var res1 = xmlSerializerHelper.DeserializeFromXml<T>(text);
        //    var res = (T)obj;

        //    Test("8.X.3.6");
        //    return res;

        //}

        //public static MemoryStream Serialize<T>(this T Source)
        //{
        //    MemoryStream result = new MemoryStream();
        //    new XmlSerializer(typeof(T)).Serialize(result, Source);
        //    result.Seek(0, SeekOrigin.Begin);
        //    return result;

        //}
        ////public static void Serialize<T>(this T Source, string SaveLocation)
        ////{
        ////    //Directory.CreateDirectory(Path.GetDirectoryName(SaveLocation));
        ////    using (var originalFileStream = Source.Serialize())
        ////    {
        ////        using (var zipStream = originalFileStream.ToZipStream(Path.GetFileName(SaveLocation)))
        ////        {
        ////            var st = System.Threading.Thread.CurrentThread.IsBackground;
        ////            System.Threading.Thread.CurrentThread.IsBackground = false;
        ////            using (FileStream fileStream = new FileStream(SaveLocation, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
        ////            {
        ////                zipStream.CopyTo(fileStream);
        ////            }
        ////            System.Threading.Thread.CurrentThread.IsBackground = st;

        ////        }
        ////        //using (FileStream source = new FileStream(SaveLocation, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
        ////        //{
        ////        //    using (var compressionStream = new GZipStream(source, CompressionMode.Compress))
        ////        //    {
        ////        //        compressionStream.Write(originalFileStream.ToArray(), 0, (int)originalFileStream.Length);
        ////        //    }
        ////        //}
        ////    }
        ////}
        //public static void Serialize<T>(this T Source, string SaveLocation)
        //{

        //    using (var originalFileStream = Source.JsonSerialize())
        //    {
        //        using (var zipStream = originalFileStream.ToZipStream(Path.GetFileName(SaveLocation)))
        //        {
        //            var st = System.Threading.Thread.CurrentThread.IsBackground;
        //            System.Threading.Thread.CurrentThread.IsBackground = false;
        //            using (FileStream fileStream = new FileStream(SaveLocation, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
        //            {
        //                zipStream.CopyTo(fileStream);
        //            }
        //            System.Threading.Thread.CurrentThread.IsBackground = st;

        //        }
        //    }
        //}



        //public static void SerializeRaw<T>(this T Source, string SaveLocation)
        //{
        //    //Directory.CreateDirectory(Path.GetDirectoryName(SaveLocation));
        //    using (var originalFileStream = Source.Serialize())
        //    {

        //        using (FileStream fileStream = new FileStream(SaveLocation, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
        //        {
        //            originalFileStream.CopyTo(fileStream);
        //        }
        //        System.Threading.Thread.CurrentThread.IsBackground = false;
        //    }
        //}

        //public static void SerializeDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, string SaveLocation)
        //{
        //    Directory.CreateDirectory(Path.GetDirectoryName(SaveLocation));
        //    var st = System.Threading.Thread.CurrentThread.IsBackground;
        //    System.Threading.Thread.CurrentThread.IsBackground = false;
        //    using StreamWriter writer = new StreamWriter(SaveLocation);
        //    new XmlSerializer(typeof(List<SerializableKeyValuePair<TKey, TValue>>)).Serialize(writer, dictionary.Select(x => new SerializableKeyValuePair<TKey, TValue> { Key = x.Key, Value = x.Value }).ToList());
        //    writer.Flush();
        //    System.Threading.Thread.CurrentThread.IsBackground = st;


        //    //try // try to serialize the collection to a file
        //    //{
        //    //    using (var stream = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        //    //    {
        //    //        // create BinaryFormatter
        //    //        BinaryFormatter bin = new BinaryFormatter();
        //    //        // serialize the collection (EmployeeList1) to file (stream)
        //    //        bin.Serialize(stream, dictionary);
        //    //    }
        //    //}
        //    //catch (IOException) { }


        //}

        //[System.Serializable, XmlType(nameof(KeyValuePair<TKey, TValue>))]
        //public class SerializableKeyValuePair<TKey, TValue>
        //{
        //    public TKey Key { get; set; }
        //    public TValue Value { get; set; }
        //}

        ////[System.Serializable, XmlType(nameof(Dictionary<TKey, TValue>))]
        ////private class DictionaryExt<TKey, TValue>
        ////{
        ////    List<KeyValuePairExt<TKey, TValue>> KeyValues = new List<KeyValuePairExt<TKey, TValue>>
        ////}


        //public static Dictionary<TKey, TValue> DeserializeDictionary<TKey, TValue>(this string Location)
        //{
        //    try
        //    {
        //        using XmlReader xmlReader = XmlReader.Create(Location, new XmlReaderSettings() { DtdProcessing = DtdProcessing.Prohibit });

        //        return ((List<SerializableKeyValuePair<TKey, TValue>>)new XmlSerializer(typeof(List<SerializableKeyValuePair<TKey, TValue>>)).Deserialize(xmlReader)).ToDictionary(x => x.Key, x => x.Value);
        //    }
        //    catch (InvalidOperationException Ex)
        //    {
        //        throw new Exception($"Error loading file \"{Location}\"", Ex);
        //    }
        //    //Dictionary<TKey, TValue> ret = null;
        //    //try
        //    //{
        //    //    using (var stream = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        //    //    {
        //    //        // create BinaryFormatter
        //    //        BinaryFormatter bin = new BinaryFormatter();
        //    //        // deserialize the collection (Employee) from file (stream)
        //    //        ret = (Dictionary<TKey, TValue>)bin.Deserialize(stream);
        //    //    }
        //    //}
        //    //catch (IOException) { }
        //    //return ret;
        //}

        //public static void SerializeBinary<T>(this T dictionary, string Path)
        //{
        //    try
        //    {
        //        var st = System.Threading.Thread.CurrentThread.IsBackground;
        //        System.Threading.Thread.CurrentThread.IsBackground = false;
        //        using (var stream = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        //        {
        //            BinaryFormatter bin = new BinaryFormatter();
        //            bin.Serialize(stream, dictionary);
        //        }
        //        System.Threading.Thread.CurrentThread.IsBackground = st;
        //    }
        //    catch (Exception Ex)
        //    {
        //        throw new Exception($"Error loading file \"{Path}\"", Ex);
        //    }
        //}
        //public static T DeserializeBinary<T>(this string Path)
        //{
        //    try
        //    {
        //        using (var stream = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        //        {
        //            BinaryFormatter bin = new BinaryFormatter();
        //            return (T)bin.Deserialize(stream);
        //        }
        //    }
        //    catch (Exception Ex)
        //    {
        //        throw new Exception($"Error loading file \"{Path}\"", Ex);
        //    }
        //}


    }

    public static class XmlSerializationExt
    {
        private static readonly Dictionary<Type, XmlSerializer> XmlSerializers = new Dictionary<Type, XmlSerializer>();

        #region Internal
        private static XmlSerializer GetSerializer(Type type)
        {
            if (XmlSerializers.TryGetValue(type, out var match)) return match;
            else
            {
                var res = new XmlSerializer(type);
                try { XmlSerializers.Add(type, res); } catch { }
                return res;
            }
        }
        private static void InternalXmlSerialize<T>(T source, Stream stream)
        {
            using (var xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true, CloseOutput = false }))
            {
                var xmlSerializer = GetSerializer(source.GetType());
                xmlSerializer.Serialize(xmlWriter, source);
            }
        }
        private static T InternalXmlDeSerialize<T>(Stream stream)
        {
            using (var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, CloseInput = false }))
            {
                var xmlSerializer = GetSerializer(typeof(T));
                return (T)xmlSerializer.Deserialize(xmlReader);
            }
        }
        private static void CheckAndCreateFolder(string Location)
        {
            var dirPath = Path.GetDirectoryName(Location);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }
        //private static void InternalXmlSerializeWithSeek<T>(T source, Stream stream)
        //{
        //    stream.SeekBegin();
        //    InternalXmlSerialize(source, stream);
        //    stream.SetEndLength();
        //    stream.SeekBegin();
        //}
        //private static T InternalXmlDeSerializeWithSeek<T>(Stream stream)
        //{
        //    stream.SeekBegin();
        //    var result = InternalXmlDeSerialize<T>(stream);
        //    stream.SeekBegin();
        //    return result;
        //}
        #endregion


        #region Direct

        #region From File
        public static void XmlSerialize<T>(this T source, string Location)
        {
            CheckAndCreateFolder(Location);
            using (var fs = new FileStream(Location, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            {
                InternalXmlSerialize(source, fs);
                fs.SetEndLength();
            }
        }
        public static T XmlDeSerialize<T>(this string Location)
        {
            using (var fs = new FileStream(Location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return InternalXmlDeSerialize<T>(fs);
            }
        }
        #endregion

        #region From Stream
        public static MemoryStream XmlSerialize<T>(this T source)
        {
            var result = new MemoryStream();
            InternalXmlSerialize(source, result);
            result.SeekBegin();
            return result;
        }
        public static T XmlDeSerialize<T>(this Stream stream) => InternalXmlDeSerialize<T>(stream);
        #endregion

        #region From Text
        public static string XmlSerializeAsText<T>(this T source)
        {
            using (StringWriter textWriter = new StringWriter())
            {
                var xmlSerializer = GetSerializer(source.GetType());
                xmlSerializer.Serialize(textWriter, source);
                return textWriter.ToString();
            }
        }
        public static T XmlDeSerializeFromText<T>(this string text)
        {
            using (var stringReader = new StringReader(text))
            {
                using (var reader = XmlReader.Create(stringReader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, CloseInput = false }))
                {
                    var xmlSerializer = GetSerializer(typeof(T));
                    return (T)xmlSerializer.Deserialize(reader);
                }
            }
        }
        #endregion

        #region Deep Copy
        public static T XmlSerializationDeepCopy<T>(this T source) => XmlDeSerializeFromText<T>(XmlSerializeAsText(source));
        #endregion

        #endregion


        #region Zip

        #region Internal
        private static void InternalXmlSerializeZip<T>(this T source, Stream stream)
        {
            using (var newStream = stream.ZipStream())
            {
                InternalXmlSerialize(source, newStream);
            }
        }
        private static T InternalUnZipXmlDeSerialize<T>(this Stream stream)
        {
            using (var newStream = stream.UnZipStream())
            {
                return InternalXmlDeSerialize<T>(newStream);
            }
        }
        #endregion

        #region From File
        public static void XmlSerializeZip<T>(this T source, string Location)
        {
            CheckAndCreateFolder(Location);
            using (var fs = new FileStream(Location, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            {
                InternalXmlSerializeZip(source, fs);
                fs.SetEndLength();
            }

        }
        public static T UnZipXmlDeSerialize<T>(this string Location)
        {
            using (var fs = new FileStream(Location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return InternalUnZipXmlDeSerialize<T>(fs);
            }

        }
        #endregion

        #region From Stream
        public static void XmlSerializeZip<T>(this T source, Stream stream)
        {
            InternalXmlSerializeZip(source, stream);
            stream.SetEndLength();
        }
        public static T UnZipXmlDeSerialize<T>(this Stream stream)
        {
            return InternalUnZipXmlDeSerialize<T>(stream);


        }
        #endregion

        #endregion



        #region Deflate

        #region Internal
        private static void InternalXmlSerializeDeflate<T>(this T source, Stream stream)
        {
            using (var newStream = stream.DeflateStream())
            {
                InternalXmlSerialize(source, newStream);
            }
        }
        private static T InternalUnDeflateXmlDeSerialize<T>(this Stream stream)
        {
            using (var newStream = stream.UnDeflateStream())
            {
                return InternalXmlDeSerialize<T>(newStream);
            }
        }
        #endregion

        #region From File
        public static void XmlSerializeDeflate<T>(this T source, string Location)
        {
            CheckAndCreateFolder(Location);
            using (var fs = new FileStream(Location, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            {
                InternalXmlSerializeDeflate(source, fs);
                fs.SetEndLength();
            }
        }
        public static T UnDeflateXmlDeSerialize<T>(this string Location)
        {
            using (var fs = new FileStream(Location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return InternalUnDeflateXmlDeSerialize<T>(fs);
            }
        }
        #endregion

        #region From Stream
        public static void XmlSerializeDeflate<T>(this T source, Stream stream)
        {
            InternalXmlSerializeDeflate(source, stream);
            stream.SetEndLength();
        }
        public static T UnDeflateXmlDeSerialize<T>(this Stream stream) => InternalUnDeflateXmlDeSerialize<T>(stream);
        #endregion

        #endregion





        #region Dictionary Serializable
        public static List<KeyValuePair<TKey, TValue>> ToSerializableList<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return dictionary.Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value)).ToList();
        }
        public static Dictionary<TKey, TValue> XmlDeSerializeDictionary<TKey, TValue>(this List<KeyValuePair<TKey, TValue>> SerializableList)
        {
            return SerializableList.ToDictionary(x => x.Key, x => x.Value);
        }
        //[Serializable, XmlType(nameof(KeyValuePair<TKey, TValue>))]
        //public class SerializableKeyValuePair<TKey, TValue>
        //{
        //    public TKey Key { get; set; }
        //    public TValue Value { get; set; }
        //}

        //public class SerializableDictionaryList<TKey, TValue> : List<SerializableKeyValuePair<TKey, TValue>>
        //{

        //}
        #endregion
    }

    //public static class XmlSerializationExt
    //{
    //    private static readonly Dictionary<Type, XmlSerializer> XmlSerializers = new Dictionary<Type, XmlSerializer>();

    //    #region Internal
    //    private static XmlSerializer GetSerializer(Type type)
    //    {
    //        if (XmlSerializers.TryGetValue(type, out var match)) return match;
    //        else
    //        {
    //            var res = new XmlSerializer(type);
    //            XmlSerializers.Add(type, res);
    //            return res;
    //        }
    //    }
    //    private static void InternalXmlSerialize<T>(T source, Stream stream)
    //    {
    //        //using (var xmlWriter = XmlWriter.Create(stream))
    //        //{
    //        var xmlSerializer = GetSerializer(typeof(T));
    //        xmlSerializer.Serialize(stream, source);
    //        //}
    //    }
    //    private static T InternalXmlDeSerialize<T>(Stream stream)
    //    {
    //        using (var xmlReader = XmlReader.Create(stream, new XmlReaderSettings() { DtdProcessing = DtdProcessing.Prohibit, }))
    //        {
    //            var xmlSerializer = GetSerializer(typeof(T));
    //            return (T)xmlSerializer.Deserialize(xmlReader);
    //        }
    //    }
    //    #endregion

    //    #region From File
    //    public static void XmlSerialize<T>(this T source, string Location)
    //    {
    //        using (var fs = new FileStream(Location, FileMode.OpenOrCreate))
    //        {
    //            InternalXmlSerialize(source, fs);
    //        }
    //    }
    //    public static T XmlDeSerialize<T>(this string Location)
    //    {
    //        using (var fs = new FileStream(Location, FileMode.Open))
    //        {
    //            return InternalXmlDeSerialize<T>(fs);
    //        }
    //    }
    //    #endregion

    //    #region From Zip File
    //    public static void XmlSerializeAndZip<T>(this T source, string Location)
    //    {
    //        using (var ms = XmlSerialize(source))
    //        {
    //            ZipExt.Zip(ms, Location);
    //        }

    //    }
    //    public static T UnZipAndXmlDeSerialize<T>(this string Location)
    //    {
    //        using (var ms = ZipExt.UnZip(Location))
    //        {
    //            return XmlDeSerialize<T>(ms);
    //        }

    //    }
    //    #endregion

    //    #region From Stream
    //    public static MemoryStream XmlSerialize<T>(this T source)
    //    {
    //        var result = new MemoryStream();

    //        InternalXmlSerialize(source, result);

    //        result.Seek(0, SeekOrigin.Begin);
    //        return result;

    //    }
    //    public static T XmlDeSerialize<T>(this Stream stream)
    //    {
    //        return InternalXmlDeSerialize<T>(stream);
    //    }
    //    #endregion

    //    public static void XmlSerializeDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, string Location)
    //    {
    //        var source = dictionary.Select(x => new SerializableKeyValuePair<TKey, TValue> { Key = x.Key, Value = x.Value }).ToList();
    //        XmlSerialize<List<SerializableKeyValuePair<TKey, TValue>>>(source, Location);
    //    }
    //    public static Dictionary<TKey, TValue> XmlDeSerializeDictionary<TKey, TValue>(this string Location)
    //    {
    //        return XmlDeSerialize<SerializableKeyValuePair<TKey, TValue>[]>(Location).ToDictionary(x => x.Key, x => x.Value);
    //    }

    //    [Serializable, XmlType(nameof(KeyValuePair<TKey, TValue>))]
    //    public class SerializableKeyValuePair<TKey, TValue>
    //    {
    //        public TKey Key { get; set; }
    //        public TValue Value { get; set; }
    //    }
    //}


    //public static class JsonSerializationExt
    //{
    //    private static readonly JsonSerializer jsonSerializer = new JsonSerializer() { MissingMemberHandling = MissingMemberHandling.Ignore, ContractResolver = new WritablePropertiesOnlyResolver(), };

    //    private class WritablePropertiesOnlyResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
    //    {
    //        protected override IList<Newtonsoft.Json.Serialization.JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    //        {
    //            var props = base.CreateProperties(type, memberSerialization);
    //            return props.Where(p => p.Writable || (p.Readable && p.PropertyType != typeof(string) &&
    //            (typeof(System.Collections.IEnumerable).IsAssignableFrom(p.PropertyType) ||
    //            typeof(System.Collections.IDictionary).IsAssignableFrom(p.PropertyType)))).ToList();
    //        }
    //    }


    //    #region Internal
    //    private static void InternalJsonSerialize<T>(T source, Stream stream)
    //    {
    //        using (var sw = CreateStreamWriter(stream, true))
    //        {
    //            using (var writer = new JsonTextWriter(sw) { Formatting = Newtonsoft.Json.Formatting.Indented, })
    //            {
    //                jsonSerializer.Serialize(writer, source);
    //            }
    //        }
    //    }
    //    private static StreamWriter CreateStreamWriter(Stream stream, bool leaveOpen)
    //    {
    //        return new StreamWriter(stream, Encoding.UTF8, 4096, leaveOpen);
    //    }

    //    private static T InternalJsonDeSerialize<T>(Stream stream)
    //    {
    //        using (var sw = new StreamReader(stream))
    //        {
    //            using (var reader = new JsonTextReader(sw))
    //            {
    //                return jsonSerializer.Deserialize<T>(reader);
    //            }
    //        }
    //    }
    //    #endregion

    //    #region From File
    //    public static void JsonSerialize<T>(this T source, string Location)
    //    {
    //        using (var fs = new FileStream(Location, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
    //        {
    //            InternalJsonSerialize(source, fs);
    //        }
    //    }
    //    public static T JsonDeSerialize<T>(this string Location)
    //    {
    //        using (var fs = new FileStream(Location, FileMode.Open))
    //        {
    //            return InternalJsonDeSerialize<T>(fs);
    //        }
    //    }
    //    #endregion

    //    #region From Zip File
    //    public static void JsonSerializeAndZip<T>(this T source, string Location)
    //    {
    //        using (var ms = JsonSerialize(source))
    //        {
    //            ZipExt.Zip(ms, Location);
    //        }

    //    }
    //    public static T UnZipAndJsonDeSerialize<T>(this string Location)
    //    {
    //        using (var ms = ZipExt.UnZip(Location))
    //        {
    //            return JsonDeSerialize<T>(ms);
    //        }

    //    }
    //    #endregion

    //    #region From Stream
    //    public static MemoryStream JsonSerialize<T>(this T source)
    //    {
    //        var result = new MemoryStream();

    //        InternalJsonSerialize(source, result);

    //        result.Seek(0, SeekOrigin.Begin);
    //        return result;

    //    }
    //    public static T JsonDeSerialize<T>(this Stream stream)
    //    {
    //        return InternalJsonDeSerialize<T>(stream);
    //    }
    //    #endregion

    //}

}
