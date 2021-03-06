﻿using System;
using System.Text;

namespace Encryption_DES.DES
{
    class ProcessDES : CommonProcess
    {
        public ProcessDES(Form1.ProgressEventHandler IncrementProgress, Form1.ProgressInitHandler InitProgress)
        {
            this.IncrementProgress = IncrementProgress;
            this.InitProgress = InitProgress;
        }

        public event Form1.ProgressInitHandler InitProgress;
        public event Form1.ProgressEventHandler IncrementProgress;

        protected virtual void OnIncrementProgress(ProgressEventArgs e)
        {
            if (IncrementProgress != null)
                IncrementProgress(this, e);
        }

        protected virtual void OnInitProgress(ProgressInitArgs e)
        {
            if (InitProgress != null)
                InitProgress(this, e);
        }

        public override string EncryptionStart(string text, string key, bool IsTextBinary)
        {
            string hex_key = this.FromTextToHex(key);
            string binary_key = this.FromHexToBinary(hex_key);
            string key_plus = this.DoPermutation(binary_key, DESData.pc_1);

            string C0 = "", D0 = "";

            C0 = this.SetLeftHalvesKey(key_plus);
            D0 = this.SetRightHalvesKey(key_plus);

            Keys keys = this.SetAllKeys(C0, D0);

            string binaryText = "";

            if (IsTextBinary == false)
            {
                binaryText = this.FromTextToBinary(text);
            }
            else
            {
                binaryText = text;
            }

            binaryText = this.setTextMutipleOf64Bits(binaryText);

            OnInitProgress(new ProgressInitArgs(binaryText.Length));

            StringBuilder EncryptedTextBuilder = new StringBuilder(binaryText.Length);

            for (int i = 0; i < (binaryText.Length / 64); i++)
            {
                string PermutatedText = this.DoPermutation(binaryText.Substring(i * 64, 64), DESData.ip);

                string L0 = "", R0 = "";

                L0 = this.SetLeftHalvesKey(PermutatedText);
                R0 = this.SetRightHalvesKey(PermutatedText);

                string FinalText = this.FinalEncription(L0, R0, keys, false);

                EncryptedTextBuilder.Append(FinalText);

                OnIncrementProgress(new ProgressEventArgs(FinalText.Length));
            }

            return EncryptedTextBuilder.ToString();
        }

        public override string DecryptionStart(string text, string key, bool IsTextBinary)
        {
            string hex_key = this.FromTextToHex(key);
            string binary_key = this.FromHexToBinary(hex_key);
            string key_plus = this.DoPermutation(binary_key, DESData.pc_1);

            string C0 = "", D0 = "";

            C0 = this.SetLeftHalvesKey(key_plus);
            D0 = this.SetRightHalvesKey(key_plus);

            Keys keys = this.SetAllKeys(C0, D0);

            string binaryText = "";

            if (IsTextBinary == false)
            {
                binaryText = this.FromTextToBinary(text);
            }
            else
            {
                binaryText = text;
            }

            binaryText = this.setTextMutipleOf64Bits(binaryText);

            OnInitProgress(new ProgressInitArgs(binaryText.Length));

            StringBuilder DecryptedTextBuilder = new StringBuilder(binaryText.Length);

            for (int i = 0; i < (binaryText.Length / 64); i++)
            {
                string PermutatedText = this.DoPermutation(binaryText.Substring(i * 64, 64), DESData.ip);

                string L0 = "", R0 = "";

                L0 = this.SetLeftHalvesKey(PermutatedText);
                R0 = this.SetRightHalvesKey(PermutatedText);

                string FinalText = this.FinalEncription(L0, R0, keys, true);

                if ((i * 64 + 64) == binaryText.Length)
                {
                    StringBuilder last_text = new StringBuilder(FinalText.TrimEnd('0'));

                    int count = FinalText.Length - last_text.Length;

                    if ((count % 8) != 0)
                    {
                        count = 8 - (count % 8);
                    }

                    string append_text = "";

                    for (int k = 0; k < count; k++)
                    {
                        append_text += "0";
                    }

                    DecryptedTextBuilder.Append(last_text.ToString() + append_text);
                }
                else
                {
                    DecryptedTextBuilder.Append(FinalText);
                }

                OnIncrementProgress(new ProgressEventArgs(FinalText.Length));
            }

            return DecryptedTextBuilder.ToString();
        }

        public bool IsKorean(char word)
        {
            if (word >= '\xAC00' && word <= '\xD7AF')
            {
                return true;
            }

            if (word >= '\x3130' && word <= '\x318F')
            {
                return true;
            }

            return false;
        }

        public string FromTextToHex(string text)
        {
            string hexstring = "";

            foreach (char word in text)
            {
                hexstring += String.Format("{0:X}", Convert.ToInt32(word));
            }

            return hexstring;
        }

        public string FromHexToText(string hexstring)
        {
            StringBuilder text = new StringBuilder(hexstring.Length / 2);

            for (int i = 0; i < (hexstring.Length / 2); i++)
            {
                string word = hexstring.Substring(i * 2, 2);
                text.Append((char)Convert.ToInt32(word, 16));
            }
            return text.ToString();
        }

        public string FromBinaryToText(string binarystring)
        {
            StringBuilder text = new StringBuilder(binarystring.Length / 8);

            for (int i = 0; i < (binarystring.Length / 8); i++)
            {
                string word = binarystring.Substring(i * 8, 8);
                text.Append((char)Convert.ToInt32(word, 2));
            }

            return text.ToString();
        }

        public string setTextMutipleOf64Bits(string text)
        {
            if ((text.Length % 64) != 0)
            {
                int maxLength = 0;
                maxLength = ((text.Length / 64) + 1) * 64;
                text = text.PadRight(maxLength, '0');
            }
            return text;
        }

        public string FromTextToBinary(string text)
        {
            StringBuilder binarystring = new StringBuilder(text.Length * 8);

            foreach (char word in text)
            {
                int binary = (int)word;
                int factor = 128;

                for (int i = 0; i < 8; i++)
                {
                    if (binary >= factor)
                    {
                        binary -= factor;
                        binarystring.Append("1");
                    }
                    else
                    {
                        binarystring.Append("0");
                    }
                    factor /= 2;
                }
            }

            return binarystring.ToString();
        }

        public static string FromDeciamlToBinary(int binary)
        {
            if (binary < 0)
            {
                Console.WriteLine("It requires a integer greater than 0.");
                return null;
            }

            string binarystring = "";
            int factor = 128;

            for (int i = 0; i < 8; i++)
            {
                if (binary >= factor)
                {
                    binary -= factor;
                    binarystring += "1";
                }
                else
                {
                    binarystring += "0";
                }
                factor /= 2;
            }

            return binarystring;
        }

        public static byte FromBinaryToByte(string binary)
        {
            byte value = 0;
            int factor = 128;

            for (int i = 0; i < 8; i++)
            {
                if (binary[i] == '1')
                {
                    value += (byte)factor;
                }

                factor /= 2;
            }

            return value;
        }

        public string FromHexToBinary(string hexstring)
        {
            string binarystring = "";

            try
            {
                for (int i = 0; i < hexstring.Length; i++)
                {
                    int hex = Convert.ToInt32(hexstring[i].ToString(), 16);

                    int factor = 8;

                    for (int j = 0; j < 4; j++)
                    {
                        if (hex >= factor)
                        {
                            hex -= factor;
                            binarystring += "1";
                        }
                        else
                        {
                            binarystring += "0";
                        }
                        factor /= 2;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " - wrong hexa integer format.");
            }

            return binarystring;
        }

        public string DoPermutation(string text, int[] order)
        {
            StringBuilder PermutatedText = new StringBuilder(order.Length);

            for (int i = 0; i < order.Length; i++)
            {
                PermutatedText.Append(text[order[i] - 1]);
            }

            return PermutatedText.ToString();
        }

        public string DoPermutation(string text, int[,] order)
        {
            string PermutatedText = "";

            int rowIndex = Convert.ToInt32(text[0].ToString() + text[text.Length - 1].ToString(), 2);
            int colIndex = Convert.ToInt32(text.Substring(1, 4), 2);

            PermutatedText = ProcessDES.FromDeciamlToBinary(order[rowIndex, colIndex]);

            return PermutatedText;
        }

        public string SetLeftHalvesKey(string text)
        {
            return this.SetHalvesKey(true, text);
        }

        public string SetRightHalvesKey(string text)
        {
            return this.SetHalvesKey(false, text);
        }

        public string SetHalvesKey(bool IsLeft, string text)
        {
            if ((text.Length % 8) != 0)
            {
                Console.WriteLine("The key is not multiple of 8bit.");
                return null;
            }

            int midindex = (text.Length / 2) - 1;

            string result = "";

            if (IsLeft)
            {
                result = text.Substring(0, midindex + 1);
            }
            else
            {
                result = text.Substring(midindex + 1);
            }

            return result;
        }

        public string LeftShift(string text)
        {
            return this.LeftShift(text, 1);
        }

        public string LeftShift(string text, int count)
        {
            if (count < 1)
            {
                Console.WriteLine("The count of leftshift is must more than 1 time.");
                return null;
            }

            string temp = text.Substring(0, count);
            StringBuilder shifted = new StringBuilder(text.Length);
            shifted.Append(text.Substring(count) + temp);

            return shifted.ToString();
        }

        public Keys SetAllKeys(string C0, string D0)
        {
            Keys keys = new Keys();
            keys.Cn[0] = C0;
            keys.Dn[0] = D0;

            for (int i = 1; i < keys.Cn.Length; i++)
            {
                keys.Cn[i] = this.LeftShift(keys.Cn[i - 1], DESData.nrOfShifts[i]);
                keys.Dn[i] = this.LeftShift(keys.Dn[i - 1], DESData.nrOfShifts[i]);
                keys.Kn[i - 1] = this.DoPermutation(keys.Cn[i] + keys.Dn[i], DESData.pc_2);
            }

            return keys;
        }

        public string FinalEncription(string L0, string R0, Keys keys, bool IsReverse)
        {
            string Ln = "", Rn = "", Ln_1 = L0, Rn_1 = R0;

            int i = 0;

            if (IsReverse == true)
            {
                i = 15;
            }

            while (this.IsEnough(i, IsReverse))
            {
                Ln = Rn_1;
                Rn = this.XOR(Ln_1, this.f(Rn_1, keys.Kn[i]));

                //Next Step of L1, R1 is L2 = R1, R2 = L1 + f(R1, K2), hence, value of Step1's Ln, Rn is Rn_1, Ln_1 in Step2.
                Ln_1 = Ln;
                Rn_1 = Rn;

                if (IsReverse == false)
                {
                    i += 1;
                }
                else
                {
                    i -= 1;
                }
            }

            string R16L16 = Rn + Ln;

            string Encripted_Text = this.DoPermutation(R16L16, DESData.ip_1);

            return Encripted_Text;
        }

        public bool IsEnough(int i, bool IsReverse)
        {
            return (IsReverse == false) ? i < 16 : i >= 0;
        }

        public string f(string Rn_1, string Kn)
        {
            string E_Rn_1 = this.E_Selection(Rn_1);

            string XOR_Rn_1_Kn = this.XOR(E_Rn_1, Kn);

            string sBoxedText = this.sBox_Transform(XOR_Rn_1_Kn);

            string P_sBoxedText = this.P(sBoxedText);

            return P_sBoxedText;
        }

        public string P(string text)
        {
            string PermutatedText = "";

            PermutatedText = this.DoPermutation(text, DESData.pc_p);

            return PermutatedText;
        }

        public string sBox_Transform(string text)
        {
            StringBuilder TransformedText = new StringBuilder(32);

            for (int i = 0; i < 8; i++)
            {
                string temp = text.Substring(i * 6, 6);
                TransformedText.Append(this.DoPermutation(temp, DESData.sBoxes[i]));
            }

            return TransformedText.ToString();
        }

        public string E_Selection(string Rn_1)
        {
            string ExpandedText = this.DoPermutation(Rn_1, DESData.pc_e);

            return ExpandedText;
        }

        public string XOR(string text1, string text2)
        {
            if (text1.Length != text2.Length)
            {
                Console.WriteLine("Two data blocks for XOR are must get same size.");
                return null;
            }

            StringBuilder XORed_Text = new StringBuilder(text1.Length);

            for (int i = 0; i < text1.Length; i++)
            {
                if (text1[i] != text2[i])
                {
                    XORed_Text.Append("1");
                }
                else
                {
                    XORed_Text.Append("0");
                }
            }

            return XORed_Text.ToString();
        }
    }
}
