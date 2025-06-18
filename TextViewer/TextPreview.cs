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
        public List<string> previewableTextExtensions = [ ".txt", ".cs", ".csv" ];
        public List<string> previewableImageExtensions = [".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tif", ".tiff"];
        public string filePath = "";
        string[] lines = [];
        public bool FileIsText = false;
        public bool FileIsImage = false;
        bool previewImages = true;
        ImageParser? imageParser = null;
        //bool fileLoaded = false;

        internal void LoadAllowableFileTypeConfig(string configPath)
        {
            string[] types;
            if (File.Exists(configPath))
            {
                Debug.WriteLine($"Loading previewable file type config: {configPath}");
                try
                {
                    types = File.ReadAllLines(configPath);
                    previewableTextExtensions = types.ToList();
                    Debug.WriteLine($"Loaded previewable file types. Count {previewableTextExtensions.Count}");
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
            Debug.WriteLine($"Loading file for preview {loadFilePath}");
            
            //fileLoaded = false;
            if ((filePath == loadFilePath) && force == false)
            {
                Debug.WriteLine($"Skipping reload of file, already loaded");
                return false;
            }
            FileIsText = false;
            FileIsImage = false;

            filePath = loadFilePath;
            if (filePath != "" && File.Exists(filePath))
            {
                string fileExt = Path.GetExtension(filePath).ToLowerInvariant();
                
                foreach (string textExtension in previewableTextExtensions)
                {
                    if (textExtension == fileExt)
                    {
                        FileIsText = true;
                        break;
                    }
                }

                if (previewImages && FileIsText == false)
                {
                    foreach (string imageExtension in previewableImageExtensions)
                    {
                        if (imageExtension == fileExt)
                        {
                            FileIsImage = true;
                        }
                    }
                }

            }

            if (filePath == "")
            {
                lines = [];
                return false;
            }
            else if (FileIsText)
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
            else if (FileIsImage)
            {
                imageParser = new(filePath);
                lines = [];
                return true;
            }
            else
            {
                filePath = "";
                lines = [];
                return false;
            }
            return false;
        }

        internal void PreviewFile(int left, int top, int width, int height)
        {
            int addY = 0;
            
            Console.SetCursorPosition(left, top + addY++);
            Console.Write("┏━[P] Preview ".PadRight(width-1, '━'));
            Console.Write("┓");
            

            if (FileIsText)
            {
                Debug.WriteLine($"text preview");
                Console.SetCursorPosition(left, top + addY);
                RenderLines(lines, left, top, width, height, addY);
            }
            else if (FileIsImage)
            {
                Debug.WriteLine($"image preview");
                if (imageParser != null)
                {
                    imageParser.WriteImageToConsole(left + 1, top + 1, width - 2, height - 2, true);
                }
            }
            else
            {
                Debug.WriteLine($"Not a previewable format");
            }

            Console.SetCursorPosition(left, top + height - 1);
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
