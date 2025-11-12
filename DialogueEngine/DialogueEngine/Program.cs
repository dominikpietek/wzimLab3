using DialogueEngine;
using DTOModel;
using Newtonsoft.Json;
using System;
using System.IO.Pipes;
using System.Reflection;
using File = System.IO.File;

namespace DialogueEngineApp
{
    internal class Program
    {
        private const string PIPENAME = "DialogueEnginePipe";
        private static readonly string _errorLogPath = AppDomain.CurrentDomain.BaseDirectory + "/error_log.txt";
        private static readonly DialogueEngine.DialogueEngine _dialogueEngine = new DialogueEngine.DialogueEngine();

        public static async Task Main(string[] args)
        {
            while (true)
            {
                using var server = new NamedPipeServerStream(
                    PIPENAME,
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await server.WaitForConnectionAsync();

                var reader = new StreamReader(server);
                var writer = new StreamWriter(server)
                {
                    AutoFlush = true
                };

                while (server.IsConnected)
                {
                    string? line = await reader.ReadLineAsync();

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string response = string.Empty;
                    try
                    {
                        response = HandleCommand(line);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(_errorLogPath, ex.Message + '\n');
                    }

                    if (string.IsNullOrWhiteSpace(response) == false)
                    {
                        await writer.WriteLineAsync(response);
                    }
                }
            }
        }

        private static string HandleCommand(string json)
        {
            if (string.IsNullOrWhiteSpace(json) == true)
            {
                File.AppendAllText(_errorLogPath, "Empty method JSON!\n");
            }
           
            MethodDTO methodDTO = JsonConvert.DeserializeObject<MethodDTO>(json);

            if (methodDTO == null)
            {
                File.AppendAllText(_errorLogPath, "Wrong method JSON!\n");
            }

            MethodInfo method = typeof(DialogueEngine.DialogueEngine).GetMethod(methodDTO.MethodName);

            if (method == null)
            {
                File.AppendAllText(_errorLogPath, $"No method called: {methodDTO.MethodName}!\n");
            }

           object response = method.Invoke(_dialogueEngine, null);

            return response.ToString();
        }
    }
}
