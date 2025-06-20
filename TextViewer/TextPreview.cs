using System.Diagnostics;

namespace TextViewer
{
    internal class TextPreview
    {
        //public List<string> previewableTextExtensions = [ ".txt", ".cs", ".csv" ];
        //public List<string> previewableImageExtensions = [".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tif", ".tiff"];
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
                    FileTypes.AddTextTypes(types);
                    Debug.WriteLine($"Loaded previewable file types. Count {FileTypes.TextExtensions.Count}");
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

                foreach (string textExtension in FileTypes.TextExtensions)
                {
                    if (textExtension == fileExt)
                    {
                        FileIsText = true;
                        break;
                    }
                }

                if (previewImages && FileIsText == false)
                {
                    foreach (string imageExtension in FileTypes.ImageExtensions)
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
            Console.SetCursorPosition(left, top);
            Console.Write("┏━[P] Preview ".PadRight(width - 1, '━'));
            Console.Write("┓");

            for (int i = 1; i < height - 1; i++)
            {
                Console.SetCursorPosition(left, top + i);
                Console.Write("┃".PadRight(width-1,' ') + "┃");
            }


            if (FileIsText)
            {
                Debug.WriteLine($"text preview");
                Console.SetCursorPosition(left, top);
                RenderLines(lines, left+1, top + 1, width-2, height-2);
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
            Console.Write("┗━".PadRight(width - 1, '━'));
            Console.Write("┛");
            //┏━┓
            //┃ ┃
            //┗━┛
        }

        private void RenderLines(string[] lines, int left, int top, int width, int height)
        {
            for (int i = 0; i < height - 2; i++)
            {
                string line = "";
                if (i < lines.Length)
                {
                    line = lines[i];
                }
                Console.SetCursorPosition(left, top + i);
                int maxLength = Math.Min(width, line.Length);
                if (maxLength < 0) maxLength = 0;
                Console.Write(line[..maxLength].Replace("\t", "   ").PadRight(width, ' '));
            }
        }
    }
}
