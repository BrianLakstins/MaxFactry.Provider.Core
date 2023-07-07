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
    using System.IO;
    using MaxFactry.Base.DataLayer;
    using MaxFactry.Base.DataLayer.Provider;
    using MaxFactry.Provider.CoreProvider.DataLayer;

    /// <summary>
    /// Defines a class for implementing the MaxSecurityLibrary functionality
    /// </summary>
    public class MaxSecurityLibraryCoreProvider : MaxSecurityLibraryDefaultProvider
    {
        /// <summary>
        /// Uses client_id and client_secret as basic authentication and includes grant_type and scope in body as formurlencoded data
        /// </summary>
        /// <param name="loTokenUrl"></param>
        /// <param name="lsClientId"></param>
        /// <param name="lsClientSecret"></param>
        /// <param name="lsScope"></param>
        /// <returns></returns>
        public override string GetAccessToken(Uri loTokenUrl, string lsClientId, string lsClientSecret, string lsScope)
        {
            string lsR = null;
#if net4_52
            MaxIndex loConfigIndex = new MaxIndex();
            loConfigIndex.Add("BasicAuthClientId", lsClientId);
            loConfigIndex.Add("BasicAuthClientSecret", lsClientSecret);
            loConfigIndex.Add("grant_type", "client_credentials");
            loConfigIndex.Add("scope", lsScope);
            lsR = this.GetAccessToken(loTokenUrl, loConfigIndex);
#endif
            return lsR;
        }

        /// <summary>
        /// Uses MaxHttpClientDataModel to get token
        /// </summary>
        /// <param name="loTokenUrl"></param>
        /// <param name="loCredentialIndex"></param>
        /// <returns></returns>
        public override string GetAccessToken(Uri loTokenUrl, MaxIndex loConfigIndex)
        {
            string lsR = null;
#if net4_52
            MaxHttpClientDataModel loDataModel = new MaxHttpClientDataModel();
            MaxData loData = new MaxData(loDataModel);
            loData.Set(loDataModel.RequestUrl, loTokenUrl);
            loData.Set(loDataModel.RequestContent, loConfigIndex);
            int lnTotal = 0;
            MaxDataList loDataList = MaxStorageWriteRepository.Select(loData, null, 0, 0, string.Empty, out lnTotal);
            if (loDataList.Count == 1)
            {
                object loResponse = loDataList[0].Get(loDataModel.ResponseRaw);
                lsR = loResponse as string;
                if (null == lsR && loResponse is Stream)
                {
                    lsR = new StreamReader(loResponse as Stream).ReadToEnd();
                }
            }
#endif

            return lsR;
        }
    }
}