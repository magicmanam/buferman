using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Windows
{
    public class KeyboardEmulator
    {
        private const string SPECIAL_CHARS = "+^%[]{}~";
        private const string CTRL_CODE = "^";
        private const string SHIFT_CODE = "+";
        private const string ALT_CODE = "%";

        private string _ctrlShiftAltState = "";
        private bool _isCtrlHold = false;
        private bool _isShiftHold = false;
        private bool _isAltHold = false;

        public KeyboardEmulator()
        {
            this.Wait();
        }

        public KeyboardEmulator PressTab(uint count = 1)
        {
            return this.PressKey(KeyCodes.TAB, count);
        }

        public KeyboardEmulator PressKey(KeyCodes keyCode, uint count = 1)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Must be more than zero");
            }

            this.SendKeyboardKeys($"{{{keyCode.ToString()} {count}}}");

            return this;
        }

        public KeyboardEmulator PressEnter(uint count = 1)
        {
            return this.PressKey(KeyCodes.ENTER, count);
        }

        public KeyboardEmulator TypeText(string keysCombination)
        {
            this.Wait();

            var currentLanguage = InputLanguage.CurrentInputLanguage;
            InputLanguage.CurrentInputLanguage = InputLanguage.FromCulture(new CultureInfo("en-US"));//This culture should be calculated automatically here and in other place.

            foreach (var escapedChar in KeyboardEmulator._ReplaceSpecialSendKeysCharacters(keysCombination))
            {
                this.SendKeyboardKeys(escapedChar);
            }

            InputLanguage.CurrentInputLanguage = currentLanguage;

            return this;
        }

        public KeyboardEmulator HoldDownCtrl()
        {
            if (this._isCtrlHold)
            {
                throw new InvalidOperationException("Ctrl key is already held down!");
            }

            this._isCtrlHold = true;
            this._rebuildCtrlShiftAltState();

            return this;
        }

        private void _rebuildCtrlShiftAltState()
        {
            this._ctrlShiftAltState = string.Empty;

            if (this._isCtrlHold) { this._ctrlShiftAltState += KeyboardEmulator.CTRL_CODE; }
            if (this._isShiftHold) { this._ctrlShiftAltState += KeyboardEmulator.SHIFT_CODE; }
            if (this._isAltHold) { this._ctrlShiftAltState += KeyboardEmulator.ALT_CODE; }
        }

        public KeyboardEmulator HoldUpCtrl()
        {
            if (!this._isCtrlHold)
            {
                throw new InvalidOperationException("Ctrl key is not held down!");
            }

            this._isCtrlHold = false;
            this._rebuildCtrlShiftAltState();

            return this;
        }

        public KeyboardEmulator HoldDownShift()
        {
            if (this._isShiftHold)
            {
                throw new InvalidOperationException("Shift key is already held down!");
            }

            this._isShiftHold = true;
            this._rebuildCtrlShiftAltState();

            return this;
        }

        public KeyboardEmulator HoldUpShift()
        {
            if (!this._isShiftHold)
            {
                throw new InvalidOperationException("Shift key is not held down!");
            }
            this._isShiftHold = false;
            this._rebuildCtrlShiftAltState();

            return this;
        }

        public KeyboardEmulator HoldDownAlt()
        {
            if (this._isAltHold)
            {
                throw new InvalidOperationException("Alt key is already held down!");
            }
            this._isAltHold = true;
            this._rebuildCtrlShiftAltState();

            return this;
        }

        public KeyboardEmulator HoldUpAlt()
        {
            if (!this._isAltHold)
            {
                throw new InvalidOperationException("Alt key is not held down!");
            }
            this._isAltHold = false;
            this._rebuildCtrlShiftAltState();

            return this;
        }

        public KeyboardEmulator SendKeyboardKeys(string keys, bool withWaiting = true)
        {
            if (!string.IsNullOrEmpty(this._ctrlShiftAltState))
            {
                keys = $"{this._ctrlShiftAltState}({keys})";
            }

            if (withWaiting)
            {
                SendKeys.SendWait(keys);
            } else
            {
                SendKeys.Send(keys);
            }

            return this;
        }

        public KeyboardEmulator Wait()
        {
            SendKeys.Flush();
            return this;
        }

        private static IEnumerable<string> _ReplaceSpecialSendKeysCharacters(string text)
        {
            return text.ToCharArray().Select(c => KeyboardEmulator.SPECIAL_CHARS.Contains(c) ? $"{{{c}}}" : c.ToString());
        }
    }
}