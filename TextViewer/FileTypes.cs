using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextViewer
{
    public class FileTypes
    {
        public enum ContentTypes
        {
            Text,
            Image,
            Binary,
            Video,
            Unknown
        }

        private static List<string> _textExtensions = [".txt", ".cs", ".csv"];
        private static List<string> _imageExtensions = [".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tif", ".tiff"];

        public static ContentTypes GetContentType(string extension)
        {
            if (IsTextType(extension)) return ContentTypes.Text;
            if (IsImageType(extension)) return ContentTypes.Image;

            return ContentTypes.Unknown;
        }

        public static ReadOnlyCollection<string> TextExtensions
        {
            get
            {
                return _textExtensions.AsReadOnly();
            }
        }

        public static ReadOnlyCollection<string> ImageExtensions
        {
            get
            {
                return _imageExtensions.AsReadOnly();
            }
        }

        public static void AddTextTypes(IEnumerable<string> extensions)
        {
            foreach (string extension in extensions)
            {
                if (_textExtensions.Contains(extension) == false)
                {
                    _textExtensions.Add(extension);
                }
            }
        }

        public static void AddImageTypes(IEnumerable<string> extensions)
        {
            foreach (string extension in extensions)
            {
                if (_imageExtensions.Contains(extension) == false)
                {
                    _imageExtensions.Add(extension);
                }
            }
        }

        public static bool IsTextType(string extension)
        {
            return _textExtensions.Contains(extension.ToLower());
        }

        public static bool IsImageType(string extension)
        {
            return _imageExtensions.Contains(extension.ToLower());
        }

        //private List<string> ExpandedTextTypes;
        //private List<string> ExpandedImageTypes;

        //public static readonly FileTypes Instance = new();

        //public FileTypes()
        //{
        //    Instance.ExpandedTextTypes = new(TextExtensions);

        //    Instance.ExpandedImageTypes = new(ImageExtensions);
        //}

        //public void AddTextTypes(List<string> newTypes)
        //{
        //    Instance.ExpandedTextTypes.AddRange(newTypes);
        //}
    }
}
