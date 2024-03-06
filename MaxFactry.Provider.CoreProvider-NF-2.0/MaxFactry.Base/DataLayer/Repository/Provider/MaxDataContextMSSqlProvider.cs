// <copyright file="MaxDataContextMSSqlProvider.cs" company="Lakstins Family, LLC">
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
// <change date="2/3/2014" author="Brian A. Lakstins" description="Initial Release">
// <change date="2/7/2014" author="Brian A. Lakstins" description="Review and update documentation.">
// <change date="4/14/2014" author="Brian A. Lakstins" description="Allow the same field to be used as multiple query parameters.">
// <change date="6/26/2014" author="Brian A. Lakstins" description="Update for addition of StorageKey.">
// <change date="8/21/2014" author="Brian A. Lakstins" description="Added stream methods.">
// <change date="1/13/2015" author="Brian A. Lakstins" description="Move functionality to generic ADO provider and base this one on that one.">
// </changelog>
#endregion

namespace MaxFactry.Base.DataLayer.Provider
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.IO;
    using MaxFactry.Core;
    using MaxFactry.Base.DataLayer.Library;
    using MaxFactry.Provider.CoreProvider.DataLayer;

    /// <summary>
    /// Data Context used to work with Microsoft SQL databases through ADO.NET (Connection, Command, Parameter)
    /// </summary>
    public class MaxDataContextMSSqlProvider : MaxDataContextADODbProvider
	{
        public override void Initialize(string lsName, MaxIndex loConfig)
        {
            base.Initialize(lsName, loConfig);
        }

        /// <summary>
        /// Selects data from the database.
        /// </summary>
        /// <param name="loData">Element with data used in the filter.</param>
        /// <param name="loDataQuery">Query information to filter results.</param>
        /// <param name="lnPageIndex">Page to return.</param>
        /// <param name="lnPageSize">Items per page.</param>
        /// <param name="lsOrderBy">Sort Information.</param>
        /// <param name="lnTotal">Total items found.</param>
        /// <param name="laDataNameList">list of fields to return from select.</param>
        /// <returns>List of data from select.</returns>
        public override MaxDataList Select(MaxData loData, MaxDataQuery loDataQuery, int lnPageIndex, int lnPageSize, string lsOrderBy, out int lnTotal, params string[] laDataNameList)
        {
            MaxLogLibrary.Log(MaxEnumGroup.LogDebug, "Select [" + loData.DataModel.DataStorageName + "] start", "MaxDataContextADODbProvider");
            MaxDataList loR = new MaxDataList(loData.DataModel);
            lnTotal = 0;
            DbConnection loConnection = MaxDbProviderFactoryLibrary.GetConnection(this.DbProviderFactoryProviderName, this.DbProviderFactoryProviderType);
            if (this.IsTableFound(loData.DataModel, loConnection))
            {
                string lsStorageKey = MaxConvertLibrary.ConvertToString(typeof(object), loData.Get(loData.DataModel.StorageKey));
                loData.Set(loData.DataModel.StorageKey, lsStorageKey);
                string lsSql = MaxSqlGenerationLibrary.GetSelect(this.SqlProviderName, this.SqlProviderType, loData, loDataQuery, laDataNameList);
                MaxLogLibrary.Log(new MaxLogEntryStructure("SQL", MaxEnumGroup.LogDebug, "Select [{DataStorageName}] sql [{SQL}]", loData.DataModel.DataStorageName, lsSql));
                if (lsSql.Length > 0)
                {
                    string lsOrderSql = string.Empty;
                    if (!string.IsNullOrEmpty(lsOrderBy))
                    {
                        lsOrderSql = " ORDER BY " + lsOrderBy;
                        if (lnPageIndex > 0 && lnPageSize > 0)
                        {
                            int lnOffset = (lnPageIndex - 1) * lnPageSize;
                            lsOrderSql += " OFFSET " + lnOffset + " ROWS FETCH NEXT " + lnPageSize + " ROWS ONLY";
                        }
                    }

                    List<string> loParameters = this.GetParameterNames(lsSql);
                    DbCommand loCommand = MaxDbProviderFactoryLibrary.GetCommand(this.DbProviderFactoryProviderName, this.DbProviderFactoryProviderType);
                    try
                    {
                        loCommand.Connection = loConnection;
                        loCommand.CommandText = MaxSqlGenerationLibrary.GetCommandText(this.SqlProviderName, this.SqlProviderType, lsSql + lsOrderSql);

                        List<string> loDataKey = new List<string>(loData.DataModel.GetKeyList());
                        object[] laDataQuery = loDataQuery.GetQuery();
                        foreach (string lsParameterName in loParameters)
                        {
                            object loValue = null;
                            for (int lnDQ = 0; lnDQ < laDataQuery.Length; lnDQ++)
                            {
                                object loStatement = laDataQuery[lnDQ];
                                if (loStatement is MaxDataFilter)
                                {
                                    MaxDataFilter loDataFilter = (MaxDataFilter)loStatement;
                                    if (lsParameterName.Equals("DQ" + lnDQ.ToString() + loDataFilter.Name))
                                    {
                                        loValue = loDataFilter.Value;
                                    }
                                }
                            }

                            if (null == loValue && loDataKey.Contains(lsParameterName))
                            {
                                loValue = loData.Get(lsParameterName);
                            }

                            if (null != loValue)
                            {
                                MaxLogLibrary.Log(new MaxLogEntryStructure("SQLParam", MaxEnumGroup.LogDebug, "Select [{DataStorageName}] add parameter [{ParameterName}][{Value}]", loData.DataModel.DataStorageName, lsParameterName, loValue));
                                this.AddDbParameter(loValue, lsParameterName, loCommand);
                            }
                            else
                            {
                                MaxLogLibrary.Log(new MaxLogEntryStructure("SQLParam", MaxEnumGroup.LogDebug, "Select [{DataStorageName}] add parameter [{ParameterName}] is null", loData.DataModel.DataStorageName, lsParameterName));
                            }
                        }

                        lnTotal = MaxDbCommandLibrary.Fill(this.DbCommandProviderName, this.DbCommandLibraryProviderType, loCommand, loR, 0, 0);

                        if (lnPageIndex > 0 && lnPageSize > 0)
                        {
                            string lsCountSql = MaxSqlGenerationLibrary.GetSelectCount(this.SqlProviderName, this.SqlProviderType, loData, loDataQuery);
                            loCommand.CommandText = MaxSqlGenerationLibrary.GetCommandText(this.SqlProviderName, this.SqlProviderType, lsCountSql);
                            object loResult = MaxDbCommandLibrary.ExecuteScaler(this.DbCommandProviderName, this.DbCommandLibraryProviderType, loCommand);
                            lnTotal = MaxConvertLibrary.ConvertToInt(typeof(object), loResult);
                        }
                    }
                    finally
                    {
                        loCommand.Connection = null;
                        loCommand.Dispose();
                        loCommand = null;
                        loConnection.Dispose();
                        loConnection = null;
                    }
                }
            }

            MaxLogLibrary.Log(MaxEnumGroup.LogDebug, "Select [" + loData.DataModel.DataStorageName + "] end", "MaxDataContextADODbProvider");
            return loR;
        }

    }
}
