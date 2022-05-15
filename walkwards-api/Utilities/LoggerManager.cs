using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace walkwards_api.Utilities
{
    public static class LoggerManager
    {
        public static async Task WriteLog(string message)
        {
            string log = $"[{DateTime.Now:g}] {message} \n";
            
            //check if folder exists
            if (!Directory.Exists("./logs"))
            {
                Directory.CreateDirectory("./logs");
            }

            await File.AppendAllTextAsync(@$"./logs/log_{DateTime.Now:MM.dd.yyyy}.log", log);
        }
        
        public static async Task<string[]> ReadLogs()
        {
            if (!File.Exists(@$"./logs/log_{DateTime.Now:MM.dd.yyyy}"))
            {
                return new string[0];
            }
            
            string path = $"./logs/log_{DateTime.Now:MM.dd.yyyy}";
            return await File.ReadAllLinesAsync(path);
        }    
    }
}