using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace TextViewer
{
    internal class TextPreview
    {
        public List<string> previewableExtensions = [ ".txt", ".cs", ".csv" ];
        public string filePath = "";
        string[] lines = [];
        bool loadFile = false;
        //bool fileLoaded = false;

        internal void LoadAllowableFileTypeConfig(string configPath)
        {
            string[] types;
            if (File.Exists(configPath))
            {
                Debug.WriteLine($"Loading previewable file type config, not found {configPath}");
                try
                {
                    types = File.ReadAllLines(configPath);
                    previewableExtensions = types.ToList();
                    Debug.WriteLine($"Loaded previewable file types. Count {previewableExtensions.Count}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Couldn't load file with previewable preview file types: {configPath}\n{ex.Message}");
                }
            }
            else
            {
                Debug.WriteLine($"Can't load previewable file type config, not found {configPath}");
            }
        }

        public bool LoadFile(string loadFilePath, bool force = false)
        {
            loadFile = false;
            //fileLoaded = false;
            if ((filePath == loadFilePath) && force == false)
            {
                Debug.WriteLine($"Skipping reload of file, already loaded");
                return false;
            }
            filePath = loadFilePath;
            if (filePath != "" && File.Exists(filePath))
            {
                string fileExt = Path.GetExtension(filePath).ToLowerInvariant();
                foreach (string allowedExtension in previewableExtensions)
                {
                    if (allowedExtension == fileExt)
                    {
                        loadFile = true;
                        break;
                    }
                }
            }

            if (loadFile == false || filePath == "")
            {
                filePath = "";
                lines = [];
                return false;
            }
            else
            {
                try
                {
                    lines = File.ReadAllLines(filePath);
                    //fileLoaded = true;
                    Debug.WriteLine($"Loaded preview file {filePath}");
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error reading preview file\n{ex.Message}");
                }
            }
            return false;
        }

        internal void PreviewText(int left, int top, int width, int height)
        {
            int addY = 0;
            
            Console.SetCursorPosition(left, top + addY++);
            Console.Write("┏━[P] Text Preview ".PadRight(width-1, '━'));
            Console.Write("┓");
            Console.SetCursorPosition(left, top + addY);

            RenderLines(lines, left, top, width, height, addY);

            Console.SetCursorPosition(left, top + height-1);
            Console.Write("┗━".PadRight(width-1, '━'));
            Console.Write("┛");
            //┏━┓
            //┃ ┃
            //┗━┛
        }

        private void RenderLines(string[] lines, int left, int top, int width, int height, int addY)
        {
            for (int i = 0; i < height - 2; i++)
            {
                string line = "";
                if (i < lines.Length)
                {
                    line = lines[i];
                }
                Console.SetCursorPosition(left, top + addY++);
                int maxLength = Math.Min(width - 6, line.Length);
                if (maxLength < 0) maxLength = 0;
                Console.Write("┃ " + line[..maxLength].Replace("\t", "   ").PadRight(width-4,' ') + " ┃");
            }
        }
    }
}
