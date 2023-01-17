using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GenesixInv.Helper
{
    public class Formato
    {
        public String Codigo { get; set; }
        public String Desc { get; set; }
        public decimal CantUni { get; set; }
        public decimal Cant { get; set; }
        public String CodigoInf { get; set; }
        public int CodForm { get; set; }
        public string UoMCod { get; set; }

    }
    public class Lote
    {
        public String Codigo { get; set; }
        public String ItemCode { get; set; }
        public DateTime Caducidad { get; set; }
        public Boolean ok { get; set; }
        public int CodErr { get; set; }
        public String MsgErr { get; set; }
    }

    public class ParserIO_func
    {
        private static int[] _month_days = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        private static System.Collections.Generic.List<char> nValuesAssignmets = new System.Collections.Generic.List<char>
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '-', '.', ' ', '$', '/', '+', '%'
        };

        private static bool Alphabetic(char c)
        // [fr] Pas utilisé la classe Char du .Net Framework pour faciliter la transposition dans un autre langage
        // [en] Not used the Char class of the .Net Framewort to facilitate the translation into another language
        {
            return (
              c == 'A' |
              c == 'B' |
              c == 'C' |
              c == 'D' |
              c == 'E' |
              c == 'F' |
              c == 'G' |
              c == 'H' |
              c == 'I' |
              c == 'J' |
              c == 'K' |
              c == 'L' |
              c == 'M' |
              c == 'N' |
              c == 'O' |
              c == 'P' |
              c == 'Q' |
              c == 'R' |
              c == 'S' |
              c == 'T' |
              c == 'U' |
              c == 'V' |
              c == 'W' |
              c == 'X' |
              c == 'Y' |
              c == 'Z');
        }

        public string ADDITIONALID(string code)
        {
            string result = "";
            string type = Type(code);
            string subType = SubType(code, type);
            result = ADDITIONALID(code, type, subType);
            return result;
        }

        public string ADDITIONALID(string code, string type, string subType)
        {
            code = Cleanse(code);
            string result = "";
            if (type == "GS1-128")
            {
                code = CleanSymbologyId(code);
                if (subType == "01.17.10.240")
                {
                    if (containsBL(code))
                    {
                        int nextBL = indexOfBL(code, 27);
                        result = code.Substring(nextBL + 4, code.Length - (nextBL + 4));
                    }
                }
                else if (subType == "01.240")
                {
                    result = code.Substring(19, code.Length - 19);
                }
                else if (subType.StartsWith("240"))
                {
                    if (containsBL(code))
                    {
                        int nextBL = indexOfBL(code, 3);
                        result = code.Substring(3, nextBL - 3);
                    }
                }
            }
            return result;
        }

        public string BESTBEFORE(string code)
        {
            string type = Type(code);
            string subType = SubType(code);
            string result = BESTBEFORE(code, type, subType);
            return result;
        }

        public string BESTBEFORE(string code, string type, string subType)
        {
            code = Cleanse(code);
            string result = "";
            if (type == "GS1-128")
            {
                code = CleanSymbologyId(code);
                if (subType.StartsWith("01.15"))
                    result = code.Substring(18, 6);
                else if (subType.StartsWith("02.10.15"))
                {
                    int nextBL = indexOfBL(code, 16);
                    result = code.Substring(nextBL + 3, 6);
                }
            }
            return result;
        }

        private static bool CheckEan13Key(string code)
        {
            bool result = false;
            int length = code.Length;
            if (length == 13)
            {
                int sum = 0;
                char[] array = code.ToCharArray();
                bool ok = true;
                for (int i = 0; i < 12; i++)
                {
                    char c = array[i];
                    int n;
                    if (int.TryParse(c.ToString(), out n))
                    {
                        if (i % 2 == 0)
                        { // [fr] pair 
                          // [en] even
                            sum = sum + n;
                        }
                        else
                        { // [fr] impair 
                          // [en] odd
                            sum = sum + (3 * n);
                        }
                    }
                    else
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                {
                    int n1 = (10 - (sum % 10)) % 10;
                    int n2;
                    char c = array[12];
                    if (int.TryParse(c.ToString(), out n2))
                    {
                        result = (n1 == n2);
                    }
                }
            }
            return result;
        }


        private static bool CheckHIBCKey(string code)
        {
            bool result = false;
            int sum = 0;
            char[] array = code.ToCharArray();
            int mod = 0;
            for (int i = 0; i < code.Length - 1; i++)
            {
                char c = array[i];
                sum = sum + nValuesAssignmets.IndexOf(c);
                //sum = sum + mod; 
            }
            mod = sum % 43;
            char lastCharCode = array[code.Length - 1];
            if (nValuesAssignmets[mod] == lastCharCode)
                result = true;
            return result;
        }

        private static bool CheckGTINKey(string code)
        {
            bool result = false;
            int length = code.Length;
            int sum = 0;
            char[] array = code.ToCharArray();
            int n = -1;
            for (int i = 2; i < 15; i++)
            {
                char c = array[i];
                if (int.TryParse(c.ToString(), out n))
                {
                    if (i % 2 == 0)
                    { // [fr] pair 
                      // [en] even
                        sum = sum + (3 * n);
                    }
                    else
                    { // [fr] impair 
                      // [en] odd
                        sum = sum + n;
                    }
                }
            }
            int n1 = (10 - (sum % 10)) % 10;
            int n2;
            char key = array[15];
            if (int.TryParse(key.ToString(), out n2))
                result = (n1 == n2);
            return result;
        }

        private static bool CheckSSCCKey(string code)
        {
            bool result = false;
            int length = code.Length;
            int sum = 0;
            char[] array = code.ToCharArray();
            int n = -1;
            for (int i = 2; i < 19; i++)
            {
                char c = array[i];
                if (int.TryParse(c.ToString(), out n))
                {
                    if (i % 2 == 0)
                    { // [fr] pair 
                      // [en] even
                        sum = sum + (3 * n);
                    }
                    else
                    { // [fr] impair 
                      // [en] odd
                        sum = sum + n;
                    }
                }
            }
            int n1 = (10 - (sum % 10)) % 10;
            int n2;
            char key = array[19];
            if (int.TryParse(key.ToString(), out n2))
                result = (n1 == n2);
            return result;
        }

        private static bool Check7Car(string code)
        {
            bool result = false;
            bool ok = true;
            int sum = 0;
            char[] array = code.ToCharArray();
            int n = -1;
            for (int i = 0; i < code.Length; i++)
            {
                if (!NumericChar(array[i]))
                {
                    ok = false;
                    break;
                }
            }
            if (ok)
            {
                for (int i = 0; i < 6; i++)
                {
                    char c = array[i];
                    if (int.TryParse(c.ToString(), out n))
                    {
                        sum = sum + n * (i + 2);
                    }
                }
                int n1 = (sum % 11) % 10;
                int n2;
                char key = array[6];
                if (int.TryParse(key.ToString(), out n2))
                    result = (n1 == n2);
            }
            return result;
        }

        private static string Key7Car(string code)
        {
            string result = "-1";
            bool ok = true;
            int sum = 0;
            char[] array = code.ToCharArray();
            int n = -1;
            for (int i = 0; i < code.Length; i++)
            {
                if (!NumericChar(array[i]))
                {
                    ok = false;
                    break;
                }
            }
            if (ok)
            {
                for (int i = 0; i < 6; i++)
                {
                    char c = array[i];
                    if (int.TryParse(c.ToString(), out n))
                    {
                        sum = sum + n * (i + 2);
                    }
                }
                int n1 = (sum % 11) % 10;
                result = n1.ToString();
            }
            return result;
        }

        public bool containsOrMayContainsId(string type, string subType)
        {
            bool result = false;
            if (type == "GS1-128")
            {
                if (subType.Contains("01") |
                    subType.Contains("240") |
                    subType.Contains("90") |
                    subType.Contains("91") |
                    subType.Contains("92") |
                    subType.Contains("93"))
                    result = true;
            }
            else if ((type == "HIBC") & (subType.Contains("Primary")))
            {
                result = true;
            }
            else if (type == "NaS")
            {
                if (subType == "" |
                     subType == "NaS" |
                     subType == "001" |
                     subType == "002" |
                     subType == "003" |
                     subType == "004" |
                     subType == "005" |
                     subType == "006" |
                     subType == "007" |
                     subType == "008" |
                     subType == "009" |
                     subType == "012" |
                     subType == "013" |
                     subType == "014")
                    result = true;
            }
            else if (type == "EAN 13")
            {
                result = true;
            }
            return result;
        }


        public string ACL(string code)
        {
            string result = "";
            string type = Type(code);
            string subType = SubType(code);
            result = ACL(code, type, subType);
            return result;
        }

        public string ACL(string code, string type, string subType)
        {
            code = Cleanse(code);
            string result = "";
            if (code.Length >= 7)
            {
                if ((type == "EAN 13") & (subType == "ACL 13"))
                {
                    result = code;
                }
                else if ((type == "GS1-128") & (subType.StartsWith("01")) & (code.Substring(3, 4) == "3401"))
                {
                    code = CleanSymbologyId(code);
                    result = code.Substring(3, 13);
                }
                else if ((type == "NaS") & ((subType == "002") | (subType == "003")))
                    result = code.Substring(0, 13);
            }
            return result;
        }

        public string CIP(string code)
        {
            string result = "";
            string type = Type(code);
            string subType = SubType(code);
            result = CIP(code, type, subType);
            return result;
        }

        public string CIP(string code, string type, string subType)
        {
            code = Cleanse(code);
            string result = "";
            if (type == "GS1-128")
            {
                code = CleanSymbologyId(code);
                if (subType.StartsWith("01") & (code.Substring(0, 7) == "0103400"))
                    result = code.Substring(3, 13);
            }
            else if ((type == "EAN 13") & (subType == "CIP 13"))
                result = code;

            return result;
        }

        private static string Cleanse(string code)
        {
            string result = code;
            result = result.Replace("(01)", "01");
            result = result.Replace("(10)", "10");
            result = result.Replace("(17)", "17");
            result = result.Replace("(21)", "21");
            result = result.Replace("(22)", "22");
            result = result.Replace("(240)", "240");
            if (result.StartsWith("*") & (result.EndsWith("*")))
                result = result.Substring(1, result.Length - 2);
            return result;
        }

        private static bool containsSymbologyId(string code)
        {
            bool result = false;
            if (code.StartsWith("]C0") |
                code.StartsWith("]C1") |
                code.StartsWith("]d1") |
                code.StartsWith("]d2"))
                result = true;
            return result;
        }

        private static string CleanSymbologyId(string code)
        {
            string result = code;
            if (containsSymbologyId(code))
                result = result.Substring(3, result.Length - 3);
            return result;
        }

        public string CONTENT(string code)
        {
            string type = Type(code);
            string subType = SubType(code, type);
            string result = CONTENT(code, type, subType);
            return result;
        }

        public string CONTENT(string code, string type, string subType)
        {
            code = Cleanse(code);
            string result = "";
            if (type == "GS1-128")
            {
                code = CleanSymbologyId(code);
                if (subType.Substring(0, 2) == "02")
                    result = code.Substring(2, 14);
            }
            return result;
        }

        private static DateTime ConvertDateTimeFromStr(String str, int typeDate)
        {
            DateTime dt = DateTime.MinValue;
            int y = 0, m = 0, d = 0, h = 0, j = 0;
            // y ------years, m ------months, d ------days, h -------hours

            switch (typeDate)
            {
                case 1:
                    m = int.Parse(str.Substring(0, 2));
                    y = int.Parse(str.Substring(2, 2));
                    break;
                case 2:
                    m = int.Parse(str.Substring(0, 2));
                    d = int.Parse(str.Substring(2, 2));
                    y = int.Parse(str.Substring(4, 2));
                    break;
                case 3:
                    y = int.Parse(str.Substring(0, 2));
                    m = int.Parse(str.Substring(2, 2));
                    d = int.Parse(str.Substring(4, 2));
                    break;
                case 4:
                    y = int.Parse(str.Substring(0, 2));
                    m = int.Parse(str.Substring(2, 2));
                    d = int.Parse(str.Substring(4, 2));
                    h = int.Parse(str.Substring(6, 2));
                    break;
                case 5:
                    y = int.Parse(str.Substring(0, 2));
                    j = int.Parse(str.Substring(2, 3));
                    break;
                case 6:
                    y = int.Parse(str.Substring(0, 2));
                    j = int.Parse(str.Substring(2, 3));
                    h = int.Parse(str.Substring(5, 2));
                    break;
                case 7:
                    y = int.Parse(str.Substring(2, 2));
                    m = int.Parse(str.Substring(5, 2));
                    break;
                case 8:
                    y = int.Parse(str.Substring(4, 2));
                    m = int.Parse(str.Substring(0, 2));
                    d = int.Parse(str.Substring(2, 2));
                    break;
                case 9:
                    y = int.Parse(str.Substring(8, 2));
                    m = int.Parse(str.Substring(3, 2));
                    d = int.Parse(str.Substring(0, 2));
                    break;
            }

            //convert 2 digits year to 4 digits year
            dt = DateTime.ParseExact(String.Format("{0:00}", y), "yy", CultureInfo.CurrentUICulture);
            y = dt.Year;

            if (0 == y)
            {
                //invalid date time string...
                return dt;
            }

            //convert Julian Date  to DateTime
            if (0 != j)
            {
                dt = dt.AddDays(j - 1);
                if (h > 0)
                {
                    dt = dt.AddHours(h);
                }
                return dt;
            }

            //if month is zero
            if (0 == m)
            {
                m = 12;
            }

            //if days invalid
            if (_month_days[m - 1] < d || 0 == d)
            {
                if (2 == m)
                {
                    //leap year 
                    if (DateTime.IsLeapYear(y))
                    {
                        d = 29;
                    }
                    else
                    {
                        d = 28;
                    }
                }
                else
                {
                    d = _month_days[m - 1];
                }
            }

            //convert y,m,d,h to DateTime
            if (y > 0 && m > 0)
            {
                dt = dt.AddMonths(m - 1);
                dt = dt.AddDays(d - 1);
                if (h > 0)
                {
                    dt = dt.AddHours(h);
                }
            }
            return dt;
        }

        public string COUNT(string code)
        {
            string type = Type(code);
            string subType = SubType(code, type);
            string result = COUNT(code, type, subType);
            return result;
        }

        public string COUNT(string code, string type, string subType)
        {
            code = Cleanse(code);
            string result = "";
            if (type == "GS1-128")
            {
                code = CleanSymbologyId(code);
                if (subType == "02.37")
                    result = code.Substring(18, code.Length - 18);
                else if (subType == "02.10.37")
                {
                    if (containsBL(code))
                    {
                        int nextBL = indexOfBL(code, 16);
                        result = code.Substring(nextBL + 3, code.Length - (nextBL + 3));
                    }
                    //result = code.Substring(25, 3);
                }
                else if (subType.StartsWith("02.10.15.37"))
                {
                    int nextBL = indexOfBL(code, 16);
                    result = code.Substring(nextBL + 11, code.Length - nextBL - 11);
                }
                else if (subType == "02.37.10")
                {
                    int nextBL = indexOfBL(code, 16);
                    result = code.Substring(18, nextBL - 17);
                }
                else if (subType.StartsWith("37"))
                {
                    int nextBL = indexOfBL(code, 1);
                    result = code.Substring(2, nextBL - 2);
                }
            }
            return result;
        }

        public string Expiry(string code)
        {
            string type = Type(code);
            string subType = SubType(code, type);
            string result = Expiry(code, type, subType);
            return result;
        }

        public string Expiry(string code, string type, string subType)
        {
            code = Cleanse(code);
            string result = "";
            if (type == "GS1-128")
            {
                code = CleanSymbologyId(code);
                if (subType.Length >= 5)
                {
                    if (subType.StartsWith("01.10.17"))
                    {
                        if (containsBL(code))
                        {
                            int nextBL = indexOfBL(code, 16);
                            result = code.Substring(nextBL + 3, 6);
                        }
                        //result = code.Substring(code.Length-6, 6);
                    }
                    else if (subType.StartsWith("01.11.17"))
                    {
                        result = code.Substring(26, 6);
                    }
                    else if (subType.StartsWith("01.17"))
                    {
                        result = code.Substring(18, 6);
                    }
                    else if (subType.StartsWith("02.17"))
                    {
                        result = code.Substring(18, 6);
                    }
                    else if (subType.Equals("10.17"))
                    {
                        int length = code.Length;
                        result = code.Substring(length - 6, 6);
                    }
                    else if (subType.StartsWith("11.17"))
                    {
                        result = code.Substring(10, 6);
                    }

                    else if (subType.StartsWith("17"))
                    {
                        result = code.Substring(2, 6);
                    }
                    else if (subType == "01.21.17")
                    {
                        if (containsBL(code))
                        {
                            int nextBL = indexOfBL(code, 18);
                            result = code.Substring(nextBL + 3, 6);
                        }
                    }

                    else if (subType.StartsWith("20.17"))
                    {
                        if (containsBL(code))
                        {
                            int nextBL = indexOfBL(code, 1);
                            result = code.Substring(nextBL + 3, 6);
                        }
                    }
                    else if (subType.StartsWith("91.17.10"))
                    {
                        if (containsBL(code))
                        {
                            int nextBL = indexOfBL(code, 1);
                            result = code.Substring(nextBL + 3, 6);
                        }

                    }
                }
            }

            else if (type == "HIBC")
            {
                code = CleanSymbologyId(code);
                string secondaryCode = null;
                if (subType.StartsWith(@"Primary/Secondary"))
                {
                    int position = code.IndexOf('/');
                    secondaryCode = "+" + code.Substring(position + 1);
                }
                else
                {
                    secondaryCode = code;
                }
                int length = secondaryCode.Length;
                if (subType.EndsWith("Secondary.N") & (length >= 8))
                {
                    result = secondaryCode.Substring(1, 5);
                }
                else if (subType.EndsWith("Secondary.$$") & (length > 7))
                {
                    result = secondaryCode.Substring(3, 4);
                }
                else if (subType.EndsWith("Secondary.$$.2") & (length > 10))
                {
                    result = secondaryCode.Substring(4, 6);
                }
                else if (subType.EndsWith("Secondary.$$.3") & (length > 10))
                {
                    result = secondaryCode.Substring(4, 6);
                }
                else if (subType.EndsWith("Secondary.$$.4") & (length > 12))
                {
                    result = secondaryCode.Substring(4, 8);
                }
                else if (subType.EndsWith("Secondary.$$.5") & (length > 9))
                {
                    result = secondaryCode.Substring(4, 5);
                }
                else if (subType.EndsWith("Secondary.$$.6") & (length > 11))
                {
                    result = secondaryCode.Substring(4, 7);
                }
                else if (subType.EndsWith("Secondary.$$.8") & (length > 10))
                {
                    result = secondaryCode.Substring(6, 4);
                }
                else if (subType.EndsWith("Secondary.$$.8.2") & (length > 13))
                {
                    result = secondaryCode.Substring(7, 6);
                }
                else if (subType.EndsWith("Secondary.$$.8.3") & (length > 13))
                {
                    result = secondaryCode.Substring(7, 6);
                }
                else if (subType.EndsWith("Secondary.$$.8.4") & (length > 15))
                {
                    result = secondaryCode.Substring(7, 8);
                }
                else if (subType.EndsWith("Secondary.$$.8.5") & (length > 12))
                {
                    result = secondaryCode.Substring(7, 5);
                }
                else if (subType.EndsWith("Secondary.$$.8.6") & (length > 14))
                {
                    result = secondaryCode.Substring(7, 7);
                }
                else if (subType.EndsWith("Secondary.$$.9") & (length > 13))
                {
                    result = secondaryCode.Substring(9, 4);
                }
                else if (subType.EndsWith("Secondary.$$.9.2") & (length > 16))
                {
                    result = secondaryCode.Substring(10, 6);
                }
                else if (subType.EndsWith("Secondary.$$.9.3") & (length > 16))
                {
                    result = secondaryCode.Substring(10, 6);
                }
                else if (subType.EndsWith("Secondary.$$.9.4") & (length > 18))
                {
                    result = secondaryCode.Substring(10, 8);
                }
                else if (subType.EndsWith("Secondary.$$.9.5") & (length > 15))
                {
                    result = secondaryCode.Substring(10, 5);
                }
                else if (subType.EndsWith("Secondary.$$.9.6") & (length > 17))
                {
                    result = secondaryCode.Substring(10, 7);
                }
            }
            else if (type == "NaS")
            {
                if (subType == "005")
                {
                    result = code.Substring(21, 7);
                }
                else if (subType == "007")
                {
                    result = code.Substring(16, 6);
                }
                else if (subType == "015")
                {
                    result = code.Substring(5, 10);  //H080-25.01.2014-1

                }
            }
            //if (!NumericString(result)) // Il faut améliorer NormalizedExpriry pour supprimer définitivement ce code
            //{
            //  result = "";
            //}

            return result;
        }

        public string Family(string code)
        {
            string type = Type(code);
            string subType = SubType(code, type);
            string result = Family(code, type, subType);
            return result;
        }

        public string Family(string code, string type, string subType)
        {
            code = Cleanse(code);
            string result = "";
            if (code.Length >= 7)
            {
                if (subType == "ACL 13")
                    result = code.Substring(4, 1);
                else if ((type == "GS1-128") & (subType.StartsWith("01") & (code.Substring(3, 4) == "3401")))
                {
                    code = CleanSymbologyId(code);
                    result = code.Substring(7, 1);
                }
                else if ((type == "NaS") & ((subType == "002") | (subType == "003")))
                    result = code.Substring(4, 1);
            }
            return result;
        }

        public string NaS7(string code)
        {
            string result = "";
            string type = Type(code);
            string subType = SubType(code, type);
            result = NaS7(code, type, subType);
            return result;
        }

        public string NaS7(string code, string type, string subType)
        {
            string result = "";
            if ((type == "NaS") & (subType == "NaS7"))
                result = code;
            return result;
        }

        private string NormalizedDate(string dateBrute, string type, string subType, string code)
        {
            code = Cleanse(code);
            code = CleanSymbologyId(code);
            string result = "";
            if (!String.IsNullOrEmpty(dateBrute))
            {
                int dateType = GetDateType(type, subType, code);
                if (dateType == 3)
                {
                    if (dateBrute.StartsWith("20"))
                        if ((dateBrute.StartsWith("2013") |
                             dateBrute.StartsWith("2014") |
                             dateBrute.StartsWith("2015") |
                             dateBrute.StartsWith("2016") |
                             dateBrute.StartsWith("2017") |
                             dateBrute.StartsWith("2018") |
                             dateBrute.StartsWith("2019")) &
                            (dateBrute.EndsWith("01") |
                             dateBrute.EndsWith("02") |
                             dateBrute.EndsWith("03") |
                             dateBrute.EndsWith("04") |
                             dateBrute.EndsWith("05") |
                             dateBrute.EndsWith("06") |
                             dateBrute.EndsWith("07") |
                             dateBrute.EndsWith("08") |
                             dateBrute.EndsWith("09") |
                             dateBrute.EndsWith("10") |
                             dateBrute.EndsWith("11") |
                             dateBrute.EndsWith("12")))
                        {
                            // [fr] Est de la forme AAAAMM, devrait être AAMMJJ
                            // [en] Is in YYYYMM form, should be YYMMDD
                            dateBrute = dateBrute.Substring(2) + "00";  //27/01/2012 DU: A vérifier 
                        }
                }

                if (dateType != -1)
                {
                    DateTime dateTime = ConvertDateTimeFromStr(dateBrute, dateType);
                    if (dateTime != DateTime.MinValue)
                    {
                        if (dateTime.Hour > 0)
                        {
                            result = dateTime.ToString("yyyyMMddHH");
                        }
                        else
                        {
                            result = dateTime.ToString("yyyyMMdd");
                        }
                    }
                }
            }
            return result;
        }

        public string NormalizedBESTBEFORE(string code)
        {
            string dateBrute = BESTBEFORE(code);
            string type = Type(code);
            string subType = SubType(code);
            string result = NormalizedDate(dateBrute, type, subType, code);
            return result;
        }

        public string NormalizedBESTBEFORE(string code, string type, string subType)
        {
            string dateBrute = BESTBEFORE(code);
            string result = NormalizedDate(dateBrute, type, subType, code);
            return result;
        }

        public string NormalizedExpiry(string code)
        {
            string dateBrute = Expiry(code);
            string type = Type(code);
            string subType = SubType(code);
            string result = NormalizedDate(dateBrute, type, subType, code);
            return result;
        }

        public string NormalizedExpiry(string code, string type, string subType)
        {
            code = Cleanse(code);
            string dateBrute = Expiry(code, type, subType);
            string result = NormalizedDate(dateBrute, type, subType, code);
            return result;
        }

        public string NormalizedPRODDATE(string code)
        {
            string dateBrute = PRODDATE(code);
            string type = Type(code);
            string subType = SubType(code);
            string result = NormalizedDate(dateBrute, type, subType, code);
            return result;
        }

        public string NormalizedPRODDATE(string code, string type, string subType)
        {
            code = Cleanse(code);
            string dateBrute = PRODDATE(code);
            string result = NormalizedDate(dateBrute, type, subType, code);
            return result;
        }
        public string Company(string code)
        {
            string type = Type(code);
            string subType = SubType(code, type);
            string result = Company(code, type, subType);
            return result;
        }

        public string Company(string code, string type, string subType)
        {
            code = Cleanse(code);
            string result = "";
            if ((type == "EAN 13") & ((subType == "")))
                result = code.Substring(0, 7);
            if ((type == "NaS") & (subType == "001") | (subType == "004"))
                result = code.Substring(0, 7);
            return result;
        }

        /// <summary>
        /// get type of date time
        /// </summary>
        /// <param name="typeBarcode">type of barcode</param>
        /// <param name="subType">subType</param>
        /// <param name="code">barcode</param>
        /// <returns>type of date</returns>
        /// 1-------MMyy
        /// 2-------MMddyy
        /// 3-------yyMMdd
        /// 4-------yyMMddHH
        /// 5-------yyJJJ
        /// 6-------yyJJJHH
        /// 7-------yyyy-MM
        /// 8-------MMddyy
        /// -1------error type

        private static int GetDateType(String typeBarcode, String subType, String code)
        {
            code = Cleanse(code);
            code = CleanSymbologyId(code);
            int typeDate = -1;
            if (typeBarcode == "GS1-128")
            {
                typeDate = 3;
            }
            else if (typeBarcode == "HIBC")
            {
                string secondaryCode = null;
                if (subType.StartsWith(@"Primary/Secondary"))
                {
                    int position = code.IndexOf('/');
                    secondaryCode = "+" + code.Substring(position + 1);
                }
                else
                {
                    secondaryCode = code;
                }
                int length = secondaryCode.Length;
                if (subType.EndsWith("Secondary.N") & (length >= 8))
                {
                    typeDate = 5;
                }
                else if (subType.EndsWith("Secondary.$$") & (length > 7))
                {
                    typeDate = 1;
                }
                else if (subType.EndsWith("Secondary.$$.2") & (length > 10))
                {
                    typeDate = 2;
                }
                else if (subType.EndsWith("Secondary.$$.3") & (length > 10))
                {
                    typeDate = 3;
                }
                else if (subType.EndsWith("Secondary.$$.4") & (length > 12))
                {
                    typeDate = 4;
                }
                else if (subType.EndsWith("Secondary.$$.5") & (length > 9))
                {
                    typeDate = 5;
                }
                else if (subType.EndsWith("Secondary.$$.6") & (length > 11))
                {
                    typeDate = 6;
                }
                else if (subType.EndsWith("Secondary.$$.8") & (length > 10))
                {
                    typeDate = 1;
                }
                else if (subType.EndsWith("Secondary.$$.8.2") & (length > 13))
                {
                    typeDate = 2;
                }
                else if (subType.EndsWith("Secondary.$$.8.3") & (length > 13))
                {
                    typeDate = 3;
                }
                else if (subType.EndsWith("Secondary.$$.8.4") & (length > 15))
                {
                    typeDate = 4;
                }
                else if (subType.EndsWith("Secondary.$$.8.5") & (length > 12))
                {
                    typeDate = 5;
                }
                else if (subType.EndsWith("Secondary.$$.8.6") & (length > 14))
                {
                    typeDate = 6;
                }
                else if (subType.EndsWith("Secondary.$$.9") & (length > 13))
                {
                    typeDate = 1;
                }
                else if (subType.EndsWith("Secondary.$$.9.2") & (length > 16))
                {
                    typeDate = 2;
                }
                else if (subType.EndsWith("Secondary.$$.9.3") & (length > 16))
                {
                    typeDate = 3;
                }
                else if (subType.EndsWith("Secondary.$$.9.4") & (length > 18))
                {
                    typeDate = 4;
                }
                else if (subType.EndsWith("Secondary.$$.9.5") & (length > 15))
                {
                    typeDate = 5;
                }
                else if (subType.EndsWith("Secondary.$$.9.6") & (length > 17))
                {
                    typeDate = 6;
                }
            }
            else if (typeBarcode == "NaS")
            {
                if (subType == "005")
                {
                    typeDate = 7;
                }
                else if (subType == "007")
                {
                    typeDate = 8;
                }
                else if (subType == "015")
                {
                    typeDate = 9;
                }
            }
            return typeDate;
        }

        public string GTIN(string code)
        {
            code = Cleanse(code);
            string type = Type(code);
            string subType = SubType(code, type);
            string result = GTIN(code, type, subType);
            return result;
        }

        public string GTIN(string code, string type, string subType)
        {
            code = Cleanse(code);
            string result = "";
            if (type == "GS1-128")
            {
                code = CleanSymbologyId(code);
                if ((subType.Substring(0, 2) == "01")) //& !(code.Substring(0, 7) == "0103400") & !(code.Substring(0, 7) == "0103401"))
                    result = code.Substring(2, 14);
            }
            return result;
        }

        public string LIC(string code)
        {
            string type = Type(code);
            string subType = SubType(code, type);
            string result = LIC(code, type, subType);
            return result;
        }

        public string LIC(string code, string type, string subType)
        {
            code = Cleanse(code);
            string result = "";
            if (type == "HIBC")
                if (subType.StartsWith("Primary"))
                {
                    code = CleanSymbologyId(code);
                    result = code.Substring(1, 4);
                }
            return result;
        }

        public string Lot(string code)
        {
            string type = Type(code);
            string subType = SubType(code, type);
            string result = Lot(code, type, subType);
            return result;
        }

        public string Lot(string code, string type, string subType)
        {
            code = Cleanse(code);
            string result = "";
            if (type == "GS1-128")
            {
                code = CleanSymbologyId(code);
                if (subType == "01.10")
                {
                    result = code.Substring(18);
                }
                else if (subType.StartsWith("01.10.17"))
                {
                    if (containsBL(code))
                    {
                        int nextBL = indexOfBL(code, 16);
                        result = code.Substring(18, nextBL - 18);
                    }
                }
                else if (subType.StartsWith("01.11.17.10"))
                {
                    int lenght = code.Length;
                    result = code.Substring(34, lenght - 34);
                }
                else if (subType.StartsWith("01.15.10"))
                {
                    int lenght = code.Length;
                    result = code.Substring(26, lenght - 26);
                }
                else if (subType == "01.17.10")
                {
                    result = code.Substring(26);
                }
                else if (subType.StartsWith("01.17.10"))
                {
                    if (containsBL(code))
                    {
                        int nextBL = indexOfBL(code, 24);
                        result = code.Substring(26, nextBL - 26);
                    }
                }
                else if (subType.StartsWith("01.17.30.10"))
                {
                    if (containsBL(code))
                    {
                        int nextBL = indexOfBL(code, 27);
                        result = code.Substring(nextBL + 3, code.Length - (nextBL + 3));
                    }
                }
                else if (subType.StartsWith("02.10"))
                {
                    if (containsBL(code))
                    {
                        int nextBL = indexOfBL(code, 16);
                        result = code.Substring(18, nextBL - 18);
                    }
                    else
                        result = code.Substring(18);
                }
                else if (subType.StartsWith("02.37.10"))
                {
                    if (containsBL(code))
                    {
                        int nextBL = indexOfBL(code, 16);
                        result = code.Substring(nextBL + 3, code.Length - (nextBL + 3));
                    }
                }
                else if (subType.StartsWith("10"))
                {
                    if (containsBL(code))
                    {
                        int nextBL = indexOfBL(code, 1);
                        result = code.Substring(2, nextBL - 2);
                    }
                }

                else if (subType.Equals("11.17.10"))
                {
                    int lenght = code.Length;
                    result = code.Substring(18, lenght - 18);
                }
                else if ((subType == "17.10"))
                {
                    result = code.Substring(10);
                }
                else if (subType.StartsWith("17.10"))
                {
                    if (containsBL(code))
                    {
                        int nextBL = indexOfBL(code, 10);
                        result = code.Substring(10, nextBL - 10);
                    }
                }
                else if (subType.StartsWith("20.17.10"))
                {
                    if (containsBL(code))
                    {
                        int firstBL = indexOfBL(code, 1);
                        result = code.Substring(firstBL + 11, code.Length - (firstBL + 11));
                    }
                }

                else if (subType.StartsWith("37.10"))
                {
                    if (containsBL(code))
                    {
                        int firstBL = indexOfBL(code, 1);
                        int secondBL = indexOfBL(code, firstBL + 1);
                        result = code.Substring(firstBL + 3, secondBL - (firstBL + 3));
                    }
                }

                else if (subType.StartsWith("91.17.10"))
                {
                    if (containsBL(code))
                    {
                        int nextBL = indexOfBL(code, 1);
                        result = code.Substring(nextBL + 11, code.Length - (nextBL + 11));
                    }
                }

                else if (subType.StartsWith("240.10"))
                {
                    int firstBL = indexOfBL(code, 1);
                    int secondBL = indexOfBL(code, firstBL + 1);

                    if (secondBL != -1)
                    {
                        result = code.Substring(firstBL + 3, secondBL - firstBL - 3);
                    }
                    else
                    {
                        result = code.Substring(firstBL + 3, code.Length - firstBL - 3);
                    }
                }

                else if (subType.StartsWith("240.21.30.10"))
                {
                    if (containsBL(code))
                    {
                        int firstBL = indexOfBL(code, 1);
                        int secondBL = indexOfBL(code, firstBL + 1);
                        int thirdBL = indexOfBL(code, secondBL + 1);
                        result = code.Substring(thirdBL + 3, code.Length - (thirdBL + 3));
                    }
                }
            }
            else if (type == "HIBC")
            {
                string secondaryCode = null;
                code = CleanSymbologyId(code);
                if (subType.StartsWith(@"Primary/Secondary"))
                {
                    int position = code.IndexOf('/');
                    secondaryCode = "+" + code.Substring(position + 1);
                }
                else
                {
                    secondaryCode = code;
                }
                int length = secondaryCode.Length;
                if (subType.Contains("Secondary"))
                {
                    if (subType.EndsWith("Secondary.N") & (length > 8))
                    {
                        result = secondaryCode.Substring(6, length - 8);
                    }
                    else if (subType.EndsWith("Secondary.$") & (length > 4))
                    {
                        result = secondaryCode.Substring(2, length - 4);
                    }
                    else if (subType.EndsWith("Secondary.$$") & (length > 9))
                    {
                        result = secondaryCode.Substring(7, length - 9);
                    }
                    else if (subType.EndsWith("Secondary.$$.2") & (length > 12))
                    {
                        result = secondaryCode.Substring(10, length - 12);
                    }
                    else if (subType.EndsWith("Secondary.$$.3") & (length > 12))
                    {
                        result = secondaryCode.Substring(10, length - 12);
                    }
                    else if (subType.EndsWith("Secondary.$$.4") & (length > 14))
                    {
                        result = secondaryCode.Substring(12, length - 14);
                    }
                    else if (subType.EndsWith("Secondary.$$.5") & (length > 8))
                    {
                        result = secondaryCode.Substring(9, length - 9 - 1);
                    }
                    else if (subType.EndsWith("Secondary.$$.6") & (length > 13))
                    {
                        result = secondaryCode.Substring(11, length - 13);
                    }
                    else if (subType.EndsWith("Secondary.$$.7") & (length > 6))
                    {
                        result = secondaryCode.Substring(4, length - 6);
                    }
                    else if (subType.EndsWith("Secondary.$$.8") & (length > 12))
                    {
                        result = secondaryCode.Substring(10, length - 12);
                    }
                    else if (subType.EndsWith("Secondary.$$.8.2") & (length > 15))
                    {
                        result = secondaryCode.Substring(13, length - 15);
                    }
                    else if (subType.EndsWith("Secondary.$$.8.3") & (length > 15))
                    {
                        result = secondaryCode.Substring(13, length - 15);
                    }
                    else if (subType.EndsWith("Secondary.$$.8.4") & (length > 17))
                    {
                        result = secondaryCode.Substring(15, length - 17);
                    }
                    else if (subType.EndsWith("Secondary.$$.8.5") & (length > 14))
                    {
                        result = secondaryCode.Substring(12, length - 14);
                    }
                    else if (subType.EndsWith("Secondary.$$.8.6") & (length > 16))
                    {
                        result = secondaryCode.Substring(14, length - 16);
                    }
                    else if (subType.EndsWith("Secondary.$$.8.7") & (length > 9))
                    {
                        result = secondaryCode.Substring(7, length - 9);
                    }
                    else if (subType.EndsWith("Secondary.$$.9") & (length > 15))
                    {
                        result = secondaryCode.Substring(13, length - 15);
                    }
                    else if (subType.EndsWith("Secondary.$$.9.2") & (length > 18))
                    {
                        result = secondaryCode.Substring(16, length - 18);
                    }
                    else if (subType.EndsWith("Secondary.$$.9.3") & (length > 18))
                    {
                        result = secondaryCode.Substring(16, length - 18);
                    }
                    else if (subType.EndsWith("Secondary.$$.9.4") & (length > 20))
                    {
                        result = secondaryCode.Substring(18, length - 20);
                    }
                    else if (subType.EndsWith("Secondary.$$.9.5") & (length > 17))
                    {
                        result = secondaryCode.Substring(15, length - 17);
                    }
                    else if (subType.EndsWith("Secondary.$$.9.6") & (length > 19))
                    {
                        result = secondaryCode.Substring(17, length - 19);
                    }
                    else if (subType.EndsWith("Secondary.$$.9.7") & (length > 12))
                    {
                        result = secondaryCode.Substring(10, length - 12);
                    }
                }

                //if (subType.StartsWith("Primary/Secondary"))
                //{
                //  if (subType.EndsWith("Secondary.N") & (length > 8))
                //  {
                //    result = secondaryCode.Substring(6, length - 7);
                //  }
                //  else if (subType.EndsWith("Secondary.$") & (length > 4))
                //  {
                //    result = secondaryCode.Substring(2, length - 3);
                //  }
                //  else if (subType.EndsWith("Secondary.$$") & (length > 9))
                //  {
                //    result = secondaryCode.Substring(7, length - 8);
                //  }
                //  else if (subType.EndsWith("Secondary.$$.2") & (length > 12))
                //  {
                //    result = secondaryCode.Substring(10, length - 11);
                //  }
                //  else if (subType.EndsWith("Secondary.$$.3") & (length > 12))
                //  {
                //    result = secondaryCode.Substring(10, length - 11);
                //  }
                //  else if (subType.EndsWith("Secondary.$$.4") & (length > 14))
                //  {
                //    result = secondaryCode.Substring(12, length - 13);
                //  }
                //  else if (subType.EndsWith("Secondary.$$.5") & (length > 11))
                //  {
                //    result = secondaryCode.Substring(9, length - 10);
                //  }
                //  else if (subType.EndsWith("Secondary.$$.6") & (length > 13))
                //  {
                //    result = secondaryCode.Substring(11, length - 12);
                //  }
                //  else if (subType.EndsWith("Secondary.$$.7") & (length > 6))
                //  {
                //    result = secondaryCode.Substring(4, length - 5);
                //  }
                //  else if (subType.EndsWith("Secondary.$$.8") & (length > 12))
                //  {
                //    result = secondaryCode.Substring(10, length - 11);
                //  }
                //  else if (subType.EndsWith("Secondary.$$.8.2") & (length > 15))
                //  {
                //    result = secondaryCode.Substring(13, length - 14);
                //  }
                //  else if (subType.EndsWith("Secondary.$$.8.3") & (length > 15))
                //  {
                //    result = secondaryCode.Substring(13, length - 14);
                //  }
                //  else if (subType.EndsWith("Secondary.$$.8.4") & (length > 17))
                //  {
                //    result = secondaryCode.Substring(15, length - 16);
                //  }
                //  else if (subType.EndsWith("Secondary.$$.8.5") & (length > 14))
                //  {
                //    result = secondaryCode.Substring(12, length - 13);
                //  }
                //  else if (subType.EndsWith("Secondary.$$.8.6") & (length > 16))
                //  {
                //    result = secondaryCode.Substring(14, length - 15);
                //  }
                //  else if (subType.EndsWith("Secondary.$$.8.7") & (length > 9))
                //  {
                //    result = secondaryCode.Substring(7, length - 8);
                //  }
                //  else if (subType.EndsWith("Secondary.$$.9") & (length > 15))
                //  {
                //    result = secondaryCode.Substring(13, length - 14);
                //  }
                //  else if (subType.EndsWith("Secondary.$$.9.2") & (length > 18))
                //  {
                //    result = secondaryCode.Substring(16, length - 17);
                //  }
                //  else if (subType.EndsWith("Secondary.$$.9.3") & (length > 18))
                //  {
                //    result = secondaryCode.Substring(16, length - 17);
                //  }
                //  else if (subType.EndsWith("Secondary.$$.9.4") & (length > 20))
                //  {
                //    result = secondaryCode.Substring(18, length - 19);
                //  }
                //  else if (subType.EndsWith("Secondary.$$.9.5") & (length > 17))
                //  {
                //    result = secondaryCode.Substring(15, length - 16);
                //  }
                //  else if (subType.EndsWith("Secondary.$$.9.6") & (length > 19))
                //  {
                //    result = secondaryCode.Substring(17, length - 18);
                //  }
                //  else if (subType.EndsWith("Secondary.$$.9.7") & (length > 12))
                //  {
                //    result = secondaryCode.Substring(10, length - 11);
                //  }
                //}
            }
            else if (type == "NaS")
            {
                if (subType == "006")
                {
                    result = code.Substring(11, 6);
                }
                else if (subType == "007")
                {
                    result = code.Substring(8, 8);
                }
                else if (subType == "010")
                {
                    result = code.Substring(1, 6);
                }
                else if (subType == "012")
                {
                    result = code.Substring(code.IndexOf('^') + 1, code.Length - code.IndexOf('^') - 2);
                }
                else if (subType == "015")
                {
                    result = code.Substring(0, 4);
                }
            }
            return result;
        }

        public string LPP(string code)
        {
            string result = "";
            string type = Type(code);
            string subType = SubType(code);
            result = LPP(code, type, subType);
            return result;
        }


        public string LPP(string code, string type, string subType)
        {
            string result = "";
            if ((type == "NaS") & (subType == "001"))
                result = code.Substring(13, 6) + Key7Car(code.Substring(13, 6));
            if ((type == "NaS") & ((subType == "002") | (subType == "004")))
                result = code.Substring(13, 7);
            if ((type == "NaS") & (subType == "003"))
                result = code.Substring(14, 7);
            return result;
        }

        private static bool NumericChar(char c)
        // [fr] Pas utilisé la classe Char du .Net Framework pour faciliter la transposition dans un autre langage
        // [en] Not used the Char class of the .Net Framewort to facilitate the translation into another language
        {
            return (
              c == '0' |
              c == '1' |
              c == '2' |
              c == '3' |
              c == '4' |
              c == '5' |
              c == '6' |
              c == '7' |
              c == '8' |
              c == '9');
        }

        private static bool NumericString(string code)
        {
            bool ok = true;
            char[] array = code.ToCharArray();
            for (int i = 0; i < code.Length; i++)
            {
                if (!NumericChar(array[i]))
                {
                    ok = false;
                    break;
                }
            }
            return ok;
        }

        public string PCN(string code)
        {
            string type = Type(code);
            string subType = SubType(code, type);
            string result = PCN(code, type, subType);
            return result;
        }

        public string PCN(string code, string type, string subType)
        {
            code = Cleanse(code);
            string result = "";
            if (type == "HIBC")
            {
                code = CleanSymbologyId(code);
                if (subType == "Primary")
                {
                    result = code.Substring(5, code.Length - 7);
                }
                else if (subType.StartsWith(@"Primary/Secondary"))
                {
                    int position = code.IndexOf('/');
                    result = code.Substring(5, position - 6);
                }
            }
            return result;
        }

        public string PRODDATE(string code)
        {
            string type = Type(code);
            string subType = SubType(code);
            string result = PRODDATE(code, type, subType);
            return result;
        }

        public string PRODDATE(string code, string type, string subType)
        {
            code = Cleanse(code);
            string result = "";
            if (type == "GS1-128")
            {
                code = CleanSymbologyId(code);
                if (subType.StartsWith("01.11"))
                {
                    result = code.Substring(18, 6);
                }
                if (subType.StartsWith("11.17"))
                {
                    result = code.Substring(2, 6);
                }
            }
            return result;
        }

        public string Reference(string code)
        {
            string type = Type(code);
            string subType = SubType(code, type);
            string result = Reference(code, type, subType);
            return result;
        }

        public string Reference(string code, string type, string subType)
        {
            code = Cleanse(code);
            string result = "";
            if (type == "NaS")
            {
                if (subType == "005")
                {
                    result = code.Substring(0, 9);
                }
                else if (subType == "006")
                {
                    result = code.Substring(0, 10);
                }
                else if (subType == "007")
                {
                    result = code.Substring(0, 8);
                }
                else if (subType == "008")
                {
                    result = code.Substring(0, 8);
                }
                else if (subType == "009")
                {
                    result = code.Substring(1, code.Length - 1);
                }
                else if (subType == "012")
                {
                    result = code.Substring(3, 6);
                }
                else if (subType == "013")
                {
                    result = code.Substring(1, 13);
                }
                else if (subType == "014")
                {
                    result = code.Substring(0, 4) + code.Substring(5, 5);
                }
                else if (subType == "NaS")
                {
                    result = code;
                }
            }
            return result;
        }

        public string Product(string code)
        {
            string type = Type(code);
            string subType = SubType(code, type);
            string result = Product(code, type, subType);
            return result;
        }

        public string Product(string code, string type, string subType)
        {
            code = Cleanse(code);
            string result = "";
            if ((type == "EAN 13") & (subType == ""))
                result = code.Substring(7, 5);
            if ((type == "NaS") & ((subType == "001") | (subType == "004")))
                result = code.Substring(7, 5);
            return result;
        }

        public string Serial(string code)
        {
            string type = Type(code);
            string subType = SubType(code);
            string result = Serial(code, type, subType);
            return result;
        }

        public string Serial(string code, string type, string subType)
        {
            code = Cleanse(code);
            string result = "";
            if (type == "GS1-128")
            {
                code = CleanSymbologyId(code);
                if (subType == "01.17.21")
                {
                    result = code.Substring(26, code.Length - 26);
                }
                else if (subType.StartsWith("01.21"))
                {
                    if (containsBL(code))
                    {
                        int firstBL = indexOfBL(code, 18);
                        result = code.Substring(18, firstBL - 18);
                    }
                }
                else if (subType == "17.21")
                {
                    result = code.Substring(10, code.Length - 10);
                }
                else if (subType == "37.10.21")
                {
                    if (containsBL(code))
                    {
                        int firstBL = indexOfBL(code, 1);
                        int secondBL = indexOfBL(code, firstBL + 1);
                        result = code.Substring(secondBL + 3, code.Length - (secondBL + 3));
                    }
                }

                else if (subType.StartsWith("240.21"))
                {
                    if (containsBL(code))
                    {
                        int firstBL = indexOfBL(code, 3);
                        int secondBL = indexOfBL(code, firstBL + 1);
                        result = code.Substring(firstBL + 3, secondBL - (firstBL + 3));
                    }
                }
            }
            else if (type == "HIBC")
            {
                // To do
            }
            else if (type == "NaS")
            {
                if (subType == "005")
                {
                    result = code.Substring(10, 10);
                }

            }
            return result;
        }

        public string VARCOUNT(string code)
        {
            string type = Type(code);
            string subType = SubType(code);
            string result = VARCOUNT(code, type, subType);
            return result;
        }

        public string VARCOUNT(string code, string type, string subType)
        {
            code = Cleanse(code);
            string result = "";
            if (type == "GS1-128")
            {
                code = CleanSymbologyId(code);
                if (subType.StartsWith("01.10.17.30"))
                {
                    if (containsBL(code))
                    {
                        int nextBL = indexOfBL(code, 19);
                        result = code.Substring(nextBL + 11, code.Length - (nextBL + 11));
                    }
                }
                else if (subType == "01.30")
                    result = code.Substring(18, code.Length - 18);
                else if (subType.StartsWith("01.17.30"))
                {
                    if (containsBL(code))
                    {
                        int nextBL = indexOfBL(code, 24);
                        result = code.Substring(26, nextBL - 26);
                    }
                }
                else if (subType.StartsWith("01.17.10.30"))
                {
                    if (containsBL(code))
                    {
                        int nextBL = indexOfBL(code, 27);
                        result = code.Substring(nextBL + 3, code.Length - (nextBL + 3));
                    }
                }
                else if (subType.StartsWith("17.10.30"))
                {
                    if (containsBL(code))
                    {
                        int nextBL = indexOfBL(code, 8);
                        result = code.Substring(nextBL + 3, code.Length - (nextBL + 3));
                    }
                }
                else if (subType.StartsWith("240.21.30"))
                {
                    if (containsBL(code))
                    {
                        int firstBL = indexOfBL(code, 1);
                        int secondBL = indexOfBL(code, firstBL + 1);
                        int thirdBL = indexOfBL(code, secondBL + 1);
                        result = code.Substring(secondBL + 3, thirdBL - (secondBL + 3));
                    }
                }
            }

            return result;
        }

        public string VARIANT(string code)
        {
            string type = Type(code);
            string subType = SubType(code);
            string result = VARIANT(code, type, subType);
            return result;
        }

        public string VARIANT(string code, string type, string subType)
        {
            code = Cleanse(code);
            string result = "";
            if (type == "GS1-128")
            {
                code = CleanSymbologyId(code);
                if (subType.StartsWith("20"))
                    result = code.Substring(2, 2);
            }
            return result;
        }

        public string Quantity(string code)
        {
            string type = Type(code);
            string subType = SubType(code);
            string result = Quantity(code, type, subType);
            return result;
        }

        public string Quantity(string code, string type, string subType)
        {
            code = Cleanse(code);
            string result = "";

            if (type == "HIBC")
            {
                string secondaryCode = null;
                code = CleanSymbologyId(code);
                if (subType.StartsWith(@"Primary/Secondary"))
                {
                    int position = code.IndexOf('/');
                    secondaryCode = "+" + code.Substring(position + 1);
                }
                else
                {
                    secondaryCode = code;
                }
                int length = secondaryCode.Length;
                if (subType.EndsWith("Secondary.$$.8") & (length > 8))
                {
                    result = secondaryCode.Substring(4, 2);
                }
                else if (subType.EndsWith("Secondary.$$.9") & (length > 4))
                {
                    result = secondaryCode.Substring(4, 5);
                }
            }
            //if (type == "HIBC")
            //{
            //  if (subType.StartsWith("Secondary.$$.8"))
            //    result = code.Substring(4, 2);
            //  if (subType.StartsWith("Secondary.$$.9"))
            //    result = code.Substring(4, 5);
            //}


            else if (type == "NaS")
            {
                if (subType == "011")
                    result = code.Substring(1, 1);
            }
            return result;
        }

        public string SSCC(string code)
        {
            string type = Type(code);
            string subType = SubType(code, type);
            string result = SSCC(code, type, subType);
            return result;
        }

        public string SSCC(string code, string type, string subType)
        {
            code = Cleanse(code);
            string result = "";
            if (type == "GS1-128")
            {
                code = CleanSymbologyId(code);
                if (subType.Substring(0, 2) == "00")
                    result = code.Substring(2, 18);
            }
            return result;
        }

        public string Type(string code)
        {
            string result = "NaS";
            int length = code.Length;
            code = Cleanse(code);
            if (code.StartsWith("]C1") | code.StartsWith("]d2"))
            {
                result = "GS1-128";
            }
            else if (code.StartsWith("]d1"))
            {
                result = "HIBC";
            }
            //else if (length == 13)
            //{
            //  bool ok = true;
            //  char[] array = code.ToCharArray();
            //  for (int i = 0; i < length; i++)
            //  {
            //    if (!NumericChar(array[i]))
            //    {
            //      ok = false;
            //      break;
            //    }
            //  }
            //  if (ok)
            //  {
            //    if (CheckEan13Key(code))
            //      result = "EAN 13";
            //  }
            //}
            else if ((code.StartsWith("00") & (length >= 20)))
            {
                bool ok = true;
                char[] array = code.ToCharArray();
                for (int i = 2; i < 20; i++)
                {
                    if (!NumericChar(array[i]))
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                {
                    if (CheckSSCCKey(code))
                    {
                        result = "GS1-128";
                    }
                }
            }
            else if ((code.StartsWith("01") & (length >= 16)))
            {
                bool ok = true;
                char[] array = code.ToCharArray();
                for (int i = 2; i < 16; i++)
                {
                    if (!NumericChar(array[i]))
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                {
                    if (CheckGTINKey(code))
                    {
                        result = "GS1-128";
                    }
                }
            }
            else if ((code.StartsWith("02") & (length >= 16)))
            {
                bool ok = true;
                char[] array = code.ToCharArray();
                for (int i = 2; i < 16; i++)
                {
                    if (!NumericChar(array[i]))
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                {
                    if (CheckGTINKey(code))
                    {
                        result = "GS1-128";
                    }
                }
            }

            else if (code.StartsWith("11") & (length >= 10))
            {
                if ((code.Substring(8, 2).Equals("17")) & (length >= 16))
                    if ((code.Substring(16, 2).Equals("10")) & (length >= 18))
                        result = "GS1-128";
            }
            // This conditions is not enough strong
            //else if (code.StartsWith("17") & (length >= 11))
            //{
            //  string ai2 = code.Substring(8, 2);
            //  if ((ai2 == "10") | (ai2 == "30") | (ai2 == "21"))
            //    result = "GS1-128";
            //}

            // This conditions is not enough strong
            //else if (code.StartsWith("20") & (length >= 6))
            //{
            //  string ai2 = code.Substring(4, 2);
            //  if (ai2 == "17")
            //    result = "GS1-128";
            //}

            //else if ((code.StartsWith("240") & (length >= 4)))
            //{
            //  result = "GS1-128";
            //}
            //else if (containsBL(code))
            //{
            //  result = "GS1-128";
            //  // int testposition = indexOfBL(code, 0);
            //}

            else if (code.StartsWith("+"))
            {
                if (CheckHIBCKey(code))
                    result = "HIBC";
                else
                    //Asumimos que siempre que empieza por + és un HIBC
                    result = "HIBC-";

            }
            return result;
        }

        private static bool containsBL(string code) //Boundary Length
        {
            bool result = false;
            //     string GS = ((char)0x001d).ToString();
            string GS = " ";
            if (code.Contains(GS) | code.Contains("@"))
            {
                result = true;
            }
            return result;
        }

        private static string containsBL_2(string code)
        {
            string result = "Not";
            //string GS = ((char)0x001d).ToString();
            string GS = " ";
            if (code.Contains(GS))
                result = "GS";
            else if (code.Contains("@"))
                result = "@";
            return result;
        }

        private static int indexOfBL(string code, int start) //Boundary Length
        {
            int result = -1;
            string BLchar = containsBL_2(code);
            if (BLchar == "GS")
            {
                //string GS = ((char)0x001d).ToString();
                string GS = " ";
                result = code.IndexOf(GS, start);
            }
            else if (BLchar == "@")
                result = code.IndexOf("@", start);
            return result;
        }

        //public string UCD(string code)
        //{
        //  string result = "";
        //  string type = Type(code);
        //  string subType = SubType(code);
        //  result = UCD(code, type, subType);
        //  return result;
        //}

        //public string UCD(string code, string type, string subType)
        //{
        //  string result = "";
        //  code = Cleanse(code);
        //  // To do
        //  return result;
        //}

        public string UoM(string code)
        {
            string result = "";
            string type = Type(code);
            string subType = SubType(code, type);
            result = UoM(code, type, subType);
            return result;
        }

        public string UoM(string code, string type, string subType)
        {
            string result = "";
            code = Cleanse(code);
            if (type == "HIBC")
            {
                code = CleanSymbologyId(code);
                if (subType == "Primary")
                    result = code.Substring(code.Length - 2, 1);
                else if (subType.StartsWith("Primary/Secondary"))
                    result = code.Substring(code.IndexOf("/") - 1, 1);
            }
            return result;
        }

        public string SubType(string code)
        {
            string type = Type(code);
            string result = SubType(code, type);
            return result;
        }

        public string SubType(string code, string type)
        {
            code = Cleanse(code);
            code = CleanSymbologyId(code);
            string result = "NaS";
            if (type == "EAN 13")
            {
                if (code.Substring(0, 4) == "3401")
                    result = "ACL 13";
                else if (code.Substring(0, 4) == "3400")
                    result = "CIP 13";
                else
                    result = "";
            }

            else if (type == "GS1-128")
            {
                int length = code.Length;
                if (length >= 20)
                    if (code.Substring(0, 2) == "00")
                        result = "00"; // 00
                if (length >= 16)
                {
                    if (code.Substring(0, 2) == "01")
                        result = "01"; // 01
                    if (code.Substring(0, 2) == "02")
                        result = "02"; // 02
                }
                if ((result == "01") & (length >= 18))
                {
                    string ai2 = code.Substring(16, 2);
                    string ai3 = code.Substring(16, 3);
                    if (ai2 == "10")
                    {
                        result = result + ".10"; //01.10
                        int nextBL = indexOfBL(code, 16);
                        if (nextBL != -1)
                        {
                            ai2 = code.Substring(nextBL + 1, 2);
                            if (ai2 == "17")
                            {
                                result = result + ".17"; // 01.10.17
                                if (code.Length > nextBL + 10)
                                {
                                    ai2 = code.Substring(nextBL + 9, 2);
                                    if (ai2 == "30")
                                    {
                                        result = result + ".30"; // 01.10.17.30
                                    }
                                }
                            }
                        }
                    }
                    else if (ai2 == "11")
                    {
                        result = result + ".11"; // 01.11
                        if (length >= 24)
                        {
                            ai2 = code.Substring(24, 2);
                            if (ai2 == "17")
                            {
                                result = result + ".17"; // 01.11.17
                                if (length >= 30)
                                {
                                    ai2 = code.Substring(32, 2);
                                    if (ai2 == "10")
                                        result = result + ".10"; //01.11.17.10
                                }
                            }
                        }
                    }

                    else if (ai2 == "15")
                    {
                        result = result + ".15"; // 01.15
                        if (length >= 26)
                        {
                            ai2 = code.Substring(24, 2);
                            if (ai2 == "10")
                                result = result + ".10"; // 01.15.10
                        }
                    }

                    else if (ai2 == "17")
                    {
                        result = result + ".17"; // 01.17
                        if (length >= 26)
                        {
                            ai2 = code.Substring(24, 2);
                            if (ai2 == "10")
                            {
                                result = result + ".10"; // 01.17.10
                                int nextBL = indexOfBL(code, 26);
                                if (nextBL != -1)
                                {
                                    ai2 = code.Substring(nextBL + 1, 2);
                                    ai3 = code.Substring(nextBL + 1, 3);
                                    if (ai2 == "30")
                                    {
                                        result = result + ".30";
                                    }
                                    else if (ai2 == "91")
                                    {
                                        result = result + ".91"; // 01.17.10.91
                                    }
                                    else if (ai2 == "93")
                                    {
                                        result = result + ".93"; // 01.17.10.93
                                    }
                                    else if (ai3 == "240")
                                    {
                                        result = result + ".240"; // 01.17.10.240
                                    }
                                }

                                //if (code.Length >= 36)
                                //{
                                //  ai3 = code.Substring(33, 3);
                                //  if (ai3 == "240")
                                //    result = result + ".240";
                                //}
                            }
                            else if (ai2 == "30")
                            {
                                result = result + ".30"; // 01.17.30
                                int nextBL = indexOfBL(code, 26);
                                if (nextBL != -1)
                                {
                                    ai2 = code.Substring(nextBL + 1, 2);
                                    if (ai2 == "10")
                                    {
                                        result = result + ".10"; // 01.17.30.10
                                    }
                                }

                            }
                            else if (ai2 == "21")
                                result = result + ".21"; // 01.17.21
                                                         //else if (ai2 == "30")
                                                         //{
                                                         //  result = result + ".30"; // 01.17.30
                                                         //  int nextBL = indexOfBL(code, 26);
                                                         //  if (nextBL != -1)
                                                         //  {
                                                         //    ai2 = code.Substring(nextBL + 1, 2);
                                                         //    if (ai2 == "10")
                                                         //    {
                                                         //      result = result + ".10"; // 01.17.30.10
                                                         //    }
                                                         //  }
                                                         //}
                        }
                    }
                    else if (ai2 == "21")
                    {
                        result = result + ".21"; // 01.21
                        int nextBL = indexOfBL(code, 18);
                        if (nextBL != -1)
                        {
                            ai3 = code.Substring(nextBL + 1, 2);
                            if (ai3 == "17")
                            {
                                result = result + ".17"; // 01.21.17
                            }
                        }
                    }

                    else if (ai2 == "30")
                    {
                        result = result + ".30";
                    }
                    else if (ai3 == "240")
                    {
                        result = result + ".240";
                    }
                } // End 01.XX

                if ((result == "02") & (length >= 19))
                {
                    string ai2 = code.Substring(16, 2);
                    string ai3 = code.Substring(16, 3);
                    if (ai2 == "10")
                    {
                        result = result + ".10"; // 02.10
                        int nextBL = indexOfBL(code, 15);
                        if (nextBL != -1)
                        {
                            ai2 = code.Substring(nextBL + 1, 2);
                            if (ai2 == "37")
                            {
                                result = result + ".37"; // 02.10.37
                            }
                            else if (ai2 == "15")
                            {
                                result = result + ".15"; // 02.10.15
                                if (code.Length >= nextBL + 9)
                                {
                                    ai2 = code.Substring(nextBL + 9, 2);
                                    if (ai2 == "37")
                                    {
                                        result = result + ".37"; // 02.10.15.37
                                    }
                                }
                            }
                        }
                    }
                    else if (ai2 == "17")
                    {
                        result = result + ".17"; // 02.17
                        if (code.Length > 25)
                        {
                            ai2 = code.Substring(24, 2);
                            if (ai2 == "37")
                            {
                                result = result + ".37"; // 02.17.37
                            }
                        }
                    }
                    else if (ai2 == "37")
                    {
                        result = result + ".37"; // 02.37
                        int nextBL = indexOfBL(code, 15);
                        if (nextBL != -1)
                        {
                            ai2 = code.Substring(nextBL + 1, 2);
                            if (ai2 == "10")
                            {
                                result = result + ".10"; // 02.37.10
                            }
                        }
                    }
                }
                if (code.StartsWith("11") & (length >= 6))
                    if ((code.Substring(8, 2).Equals("17")) & (length >= 16))
                        if ((code.Substring(16, 2).Equals("10")) & (length >= 18))
                            result = "11.17.10";

                //if (code.StartsWith("17"))
                //{
                //  result = "17";
                //  if (code.Length>10)
                //  {
                //    string ai2 = code.Substring(8, 2);
                //    if (ai2 == "21")
                //      result = result + ".21";
                //    if (ai2 == "10")
                //      result = result + ".10";
                //  }
                //}


                if ((code.Substring(0, 2) == "20"))
                {
                    result = "20"; // 20
                    int nextBL = indexOfBL(code, 1);
                    if (nextBL != -1)
                    {
                        string ai2 = code.Substring(nextBL + 1, 2);
                        string ai3 = code.Substring(nextBL + 1, 3);
                        if (ai2 == "17")
                        {
                            result = result + ".17"; // 20.17

                            if (code.Length > nextBL + 9)
                            {
                                ai2 = code.Substring(nextBL + 9, 2);
                                if (ai2 == "10")
                                {
                                    result = result + ".10"; // 20.17.10
                                }

                            }

                            //if (length >= 8)
                            //{
                            //  ai2 = code.Substring(12, 2);
                            //  if (ai2 == "10")
                            //  {
                            //    result = result + ".10"; // 20.17.10
                            //  }
                            //}
                        }
                        else if (ai3 == "240")
                        {
                            result = result + ".240"; // 20.240
                            nextBL = indexOfBL(code, nextBL + 1);
                            ai2 = code.Substring(nextBL + 1, 2);
                            if (ai2 == "10")
                            {
                                result = result + ".10"; // 20.240.10
                                nextBL = indexOfBL(code, nextBL + 1);
                                ai2 = code.Substring(nextBL + 1, 2);
                                if (ai2 == "91")
                                {
                                    result = result + ".91"; // 20.240.10.91
                                    nextBL = indexOfBL(code, nextBL + 1);
                                    ai2 = code.Substring(nextBL + 1, 2);
                                    if (ai2 == "92")
                                    {
                                        result = result + ".92"; // 20.240.10.91.92
                                    }
                                }
                            }
                        }
                    }
                }

                if (code.Substring(0, 3) == "240")
                {
                    result = "240"; // 240
                    int nextBL = indexOfBL(code, 4);
                    if (nextBL != -1)
                    {
                        string ai2 = code.Substring(nextBL + 1, 2);
                        if (ai2 == "21")
                        {
                            result = "240.21"; //240.21
                            nextBL = indexOfBL(code, nextBL + 5);
                            ai2 = code.Substring(nextBL + 1, 2);
                            if (ai2 == "30")
                            {
                                result = result + ".30"; //240.21.30
                                nextBL = indexOfBL(code, nextBL + 3);
                                ai2 = code.Substring(nextBL + 1, 2);
                                if (ai2 == "10")
                                {
                                    result = result + ".10"; //240.21.30.10
                                }
                            }
                        }
                        else if (ai2 == "10")
                        {
                            result = result + ".10"; //240.10
                        }
                    }
                }

                if (code.Substring(0, 2) == "10")
                {
                    result = "10"; // 10
                    int nextBL = indexOfBL(code, 3);
                    if (nextBL != -1)
                    {
                        string ai2 = code.Substring(nextBL + 1, 2);
                        if (ai2 == "11")
                        {
                            result = result + ".11"; // 10.11
                            int prevBL = nextBL;
                            ai2 = code.Substring(prevBL + 9, 2);
                            if (ai2 == "17")
                            {
                                result = result + ".17"; // 10.11.17
                                ai2 = code.Substring(prevBL + 17, 2);
                                if (ai2 == "30")
                                {
                                    result = result + ".30"; // 10.11.17.30

                                }
                            }
                        }
                        else if (ai2 == "17")
                        {
                            result = result + ".17"; // 10.17
                        }
                        else if (ai2 == "21")
                        {
                            result = result + ".21"; // 10.21
                        }
                    }
                }



                if (code.Substring(0, 2) == "17")
                {
                    result = "17";
                    string ai2 = code.Substring(8, 2);
                    if (ai2 == "10")
                    {
                        result = result + ".10"; // 17.10
                        int nextBL = indexOfBL(code, 11);
                        if (nextBL != -1)
                        {
                            ai2 = code.Substring(nextBL + 1, 2);
                            if (ai2 == "21")
                            {
                                result = result + ".21"; // 17.10.21
                            }
                            else if (ai2 == "30")
                            {
                                result = result + ".30"; // 17.10.30
                            }
                            else if (ai2 == "91")
                            {
                                result = result + ".91"; // 17.10.91
                            }
                        }

                        //if (ai2 == "30")
                        //  result = result + ".30"; // 17.10.30
                    }
                    else if (ai2 == "21")
                    {
                        result = result + ".21"; // 17.21
                    }
                }

                if (code.Substring(0, 2) == "91")
                {
                    result = "91"; // 91
                    int nextBL = indexOfBL(code, 3);
                    if (nextBL != -1)
                    {
                        string ai2 = code.Substring(nextBL + 1, 2);
                        if (ai2 == "17")
                        {
                            result = result + ".17"; // 91.17
                            ai2 = code.Substring(nextBL + 9, 2);
                            if (ai2 == "10")
                            {
                                result = result + ".10"; // 91.17.10
                            }
                        }
                    }
                }

                if (code.Substring(0, 2) == "37")
                {
                    result = "37"; // 37
                    int nextBL = indexOfBL(code, 3);
                    if (nextBL != -1)
                    {
                        string ai2 = code.Substring(nextBL + 1, 2);
                        if (ai2 == "10")
                        {
                            result = result + ".10"; // 37.10
                            nextBL = indexOfBL(code, nextBL + 1);
                            ai2 = code.Substring(nextBL + 1, 2);
                            if (ai2 == "21")
                            {
                                result = result + ".21"; // 37.10.21
                            }
                        }
                    }
                }
                //  if ((result == "NaS") & (length >= 8))
                //  {
                //    string ai2 = code.Substring(0, 2);
                //    if (ai2 == "17")
                //    {
                //      result = "17"; // 17
                //      ai2 = code.Substring(8, 2);
                //      if (ai2 == "10")
                //        result = result + ".10"; // 17.10
                //      if (ai2 == "30")
                //        result = result + ".30"; // 17.30
                //    }
                //  }
                //  if ((result == "NaS") & (length >= 4))
                //    if (code.Substring(0, 3) == "240")
                //      result = "240";


            }

            else if (type == "HIBC")
            {
                code = CleanSymbologyId(code);
                int length = code.Length;
                if (length >= 8)
                {
                    char[] array = code.ToCharArray();
                    if (Alphabetic(array[1]))
                    {
                        result = "Primary";
                        int position = code.IndexOf('/');
                        if ((position != -1) & (position != code.Length - 1))
                        {
                            result = result + @"/Secondary";
                            array = code.Substring(position).ToCharArray();
                        }
                    }
                    else
                    {
                        result = "Secondary";
                    }
                    if (result.EndsWith("Secondary") & (array.Length > 0))
                    {
                        if (NumericChar(array[1]))
                            result = result + ".N";
                        else if (array[1] == '$')
                        {
                            result = result + ".$";
                            if (array[2] == '$')
                            {
                                result = result + "$";
                                char c1 = array[3];
                                if (
                                  (c1 == '2') |
                                  (c1 == '3') |
                                  (c1 == '4') |
                                  (c1 == '5') |
                                  (c1 == '6') |
                                  (c1 == '7'))
                                {
                                    result = result + "." + c1;
                                }
                                else if (
                                  (c1 == '8') |
                                  (c1 == '9'))
                                {
                                    result = result + "." + c1;
                                    if (c1 == '8')
                                    {
                                        if (length > 8)
                                        {
                                            char c2 = array[6];
                                            if (
                                              (c2 == '2') |
                                              (c2 == '3') |
                                              (c2 == '4') |
                                              (c2 == '5') |
                                              (c2 == '6') |
                                              (c2 == '7'))
                                            {
                                                result = result + "." + c2;
                                            }
                                        }
                                    }
                                    if (c1 == '9')
                                    {
                                        if (length > 11)
                                        {
                                            char c2 = array[9];
                                            if (
                                              (c2 == '2') |
                                              (c2 == '3') |
                                              (c2 == '4') |
                                              (c2 == '5') |
                                              (c2 == '6') |
                                              (c2 == '7'))
                                            {
                                                result = result + "." + c2;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (result == "NaS")
            {
                if (code.Length == 7)
                    if (Check7Car(code))
                    {
                        result = "NaS7";
                    }

                if (code.Length == 19)
                {
                    string subLeftCode = code.Substring(0, 13);
                    if (CheckEan13Key(subLeftCode))
                        result = "001"; // EAN 13 and LPP without checksum
                }

                if (code.Length == 20)
                {
                    string subLeftCode = code.Substring(0, 13);
                    string subRightCode = code.Substring(13, 7);
                    if (CheckEan13Key(subLeftCode) & Check7Car(subRightCode) & code.StartsWith("3401"))
                        result = "002"; // ACL 13 and LPP
                }

                if (code.Length == 21)
                {
                    string subLeftCode = code.Substring(0, 13);
                    string subRightCode = code.Substring(14, 7);
                    if (CheckEan13Key(subLeftCode) & Check7Car(subRightCode) & code.StartsWith("3401"))
                        result = "003"; // ACL 13 and LPP with Espace
                }

                if (code.Length == 20)
                {
                    string subLeftCode = code.Substring(0, 13);
                    string subRightCode = code.Substring(13, 7);
                    if (CheckEan13Key(subLeftCode) & Check7Car(subRightCode) & !code.StartsWith("3401"))
                        result = "004"; // EAN 13 and LPP
                }

                if (code.Length == 28)
                {
                    if ((code.Substring(20, 1) == " ") & (code.Substring(25, 1) == "-"))
                    {
                        result = "005"; // Chris Eyes Company (Example: ASK +20.0 1102745059 2016-05)
                    }
                }

                if (code.Length == 17)
                {
                    string maybeLot = code.Substring(11, 6);
                    bool ok = NumericString(maybeLot);
                    if ((ok) & (code.Substring(10, 1) == " "))
                    {
                        result = "006"; // COUSIN BIOSERV Company (Example: FBIOW8D160 102223)
                    }
                }
                if (code.Length == 22)
                {
                    string maybeRef = code.Substring(0, 8);
                    string maybeExpiry = code.Substring(16, 6);
                    string maybeLot = code.Substring(8, 8);
                    bool ok1 = NumericString(maybeRef);
                    bool ok2 = NumericString(maybeExpiry);
                    bool ok3 = !NumericString(maybeLot);
                    if (ok1 & ok2 & ok3)
                        result = "007"; // BARD France Company (Example: 58562152ANTL0294122012)
                }

                if (code.Length == 28)
                {
                    bool ok = NumericString(code);
                    if (ok)
                        result = "008"; // PHYSIOL France Company (Example: 2808123005365310060911306301)
                }                                                   //  28081230 053653 10060911306301

                if (code.Length >= 8)
                {
                    if (!NumericString(code) & (code.Substring(0, 4) == "PAR-"))
                        result = "009"; //  Arthrex Company (Example: PAR-1234-AB)
                }

                if (code.Length == 7)
                {
                    if (NumericString(code.Substring(1, 6)) & (code.Substring(0, 1) == "T"))
                        result = "010"; //  Arthrex Company (Example: T123456)
                }

                if (code.Length == 2)
                {
                    if (NumericString(code.Substring(1, 1)) & (code.Substring(0, 1) == "Q"))
                        result = "011"; //  Arthrex Company (Example: Q1)
                }

                //if (code.Length > 10)
                //{
                //  if (NumericString(code.Substring(3, 6)) & (code.Substring(0, 3) == "SEM") & (code.Substring(9, 1) == "^"))
                //    result = "012"; //  SEM (Sciences Et Medecine) Company (Example: SEM171252^P30778E4009A)
                //}

                if (code.Length > 10)
                {
                    if ((code.Substring(0, 3) == "SEM") & (code.Substring(9, 2) == "^P") & (Regex.IsMatch(code.Substring(code.Length - 1, 1), @"^[a-zA-Z]+$")))
                        result = "012"; //  SEM (Sciences Et Medecine) Company (Example: SEM171252^P30778E4009A)
                }

                if (code.Length == 14)
                {
                    if (NumericString(code.Substring(6, 8)) & (code.Substring(0, 1) == " ") & (code.Substring(5, 1) == "-"))
                        result = "013"; //   ABS BOLTON Company (Example:  BF01-11018180)
                }

                if (code.Length == 10)
                {
                    if (NumericString(code.Substring(5, 5)) & (code.Substring(0, 5) == "CPDR "))
                        result = "014"; //   CHIRURGIE OUEST / EUROSILICONE / SORMED Company (Example: CPDR 24602)
                }
                if (code.Length == 17)
                {
                    if ((code.Substring(4, 1) == "-") & (code.Substring(15, 1) == "-"))
                        result = "015"; // Symbios Orthopédie (Example: H080-25.01.2014-1)
                }
            }
            return result;
        }
    }



    public class Ean128
    {
        public string Arti;
        public string Lote;
        public string FechaCad;
        public int qty;
        public int units;
        public double peso;
        public bool ok;
        public String MsgErr { get; set; }

        private Hashtable aiinfo = new Hashtable();
        private class AII
        {
            public int minLength;
            public int maxLength;
            public AII(String id, int minLength, int maxLength)
            {
                this.minLength = minLength;
                this.maxLength = maxLength;
            }
        }
        private void ai(String id, int minLength, int maxLength)
        {
            aiinfo.Add(id, new AII(id, minLength, maxLength));
        }
        private void ai(String id, int length)
        {
            aiinfo.Add(id, new AII(id, length, length));
        }

        public Ean128()
        {
            ai("00", 18, 18);   //Serial Shiping container Code  
            ai("01", 14);     //Shipping code Dun 14
            ai("02", 14);     // Numbers of containers defined in AI37
            ai("10", 3, 20);  // Alfanumeric Lote   
            ai("11", 6);     // Production Date
            ai("12", 6);     // Vencimiento Date
            ai("13", 6);     // Packaging date
            ai("15", 6);     // Sell by date
            ai("17", 6);     // Expiration date
            ai("20", 2);     // Product Variant
            ai("21", 3, 20);     // serial Number
            ai("22", 6, 29);    // Cantidad, fecha , lteo HIBCC
            ai("30", 1, 8);     // Quantity
            ai("37", 1, 8);     // Number units contained
            // TODO: continue according to http://en.wikipedia.org/wiki/GS1-128   
        }
        /**    * Decodes a Unicode string from a Code128-like encoding.    
         * 
         *    * @param fnc1 The character that represents FNC1.    */
        public bool Parse(String s, char fnc1, string mascara = "", bool esPeso = false)
        {
            if (s.Length > 13)
            {
                ok = true;
                String ai = "";
                int index = 0;
                peso = 0;
                while (index < s.Length)
                {
                    ai += s.Substring(index++, 1);
                    AII info;
                    try
                    {
                        info = (AII)aiinfo[ai];
                    }
                    catch (Exception)
                    {
                        info = null;
                    }
                    if (info != null)
                    {
                        string value = "";
                        for (int i = 0; i < info.maxLength && index < s.Length; i++)
                        {
                            char c = s[index++];
                            if (c == fnc1)
                            {
                                break;
                            }
                            value += c;
                        }
                        if (value.Length < info.minLength)
                        {
                            MsgErr = "Short field for AI \"" + ai + "\": \"" + value + "\".";
                            ok = false;
                            break;
                            //return false;
                        }
                        switch (ai)
                        {
                            case "01":
                                Arti = value;
                                break;
                            case "10":
                                Lote = value;
                                break;
                            case "21": // El número de serie se devuelve como lote
                                Lote = value;
                                break;
                            case "17":
                                FechaCad = value;
                                break;
                            case "30":
                                qty = int.Parse(value);
                                break;
                            case "37":
                                units = int.Parse(value);
                                break;
                            case "22":
                                FechaCad = value.Substring(2, 2) + value.Substring(0, 2) + "01";
                                Lote = value.Substring(4, value.Length - 5);
                                break;
                            default:
                                break;
                        }
                        ai = "";
                    }
                }
                if (ai.Length > 0)
                {
                    MsgErr = "Unknown AI \"" + ai + "\".";
                    ok = false;

                    //return false;
                }
            }
            if (!ok)
            {
                //Intentar el parse por otros metodos
                ParserIO_func Parser = new ParserIO_func();
                string type = Parser.Type(s);
                if (type == "HIBC-") // le falta el checkdigit
                {
                    type = "HIBC";
                    s = s + "00"; //Añadimos 2 digitos para que obtenga bien el lote
                }

                string subType = Parser.SubType(s, type);
                if (type != "NaS")
                {
                    if (type == "HIBC")
                    {
                        Arti = Parser.LIC(s, type, subType) + Parser.PCN(s, type, subType);
                    }
                    else if (type == "GS1-128")
                    {
                        Arti = Parser.GTIN(s, type, subType);
                    }
                    else
                    {
                        Arti = Parser.Reference(s, type, subType);
                    }
                    Lote = Parser.Lot(s, type, subType);
                    FechaCad = Parser.NormalizedExpiry(s, type, subType);
                    if (FechaCad != "")
                        FechaCad = FechaCad.Substring(2);
                    qty = int.Parse(Parser.Quantity(s, type, subType) == "" ? "0" : Parser.Quantity(s, type, subType));
                    ok = true;
                }
                else
                {
                    //mirar si es el formato de medennium
                    if (s.IndexOf("X") == 8 && s.IndexOf("X", 9) > 9)
                    {
                        Lote = s.Substring(0, 8);
                        FechaCad = s.Substring(s.IndexOf("X", 9) + 1);
                        FechaCad = FechaCad.Substring(4, 2) + FechaCad.Substring(0, 2) + "01";
                        ok = true;
                    }
                }

            }
            if ((!string.IsNullOrEmpty(Arti) || !string.IsNullOrEmpty(s)) && !string.IsNullOrEmpty(mascara))
            {
                try
                {
                    if (string.IsNullOrEmpty(Arti))
                    {
                        Arti = s;
                        ok = true;
                    }
                    string bin = mascara.Substring(0, mascara.IndexOf("?"));
                    if (Arti.Substring(0, bin.Length) == bin && Arti.Length == mascara.Length) // Tratamos el pesable
                    {
                        string ean = bin;
                        ean += Arti.Substring(mascara.IndexOf("?"), mascara.IndexOf("-") - mascara.IndexOf("?")); //pone el articulo generico
                        ean += new string('0', Arti.Length - ean.Length);
                        ean = ean.Substring(0, ean.Length - 1) + Checkdigit(ean);
                        if (esPeso) //obtengo la cantidad de la etiqueta
                        {
                            string canteti = Arti.Substring(mascara.IndexOf('-'), mascara.IndexOf('%') - mascara.IndexOf('-'));
                            peso = double.Parse(canteti);
                        }
                        Arti = ean;

                    }
                }
                catch (Exception)
                {


                }

            }
            return true;
        }


        public static string Checkdigit(string code)
        {
            if (code != (new Regex("[^0-9]")).Replace(code, ""))
            {
                // is not numeric
                return "";
            }
            // pad with zeros to lengthen to 14 digits
            switch (code.Length)
            {
                case 8:
                    code = "000000" + code;
                    break;
                case 12:
                    code = "00" + code;
                    break;
                case 13:
                    code = "0" + code;
                    break;
                case 14:
                    break;
                default:
                    // wrong number of digits
                    return "";
            }
            // calculate check digit
            int[] a = new int[13];
            a[0] = int.Parse(code[0].ToString()) * 3;
            a[1] = int.Parse(code[1].ToString());
            a[2] = int.Parse(code[2].ToString()) * 3;
            a[3] = int.Parse(code[3].ToString());
            a[4] = int.Parse(code[4].ToString()) * 3;
            a[5] = int.Parse(code[5].ToString());
            a[6] = int.Parse(code[6].ToString()) * 3;
            a[7] = int.Parse(code[7].ToString());
            a[8] = int.Parse(code[8].ToString()) * 3;
            a[9] = int.Parse(code[9].ToString());
            a[10] = int.Parse(code[10].ToString()) * 3;
            a[11] = int.Parse(code[11].ToString());
            a[12] = int.Parse(code[12].ToString()) * 3;
            int sum = a[0] + a[1] + a[2] + a[3] + a[4] + a[5] + a[6] + a[7] + a[8] + a[9] + a[10] + a[11] + a[12];
            int check = (10 - (sum % 10)) % 10;
            return check.ToString();
        }



    }

    public class Articulo
    {
        public Articulo()
        {
            Formatos = new List<Formato>();
            ok = false;
        }
        public String ItemCode { get; set; }
        public String ItemName { get; set; }
        public Boolean GestLotes { get; set; }
        public Boolean GestNSerie { get; set; }
        public String UbiFig { get; set; }
        public Formato FormCompra { get; set; }
        public Formato FormVenda { get; set; }
        public List<Formato> Formatos { get; set; }
        public decimal Stock1 { get; set; }
        public decimal Stock2 { get; set; }
        public Boolean ok { get; set; }
        public int CodErr { get; set; }
        public String MsgErr { get; set; }
        public void CalcularCantidades()
        {
            Int16 li;
            for (li = 0; li < Formatos.Count; li++)
            {
                if (string.IsNullOrWhiteSpace(Formatos[li].CodigoInf))
                    RecalCantUni(li);
            }

        }
        private void RecalCantUni(int idx)
        {
            int li;
            for (li = 0; li < Formatos.Count; li++)
                if (Formatos[li].CodigoInf == Formatos[idx].Codigo && li != idx)
                {
                    Formatos[li].CantUni = Formatos[idx].CantUni * Formatos[li].Cant;
                    RecalCantUni(li);
                }
        }
    }

}


