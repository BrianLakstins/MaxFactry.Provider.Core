// <copyright file="MaxConvertLibraryCoreProvider.cs" company="Lakstins Family, LLC">
// Copyright (c) Brian A. Lakstins (http://www.lakstins.com/brian/)
// </copyright>

#region License
// <license>
// This software is provided 'as-is', without any express or implied warranty. In no 
// event will the author be held liable for any damages arising from the use of this 
// software.
//  
// Permission is granted to anyone to use this software for any purpose, including 
// commercial applications, and to alter it and redistribute it freely, subject to the 
// following restrictions:
// 
// 1. The origin of this software must not be misrepresented; you must not claim that 
// you wrote the original software. If you use this software in a product, an 
// acknowledgment (see the following) in the product documentation is required.
// 
// Portions Copyright (c) Brian A. Lakstins (http://www.lakstins.com/brian/)
// 
// 2. Altered source versions must be plainly marked as such, and must not be 
// misrepresented as being the original software.
// 
// 3. This notice may not be removed or altered from any source distribution.
// </license>
#endregion

#region Change Log
// <changelog>
// <change date="1/18/2017" author="Brian A. Lakstins" description="Initial creation">
// <change date="4/20/2017" author="Brian A. Lakstins" description="Update so null data will return a blank MaxIndex">
// <change date="6/13/2017" author="Brian A. Lakstins" description="Fix issue in deserializing values of a MaxIndex that are objects">
// <change date="10/24/2017" author="Brian A. Lakstins" description="Fix issues serializing MaxIndex including nested MaxIndex">
// <change date="10/30/2017" author="Brian A. Lakstins" description="Update json to be more readable.  Fix deserialize of byte[]">
// <change date="11/6/2017" author="Brian A. Lakstins" description="Fix deserialize of DateTimeOffset">
// <change date="11/8/2017" author="Brian A. Lakstins" description="Update to log serialization failures.">
// <change date="3/4/2020" author="Brian A. Lakstins" description="Update to not depend in PublicKeyToken or Version when deserializing a type.">
// <change date="4/30/2021" author="Brian A. Lakstins" description="Add parsing of unix timestamp.">
// </changelog>
#endregion

namespace MaxFactry.Core.Provider
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Defines a class for implementing the MaxConvertLibrary functionality
    /// </summary>
    public class MaxConvertLibraryCoreProvider : MaxConvertLibraryDefaultProvider
    {
        public override string SerializeToStringConditional(object loObject)
        {
            return this.SerializeToJsonConditional(loObject);
        }

        private System.Collections.Generic.Dictionary<string, object> ConvertToDictionary(MaxIndex loIndex)
        {
            System.Collections.Generic.Dictionary<string, object> loR = new System.Collections.Generic.Dictionary<string, object>();
            string[] laKey = loIndex.GetSortedKeyList();
            foreach (string lsKey in laKey)
            {
                object loValue = loIndex[lsKey];
                if (loValue is MaxIndex)
                {
                    loValue = ConvertToDictionary(loValue as MaxIndex);
                }

                loR.Add(lsKey, loValue);
            }

            return loR;
        }

        public override string SerializeToJsonConditional(object loObject)
        {
            if (loObject is MaxIndex)
            {
                loObject = ConvertToDictionary(loObject as MaxIndex);
            }

            string lsR = Newtonsoft.Json.JsonConvert.SerializeObject(loObject, Newtonsoft.Json.Formatting.Indented);
            return lsR;
        }

        public override object DeserializeFromJsonConditional(string lsJson, Type loExpectedType)
        {
            object loR = null;
            if (loExpectedType == typeof(Guid) || loExpectedType == typeof(string) || loExpectedType == typeof(DateTime) || loExpectedType == typeof(DateTimeOffset))
            {
                lsJson = "'" + lsJson + "'";
            }

            if (loExpectedType == typeof(MaxIndex))
            {
                if (lsJson == "{SBI}")
                {
                    loR = new MaxIndex();
                }
                else
                {
                    loR = this.DeserializeFromJsonConditional(lsJson, typeof(System.Collections.Generic.Dictionary<string, object>));
                    if (null == loR)
                    {
                        loR = new MaxIndex();
                    }
                    else
                    {
                        System.Collections.Generic.Dictionary<string, object> loIndexDict = (System.Collections.Generic.Dictionary<string, object>)loR;
                        MaxIndexItemStructure[] laIndexData = new MaxIndexItemStructure[loIndexDict.Count];
                        int lnCurrent = 0;
                        foreach (string lsKey in loIndexDict.Keys)
                        {
                            if (lsKey.Length <= this._sSerialTypeKeySuffix.Length || lsKey.Substring(lsKey.Length - this._sSerialTypeKeySuffix.Length) != this._sSerialTypeKeySuffix)
                            {
                                object loValue = loIndexDict[lsKey];
                                if (loIndexDict.ContainsKey(lsKey + this._sSerialTypeKeySuffix))
                                {
                                    string lsType = loIndexDict[lsKey + this._sSerialTypeKeySuffix] as string;
                                    string[] laType = lsType.Split(new char[] { ',' });
                                    lsType = string.Empty;
                                    foreach (string lsTypePart in laType)
                                    {
                                        if (!lsTypePart.Contains("PublicKeyToken=") && !lsTypePart.Contains("Version="))
                                        {
                                            if (lsType.Length > 0)
                                            {
                                                lsType += ",";
                                            }

                                            lsType += lsTypePart;
                                        }
                                    }

                                    Type loType = Type.GetType(lsType);
                                    loValue = this.DeserializeObject(loValue.ToString(), loType);
                                    if (loValue is DateTime)
                                    {
                                        loValue = DateTime.SpecifyKind((DateTime)loValue, DateTimeKind.Utc);
                                    }
                                }
                                else if (loValue is Newtonsoft.Json.Linq.JArray)
                                {
                                    Newtonsoft.Json.Linq.JArray laValue = loValue as Newtonsoft.Json.Linq.JArray;
                                    object[] laMaxIndex = new object[laValue.Count];
                                    for (int lnV = 0; lnV < laValue.Count; lnV++)
                                    {
                                        string lsJsonArray = Newtonsoft.Json.JsonConvert.SerializeObject(laValue[lnV]);
                                        if (laValue[lnV] is Newtonsoft.Json.Linq.JValue)
                                        {
                                            laMaxIndex[lnV] = ((Newtonsoft.Json.Linq.JValue)laValue[lnV]).Value;
                                        }
                                        else if (laValue[lnV] is Newtonsoft.Json.Linq.JObject)
                                        {
                                            MaxIndex loMaxIndex = this.DeserializeFromJsonConditional(lsJsonArray, typeof(MaxIndex)) as MaxIndex;
                                            laMaxIndex[lnV] = loMaxIndex;
                                        }
                                    }

                                    loValue = laMaxIndex;
                                }
                                else if (loValue is Newtonsoft.Json.Linq.JValue)
                                {
                                    loValue = ((Newtonsoft.Json.Linq.JValue)loValue).Value;
                                }
                                else if (loValue is Newtonsoft.Json.Linq.JObject)
                                {
                                    string lsJsonObject = Newtonsoft.Json.JsonConvert.SerializeObject(loValue);
                                    MaxIndex loMaxIndex = this.DeserializeFromJsonConditional(lsJsonObject, typeof(MaxIndex)) as MaxIndex;
                                    loValue = loMaxIndex;
                                }

                                laIndexData[lnCurrent] = new MaxIndexItemStructure(lnCurrent, lsKey, loValue);
                                lnCurrent++;
                            }
                        }

                        loR = new MaxIndex(laIndexData);
                    }
                }
            }
            else if (loExpectedType == typeof(byte[]))
            {
                //loR = Newtonsoft.Json.JsonConvert.DeserializeObject(lsJson, loExpectedType);
                loR = Convert.FromBase64String(lsJson);
            }
            else
            {
                try
                {
                    loR = Newtonsoft.Json.JsonConvert.DeserializeObject(lsJson, loExpectedType);
                }
                catch (Exception loE)
                {
                    MaxLogLibrary.Log(new MaxLogEntryStructure(MaxEnumGroup.LogError, "Error Deserializing Json to {Type}.  {Json}", loE, loExpectedType, lsJson));
                }
            }

            return loR;
        }
    }


}