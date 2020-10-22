using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace Deserialize
{
    class Program

    {     
        [Serializable]
        public class AsyncUploadConfiguration : IAsyncUploadConfiguration
        {

            public string TargetFolder { get; set; }
            public string TempTargetFolder { get; set; }
            public int MaxFileSize { get; set; }
            public TimeSpan TimeToLive { get; set; }
            public bool UseApplicationPoolImpersonation { get; set; }
            public string[] AllowedFileExtensions { get; set; }
            public AsyncUploadConfiguration()
            {
            }
            public AsyncUploadConfiguration(string targetFolder, string tempTargetFolder, int maxFileSize, TimeSpan timeToLive)
            {
                this.TargetFolder = targetFolder;
                this.TempTargetFolder = tempTargetFolder;
                this.MaxFileSize = maxFileSize;
                this.TimeToLive = timeToLive;
            }
        }
        internal class AsyncUploadConfigurationConverter : JavaScriptConverter
        {
            public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
            {
                IDictionary<string, object> dictionary2 = dictionary["TimeToLive"] as IDictionary<string, object>;
                IAsyncUploadConfiguration asyncUploadConfiguration = Activator.CreateInstance(type) as IAsyncUploadConfiguration;
                long value = Convert.ToInt64(dictionary2["Ticks"]);
                asyncUploadConfiguration.TimeToLive = TimeSpan.FromTicks(value);
                MethodInfo methodInfo = typeof(JavaScriptSerializer).GetMethod("ConvertToType", new Type[]
                {
                typeof(object)
                }, null).MakeGenericMethod(new Type[]
                {
                type
                });
                object obj = methodInfo.Invoke(new JavaScriptSerializer(), new object[]
                {
                dictionary
                });
                return this.MergeDefaultConfiguration(asyncUploadConfiguration, (IAsyncUploadConfiguration)obj);
            }
            private object MergeDefaultConfiguration(IAsyncUploadConfiguration config, IAsyncUploadConfiguration customObject)
            {
                customObject.TimeToLive = config.TimeToLive;
                return customObject;
            }
            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
            {
                throw new NotImplementedException();
            }
            public override IEnumerable<Type> SupportedTypes
            {
                get
                {
                    return new Type[]
                    {
                    typeof(AsyncUploadConfiguration)
                    };
                }
            }
        }
        internal static object Deserialize(string obj, Type type)
        {
            JavaScriptSerializer serializer = GetSerializer(obj.Length);
            ApplyConverters(type, serializer);
            MethodInfo methodInfo = typeof(JavaScriptSerializer).GetMethod("Deserialize", new Type[]
            {
                typeof(string)
            }, null).MakeGenericMethod(new Type[]
            {
                type
            });
            return methodInfo.Invoke(serializer, new object[]
            {
                obj
            });
        }
        private static string GetEncryptionKey()
        {
            string text = ConfigurationManager.AppSettings.Get(customEncryptionKey);
            if (text != null)
            {
                return text;
            }
            return defaultEncryptionKey;
        }
        private static readonly string customEncryptionKey = "Telerik.AsyncUpload.ConfigurationEncryptionKey";
        private static readonly string defaultEncryptionKey = "PrivateKeyForEncryptionOfRadAsyncUploadConfiguration";
        private static byte[] Decrypt(byte[] encryptedBytes, byte[] key, byte[] iv)
        {
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, new AesCryptoServiceProvider
            {
                Key = key,
                IV = iv
            }.CreateDecryptor(), (CryptoStreamMode)1);
            cryptoStream.Write(encryptedBytes, 0, encryptedBytes.Length);
            cryptoStream.Close();
            return memoryStream.ToArray();
        }
        internal static string Decrypt(string encryptedString, string password)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedString);
            byte[] array = new byte[]
            {
                58,
                84,
                91,
                25,
                10,
                34,
                29,
                68,
                60,
                88,
                44,
                51,
                1
            };
            PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(password, array);
            byte[] bytes = Decrypt(encryptedBytes, passwordDeriveBytes.GetBytes(32), passwordDeriveBytes.GetBytes(16));
            return Encoding.Unicode.GetString(bytes);
        }
        public static string Decrypt(string encryptedString)
        {
            string encryptionKey = GetEncryptionKey();
            return Decrypt(encryptedString, encryptionKey);
        }
        internal static object Deserialize(string obj, Type type, bool decrypt)
        {
            if (decrypt)
            {
                obj = Decrypt(obj);
            }
            return Deserialize(obj, type);
        }
        internal static JavaScriptSerializer GetSerializer(int maxJsonLength)
        {
            return new JavaScriptSerializer
            {
                MaxJsonLength = maxJsonLength
            };
        }
        private static void ApplyConverters(Type type, JavaScriptSerializer serializer)
        {
            if (type.GetInterface(typeof(IAsyncUploadConfiguration).FullName) != null)
            {
                serializer.RegisterConverters(new AsyncUploadConfigurationConverter[]
                {
                    new AsyncUploadConfigurationConverter()
                });
            }
        }
        static void Main(string[] args)
        {

            var map = new Dictionary<string, string>();
            map.Add("__type", "System.Configuration.Install.AssemblyInstaller, System.Configuration.Install, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            string resultType = map["__type"];
            var obj = Console.ReadLine();

            Type type = Type.GetType(resultType);
            Deserialize(obj, type, false);
            Console.ReadKey(); 
        }
    }
}
