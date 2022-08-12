// <copyright file="MaxSecurityLibraryCoreProvider.cs" company="Lakstins Family, LLC">
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
// <change date="3/1/2022" author="Brian A. Lakstins" description="Initial creation">
// </changelog>
#endregion

namespace MaxFactry.Core.Provider
{
    using System;
    using MaxFactry.Base.BusinessLayer;
    using MaxFactry.General.BusinessLayer;

    /// <summary>
    /// Defines a class for implementing the MaxSecurityLibrary functionality
    /// </summary>
    public class MaxSecurityLibraryCoreProvider : MaxSecurityLibraryDefaultProvider
    {
        public override string GetAccessToken(Uri loTokenUrl, string lsClientId, string lsClientSecret, string lsScope)
        {
            string lsR = null;
            System.Net.Http.HttpClientHandler loHandler = new System.Net.Http.HttpClientHandler();
            if (loHandler.SupportsAutomaticDecompression)
            {
                loHandler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;
            }

            System.Net.Http.HttpClient loClient = new System.Net.Http.HttpClient(loHandler);
            loClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(string.Format("{0}:{1}", lsClientId, lsClientSecret))));
            System.Net.Http.Headers.CacheControlHeaderValue loCache = new System.Net.Http.Headers.CacheControlHeaderValue();
            loCache.NoCache = true;
            loClient.DefaultRequestHeaders.CacheControl = loCache;
            System.Collections.Generic.Dictionary<string, string> loAuth = new System.Collections.Generic.Dictionary<string, string>();
            loAuth.Add("grant_type", "client_credentials");
            loAuth.Add("scope", lsScope);
            System.Net.Http.HttpContent loContent = new System.Net.Http.FormUrlEncodedContent(loAuth); ;
            try
            {
                System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> loTask = loClient.PostAsync(loTokenUrl, loContent);
                while (!loTask.IsCompleted)
                {
                    System.Threading.Thread.Sleep(100);
                }
                System.Net.Http.HttpResponseMessage loHttpClientResponse = loTask.Result;
                if (loHttpClientResponse.IsSuccessStatusCode)
                {
                    MaxFactry.General.BusinessLayer.MaxUserAuthTokenEntity loTokenEntity = MaxFactry.General.BusinessLayer.MaxUserAuthTokenEntity.Create();
                    lsR = loHttpClientResponse.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    throw new MaxException("Post call to " + loTokenUrl.ToString() + " failed with response " + loHttpClientResponse.StatusCode.ToString());
                }
            }
            catch (Exception loE)
            {
                MaxLogLibrary.Log(new MaxLogEntryStructure(MaxEnumGroup.LogError, "Error getting content from {Url}", loE, loTokenUrl));
            }

            return lsR;
        }

        public override string GetAccessToken(Uri loTokenUrl, MaxIndex loCredentialIndex)
        {
            string lsR = null;
            System.Net.Http.HttpClientHandler loHandler = new System.Net.Http.HttpClientHandler();
            if (loHandler.SupportsAutomaticDecompression)
            {
                loHandler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;
            }

            System.Net.Http.HttpClient loClient = new System.Net.Http.HttpClient(loHandler);
            System.Net.Http.Headers.CacheControlHeaderValue loCache = new System.Net.Http.Headers.CacheControlHeaderValue();
            loCache.NoCache = true;
            loClient.DefaultRequestHeaders.CacheControl = loCache;
            string lsJson = MaxConvertLibrary.SerializeObjectToString(loCredentialIndex);
            System.Net.Http.HttpContent loContent = new System.Net.Http.StringContent(lsJson, System.Text.Encoding.UTF8, "application/json");
            try
            {
                System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> loTask = loClient.PostAsync(loTokenUrl, loContent);
                while (!loTask.IsCompleted)
                {
                    System.Threading.Thread.Sleep(100);
                }
                System.Net.Http.HttpResponseMessage loHttpClientResponse = loTask.Result;
                if (loHttpClientResponse.IsSuccessStatusCode)
                {
                    MaxFactry.General.BusinessLayer.MaxUserAuthTokenEntity loTokenEntity = MaxFactry.General.BusinessLayer.MaxUserAuthTokenEntity.Create();
                    lsR = loHttpClientResponse.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    throw new MaxException("Post call to " + loTokenUrl.ToString() + " failed with response " + loHttpClientResponse.StatusCode.ToString());
                }
            }
            catch (Exception loE)
            {
                MaxLogLibrary.Log(new MaxLogEntryStructure(MaxEnumGroup.LogError, "Error getting content from {Url}", loE, loTokenUrl));
            }


            return lsR;
        }
    }
}