using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TextViewer
{
    public static class Input
    {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT point);

        public struct POINT
        {
            public int x;
            public int y;
        }
        public static POINT GetMousePositionPoint()
        {
            POINT pos;
            GetCursorPos(out pos);
            //Debug.WriteLine($"--mouse {pos.x} {pos.y}");
            return pos;
        }

        public static (int x, int y) GetMousePosition()
        {
            POINT pos;
            GetCursorPos(out pos);
            return (pos.x, pos.y);
        }

        [DllImport("user32.dll")]
        public static extern bool GetAsyncKeyState(int button);
        public static bool IsMouseButtonPressed(MouseButton button)
        {
            return GetAsyncKeyState((int)button);
        }
        public enum MouseButton
        {
            LeftMouseButton = 0x01,
            RightMouseButton = 0x02,
            MiddleMouseButton = 0x04,
        }
    }
}
