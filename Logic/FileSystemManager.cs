using System.IO;

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
            string[] files = Directory.GetFiles(path);
            if (files.Length < 1)
                return;

            foreach (string file in files)
            {
                File.Delete(file);
            }
        }
    }
}
