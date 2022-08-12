// <copyright file="MaxStorageWriteRepositoryHttpClientProvider.cs" company="Lakstins Family, LLC">
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
// <change date="8/1/2019" author="Brian A. Lakstins" description="Initial Creation">
// </changelog>
#endregion

namespace MaxFactry.Base.DataLayer.Provider
{
    using System;
    using System.IO;
    using MaxFactry.Core;
    using MaxFactry.Provider.CoreProvider.DataLayer;

    /// <summary>
    /// Provides base for creating Providers for Repositories that use a subclass of MaxDataModel for storage.
    /// </summary>
    public class MaxStorageWriteRepositoryHttpClientProvider : MaxStorageWriteRepositoryDefaultProvider
    {
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
        public override MaxDataList Select(MaxData loData, MaxDataQuery loDataQuery, int lnPageIndex, int lnPageSize, string lsSort, out int lnTotal, params string[] laFields)
        {
            MaxDataList loR = new MaxDataList();
            if (loData.DataModel is MaxHttpClientDataModel)
            {
                loData.Set("IMaxDataContextProvider", typeof(MaxFactry.Base.DataLayer.Provider.MaxDataContextHttpClientProvider));
                MaxDataList loDataList = base.Select(loData, loDataQuery, lnPageIndex, lnPageIndex, lsSort, out lnTotal, laFields);
                loR = loDataList;
                lnTotal = loR.Count;
            }
            else
            {
                loR = base.Select(loData, loDataQuery, lnPageIndex, lnPageIndex, lsSort, out lnTotal, laFields);
            }

            return loR;
        }
    }
}