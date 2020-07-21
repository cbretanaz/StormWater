using System;
using System.Text;
using System.Xml;

namespace CoP.Enterprise
{
    namespace Xml
    {
        public enum ddSetBy : byte { Value, Text, SelectedIndex }

        #region Extend XmlElement, XmlDocument classes
        public class bpXmlDoc : XmlDocument
        {
            int elementCount;
            public bpXmlDoc() { elementCount = 0; }
            public bpXmlDoc(XmlNode newroot)
            {
                elementCount = 0;
                Initialize(newroot.OuterXml);
            }
            public static string CreateCData(string text)
            {
                bpXmlDoc doc = new bpXmlDoc();
                return doc.CreateCDataSection(text).OuterXml;
            }

            private void Initialize(string outrXml)
            { LoadXml(outrXml); }
            public bpElement SelectElement(string sXPath)
            { return ((bpElement)SelectSingleNode(sXPath)); }
            public XmlNodeList SelectElements(string sXPath)
            { return SelectNodes(sXPath); }
            public override XmlElement CreateElement(string prefix, string localname, string nsURI)
            { return (new bpElement(prefix, localname, nsURI, this)); }
            public bpElement CreateAIElement(string prefix, string localname, string nsURI)
            { return (new bpElement(prefix, localname, nsURI, this)); }
            public new bpElement DocumentElement { get { return ((bpElement)(base.DocumentElement)); } }
            public void LoadXml(bpElement elm) { LoadXml(elm.OuterXml); }
            public void IncrementElementCount() { elementCount++; }
            public int GetCount() { return (elementCount); }
        }
        // --------------------------------------

        public class bpElement : XmlElement
        {
            internal bpElement(string prefix, string localname,
                string nsURI, XmlDocument doc)
                : base(prefix, localname, nsURI, doc)
            { ((bpXmlDoc)doc).IncrementElementCount(); }

            public bpElement SelectElement(string sXPath)
            { return ((bpElement)SelectSingleNode(sXPath)); }
            public XmlNodeList SelectElements(string sXPath)
            { return SelectNodes(sXPath); }
            public bpElement ParentElement { get { return ((bpElement)ParentNode); } }


            #region KillNode Overloads
            /// <summary>
            /// Removes this element and all child nodes
            /// </summary>
            public void Terminate() { Kill(); }
            public void Remove() { Kill(); }
            /// <summary>
            /// Kills (Removes) this element and all child nodes
            /// </summary>
            public void Kill() { ParentElement.RemoveChild(this); }
            public void RemoveChild(string sElmName)
            {
                bpElement Child = SelectElement(sElmName);
                if (Child != null) Child.Kill();
            }

            #endregion KillNode Overloads

            #region CreateAndAppend bpElement OverLoads
            public bpElement CreateAndAppendChild(string sName)
            {
                return ((bpElement)AppendChild(
                    OwnerDocument.CreateElement(sName)));
            }

            public bpElement CreateAndAppendChild(string sName,
                string sAttribName, string sAttribValue)
            {
                bpElement elm = CreateAndAppendChild(sName);
                elm.SetAttribute(sAttribName, sAttribValue);
                return (elm);
            }
            #endregion CreateAndAppend OverLoads

            #region GetOrCreate bpElement OverLoads
            public bpElement GetOrCreateElementPath(string sElmPath, string sAttribName,
                string sAttribValue)
            {
                string[] aElmPath = sElmPath.Split('/');
                return (GetOrCreateElementPath(aElmPath, sAttribName, sAttribValue));
            }
            public bpElement GetOrCreateElementPath(string[] sElmPath, string sAttribName,
                string sAttribValue)
            {
                int iUprBnd = sElmPath.GetUpperBound(0);
                bpElement elChild = this;
                for (int i = 0; i < iUprBnd; i++)
                    elChild = elChild.GetOrCreateChildElement(sElmPath[i]);
                return (elChild.GetOrCreateChildElement(sElmPath[iUprBnd], sAttribName, sAttribValue));
            }
            public bpElement GetOrCreateElementPath(string sElmPath)
            {
                string[] aElmPath = sElmPath.Split('/');
                return (GetOrCreateElementPath(aElmPath));
            }
            public bpElement GetOrCreateElementPath(string[] sElmPath)
            {
                bpElement elChild = this;
                foreach (string elmName in sElmPath)
                    elChild = elChild.GetOrCreateChildElement(elmName);
                return (elChild);
            }

            public bpElement GetorCreateChildElement(string sName)
            { return (GetOrCreateChildElement(sName)); }

            public bpElement GetOrCreateChildElement(
                string sName, string sAttribName, string sAttribValue)
            {
                string sXPath = sName + "[@" + sAttribName + "='" + sAttribValue + "']";
                bpElement elm = SelectElement(sXPath);
                if (elm == null)
                {
                    elm = CreateAndAppendChild(sName);
                    elm.SetAttribute(sAttribName, sAttribValue);
                }
                return (elm);
            }
            public bpElement GetOrCreateChildElement(string sName)
            {
                return SelectElement(sName) ??
                    (bpElement)AppendChild(
                        OwnerDocument.CreateElement(sName));
            }

            #endregion GetOrCreate OverLoads

            #region Create/Append CDataSection
            public void CreateAndAppendCData(string text)
            { InnerXml = OwnerDocument.CreateCDataSection(text).OuterXml; }
            #endregion Create/Append CDataSection

            #region GetAttribute OverLoads

            #region Generics
            /***********
            public T GetAttribute<T>(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider, bool throwIfBlankMissing) where T : IConvertible
                { return (GetAttribute<T>(atttibuteName, style, provider, throwIfBlankMissing, 0)); }
            public T GetAttribute<T>(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider) where T : IConvertible
                { return (GetAttribute<T>(atttibuteName, style, provider, false, 0)); }
            public T GetAttribute<T>(string atttibuteName,
                IFormatProvider provider, bool throwIfBlankMissing) where T : IConvertible
                { return (GetAttribute<T>(atttibuteName, provider, throwIfBlankMissing, 0)); }
            public T GetAttribute<T>(string atttibuteName, IFormatProvider provider) where T : IConvertible
                { return (GetAttribute<T>(atttibuteName, provider, false, 0)); }
            public T GetAttribute<T>(string atttibuteName,
                System.Globalization.NumberStyles style, bool throwIfBlankMissing) where T : IConvertible
                { return (GetAttribute<T>(atttibuteName, throwIfBlankMissing, 0)); }
            public T GetAttribute<T>(string atttibuteName, System.Globalization.NumberStyles style) where T : IConvertible
                { return (GetInt16Attribute(atttibuteName, style, false, 0)); }
            public T GetAttribute<T>(string atttibuteName, bool throwIfBlankMissing) where T : IConvertible
                { return (GetAttribute<T>(atttibuteName, throwIfBlankMissing, 0)); }
            public T GetAttribute<T>(string atttibuteName) where T : IConvertible
                { return (GetAttribute<T>(atttibuteName, false, 0)); }
            // -----------------------------------------------------------
            public T GetAttribute<T>(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider, bool throwIfBlankMissing, T defaultVal) where T : IConvertible
			{
				if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
				{
					if (throwIfBlankMissing)
						throw new ArgumentException("XmlElement attribute " + 
							atttibuteName + " is blank or missing.");
					else return defaultVal;
				}
				return (Int16.Parse(GetAttribute(atttibuteName), style, provider));
			}
            public T GetAttribute<T>(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider, Int16 defaultVal) where T : IConvertible
            { return (GetAttribute<T>(atttibuteName, style, provider, false, defaultVal)); }
            public T GetAttribute<T>(string atttibuteName,
                IFormatProvider provider, bool throwIfBlankMissing, T defaultVal) where T : IConvertible
			{
				if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
				{
					if (throwIfBlankMissing)
						throw new ArgumentException("XmlElement attribute " + 
							atttibuteName + " is blank or missing.");
					else return defaultVal;
				}
				return (Int16.Parse(GetAttribute(atttibuteName), provider));
			}
            public T GetAttribute<T>(string atttibuteName, IFormatProvider provider, Int16 defaultVal)
            { return (GetAttribute<T>(atttibuteName, provider, false, defaultVal)); }
            public T GetAttribute<T>(string atttibuteName, System.Globalization.NumberStyles style,
                 bool throwIfBlankMissing, T defaultVal) where T : IConvertible
			{
				if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
				{
					if (throwIfBlankMissing)
						throw new ArgumentException("XmlElement attribute " + 
							atttibuteName + " is blank or missing.");
					else return defaultVal;
				}
                return Converter<string, T>(GetAttribute(atttibuteName));
				//return (Int16.Parse(GetAttribute(atttibuteName), style));
			}
            public T GetAttribute<T>(string atttibuteName, System.Globalization.NumberStyles style,
                Int16 defaultVal) where T : IConvertible
            { return (GetAttribute<T>(atttibuteName, style, false, defaultVal)); }

            public T GetAttribute<T>(string atttibuteName, bool throwIfBlankMissing, 
                T defaultVal) where T : IConvertible
			{
				if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
				{
					if (throwIfBlankMissing)
						throw new ArgumentException("XmlElement attribute " + 
							atttibuteName + " is blank or missing.");
					else return defaultVal;
				}
				return (Int16.Parse(GetAttribute(atttibuteName)));
			}
            public T GetAttribute<T>(string atttibuteName, T defaultVal) where T : IConvertible
            { return (GetAttribute<T>(atttibuteName, false, defaultVal)); }
            **************/
            #endregion Generics

            #region string Overrides
            public override string GetAttribute(string atttibuteName, string namspaceURI)
            { return (base.GetAttribute(atttibuteName, namspaceURI).Trim()); }
            public override string GetAttribute(string atttibuteName)
            { return (base.GetAttribute(atttibuteName).Trim()); }
            #endregion string Overrides

            #region Int16 Overloads
            public Int16 GetInt16Attribute(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider, bool throwIfBlankMissing)
            { return (GetInt16Attribute(atttibuteName, style, provider, throwIfBlankMissing, 0)); }
            public Int16 GetInt16Attribute(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider)
            { return (GetInt16Attribute(atttibuteName, style, provider, false, 0)); }
            public Int16 GetInt16Attribute(string atttibuteName,
                IFormatProvider provider, bool throwIfBlankMissing)
            { return (GetInt16Attribute(atttibuteName, provider, throwIfBlankMissing, 0)); }
            public Int16 GetInt16Attribute(string atttibuteName, IFormatProvider provider)
            { return (GetInt16Attribute(atttibuteName, provider, false, 0)); }
            public Int16 GetInt16Attribute(string atttibuteName,
                System.Globalization.NumberStyles style, bool throwIfBlankMissing)
            { return (GetInt16Attribute(atttibuteName, throwIfBlankMissing, 0)); }
            public Int16 GetInt16Attribute(string atttibuteName, System.Globalization.NumberStyles style)
            { return (GetInt16Attribute(atttibuteName, style, false, 0)); }
            public Int16 GetInt16Attribute(string atttibuteName, bool throwIfBlankMissing)
            { return (GetInt16Attribute(atttibuteName, throwIfBlankMissing, 0)); }
            public Int16 GetInt16Attribute(string atttibuteName)
            { return (GetInt16Attribute(atttibuteName, false, 0)); }
            // -------------------------------------
            public Int16 GetInt16Attribute(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider, bool throwIfBlankMissing, Int16 defaultVal)
            {
                if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
                {
                    if (throwIfBlankMissing)
                        throw new ArgumentException("XmlElement attribute " +
                            atttibuteName + " is blank or missing.");
                    return defaultVal;
                }
                return (Int16.Parse(GetAttribute(atttibuteName), style, provider));
            }
            public Int16 GetInt16Attribute(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider, Int16 defaultVal)
            { return (GetInt16Attribute(atttibuteName, style, provider, false, defaultVal)); }
            public Int16 GetInt16Attribute(string atttibuteName,
                IFormatProvider provider, bool throwIfBlankMissing, Int16 defaultVal)
            {
                if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
                {
                    if (throwIfBlankMissing)
                        throw new ArgumentException("XmlElement attribute " +
                            atttibuteName + " is blank or missing.");
                    return defaultVal;
                }
                return (Int16.Parse(GetAttribute(atttibuteName), provider));
            }
            public Int16 GetInt16Attribute(string atttibuteName, IFormatProvider provider, Int16 defaultVal)
            { return (GetInt16Attribute(atttibuteName, provider, false, defaultVal)); }
            public Int16 GetInt16Attribute(string atttibuteName,
                System.Globalization.NumberStyles style, bool throwIfBlankMissing, Int16 defaultVal)
            {
                if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
                {
                    if (throwIfBlankMissing)
                        throw new ArgumentException("XmlElement attribute " +
                            atttibuteName + " is blank or missing.");
                    return defaultVal;
                }
                return (Int16.Parse(GetAttribute(atttibuteName), style));
            }
            public Int16 GetInt16Attribute(string atttibuteName, System.Globalization.NumberStyles style, Int16 defaultVal)
            { return (GetInt16Attribute(atttibuteName, style, false, defaultVal)); }

            public Int16 GetInt16Attribute(string atttibuteName, bool throwIfBlankMissing, Int16 defaultVal)
            {
                if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
                {
                    if (throwIfBlankMissing)
                        throw new ArgumentException("XmlElement attribute " +
                            atttibuteName + " is blank or missing.");
                    return defaultVal;
                }
                return (Int16.Parse(GetAttribute(atttibuteName)));
            }
            public Int16 GetInt16Attribute(string atttibuteName, Int16 defaultVal)
            { return (GetInt16Attribute(atttibuteName, false, defaultVal)); }
            #endregion Int16 Overloads

            #region Int32 Overloads
            public Int32 GetInt32Attribute(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider, bool throwIfBlankMissing)
            { return (GetInt32Attribute(atttibuteName, style, provider, throwIfBlankMissing, 0)); }
            public Int32 GetInt32Attribute(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider)
            { return (GetInt32Attribute(atttibuteName, style, provider, false, 0)); }
            public Int32 GetInt32Attribute(string atttibuteName,
                IFormatProvider provider, bool throwIfBlankMissing)
            { return (GetInt32Attribute(atttibuteName, provider, throwIfBlankMissing, 0)); }
            public Int32 GetInt32Attribute(string atttibuteName, IFormatProvider provider)
            { return (GetInt32Attribute(atttibuteName, provider, false, 0)); }
            public Int32 GetInt32Attribute(string atttibuteName,
                System.Globalization.NumberStyles style, bool throwIfBlankMissing)
            { return (GetInt32Attribute(atttibuteName, style, throwIfBlankMissing, 0)); }
            public Int32 GetInt32Attribute(string atttibuteName, System.Globalization.NumberStyles style)
            { return (GetInt32Attribute(atttibuteName, style, false, 0)); }
            public Int32 GetInt32Attribute(string atttibuteName, bool throwIfBlankMissing)
            { return (GetInt32Attribute(atttibuteName, throwIfBlankMissing, 0)); }
            public Int32 GetInt32Attribute(string atttibuteName)
            { return (GetInt32Attribute(atttibuteName, false, 0)); }
            // -------------------------------------
            public Int32 GetInt32Attribute(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider, bool throwIfBlankMissing, Int32 defaultVal)
            {
                if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
                {
                    if (throwIfBlankMissing)
                        throw new ArgumentException("XmlElement attribute " +
                            atttibuteName + " is blank or missing.");
                    return defaultVal;
                }
                return (Int32.Parse(GetAttribute(atttibuteName), style, provider));
            }
            public Int32 GetInt32Attribute(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider, Int32 defaultVal)
            { return (GetInt32Attribute(atttibuteName, style, provider, false)); }
            public Int32 GetInt32Attribute(string atttibuteName,
                IFormatProvider provider, bool throwIfBlankMissing, Int32 defaultVal)
            {
                if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
                {
                    if (throwIfBlankMissing)
                        throw new ArgumentException("XmlElement attribute " +
                            atttibuteName + " is blank or missing.");
                    return defaultVal;
                }
                return (Int32.Parse(GetAttribute(atttibuteName), provider));
            }
            public Int32 GetInt32Attribute(string atttibuteName, IFormatProvider provider, Int32 defaultVal)
            { return (GetInt32Attribute(atttibuteName, provider, false, defaultVal)); }
            public Int32 GetInt32Attribute(string atttibuteName, System.Globalization.NumberStyles style,
                bool throwIfBlankMissing, Int32 defaultVal)
            {
                if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
                {
                    if (throwIfBlankMissing)
                        throw new ArgumentException("XmlElement attribute " +
                            atttibuteName + " is blank or missing.");
                    return defaultVal;
                }
                return (Int32.Parse(GetAttribute(atttibuteName), style));
            }
            public Int32 GetInt32Attribute(string atttibuteName, System.Globalization.NumberStyles style, Int32 defaultVal)
            { return (GetInt32Attribute(atttibuteName, style, false, defaultVal)); }
            public Int32 GetInt32Attribute(string atttibuteName, bool throwIfBlankMissing, Int32 defaultVal)
            {
                if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
                {
                    if (throwIfBlankMissing)
                        throw new ArgumentException("XmlElement attribute " +
                            atttibuteName + " is blank or missing.");
                    return defaultVal;
                }
                return (Int32.Parse(GetAttribute(atttibuteName)));
            }
            public Int32 GetInt32Attribute(string atttibuteName, Int32 defaultVal)
            { return (GetInt32Attribute(atttibuteName, false, defaultVal)); }
            #endregion  Int32 Overloads

            #region Int64 Overloads
            public Int64 GetInt64Attribute(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider, bool throwIfBlankMissing)
            { return (GetInt64Attribute(atttibuteName, style, provider, throwIfBlankMissing, 0L)); }
            public Int64 GetInt64Attribute(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider)
            { return (GetInt64Attribute(atttibuteName, style, provider, false, 0L)); }
            public Int64 GetInt64Attribute(string atttibuteName,
                IFormatProvider provider, bool throwIfBlankMissing)
            { return (GetInt64Attribute(atttibuteName, provider, throwIfBlankMissing, 0L)); }
            public Int64 GetInt64Attribute(string atttibuteName, IFormatProvider provider)
            { return (GetInt64Attribute(atttibuteName, provider, false, 0L)); }
            public Int64 GetInt64Attribute(string atttibuteName,
                System.Globalization.NumberStyles style, bool throwIfBlankMissing)
            { return (GetInt64Attribute(atttibuteName, style, throwIfBlankMissing, 0L)); }
            public Int64 GetInt64Attribute(string atttibuteName, System.Globalization.NumberStyles style)
            { return (GetInt64Attribute(atttibuteName, style, false, 0L)); }
            public Int64 GetInt64Attribute(string atttibuteName, bool throwIfBlankMissing)
            { return (GetInt64Attribute(atttibuteName, throwIfBlankMissing, 0L)); }
            public Int64 GetInt64Attribute(string atttibuteName)
            { return (GetInt64Attribute(atttibuteName, false, 0L)); }
            // --------------------------------------------------------
            public Int64 GetInt64Attribute(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider, bool throwIfBlankMissing, Int64 defaultVal)
            {
                if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
                {
                    if (throwIfBlankMissing)
                        throw new ArgumentException("XmlElement attribute " +
                            atttibuteName + " is blank or missing.");
                    return defaultVal;
                }
                return (Int64.Parse(GetAttribute(atttibuteName), style, provider));
            }
            public Int64 GetInt64Attribute(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider, Int64 defaultVal)
            { return (GetInt64Attribute(atttibuteName, style, provider, false, defaultVal)); }
            public Int64 GetInt64Attribute(string atttibuteName,
                IFormatProvider provider, bool throwIfBlankMissing, Int64 defaultVal)
            {
                if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
                {
                    if (throwIfBlankMissing)
                        throw new ArgumentException("XmlElement attribute " +
                            atttibuteName + " is blank or missing.");
                    return defaultVal;
                }
                return (Int64.Parse(GetAttribute(atttibuteName), provider));
            }
            public Int64 GetInt64Attribute(string atttibuteName, IFormatProvider provider, Int64 defaultVal)
            { return (GetInt64Attribute(atttibuteName, provider, false, defaultVal)); }
            public Int64 GetInt64Attribute(string atttibuteName,
                System.Globalization.NumberStyles style, bool throwIfBlankMissing, Int64 defaultVal)
            {
                if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
                {
                    if (throwIfBlankMissing)
                        throw new ArgumentException("XmlElement attribute " +
                            atttibuteName + " is blank or missing.");
                    return defaultVal;
                }
                return (Int64.Parse(GetAttribute(atttibuteName), style));
            }
            public Int64 GetInt64Attribute(string atttibuteName, System.Globalization.NumberStyles style, Int64 defaultVal)
            { return (GetInt64Attribute(atttibuteName, style, false, defaultVal)); }

            public Int64 GetInt64Attribute(string atttibuteName, bool throwIfBlankMissing, Int64 defaultVal)
            {
                if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
                {
                    if (throwIfBlankMissing)
                        throw new ArgumentException("XmlElement attribute " +
                            atttibuteName + " is blank or missing.");
                    return defaultVal;
                }
                return (Int64.Parse(GetAttribute(atttibuteName)));
            }
            public Int64 GetInt64Attribute(string atttibuteName, Int64 defaultVal)
            { return (GetInt64Attribute(atttibuteName, false, defaultVal)); }
            #endregion Int64 Overloads

            #region Boolean OverLoads
            public bool GetBooleanAttribute(string atttibuteName)
            { return (GetBooleanAttribute(atttibuteName, false)); }
            public bool GetBooleanAttribute(string atttibuteName, bool defaultVal)
            {
                if (HasAttribute(atttibuteName))
                    return (GetAttribute(atttibuteName) == "Y");
                return (defaultVal);
            }

            #endregion Boolean OverLoads

            #region Double Overloads
            public double GetDoubleAttribute(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider, bool throwIfBlankMissing)
            { return (GetDoubleAttribute(atttibuteName, style, provider, throwIfBlankMissing, 0.0d)); }
            public double GetDoubleAttribute(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider)
            { return (GetDoubleAttribute(atttibuteName, style, provider, false, 0.0d)); }
            public double GetDoubleAttribute(string atttibuteName,
                IFormatProvider provider, bool throwIfBlankMissing)
            { return (GetDoubleAttribute(atttibuteName, provider, throwIfBlankMissing, 0.0d)); }
            public double GetDoubleAttribute(string atttibuteName, IFormatProvider provider)
            { return (GetDoubleAttribute(atttibuteName, provider, false, 0.0d)); }
            public double GetDoubleAttribute(string atttibuteName,
                System.Globalization.NumberStyles style, bool throwIfBlankMissing)
            { return (GetDoubleAttribute(atttibuteName, style, throwIfBlankMissing, 0.0d)); }
            public double GetDoubleAttribute(string atttibuteName, System.Globalization.NumberStyles style)
            { return (GetDoubleAttribute(atttibuteName, style, false, 0.0d)); }
            public double GetDoubleAttribute(string atttibuteName, bool throwIfBlankMissing)
            { return (GetDoubleAttribute(atttibuteName, throwIfBlankMissing, 0.0d)); }
            public double GetDoubleAttribute(string atttibuteName)
            { return (GetDoubleAttribute(atttibuteName, false, 0.0d)); }
            // -------------------------------------------------------
            public double GetDoubleAttribute(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider, bool throwIfBlankMissing, double defaultVal)
            {
                if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
                {
                    if (throwIfBlankMissing)
                        throw new ArgumentException("XmlElement attribute " +
                            atttibuteName + " is blank or missing.");
                    return defaultVal;
                }
                return (double.Parse(GetAttribute(atttibuteName), style, provider));
            }
            public double GetDoubleAttribute(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider, double defaultVal)
            { return (GetDoubleAttribute(atttibuteName, style, provider, false, defaultVal)); }
            public double GetDoubleAttribute(string atttibuteName,
                IFormatProvider provider, bool throwIfBlankMissing, double defaultVal)
            {
                if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
                {
                    if (throwIfBlankMissing)
                        throw new ArgumentException("XmlElement attribute " +
                            atttibuteName + " is blank or missing.");
                    return defaultVal;
                }
                return (double.Parse(GetAttribute(atttibuteName), provider));
            }
            public double GetDoubleAttribute(string atttibuteName, IFormatProvider provider, double defaultVal)
            { return (GetDoubleAttribute(atttibuteName, provider, false, defaultVal)); }
            public double GetDoubleAttribute(string atttibuteName,
                System.Globalization.NumberStyles style, bool throwIfBlankMissing, double defaultVal)
            {
                if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
                {
                    if (throwIfBlankMissing)
                        throw new ArgumentException("XmlElement attribute " +
                            atttibuteName + " is blank or missing.");
                    return defaultVal;
                }
                return (double.Parse(GetAttribute(atttibuteName), style));
            }
            public double GetDoubleAttribute(string atttibuteName, System.Globalization.NumberStyles style, double defaultVal)
            { return (GetDoubleAttribute(atttibuteName, style, false, defaultVal)); }

            public double GetDoubleAttribute(string atttibuteName, bool throwIfBlankMissing, double defaultVal)
            {
                if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
                {
                    if (throwIfBlankMissing)
                        throw new ArgumentException("XmlElement attribute " +
                            atttibuteName + " is blank or missing.");
                    return defaultVal;
                }
                return (double.Parse(GetAttribute(atttibuteName)));
            }
            public double GetDoubleAttribute(string atttibuteName, double defaultVal)
            { return (GetDoubleAttribute(atttibuteName, false, defaultVal)); }
            #endregion Double Overloads

            #region Decimal Overloads
            public decimal GetDecimalAttribute(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider, bool throwIfBlankMissing)
            { return (GetDecimalAttribute(atttibuteName, style, provider, throwIfBlankMissing, 0m)); }
            public decimal GetDecimalAttribute(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider)
            { return (GetDecimalAttribute(atttibuteName, style, provider, false, 0m)); }
            public decimal GetDecimalAttribute(string atttibuteName,
                IFormatProvider provider, bool throwIfBlankMissing)
            { return (GetDecimalAttribute(atttibuteName, provider, throwIfBlankMissing, 0m)); }
            public decimal GetDecimalAttribute(string atttibuteName, IFormatProvider provider)
            { return (GetDecimalAttribute(atttibuteName, provider, false, 0m)); }
            public decimal GetDecimalAttribute(string atttibuteName,
                System.Globalization.NumberStyles style, bool throwIfBlankMissing)
            { return (GetDecimalAttribute(atttibuteName, style, throwIfBlankMissing, 0m)); }
            public decimal GetDecimalAttribute(string atttibuteName, System.Globalization.NumberStyles style)
            { return (GetDecimalAttribute(atttibuteName, style, false, 0m)); }

            public decimal GetDecimalAttribute(string atttibuteName, bool throwIfBlankMissing)
            { return (GetDecimalAttribute(atttibuteName, throwIfBlankMissing, 0m)); }
            public decimal GetDecimalAttribute(string atttibuteName)
            { return (GetDecimalAttribute(atttibuteName, false, 0m)); }
            // -------------------------------------
            public decimal GetDecimalAttribute(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider, bool throwIfBlankMissing, decimal defaultVal)
            {
                if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
                {
                    if (throwIfBlankMissing)
                        throw new ArgumentException("XmlElement attribute " +
                            atttibuteName + " is blank or missing.");
                    return defaultVal;
                }
                return (decimal.Parse(GetAttribute(atttibuteName), style, provider));
            }
            public decimal GetDecimalAttribute(string atttibuteName, System.Globalization.NumberStyles style,
                IFormatProvider provider, decimal defaultVal)
            { return (GetDecimalAttribute(atttibuteName, style, provider, false, defaultVal)); }
            public decimal GetDecimalAttribute(string atttibuteName,
                IFormatProvider provider, bool throwIfBlankMissing, decimal defaultVal)
            {
                if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
                {
                    if (throwIfBlankMissing)
                        throw new ArgumentException("XmlElement attribute " +
                            atttibuteName + " is blank or missing.");
                    return defaultVal;
                }
                return (decimal.Parse(GetAttribute(atttibuteName), provider));
            }
            public decimal GetDecimalAttribute(string atttibuteName, IFormatProvider provider, decimal defaultVal)
            { return (GetDecimalAttribute(atttibuteName, provider, false, defaultVal)); }
            public decimal GetDecimalAttribute(string atttibuteName,
                System.Globalization.NumberStyles style, bool throwIfBlankMissing, decimal defaultVal)
            {
                if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
                {
                    if (throwIfBlankMissing)
                        throw new ArgumentException("XmlElement attribute " +
                            atttibuteName + " is blank or missing.");
                    return defaultVal;
                }
                return (decimal.Parse(GetAttribute(atttibuteName), style));
            }
            public decimal GetDecimalAttribute(string atttibuteName,
                System.Globalization.NumberStyles style, decimal defaultVal)
            { return (GetDecimalAttribute(atttibuteName, style, false, defaultVal)); }

            public decimal GetDecimalAttribute(string atttibuteName, bool throwIfBlankMissing, decimal defaultVal)
            {
                if (!HasAttribute(atttibuteName) || GetAttribute(atttibuteName).Length == 0)
                {
                    if (throwIfBlankMissing)
                        throw new ArgumentException("XmlElement attribute " +
                            atttibuteName + " is blank or missing.");
                    return defaultVal;
                }
                return (decimal.Parse(GetAttribute(atttibuteName)));
            }
            public decimal GetDecimalAttribute(string atttibuteName, decimal defaultVal)
            { return (GetDecimalAttribute(atttibuteName, false, defaultVal)); }
            #endregion Decimal Overloads

            #region DateTime Overloads
            public DateTime GetDateTimeAttribute(string atttibuteName, IFormatProvider provider,
                System.Globalization.DateTimeStyles styles, bool throwIfBlankMissing)
            {
                if ((throwIfBlankMissing) && (!HasAttribute(atttibuteName) ||
                    GetAttribute(atttibuteName).Length == 0))
                    throw new ArgumentException("XmlElement attribute " + atttibuteName + " is blank or missing.");
                return (DateTime.Parse(GetAttribute(atttibuteName), provider, styles));
            }
            public DateTime GetDateTimeAttribute(string atttibuteName, IFormatProvider provider,
                System.Globalization.DateTimeStyles styles)
            { return (GetDateTimeAttribute(atttibuteName, provider, styles, false)); }

            public DateTime GetDateTimeAttribute(string atttibuteName,
                IFormatProvider provider, bool throwIfBlankMissing)
            {
                if ((throwIfBlankMissing) && (!HasAttribute(atttibuteName) ||
                    GetAttribute(atttibuteName).Length == 0))
                    throw new ArgumentException("XmlElement attribute " + atttibuteName + " is blank or missing.");
                return (DateTime.Parse(GetAttribute(atttibuteName), provider));
            }
            public DateTime GetDateTimeAttribute(string atttibuteName, IFormatProvider provider)
            { return (GetDateTimeAttribute(atttibuteName, provider, false)); }

            public DateTime GetDateTimeAttribute(string atttibuteName, bool throwIfBlankMissing)
            {
                if ((throwIfBlankMissing) && (!HasAttribute(atttibuteName) ||
                    GetAttribute(atttibuteName).Length == 0))
                    throw new ArgumentException("XmlElement attribute " + atttibuteName + " is blank or missing.");
                return (DateTime.Parse(GetAttribute(atttibuteName)));
            }
            public DateTime GetDateTimeAttribute(string atttibuteName)
            { return (GetDateTimeAttribute(atttibuteName, false)); }
            #endregion DateTime Overloads

            #endregion GetAttribute OverLoads

            #region SetAttribute OverLoads
            //--------- Nullables Nullable<>  -----------------------------------------------------------
            public void SetAttribute(string atttibuteName, bool? val) { SetAttribute(atttibuteName, val, false); }
            public void SetAttribute(string atttibuteName, bool? val, bool throwIfBlankMissing)
            {
                if (val.HasValue) SetAttribute(atttibuteName, val.Value, throwIfBlankMissing);
                else if (throwIfBlankMissing)
                    throw new ArgumentException(
                        "XmlElement has no " + atttibuteName + " attribute.");
                else SetAttribute(atttibuteName, string.Empty);
            }
            public void SetAttribute<T>(string atttibuteName, T? val) where T: struct
            { SetAttribute(atttibuteName, val, false); }
            public void SetAttribute<T>(string atttibuteName, T? val, bool throwIfBlankMissing) where T : struct
            {
                if (val.HasValue) SetAttribute(atttibuteName, val.Value.ToString());
                else if (throwIfBlankMissing)
                    throw new ArgumentException(
                        "XmlElement has no " + atttibuteName + " attribute.");
                else SetAttribute(atttibuteName, string.Empty);
            }
            //--------- string -------------------------------------------------------------------
            public void SetAttribute(string atttibuteName, string attribValue, AttribProcess process)
            { SetAttribute(atttibuteName, attribValue, false, process); }
            public void SetAttribute(string atttibuteName, string attribValue, 
                bool throwIfBlankMissing, AttribProcess process)
            {
                if (!HasAttribute(atttibuteName) && throwIfBlankMissing)
                    throw new ArgumentException("XmlElement has no " + atttibuteName + " attribute.");
                if (process == AttribProcess.StripIllegalCharacters) 
                    StripIllegals(ref attribValue);
                SetAttribute(atttibuteName, attribValue);
            }
            private static void StripIllegals(ref string s)
            {
                StringBuilder sOut = new StringBuilder();
                foreach (char c in s)
                {
                    int cCd = c;
                    if (cCd == 9 || cCd == 10 || cCd == 13 ||
                        (cCd >= 32 && cCd <= 125))
                        sOut.Append(c);
                }
                s = sOut.ToString();
            }
            public void SetAttribute(string atttibuteName, string attribValue, int maxCharacters)
            {SetAttribute(atttibuteName, attribValue, maxCharacters, false);  }
            public void SetAttribute(string atttibuteName, string attribValue, int maxCharacters, bool throwBlankMissing)
            {
                SetAttribute(atttibuteName, 
                    string.IsNullOrEmpty(attribValue)? string.Empty: 
                    (attribValue.Length <= maxCharacters) ? attribValue : 
                            attribValue.Substring(0, maxCharacters));
            }
            public void SetAttribute(string atttibuteName, string attribValue, bool throwIfBlankMissing)
            {
                if (!HasAttribute(atttibuteName) && throwIfBlankMissing)
                    throw new ArgumentException("XmlElement has no " + atttibuteName + " attribute.");
                SetAttribute(atttibuteName, attribValue);
            }
            //-------- char --------------------------------------------------------------------
            public void SetAttribute(string atttibuteName, char attribValue, bool bThrowMissing)
            { SetAttribute(atttibuteName, attribValue.ToString(), bThrowMissing); }
            public void SetAttribute(string atttibuteName, char attribValue)
            { SetAttribute(atttibuteName, attribValue, false); }
            //------- Int16 ---------------------------------------------------------------------
            public void SetAttribute(string atttibuteName, Int16 attribValue, bool throwIfBlankMissing)
            { SetAttribute(atttibuteName, attribValue.ToString(), throwIfBlankMissing); }
            public void SetAttribute(string atttibuteName, Int16 attribValue)
            { SetAttribute(atttibuteName, attribValue, false); }
            //------ Int32 ----------------------------------------------------------------------
            public void SetAttribute(string atttibuteName, Int32 attribValue, bool throwIfBlankMissing)
            { SetAttribute(atttibuteName, attribValue.ToString(), throwIfBlankMissing); }
            public void SetAttribute(string atttibuteName, Int32 attribValue)
            { SetAttribute(atttibuteName, attribValue, false); }
            //------ Int64 -----------------------------------------------------------------------
            public void SetAttribute(string atttibuteName, Int64 attribValue, bool throwIfBlankMissing)
            { SetAttribute(atttibuteName, attribValue.ToString(), throwIfBlankMissing); }
            public void SetAttribute(string atttibuteName, Int64 attribValue)
            { SetAttribute(atttibuteName, attribValue, false); }
            //------ bool --------------------------------------------------------------------
            public void SetAttribute(string atttibuteName, bool attribValue, bool throwIfBlankMissing)
            { SetAttribute(atttibuteName, ((attribValue) ? "Y" : "N"), throwIfBlankMissing); }
            public void SetAttribute(string atttibuteName, bool attribValue)
            { SetAttribute(atttibuteName, attribValue, false); }
            //----- decimal -------------------------------------------------------------------
            public void SetAttribute(string atttibuteName, decimal attribValue, bool throwIfBlankMissing)
            { SetAttribute(atttibuteName, attribValue.ToString(), throwIfBlankMissing); }
            public void SetAttribute(string atttibuteName, decimal attribValue)
            { SetAttribute(atttibuteName, attribValue, false); }
            //------ double --------------------------------------------------------------------
            public void SetAttribute(string atttibuteName, double attribValue, bool throwIfBlankMissing)
            { SetAttribute(atttibuteName, attribValue.ToString(), throwIfBlankMissing); }
            public void SetAttribute(string atttibuteName, double attribValue)
            { SetAttribute(atttibuteName, attribValue, false); }
            //----- DateTime --------------------------------------------------------------
            public void SetAttribute(string atttibuteName, DateTime attribValue, bool throwIfBlankMissing)
            { SetAttribute(atttibuteName, attribValue, null, throwIfBlankMissing); }
            public void SetAttribute(string atttibuteName, DateTime attribValue, string format,  bool throwIfBlankMissing)
            {
                if (string.IsNullOrEmpty(format)) format = "yyyy-MM-dd HH:mm:ss";
                SetAttribute(atttibuteName, attribValue.ToString(format), throwIfBlankMissing);
            }
            public void SetAttribute(string atttibuteName, DateTime attribValue)
            { SetAttribute(atttibuteName, attribValue, false); }
            //---------------------------------------------------------------------------------
            #endregion SetAttribute OverLoads
            public enum AttribProcess {None, StripIllegalCharacters}
        } //  end bpElement class. 
        #endregion
    }
}
