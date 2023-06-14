// <copyright file="MaxDataContextHttpClientProvider.cs" company="Lakstins Family, LLC">
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
// <change date="6/19/2019" author="Brian A. Lakstins" description="Initial creation">
// <change date="6/20/2019" author="Brian A. Lakstins" description="Fix issue with returning data.">
// <change date="8/1/2019" author="Brian A. Lakstins" description="Fix issue with sending content.  Add return of Request URL.">
// <change date="10/30/2019" author="Brian A. Lakstins" description="Add User-Agent and compression decoding.">
// </changelog>
#endregion

namespace MaxFactry.Base.DataLayer.Provider
{
    using System;
    using System.IO;
    using MaxFactry.Core;
    using MaxFactry.Base.DataLayer;
    using MaxFactry.Provider.CoreProvider.DataLayer;

    /// <summary>
    /// Data Context used to work with http data sources
    /// </summary>
    public class MaxDataContextHttpClientProvider : MaxProvider, IMaxDataContextProvider
    {
        protected MaxIndex _oLog = new MaxIndex();

        private static object _oLock = new object();

        /// <summary>
        /// Initializes a new instance of the MaxDataContextSerialPortProvider class
        /// </summary>
        public MaxDataContextHttpClientProvider()
            : base()
        {
        }

        /// <summary>
        /// Initializes the provider
        /// </summary>
        /// <param name="lsName">Name of the provider</param>
        /// <param name="loConfig">Configuration information</param>
        public override void Initialize(string lsName, MaxIndex loConfig)
        {
            base.Initialize(lsName, loConfig);
        }

        /// <summary>
        /// Inserts a list of data objects.
        /// </summary>
        /// <param name="loDataList">The list of data objects to insert.</param>
        /// <returns>The count affected.</returns>
        public virtual int Insert(MaxDataList loDataList)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates a list of data objects.
        /// </summary>
        /// <param name="loDataList">The list of data objects to insert.</param>
        /// <returns>The count affected.</returns>
        public virtual int Update(MaxDataList loDataList)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes a list of data objects.
        /// </summary>
        /// <param name="loDataList">The list of data objects to insert.</param>
        /// <returns>The count affected.</returns>
        public virtual int Delete(MaxDataList loDataList)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes stream data to storage.
        /// </summary>
        /// <param name="loData">The data index for the object</param>
        /// <param name="lsKey">Data element name to write</param>
        /// <returns>Number of bytes written to storage.</returns>
        public virtual bool StreamSave(MaxData loData, string lsKey)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Opens stream data in storage
        /// </summary>
        /// <param name="loData">The data index for the object</param>
        /// <param name="lsKey">Data element name to write</param>
        /// <returns>Stream that was opened.</returns>
        public virtual Stream StreamOpen(MaxData loData, string lsKey)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes stream from storage.
        /// </summary>
        /// <param name="loData">The data index for the object</param>
        /// <param name="lsKey">Data element name to remove</param>
        /// <returns>true if successful.</returns>
        public virtual bool StreamDelete(MaxData loData, string lsKey)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the Url of a saved stream.
        /// </summary>
        /// <param name="loData">The data index for the object</param>
        /// <param name="lsKey">Data element name</param>
        /// <returns>Url of stream if one can be provided.</returns>
        public virtual string GetStreamUrl(MaxData loData, string lsKey)
        {
            return string.Empty;
        }

        /// <summary>
        /// Selects all data from the data storage name for the specified type.
        /// </summary>
        /// <param name="lsDataStorageName">Name of the data storage (table name).</param>
        /// <param name="laFields">list of fields to return from select</param>
        /// <returns>List of data elements with a base data model.</returns>
        public virtual MaxDataList SelectAll(string lsDataStorageName, params string[] laFields)
        {
            MaxLogLibrary.Log(MaxEnumGroup.LogDebug, "Select [" + lsDataStorageName + "] start", "MaxDataContextHttpClientProvider");
            MaxDataModel loDataModel = new MaxDataModel(lsDataStorageName);
            MaxDataList loR = new MaxDataList(loDataModel);


            return loR;
        }

        /// <summary>
        /// Selects data from the database.
        /// </summary>
        /// <param name="loData">Element with data used in the filter.</param>
        /// <param name="loDataQuery">Query information to filter results.</param>
        /// <param name="lnPageIndex">Page to return.</param>
        /// <param name="lnPageSize">Items per page.</param>
        /// <param name="lsSort">Sort information.</param>
        /// <param name="lnTotal">Total items found.</param>
        /// <param name="laFields">list of fields to return from select.</param>
        /// <returns>List of data from select.</returns>
        public virtual MaxDataList Select(MaxData loData, MaxDataQuery loDataQuery, int lnPageIndex, int lnPageSize, string lsSort, out int lnTotal, params string[] laFields)
        {
            return this.SelectConditional(loData, loDataQuery, lnPageIndex, lnPageIndex, out lnTotal, laFields);
        }

        /// <summary>
        /// Gets the number of records that match the filter.
        /// </summary>
        /// <param name="loData">Element with data used in the filter.</param>
        /// <param name="loDataQuery">Query information to filter results.</param>
        /// <returns>number of records that match.</returns>
        public virtual int SelectCount(MaxData loData, MaxDataQuery loDataQuery)
        {
            int lnR = 0;

            return lnR;
        }

#if net4_52 

        protected static System.Net.Http.HttpClient GetMaxClient()
        {
            System.Net.Http.HttpClient loR = null;
            System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
            System.Net.Http.HttpClientHandler loHandler = new System.Net.Http.HttpClientHandler();
            if (loHandler.SupportsAutomaticDecompression)
            {
                loHandler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;
            }

            loHandler.CookieContainer = new System.Net.CookieContainer();
            loR = new System.Net.Http.HttpClient(loHandler);
            loR.Timeout = new TimeSpan(0, 0, 10);
            //_oHttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.70 Safari/537.36");
            loR.DefaultRequestHeaders.Add("User-Agent", "Mozilla /5.0 (MaxFactry .NET Framework)");
            loR.DefaultRequestHeaders.Add("DNT", "1");
            //_oHttpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3");
            loR.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            loR.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            //_oHttpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            return loR;
        }

        private static System.Net.Http.HttpClient _oHttpClient = null;

        protected static System.Net.Http.HttpClient HttpClient
        {
            get
            {
                if (null == _oHttpClient)
                {
                    lock (_oLock)
                    {
                        if (null == _oHttpClient)
                        {
                            _oHttpClient = GetMaxClient();
                        }
                    }
                }

                return _oHttpClient;
            }
        }

        /// <summary>
        /// Selects data from the database.
        /// </summary>
        /// <param name="loData">Element with data used in the filter.</param>
        /// <param name="loDataQuery">Query information to filter results.</param>
        /// <param name="lnPageIndex">Page to return.</param>
        /// <param name="lnPageSize">Items per page.</param>
        /// <param name="lnTotal">Total items found.</param>
        /// <param name="laFields">list of fields to return from select.</param>
        /// <returns>List of data from select.</returns>
        public virtual MaxDataList SelectConditional(MaxData loData, MaxDataQuery loDataQuery, int lnPageIndex, int lnPageSize, out int lnTotal, params string[] laFields)
        {
            MaxLogLibrary.Log(MaxEnumGroup.LogDebug, "Select [" + loData.DataModel.DataStorageName + "] start", "MaxDataContextHttpClientProvider");
            MaxHttpClientDataModel loDataModel = loData.DataModel as MaxHttpClientDataModel;
            if (null == loDataModel)
            {
                throw new MaxException("MaxHttpClientDataModel is expected by MaxDataContextHttpClientProvider");
            }

            MaxDataList loR = new MaxDataList(loDataModel);
            MaxData loDataReturn = new MaxData(loData.DataModel);
            lnTotal = 0;
            DateTime ldRequestStart = DateTime.UtcNow;
            Uri loReqestUrl = loData.Get(loDataModel.RequestUrl) as Uri;
            if (null == loReqestUrl)
            {
                string lsRequestUrl = loData.Get(loDataModel.RequestUrl) as string;
                if (null != lsRequestUrl)
                {
                    loReqestUrl = new Uri(lsRequestUrl);
                }
            }
            
            object loRequestContent = loData.Get(loDataModel.RequestContent);
            MaxIndex loResponse = new MaxIndex();
            try
            {
                System.Net.Http.HttpClient loClient = HttpClient;
                System.Net.Http.HttpContent loContent = null;
                if (loRequestContent is System.Net.Http.HttpContent)
                {
                    loContent = (System.Net.Http.HttpContent)loRequestContent;
                }
                else if (loRequestContent is string)
                {
                    loContent = new System.Net.Http.StringContent((string)loRequestContent);
                }
                else if (loRequestContent is MaxIndex)
                {
                    System.Collections.Generic.Dictionary<string, string> loContentDictionary = new System.Collections.Generic.Dictionary<string, string>();
                    string[] laKey = ((MaxIndex)loRequestContent).GetSortedKeyList();
                    string lsClientId = string.Empty;
                    string lsClientSecret = string.Empty;
                    string lsToken = string.Empty;
                    bool lbStringContent = false;
                    foreach (string lsKey in laKey)
                    {
                        if (lsKey == "BasicAuthClientId")
                        {
                            lsClientId = ((MaxIndex)loRequestContent).GetValueString(lsKey);
                        }
                        else if (lsKey == "BasicAuthClientSecret")
                        {
                            lsClientSecret = ((MaxIndex)loRequestContent).GetValueString(lsKey);
                        }
                        else if (lsKey == "BearerAuthToken")
                        {
                            lsToken = ((MaxIndex)loRequestContent).GetValueString(lsKey);
                        }
                        else if (lsKey == "StringContent")
                        {
                            lbStringContent = MaxConvertLibrary.ConvertToBoolean(typeof(object), ((MaxIndex)loRequestContent)[lsKey]);
                        }
                        else
                        {
                            loContentDictionary.Add(lsKey, ((MaxIndex)loRequestContent).GetValueString(lsKey));
                        }
                    }

                    if (loContentDictionary.Count > 0)
                    {
                        if (lbStringContent)
                        {
                            loContent = new System.Net.Http.StringContent(MaxConvertLibrary.SerializeObjectToString(loContentDictionary));
                        }
                        else
                        {
                            loContent = new System.Net.Http.FormUrlEncodedContent(loContentDictionary);
                        }
                    }

                    if ((!string.IsNullOrEmpty(lsClientId) && !string.IsNullOrEmpty(lsClientSecret)) || !string.IsNullOrEmpty(lsToken))
                    {
                        loClient = GetMaxClient();
                        if (!string.IsNullOrEmpty(lsClientId) && !string.IsNullOrEmpty(lsClientSecret))
                        {
                            loClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(string.Format("{0}:{1}", lsClientId, lsClientSecret))));
                        }
                        else if (!string.IsNullOrEmpty(lsToken))
                        {
                            loClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", lsToken);
                        }

                        System.Net.Http.Headers.CacheControlHeaderValue loCache = new System.Net.Http.Headers.CacheControlHeaderValue();
                        loCache.NoCache = true;
                        loClient.DefaultRequestHeaders.CacheControl = loCache;
                    }
                }
                else if (null != loRequestContent)
                {
                    loContent = new System.Net.Http.StringContent(MaxConvertLibrary.SerializeObjectToString(loRequestContent));
                }

                System.Net.Http.HttpResponseMessage loHttpClientResponse = null;
                object loResponseContent = null;
                System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> loTask = null;
                if (null != loContent)
                {
                    loTask = loClient.PostAsync(loReqestUrl, loContent);
                }
                else
                {
                    loTask = loClient.GetAsync(loReqestUrl);
                }

                while (!loTask.IsCompleted)
                {
                    System.Threading.Thread.Sleep(10);
                }

                if (loTask.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
                {
                    loHttpClientResponse = loTask.Result;
                    if (loHttpClientResponse.IsSuccessStatusCode)
                    {
                        if (loHttpClientResponse.Content != null)
                        {
                            System.Threading.Tasks.Task loContentTask = null;
                            if (loHttpClientResponse.Content.GetType() == typeof(System.Net.Http.StreamContent))
                            {
                                loContentTask = loHttpClientResponse.Content.ReadAsStreamAsync();
                            }
                            else if (loHttpClientResponse.Content.GetType() == typeof(string))
                            {
                                loContentTask = loHttpClientResponse.Content.ReadAsStringAsync();
                            }
                            else if (loHttpClientResponse.Content.GetType() == typeof(byte[]))
                            {
                                loContentTask = loHttpClientResponse.Content.ReadAsByteArrayAsync();
                            }

                            while (!loContentTask.IsCompleted)
                            {
                                System.Threading.Thread.Sleep(10);
                            }

                            if (loContentTask.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
                            {
                                if (loContentTask is System.Threading.Tasks.Task<Stream>)
                                {
                                    loResponseContent = ((System.Threading.Tasks.Task<Stream>)loContentTask).Result;
                                }
                                else if (loContentTask is System.Threading.Tasks.Task<string>)
                                {
                                    loResponseContent = ((System.Threading.Tasks.Task<string>)loContentTask).Result;
                                }
                                else if (loContentTask is System.Threading.Tasks.Task<byte[]>)
                                {
                                    loResponseContent = ((System.Threading.Tasks.Task<byte[]>)loContentTask).Result;
                                }
                            }
                            else
                            {
                                throw new MaxException("Read content task to " + loReqestUrl + " completed with status " + loTask.Status.ToString());
                            }
                        }
                    }
                    else
                    {
                        throw new MaxException("Post call to " + loReqestUrl + " failed with response " + loHttpClientResponse.StatusCode.ToString());
                    }
                }
                else
                {
                    throw new MaxException("Task to " + loReqestUrl + " completed with status " + loTask.Status.ToString());
                }

                DateTime ldResponseEnd = DateTime.UtcNow;

                loResponse.Add("ReasonPhrase", loHttpClientResponse.ReasonPhrase);
                loResponse.Add("StatusCode", MaxConvertLibrary.ConvertToString(typeof(object), loHttpClientResponse.StatusCode));
                loResponse.Add("Version", MaxConvertLibrary.ConvertToString(typeof(object), loHttpClientResponse.Version));
                loResponse.Add("RequestMessage.RequestUri", MaxConvertLibrary.ConvertToString(typeof(object), loHttpClientResponse.RequestMessage.RequestUri));

                loDataReturn.Set(loDataModel.RequestTime, ldRequestStart);
                loDataReturn.Set(loDataModel.ResponseTime, ldResponseEnd);
                loDataReturn.Set(loDataModel.Response, MaxConvertLibrary.SerializeObjectToString(loResponse));
                loDataReturn.Set(loDataModel.ResponseRaw, loResponseContent);
                loDataReturn.Set(loDataModel.Log, MaxConvertLibrary.SerializeObjectToString(this._oLog));
                loDataReturn.Set(loDataModel.RequestUrl, loReqestUrl);
                loDataReturn.Set(loDataModel.RequestContent, loContent);
            }
            catch (Exception loE)
            {
                loDataReturn.Set(loDataModel.Exception, loE);
            }

            loR.Add(loDataReturn);
            lnTotal = loR.Count;
            MaxLogLibrary.Log(MaxEnumGroup.LogDebug, "Select [" + loData.DataModel.DataStorageName + "] end", "MaxDataContextHttpClientProvider");
            return loR;
        }

#else
        /// <summary>
        /// Selects data from the database.
        /// </summary>
        /// <param name="loData">Element with data used in the filter.</param>
        /// <param name="loDataQuery">Query information to filter results.</param>
        /// <param name="lnPageIndex">Page to return.</param>
        /// <param name="lnPageSize">Items per page.</param>
        /// <param name="lnTotal">Total items found.</param>
        /// <param name="laFields">list of fields to return from select.</param>
        /// <returns>List of data from select.</returns>
        public virtual MaxDataList SelectConditional(MaxData loData, MaxDataQuery loDataQuery, int lnPageIndex, int lnPageSize, out int lnTotal, params string[] laFields)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
