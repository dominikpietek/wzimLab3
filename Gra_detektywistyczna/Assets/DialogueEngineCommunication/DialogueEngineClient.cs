using DTOModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class DialogueEngineClient : IDisposable
{
    private readonly string _exePath;
    private Process _process;
    private NamedPipeClientStream _pipe;
    private StreamReader _reader;
    private StreamWriter _writer;
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    private const string PIPENAME = "DialogueEnginePipe";

    public DialogueEngineClient(string exePath)
    {
        _exePath = exePath;
    }

    public async Task InitializeAsync()
    {
        StartProcess();

        _pipe = new NamedPipeClientStream(
            ".",
            PIPENAME,
            PipeDirection.InOut,
            PipeOptions.Asynchronous);

        await _pipe.ConnectAsync();

        _reader = new StreamReader(_pipe);
        _writer = new StreamWriter(_pipe)
        {
            AutoFlush = true
        };
    }

    private void StartProcess()
    {
        if (File.Exists(_exePath) == false)
        {
            Debug.LogError($"Nie znaleziono pliku: {_exePath}");
            return;
        }

        ProcessStartInfo processStartInfo = new ProcessStartInfo
        {
            FileName = _exePath,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        _process = Process.Start(processStartInfo);
        if (_process == null)
        {
            Debug.LogError("Nie udalo sie wystartowac DialogueEngine!");
        }
    }

    private async Task<string> SendCommandAsync(string line)
    {
        await _sendLock.WaitAsync();
        try
        {
            if (_writer == null || _reader == null)
                throw new InvalidOperationException("Brak writer'a lub reader'a!");

            await _writer.WriteLineAsync(line);
            string response = await _reader.ReadLineAsync();
            return response;
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public async Task<GamesToContinueDTO> GetGamesToContinueAsync()
    {
        MethodDTO methodDTO = new MethodDTO() { MethodName = "GetGamesToContinue" };
        string serializedMethodDTO = JsonUtility.ToJson(methodDTO);

        string response = await SendCommandAsync(serializedMethodDTO);

        object responseJSON = JsonUtility.FromJson(response, typeof(GamesToContinueDTO));
        GamesToContinueDTO convertedResponse = responseJSON as GamesToContinueDTO;

        return convertedResponse;
    }

    public void Dispose()
    {
        try
        {
            _reader?.Dispose();
            _writer?.Dispose();
            _pipe?.Dispose();
            _sendLock?.Dispose();
        }
        catch { }

        ForceKill();
    }

    public void ForceKill()
    {
        try
        {
            if (_process != null && !_process.HasExited)
                _process.Kill();
        }
        catch { }
    }
}
