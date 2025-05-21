using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace Shavkat_grabber.Logic;

public class FileSystemManager
{
    private static readonly char[] Letters = "abcdefghjklmnopqrstuwxyz".ToCharArray();

    public void DirClearOrCreateIfNotExist(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        else
        {
            foreach (string file in Directory.GetFiles(path))
            {
                File.Delete(file);
            }
        }
    }

    public async Task StartProcessAndWait(string path, string arguments)
    {
        using Process process = new Process();
        process.StartInfo.FileName = path;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.UseShellExecute = false;
        process.Start();
        await process.WaitForExitAsync();
    }

    public string SaveBitmapInTempAndGetFullPath(Bitmap bmp)
    {
        char[] buff = Random.Shared.GetItems(Letters, 5);
        string path = GetTempDirPathWithFile($"{buff}.png");
        bmp.Save(path);
        return path;
    }

    private string GetTempDirPath()
    {
        return Path.Combine(Environment.CurrentDirectory, "Temp");
    }

    private string GetTempDirPathWithFile(string fileName)
    {
        return Path.Combine(Environment.CurrentDirectory, "Temp", fileName);
    }
}
