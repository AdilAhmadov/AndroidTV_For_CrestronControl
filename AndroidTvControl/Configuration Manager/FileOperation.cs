using Crestron.SimplSharp;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AndroidTvControl.Configuration_Manager
{
    public class FileHandler
    {
        public static void WriteFile(string path, string data)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.Default))
                    {
                        sw.Write(data);
                    }
                }
            }
            catch (Exception e) { ErrorLog.Exception("Error in Writing Text File: ", e); }
        }
        public static string ReadFile(string path)
        {
            string result = "";
            try
            {
                using (StreamReader reader = new StreamReader(path, Encoding.ASCII))
                {
                    result = reader.ReadToEnd();
                }
            }
            catch (Exception e) { ErrorLog.Exception("Error in Reading Text file. Error: ", e); }
            return result;
        }
        public static void StoreFile(string path, string obj)
        {
            try
            {
                using (var sw = new StreamWriter(path, false, Encoding.ASCII))
                {
                    sw.Write(obj);
                }
            }
            catch (Exception ex)
            {
                ErrorLog.Exception("Exception On Storing File: ", ex);
            }
        }
        public static async Task StoreFileAsync(string path, string obj)
        {
            try
            {
                using (var sw = new StreamWriter(path, false))
                {
                    await sw.WriteAsync(obj);
                }
            }
            catch (Exception ex)
            {
                ErrorLog.Exception("Exception On Storing File: ", ex);
            }
        }
    }
}
