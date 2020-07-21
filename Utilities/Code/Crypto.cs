using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using lib = CoP.Enterprise.Utilities;
using CfgMgr = System.Configuration.ConfigurationManager;

namespace CoP.Enterprise
{
    public static class Crypto
    {
        #region Old 'Roll yr Own' Krypt Code

        private const string KEY =
            "gh&HdAfR5%^%)0(hDdGAgsS%S^6*SaRs*(7jE^^CGO6sMa12C~a67s6hsf";

        // HexChars Structure for passing High and Low byte Values
        private struct HexChars
        {
            public char cLo;
            public char cHi;
        }

        private static HexChars GetKryptHexChars(int iVal)
        {
            int iHi = iVal/16,
                iLo = iVal%16;
            var sOut = new HexChars
            {
                cLo = (char) (iLo + ((iLo <= 9) ? 48 : 55)),
                cHi = (char) (iHi + ((iHi <= 9) ? 48 : 55))
            };
            return sOut;
        }

        /// <summary>
        /// Calculates Dimensions to use for encryption based on length of message
        /// </summary>
        /// <param name="iLen">Length of string to be encrypted</param>
        /// <param name="iDim">Out parameter, vertical (smaller) dimension</param>
        /// <param name="ihDim">Out parameter, horizontal (larger) dimension</param>
        /// <param name="iRem">Out parameter, Remainder</param>
        private static void GetKryptDims(
            int iLen, out int iDim,
            out int ihDim, out int iRem)
        {
            iDim = (int) Math.Sqrt(iLen);
            ihDim = (iLen >= (iDim*(iDim + 1)))
                ? iDim + 1
                : iDim;
            iRem = iLen - (iDim*ihDim);
        }

        private static int GetKeySig(string sKey)
        {
            int iOut = 0;
            for (int i = 0; i < sKey.Length; i++)
            {
                iOut += Convert.ToChar(sKey.Substring(i, 1));
                iOut %= 255;
            }
            return iOut;
        }

        private static int GetKryptVal(char cHi, char cLo)
        {
            return GetKryptVal((byte) cHi, (byte) cLo);
        }

        private static int GetKryptVal(byte bHi, byte bLo)
        {
            return (bHi - ((bHi < 58) ? 48 : 55))*16 +
                   (bLo - ((bLo < 58) ? 48 : 55));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hc">HexChars Structure containing 2 Hex Characters as bLo, and bHi</param>
        /// <returns>byte (char) represented by the two Hex Chars</returns>
        private static int GetKryptVal(HexChars hc)
        {
            var bLo = (byte) hc.cLo;
            var bHi = (byte) hc.cHi;
            return GetKryptVal(bHi, bLo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sV">Stringbuilder containing entire input string</param>
        /// <param name="sK">Stringbuilder containing encryption Key</param>
        /// <param name="iPos">Character within Input String to Encrypt</param>
        /// <returns>HexChars struct containing two Hex characters</returns>
        private static HexChars CharEncrypt(
            StringBuilder sV, StringBuilder sK, int iPos)
        {
            var vCh = Convert.ToChar(sV[iPos]);
            var iCh = Convert.ToInt32((byte) vCh);
            var kCh = Convert.ToChar(sK[iPos%sK.Length]);
            iCh = Convert.ToInt32((iCh + Convert.ToInt16((byte) kCh))%256);
            return GetKryptHexChars(iCh);
        }

        // ***************************************

        // ***************************************
        // Encrypt Functions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sVal">String to encrypt</param>
        /// <returns>Encrypted string</returns>
        public static string Krypt(string sVal)
        {
            return Krypt(sVal, KEY);
        }

        /// <summary>
        /// /
        /// </summary>
        /// <param name="sVal">String to encrypt</param>
        /// <param name="sKey">Key to use during encryption</param>
        /// <returns>Encrypted string</returns>
        public static string Krypt(string sVal, string sKey)
        {
            if (string.IsNullOrEmpty(sKey)) sKey = KEY;
            var iLen = sVal.Length;
            int iDim, ihDim, iRem;
            GetKryptDims(iLen, out iDim, out ihDim, out iRem);
            StringBuilder
                sv = new StringBuilder(sVal),
                sK = new StringBuilder(sKey),
                sOut = new StringBuilder(2*iLen + 2);
            var hc = GetKryptHexChars(GetKeySig(sKey));
            sOut.Append(hc.cHi);
            sOut.Append(hc.cLo);
            for (var j = 0; j < ihDim; j++)
            {
                int iPos;
                for (int i = 0; i < iDim; i++)
                {
                    iPos = (i*ihDim) + j;
                    var hcOut = CharEncrypt(sv, sK, iPos);
                    sOut.Append(hcOut.cHi);
                    sOut.Append(hcOut.cLo);
                }
                if (j >= iRem) continue;
                iPos = (iDim*ihDim) + j;
                var lstOut = CharEncrypt(sv, sK, iPos);
                sOut.Append(lstOut.cHi);
                sOut.Append(lstOut.cLo);
            }
            return (sOut.ToString());
        }

        // ********************************************************
        // UnEncrypt Functions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sVal">String to decrypt</param>
        /// <param name="sKey">Key used to encrypt string</param>
        /// <returns>decrypted string</returns>
        public static string Dekrypt(string sVal, string sKey = KEY)
        {
            if (string.IsNullOrEmpty(sKey)) sKey = KEY;
            var iLen = (sVal.Length/2) - 1;
            var k = 0;
            int iDim, ihDim, iRem;
            var hc = new HexChars();
            if (GetKryptVal(Convert.ToChar(sVal.Substring(0, 1)),
                Convert.ToChar(sVal.Substring(1, 1))) !=
                GetKeySig(sKey))
                throw new ArgumentException("Invalid/Incorrect Key");
            GetKryptDims(iLen, out iDim, out ihDim, out iRem);
            StringBuilder
                sv = new StringBuilder(sVal.Substring(2)),
                sk = new StringBuilder(sKey),
                sbOut = new StringBuilder(iLen);
            for (int i = 0; i <= iDim; i++)
            {
                int jMax = (i == iDim) ? iRem : ihDim;
                for (int j = 0; j < jMax; j++)
                {
                    int iPos = (j <= iRem)
                        ? j*(iDim + 1) + i
                        : iRem + (j*iDim) + i;
                    int iKeyChar = Convert.ToInt32(sk[k++%sk.Length]);
                    hc.cHi = Convert.ToChar(sv[2*iPos]);
                    hc.cLo = Convert.ToChar(sv[2*iPos + 1]);
                    int iChar = (GetKryptVal(hc) - iKeyChar + 256)%256;
                    sbOut.Append((char) iChar);
                }
            }
            return sbOut.ToString();
        }

        #endregion

        #region dotNet API Crypto Code

        // Eight/twenty-four bytes randomly selected for the Key and 
        // eight for the Initialization Vector (IV).  
        // An eight byte IV is used to encrypt the first block of text  
        // so that repetitive patterns are not apparent.

        // **** Default DES and Triple-DES Keys and IVs *****
        private static readonly string CNFGKey = CfgMgr.AppSettings["encryptionKey"];
        private static readonly string CNFGIV  = CfgMgr.AppSettings["encryptionIV"];

        #region Standard DES
        #region Standard DES Key/IV  [S For standard DES Encryption]
        private static readonly byte[]
            SKEY = {27, 88, 201, 54, 112, 8, 151, 44},
            SIV = {78, 233, 246, 23, 9, 56, 177, 5};
        #endregion Standard DES Key IV

        #region Standard DES Encryption
        public static string Encrypt(string srcValue, HashAlg? ha = null)
        {
            byte[] bKEY = string.IsNullOrEmpty(CNFGKey) ?
                        SKEY : SizeKey(CNFGKey, 8, ha),
                   bIV = string.IsNullOrEmpty(CNFGIV) ?
                        SIV : SizeKey(CNFGIV, 8, ha);
            return string.IsNullOrWhiteSpace(srcValue) ? null :
                    (Encrypt(srcValue, bKEY, bIV));
        }

        public static string Encrypt(string srcValue,
            string sKey, string sIV, HashAlg? ha = null)
        {
            var cgfKey = sKey ?? CNFGKey;
            var cgfIV = sIV ?? CNFGIV;
            // -----------------------
            byte[] bKEY = string.IsNullOrEmpty(cgfKey) ? 
                    SKEY: SizeKey(cgfKey, 8, ha),
                bIV = string.IsNullOrEmpty(cgfIV)?
                     SIV: SizeKey(cgfIV, 8, ha);
            return string.IsNullOrWhiteSpace(srcValue) ? null :
                    (Encrypt(srcValue, bKEY, bIV));
        }

        public static string Encrypt(string srcValue, string sCompany,
            string sApplication, APPENV Environment, HashAlg? ha = null)
        {
            switch (Environment)
            {
                case (APPENV.NA):
                    return (Encrypt(srcValue, SKEY, SIV));

                default:
                    var sMode = lib.GetAppMode(Environment);
                    var sc = new StringBuilder(
                        sCompany + "/" + sApplication);
                    var secKey = sc.ToString();

                    var AppS = (IDictionary) CfgMgr.GetSection(secKey);
                    string sKEY = (string) AppS[sMode + "_KEY"],
                        sIV = (string) AppS[sMode + "_IV"];

                    return string.IsNullOrWhiteSpace(srcValue) ? null :
                        (sKEY == null || sIV == null)?
                         Encrypt(srcValue, SKEY, SIV):
                         Encrypt(srcValue,
                            SizeKey(sKEY, 8, ha),
                            SizeKey(sIV, 8, ha));
            }
        }

        private static string Encrypt(string srcValue, byte[] Key, byte[] IV)
        {
            if (string.IsNullOrEmpty(srcValue)) return "";
            using (var ms = new MemoryStream())
            {
                var cryptoProvider = new DESCryptoServiceProvider();
                var ICT = cryptoProvider.CreateEncryptor(Key, IV);
                var cs = new CryptoStream(ms, ICT, CryptoStreamMode.Write);
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(srcValue);
                    sw.Flush();
                    cs.FlushFinalBlock();
                    ms.Flush();
                    return string.IsNullOrWhiteSpace(srcValue) ? null :
                        (Convert.ToBase64String(ms.GetBuffer(), 0, (int) ms.Length));
                }
            }
        }

        #endregion Standard DES Encryption

        #region Standard DES Decryption

        public static string Decrypt(string srcValue, 
            string sKey, string sIV, HashAlg? ha = null)
        {
            var cgfKey = sKey ?? CNFGKey;
            var cgfIV = sIV ?? CNFGIV;
            // -----------------------
            byte[] bKEY = string.IsNullOrEmpty(cgfKey) ?
                 SKEY: SizeKey(cgfKey, 8, ha),
                bIV = string.IsNullOrEmpty(cgfIV) ?
                     SIV: SizeKey(cgfIV, 8, ha);
            return (Decrypt(srcValue, bKEY, bIV));
        }

        public static string Decrypt(string srcValue, HashAlg? ha = null)
        {
            byte[] bKEY = string.IsNullOrEmpty(CNFGKey) ?
                        SKEY : SizeKey(CNFGKey, 8, ha),
                   bIV = string.IsNullOrEmpty(CNFGIV) ?
                        SIV : SizeKey(CNFGIV, 8, ha);
            return (Decrypt(srcValue, bKEY, bIV));
        }

        public static string Decrypt(string srcValue,
            string sCompany, string sApplication, APPENV Environment, HashAlg? ha = null)
        {
            switch (Environment)
            {
                case (APPENV.NA):
                    return (Decrypt(srcValue, SKEY, SIV));

                default:
                    var sMode = lib.GetAppMode(Environment);
                    var sc = new StringBuilder(
                        sCompany + "/" + sApplication);
                    var secKey = sc.ToString();

                    var AppS = (IDictionary) CfgMgr.GetSection(secKey);
                    string sKEY = (string) AppS[sMode + "_KEY"],
                        sIV = (string) AppS[sMode + "_IV"];

                    return (sKEY == null || sIV == null)
                        ? Decrypt(srcValue, SKEY, SIV)
                        : Decrypt(srcValue,
                            SizeKey(sKEY, 8, ha),
                            SizeKey(sIV, 8, ha));
            }
        }

        private static string Decrypt(string srcValue, byte[] Key, byte[] IV)
        {
            var buffer = Convert.FromBase64String(srcValue);
            using (var ms = new MemoryStream(buffer))
            {
                var cryptoProvider = new DESCryptoServiceProvider();
                var ICT = cryptoProvider.CreateDecryptor(Key, IV);
                var cs = new CryptoStream(ms, ICT, CryptoStreamMode.Read);
                using (var sr = new StreamReader(cs)) return (sr.ReadToEnd());
            }
        }

        #endregion Standard DES Decryption

        #endregion Standard DES
        #region Triple DES
        #region Triple DES Key/IV [T For Triple DES Encryption]
        private static readonly byte[]
            TKEY = {
                69, 114, 102, 119, 101, 105, 108, 101,
                114, 76, 101, 109, 98, 101, 114, 103,
                80, 104, 97, 110, 116, 111, 109, 50 },
            TIV = { 11, 163, 216, 99, 64, 99, 237, 52 };
        #endregion Triple DES Key IV

        #region Triple DES Encryption

        public static string EncryptTripleDES(string srcValue, HashAlg? ha = null)
        {
            byte[] bKEY = string.IsNullOrEmpty(CNFGKey)? 
                        TKEY: SizeKey(CNFGKey, 24, ha),
                   bIV = string.IsNullOrEmpty(CNFGIV) ?
                        TIV : SizeKey(CNFGIV, 8, ha);
            return string.IsNullOrWhiteSpace(srcValue)? null:
                    (EncryptTripleDES(srcValue, bKEY, bIV));
        }

        public static string EncryptTripleDES(string srcValue, 
            string sKey, string sIV, HashAlg? ha = null)
        {
            var cgfKey = sKey ?? CNFGKey;
            var cgfIV = sIV ?? CNFGIV;
            // -----------------------
            byte[] bKEY = string.IsNullOrEmpty(cgfKey)? 
                    TKEY: SizeKey(cgfKey, 24, ha),
                bIV = string.IsNullOrEmpty(cgfIV)?
                    TIV: SizeKey(cgfIV, 8, ha);
            return string.IsNullOrWhiteSpace(srcValue) ? null :
                    (EncryptTripleDES(srcValue, bKEY, bIV));
        }

        public static string EncryptTripleDES(string srcValue,
            string sCompany, string sApplication, 
            APPENV Environment, HashAlg? ha = null)
        {
            switch (Environment)
            {
                case (APPENV.NA):
                    return (Encrypt(srcValue, TKEY, TIV));

                default:
                    var sMode = lib.GetAppMode(Environment);
                    var sc = new StringBuilder(
                        sCompany + "/" + sApplication);
                    var secKey = sc.ToString();

                    var AppS = (IDictionary) CfgMgr.GetSection(secKey);
                    string sKEY = (string) AppS[sMode + "_KEY"],
                        sIV = (string) AppS[sMode + "_IV"];

                    return string.IsNullOrWhiteSpace(srcValue) ? null :
                        (sKEY == null || sIV == null)?
                        EncryptTripleDES(srcValue, TKEY, TIV): 
                        EncryptTripleDES(srcValue,
                            SizeKey(sKEY, 24, ha),
                            SizeKey(sIV, 8, ha));
            }
        }

        private static string EncryptTripleDES(string srcValue, byte[] Key, byte[] IV)
        {
            if (string.IsNullOrEmpty(srcValue)) return "";
            using (var ms = new MemoryStream())
            {
                var cP = new TripleDESCryptoServiceProvider();
                var ICT = cP.CreateEncryptor(Key, IV);
                var cs = new CryptoStream(ms, ICT, CryptoStreamMode.Write);
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(srcValue);
                    sw.Flush();
                    cs.FlushFinalBlock();
                    ms.Flush();
                    return string.IsNullOrWhiteSpace(srcValue) ? null :
                        (Convert.ToBase64String(ms.GetBuffer(), 0, (int) ms.Length));
                }
            }
        }
        #endregion Triple DES Encryption

        #region Triple DES Decryption

        public static string DecryptTripleDES(string srcValue,
             string sKey, string sIV, HashAlg? ha = null)
        {
            var cgfKey = sKey ?? CNFGKey;
            var cgfIV = sIV ?? CNFGIV;
            // -----------------------
            byte[] bKEY = string.IsNullOrEmpty(cgfKey) ?
                    TKEY : SizeKey(cgfKey, 24, ha),
                bIV = string.IsNullOrEmpty(cgfIV) ?
                    TIV : SizeKey(cgfIV, 8, ha);
            return (DecryptTripleDES(srcValue, bKEY, bIV));
        }

        public static string DecryptTripleDES(string srcValue, HashAlg? ha = null)
        {
            byte[] bKEY = string.IsNullOrEmpty(CNFGKey) ?
                        SKEY : SizeKey(CNFGKey, 24, ha),
                   bIV = string.IsNullOrEmpty(CNFGIV) ?
                        SIV : SizeKey(CNFGIV, 8, ha);
            return (DecryptTripleDES(srcValue, bKEY, bIV));
        }

        public static string DecryptTripleDES(string srcValue,
            string sCompany, string sApplication, APPENV Environment,
            HashAlg? ha = null)
        {
            switch (Environment)
            {
                case (APPENV.NA):
                    return (DecryptTripleDES(srcValue, TKEY, TIV));

                default:
                    var sMode = lib.GetAppMode(Environment);
                    var sc = new StringBuilder(
                        sCompany + "/" + sApplication);
                    var secKey = sc.ToString();

                    var AppS = (IDictionary) CfgMgr.GetSection(secKey);
                    string sKEY = (string) AppS[sMode + "_KEY"],
                        sIV = (string) AppS[sMode + "_IV"];

                    return (sKEY == null || sIV == null)
                        ? DecryptTripleDES(srcValue, TKEY, TIV)
                        : DecryptTripleDES(srcValue,
                            SizeKey(sKEY, 24, ha),
                            SizeKey(sIV, 8, ha));
            }
        }

        private static string DecryptTripleDES(string srcValue, byte[] Key, byte[] IV)
        {
            var buffer = Convert.FromBase64String(srcValue);
            using (var ms = new MemoryStream(buffer))
            {
                var cP = new TripleDESCryptoServiceProvider();
                var ICT = cP.CreateDecryptor(Key, IV);
                var cs = new CryptoStream(ms, ICT, CryptoStreamMode.Read);
                using (var sr = new StreamReader(cs)) return (sr.ReadToEnd());
            }
        }

        #endregion Triple DES Decryption

        #endregion Triple DES

        private static byte[] SizeKey(string Key, int size, HashAlg? ha = null)
        {
            if (string.IsNullOrEmpty(Key))
                throw new ArgumentException("Key was null or empty.");
            // -------------------------------------------------------

            if (Key.Length < size) return SizeKey(Key.PadRight(size, '-'), size, ha);
            var hash = ha.GetValueOrDefault(HashAlg.MD5);
            var hA2Use =
                hash == HashAlg.RIPED160
                    ? new RIPEMD160Managed()
                    : hash == HashAlg.SHA256
                        ? new SHA256Managed()
                        : hash == HashAlg.SHA384
                            ? new SHA384Managed()
                            : hash == HashAlg.SHA512
                                ? new SHA512Managed()
                                : new MD5CryptoServiceProvider() as HashAlgorithm;
            var newKey = new byte[size];
            var chunkSiz = (Key.Length/size);
            for (var i = 0; i < size; i++)
            {
                var s = Key.Substring(i*chunkSiz, chunkSiz);
                var bs = Encoding.UTF8.GetBytes(s);
                bs = hA2Use.ComputeHash(bs);
                foreach (var b in bs) newKey[i] = (byte) (newKey[i] + b);
            }
            return newKey;
        }

        #endregion

        #region Composite Crypto

        public static string EncryptComposite(string srcValue)
        {
            return Krypt(EncryptTripleDES(srcValue, TKEY, TIV), KEY);
        }

        public static string EncryptComposite(string srcValue, string sKey, string sIV)
        {
            return Krypt(EncryptTripleDES(srcValue, sKey, sIV), sKey);
        }

        public static string DecryptComposite(string srcValue)
        {
            return DecryptTripleDES(Dekrypt(srcValue, KEY), TKEY, TIV);
        }

        public static string DecryptComposite(string srcValue, string sKey, string sIV)
        {
            return DecryptTripleDES(Dekrypt(srcValue, sKey), sKey, sIV);
        }

        #endregion Composite Crypto
    }
}