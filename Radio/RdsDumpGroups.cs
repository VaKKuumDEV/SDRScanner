﻿using System;
using System.Linq;
using System.Text;

namespace SDRSharp.Radio
{
    public class RdsDumpGroups
    {
        private StringBuilder _radioTextSB = new StringBuilder("                                                                        ");
        private StringBuilder _programServiceSB = new StringBuilder("                                                                        ");
        private string _radioText = string.Empty;
        private string _programService = string.Empty;
        private ushort _piCode;
        private bool _radioTextABFlag;

        public string RadioText
        {
            get { return _radioText; }
        }

        public string ProgramService
        {
            get { return _programService; }
        }

        public ushort PICode
        {
            get { return _piCode; }
        }

        public void Reset()
        {
            lock (this)
            {
                _radioTextSB = new StringBuilder("                                                                        ");
                _programServiceSB = new StringBuilder("                                                                        ");
                _radioText = string.Empty;
                _programService = string.Empty;
                _piCode = 0;
                _radioTextABFlag = false;
            }
        }

        public bool AnalyseFrames(ushort groupA, ushort groupB, ushort groupC, ushort groupD)
        {
            var result = false;

            //if ((groupB & 0xf800) == 0x4000) // 2a group radio text
            //{
            //    string messageTime = Dump4A(groupB, groupC, groupD);
            //    Console.WriteLine(messageTime);
            //}

            if ((groupB & 0xf800) == 0x2000) // 2a group radio text
            {
                int index = (groupB & 0xf) * 4; // text segment
                var abFlag = ((groupB >> 4) & 0x1) == 1;

                var sb = new StringBuilder();
                sb.Append((char)(groupC >> 8));
                sb.Append((char)(groupC & 0xff));
                sb.Append((char)(groupD >> 8));
                sb.Append((char)(groupD & 0xff));
                if (sb.ToString().Any(ch => (ch < ' ') || (ch > 0x7f)))
                {
                    return false; // ignore garbage
                }

                lock (this)
                {
                    if (abFlag != _radioTextABFlag)
                    {
                        for (var i = 0; i < _radioTextSB.Length; i++)
                        {
                            _radioTextSB[i] = ' ';
                        }
                        _radioTextABFlag = abFlag;
                    }
                    else
                    {
                        _radioTextSB.Remove(index, 4);
                    }                    
                    _radioTextSB.Insert(index, sb.ToString());
                    _radioText = _radioTextSB.ToString().Trim();
                    _piCode = groupA;
                }

                result = true;

                //Console.WriteLine(_radioText.ToString());
            }

            if ((groupB & 0xf800) == 0x0000) // 0a group radio text
            {
                int index = (groupB & 0x3) * 2; // text segment

                var sb = new StringBuilder();

                sb.Append((char)(groupD >> 8));
                sb.Append((char)(groupD & 0xff));
                if (sb.ToString().Any(ch => (ch < ' ') || (ch > 0x7f)))
                {
                    return false; // ignore garbage
                }

                lock (this)
                {
                    _programServiceSB.Remove(index, 2);
                    _programServiceSB.Insert(index, sb.ToString());
                    _programService = _programServiceSB.ToString().Substring(0, 8);
                    _piCode = groupA;
                }

                result = true;

                //Console.WriteLine(_programService.ToString());

                //Console.WriteLine("" + ((groupC >> 8) / 10.0 + 87.5) + " " + ((groupC & 0xff) / 10.0 + 87.5));
            }

            return result;
        }

        private static string Dump4A(ushort blockB, ushort block3, ushort block4)
        {
            var halfHourLocalTimeOffset = block4 & 0x1f;
            if ((block4 & 0x20) != 0)
            {
                halfHourLocalTimeOffset *= -1;
            }

            var minute = (block4 >> 6) & 0x3f;

            var hour = ((block4 >> 12) & 0x0f) | ((block3 << 4) & 0x010);

            int mjd = (block3 >> 1) | ((blockB << 15) & 0x18000);

            var y = (int)((mjd - 15078.2) / 365.25);
            var m = (int)((mjd - 14956.1 - (int)(y * 365.25)) / 30.6001);
            int d = mjd - 14956 - (int)(y * 365.25) - (int)(m * 30.6001);
            int k = 0;
            if ((m == 14) || (m == 15))
            {
                k = 1;
            }
            y = y + k + 1900;
            m = m - 1 - k * 12;
            try
            {
                var dt = new DateTime(y, m, d, hour, minute, 0);
                var ts = new TimeSpan(halfHourLocalTimeOffset / 2, (halfHourLocalTimeOffset * 30 % 60), 0);
                dt = dt + ts;
                return "4A " + dt.ToLongDateString() + " " + dt.ToLongTimeString();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
