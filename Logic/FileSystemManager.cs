using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Shavkat_grabber.Logic;

public class FileSystemManager
{
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
}
