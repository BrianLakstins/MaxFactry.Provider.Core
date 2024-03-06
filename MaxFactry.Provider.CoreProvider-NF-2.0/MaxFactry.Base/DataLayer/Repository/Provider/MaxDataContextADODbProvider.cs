// <copyright file="MaxDataContextADODbProvider.cs" company="Lakstins Family, LLC">
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
// <change date="5/27/2015" author="Brian A. Lakstins" description="Adding ability to run some initialization Sql.">
// <change date="1/16/2016" author="Brian A. Lakstins" description="Update streamsave to throw exception only if there is a stream to save.">
// <change date="1/28/2016" author="Brian A. Lakstins" description="Fix using name for getting configuration. Add easy way to specify MS Access database file.">
// <change date="4/22/2016" author="Brian A. Lakstins" description="Updated to support altering a table.">
// <change date="5/10/2016" author="Brian A. Lakstins" description="Updates to support using multiple providers by name.">
// <change date="5/18/2016" author="Brian A. Lakstins" description="Add support for server side id (autoincrement).">
// <change date="7/4/2016" author="Brian A. Lakstins" description="Updated to access provider configuration using base provider methods.">
// <change date="7/22/2016" author="Brian A. Lakstins" description="Updated most methods to virtual so they can be overridden.">
// <change date="12/29/2016" author="Brian A. Lakstins" description="Updated to allow more configuration information for creating configuration ADODbProviders.">
// <change date="12/22/2017" author="Brian A. Lakstins" description="Fix issue with checking for table existance on Insert and Update.">
// <change date="1/23/2018" author="Brian A. Lakstins" description="Update to store table existance in cache so it can be cleared.  Fix issue about not creating tables.">
// <change date="11/30/2018" author="Brian A. Lakstins" description="Updated for changes to base.">
// <change date="6/5/2020" author="Brian A. Lakstins" description="Updated for change to base.">
// <change date="6/11/2020" author="Brian A. Lakstins" description="Fix error when value is null.">
// <change date="7/20/2023" author="Brian A. Lakstins" description="Use constants to access configuration names instead of strings.">
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
    /// Data Context used to work with SQL databases through ADO.NET (Connection, Command, Parameter)
    /// </summary>
    public class MaxDataContextADODbProvider : MaxDataContextDefaultProvider
	{
        /// <summary>
        /// Keeps track of which databases are initialized
        /// </summary>
        private static MaxIndex _oIsInitializedIndex = new MaxIndex();

		/// <summary>
		/// Object used to lock thread access when using DML
		/// </summary>
		private static object _oLock = new object();

		/// <summary>
		/// Name of the DbCommandRepositoryProvider to use for this database provider
		/// </summary>
        private Type _oDbCommandLibraryProviderType = typeof(MaxFactry.Provider.CoreProvider.DataLayer.Provider.MaxDbCommandLibraryDefaultProvider);

		/// <summary>
		/// Name of the DbRepositoryProvider to use for this database provider
		/// </summary>
        private Type _oDbProviderFactoryProviderType = typeof(MaxFactry.Provider.CoreProvider.DataLayer.Provider.MaxDbProviderFactoriesDefaultLibraryProvider);

		/// <summary>
		/// Name of the SqlRepositoryProvider to use for this database provider
		/// </summary>
		private Type _oSqlProviderType = typeof(MaxFactry.Base.DataLayer.Library.Provider.MaxSqlGenerationLibraryDefaultProvider);

        private string _sDbCommandProviderName = "Default";

        private string _sDbProviderFactoryProviderName = "Default";

        private string _sSqlProviderName = "Default";

		/// <summary>
        /// Initializes a new instance of the MaxDataContextADODbProvider class
		/// </summary>
        public MaxDataContextADODbProvider()
			: base()
		{
		}

        /// <summary>
        /// Gets the name of the DbCommandRepositoryProvider to use for this database provider
        /// </summary>
        protected Type DbCommandLibraryProviderType
		{
			get
			{
				return this._oDbCommandLibraryProviderType;
			}

            set
            {
                this._oDbCommandLibraryProviderType = value;
            }
		}

		/// <summary>
		/// Gets the name of the DbRepositoryProvider to use for this database provider
		/// </summary>
        protected Type DbProviderFactoryProviderType
		{
			get
			{
				return this._oDbProviderFactoryProviderType;
			}

            set
            {
                this._oDbProviderFactoryProviderType = value;
            }
        }

		/// <summary>
		/// Gets the name of the SqlRepositoryProvider to use for this database provider
		/// </summary>
        protected Type SqlProviderType
		{
			get
			{
				return this._oSqlProviderType;
			}

            set
            {
                this._oSqlProviderType = value;
            }
        }

        protected string DbCommandProviderName
        {
            get
            {
                return this._sDbCommandProviderName;
            }
            
            set
            {
                this._sDbCommandProviderName = value;
            }        
        }

        protected string DbProviderFactoryProviderName
        {
            get
            {
                return this._sDbProviderFactoryProviderName;
            }

            set
            {
                this._sDbProviderFactoryProviderName = value;
            }
        }

        protected string SqlProviderName
        {
            get
            {
                return this._sSqlProviderName;
            }

            set
            {
                this._sSqlProviderName = value;
            }
        }

        /// <summary>
        /// Initializes the provider
        /// </summary>
        /// <param name="lsName">Name of the provider</param>
        /// <param name="loConfig">Configuration information</param>
        public override void Initialize(string lsName, MaxIndex loConfig)
        {
            base.Initialize(lsName, loConfig);
            string lsDbCommandLibraryProviderName = this.GetConfigValue(loConfig, "DbCommandLibraryProviderName") as string;
            if (null != lsDbCommandLibraryProviderName)
            {
                this._sDbCommandProviderName = lsDbCommandLibraryProviderName;
            }

            string lsDbProviderFactoryProviderName = this.GetConfigValue(loConfig, "DbProviderFactoryProviderName") as string;
            if (null != lsDbProviderFactoryProviderName)
            {
                this._sDbProviderFactoryProviderName = lsDbProviderFactoryProviderName;
            }

            string lsSqlProviderName = this.GetConfigValue(loConfig, "SqlProviderName") as string;
            if (null != lsSqlProviderName)
            {
                this._sSqlProviderName = lsSqlProviderName;
            }

            object loDbCommandLibraryProviderType = this.GetConfigValue(loConfig, "DbCommandLibraryProviderType");
            if (loDbCommandLibraryProviderType is Type)
            {
                this._oDbCommandLibraryProviderType = loDbCommandLibraryProviderType as Type;
            }
            else if (loDbCommandLibraryProviderType is string)
            {
                this._oDbCommandLibraryProviderType = Type.GetType(loDbCommandLibraryProviderType as string);
            }

            object loDbProviderFactoryProviderType = this.GetConfigValue(loConfig, "DbProviderFactoryProviderType");
            if (loDbProviderFactoryProviderType is Type)
            {
                this._oDbProviderFactoryProviderType = loDbProviderFactoryProviderType as Type;
            }
            else if (loDbProviderFactoryProviderType is string)
            {
                this._oDbProviderFactoryProviderType = Type.GetType(loDbProviderFactoryProviderType as string);
            }

            object loSqlProviderType = this.GetConfigValue(loConfig, "SqlProviderType");
            if (loSqlProviderType is Type)
            {
                this._oSqlProviderType = loSqlProviderType as Type;
            }
            else if (loSqlProviderType is string)
            {
                this._oSqlProviderType = Type.GetType(loSqlProviderType as string);
            }
        }

		/// <summary>
		/// Inserts a list of data objects.
		/// </summary>
		/// <param name="loDataList">The list of data objects to insert.</param>
		/// <returns>The count affected.</returns>
		public override int Insert(MaxDataList loDataList)
		{
			int lnR = 0;
            DbConnection loConnection = MaxDbProviderFactoryLibrary.GetConnection(this.DbProviderFactoryProviderName, this.DbProviderFactoryProviderType);
            if (this.HasTable(loDataList.DataModel, loConnection))
            {
                string lsSql = MaxSqlGenerationLibrary.GetInsert(this.SqlProviderName, this.SqlProviderType, loDataList);
                DbCommand loCommand = MaxDbProviderFactoryLibrary.GetCommand(this.DbProviderFactoryProviderName, this.DbProviderFactoryProviderType);
                try
                {
                    loCommand.Connection = loConnection;
                    loCommand.CommandText = MaxSqlGenerationLibrary.GetCommandText(this.SqlProviderName, this.SqlProviderType, lsSql);
                    MaxLogLibrary.Log(MaxEnumGroup.LogDebug, "INSERT sql [" + loCommand.CommandText + "]", "MaxDataContextADODbProvider");
                    string[] laKey = loDataList.DataModel.GetKeyList();
                    for (int lnK = 0; lnK < laKey.Length; lnK++)
                    {
                        bool lbIsServerId = loDataList.DataModel.GetPropertyAttributeSetting(laKey[lnK], "IsServerId");
                        if (!lbIsServerId)
                        {
                            for (int lnDL = 0; lnDL < loDataList.Count; lnDL++)
                            {
                                object loValue = loDataList[lnDL].Get(laKey[lnK]);
                                if (null == loValue && typeof(bool).Equals(loDataList.DataModel.GetValueType(laKey[lnK])))
                                {
                                    loValue = false;
                                }

                                if (null != loValue)
                                {
                                    string lsParameterName = MaxSqlGenerationLibrary.GetParameterName(laKey[lnK], lnDL);
                                    this.AddDbParameter(loValue, lsParameterName, loCommand);
                                    MaxLogLibrary.Log(MaxEnumGroup.LogDebug, "INSERT Parameter [" + lsParameterName + "][" + loValue.ToString() + "]", "MaxDataContextADODbProvider");
                                }
                            }
                        }
                    }

                    if (lsSql.Contains(";SELECT "))
                    {
                        object loResult = MaxDbCommandLibrary.ExecuteScaler(this.DbCommandProviderName, this.DbCommandLibraryProviderType, loCommand);
                        if (loResult is int)
                        {
                            lnR = (int)loResult;
                        }
                        else
                        {
                            int lnResult = 0;
                            int.TryParse(loResult.ToString(), out lnResult);
                            lnR = lnResult;
                        }
                    }
                    else
                    {
                        lnR = MaxDbCommandLibrary.ExecuteNonQueryTransaction(this.DbCommandProviderName, this.DbCommandLibraryProviderType, loCommand);
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

                for (int lnD = 0; lnD < loDataList.Count; lnD++)
                {
                    loDataList[lnD].ClearChanged();
                }
            }

			return lnR;
		}

		/// <summary>
		/// Updates a list of data objects.
		/// </summary>
		/// <param name="loDataList">The list of data objects to insert.</param>
		/// <returns>The count affected.</returns>
		public override int Update(MaxDataList loDataList)
		{
			int lnR = 0;
            DbConnection loConnection = MaxDbProviderFactoryLibrary.GetConnection(this.DbProviderFactoryProviderName, this.DbProviderFactoryProviderType);
            if (this.HasTable(loDataList.DataModel, loConnection))
            {
                string lsSql = MaxSqlGenerationLibrary.GetUpdate(this.SqlProviderName, this.SqlProviderType, loDataList);
                if (lsSql.Length > 0)
                {
                    DbCommand loCommand = MaxDbProviderFactoryLibrary.GetCommand(this.DbProviderFactoryProviderName, this.DbProviderFactoryProviderType);
                    try
                    {
                        List<string> loParameters = this.GetParameterNames(lsSql);
                        loCommand.Connection = loConnection;
                        loCommand.CommandText = MaxSqlGenerationLibrary.GetCommandText(this.SqlProviderName, this.SqlProviderType, lsSql);
                        foreach (string lsParameterName in loParameters)
                        {
                            string[] laName = lsParameterName.Split('$');
                            string lsKey = laName[0];
                            int lnDL = Convert.ToInt32(laName[1]);
                            object loValue = loDataList[lnDL].Get(lsKey);
                            this.AddDbParameter(loValue, lsParameterName, loCommand);
                        }

                        lnR = MaxDbCommandLibrary.ExecuteNonQueryTransaction(this.DbCommandProviderName, this.DbCommandLibraryProviderType, loCommand);
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

                for (int lnD = 0; lnD < loDataList.Count; lnD++)
                {
                    loDataList[lnD].ClearChanged();
                }
            }

			return lnR;
		}

		/// <summary>
		/// Deletes a list of data objects.
		/// </summary>
		/// <param name="loDataList">The list of data objects to insert.</param>
		/// <returns>The count affected.</returns>
		public override int Delete(MaxDataList loDataList)
		{
			int lnR = 0;
            DbConnection loConnection = MaxDbProviderFactoryLibrary.GetConnection(this.DbProviderFactoryProviderName, this.DbProviderFactoryProviderType);
            if (this.HasTable(loDataList.DataModel, loConnection))
			{
                string lsSql = MaxSqlGenerationLibrary.GetDelete(this.SqlProviderName, this.SqlProviderType, loDataList);
                DbCommand loCommand = MaxDbProviderFactoryLibrary.GetCommand(this.DbProviderFactoryProviderName, this.DbProviderFactoryProviderType);
				try
				{
					List<string> loParameters = this.GetParameterNames(lsSql);
					loCommand.Connection = loConnection;
                    loCommand.CommandText = MaxSqlGenerationLibrary.GetCommandText(this.SqlProviderName, this.SqlProviderType, lsSql);
					foreach (string lsParameterName in loParameters)
					{
						string[] laName = lsParameterName.Split('$');
						string lsKey = laName[0];
						int lnDL = Convert.ToInt32(laName[1]);
                        object loValue = loDataList[lnDL].Get(lsKey);
						this.AddDbParameter(loValue, lsParameterName, loCommand);
					}

                    lnR = MaxDbCommandLibrary.ExecuteNonQueryTransaction(this.DbCommandProviderName, this.DbCommandLibraryProviderType, loCommand);
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

			return lnR;
		}

        /// <summary>
        /// Selects all data from the data storage name for the specified type.
        /// </summary>
        /// <param name="lsDataStorageName">Name of the data storage (table name).</param>
        /// <param name="laDataNameList">list of fields to return from select</param>
        /// <returns>List of data elements with a base data model.</returns>
        public override MaxDataList SelectAll(string lsDataStorageName, params string[] laDataNameList)
        {
            MaxLogLibrary.Log(MaxEnumGroup.LogDebug, "Select [" + lsDataStorageName + "] start", "MaxDataContextADODbProvider");
            MaxDataModel loDataModel = new MaxDataModel(lsDataStorageName);
            MaxDataList loR = new MaxDataList(loDataModel);
            DbConnection loConnection = MaxDbProviderFactoryLibrary.GetConnection(this.DbProviderFactoryProviderName, this.DbProviderFactoryProviderType);
            if (this.IsTableFound(loDataModel, loConnection))
            {
                string lsSql = MaxSqlGenerationLibrary.GetSelect(this.SqlProviderName, this.SqlProviderType, lsDataStorageName, laDataNameList);
                MaxLogLibrary.Log(MaxEnumGroup.LogDebug, "Select [" + lsDataStorageName + "] sql [" + lsSql + "]", "MaxDataContextADODbProvider");
                if (lsSql.Length > 0)
                {
                    List<string> loParameters = this.GetParameterNames(lsSql);
                    DbCommand loCommand = MaxDbProviderFactoryLibrary.GetCommand(this.DbProviderFactoryProviderName, this.DbProviderFactoryProviderType);
                    
                    try
                    {
                        loCommand.Connection = loConnection;
                        loCommand.CommandText = MaxSqlGenerationLibrary.GetCommandText(this.SqlProviderName, this.SqlProviderType, lsSql);
                        int lnTotal = MaxDbCommandLibrary.Fill(this.DbCommandProviderName, this.DbCommandLibraryProviderType, loCommand, loR, 1, 0);
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

            return loR;
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
					List<string> loParameters = this.GetParameterNames(lsSql);
                    DbCommand loCommand = MaxDbProviderFactoryLibrary.GetCommand(this.DbProviderFactoryProviderName, this.DbProviderFactoryProviderType);
					try
					{
						loCommand.Connection = loConnection;
                        loCommand.CommandText = MaxSqlGenerationLibrary.GetCommandText(this.SqlProviderName, this.SqlProviderType, lsSql);

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

                        lnTotal = MaxDbCommandLibrary.Fill(this.DbCommandProviderName, this.DbCommandLibraryProviderType, loCommand, loR, lnPageIndex, lnPageSize);
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

		/// <summary>
		/// Gets the number of records that match the filter.
		/// </summary>
		/// <param name="loData">Element with data used in the filter.</param>
        /// <param name="loDataQuery">Query information to filter results.</param>
        /// <returns>number of records that match.</returns>
        public override int SelectCount(MaxData loData, MaxDataQuery loDataQuery)
		{
			int lnR = 0;
            DbConnection loConnection = MaxDbProviderFactoryLibrary.GetConnection(this.DbProviderFactoryProviderName, this.DbProviderFactoryProviderType);
            if (this.IsTableFound(loData.DataModel, loConnection))
			{
                string lsStorageKey = MaxConvertLibrary.ConvertToString(typeof(object), loData.Get(loData.DataModel.StorageKey));
                loData.Set(loData.DataModel.StorageKey, lsStorageKey);
                string lsSql = MaxSqlGenerationLibrary.GetSelectCount(this.SqlProviderName, this.SqlProviderType, loData, loDataQuery);
				if (lsSql.Length > 0)
				{
					List<string> loParameters = this.GetParameterNames(lsSql);
                    DbCommand loCommand = MaxDbProviderFactoryLibrary.GetCommand(this.DbProviderFactoryProviderName, this.DbProviderFactoryProviderType);
					try
					{
						loCommand.Connection = loConnection;
                        loCommand.CommandText = MaxSqlGenerationLibrary.GetCommandText(this.SqlProviderName, this.SqlProviderType, lsSql);

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
                                    if (loDataFilter.Name.Equals(lsParameterName))
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
								this.AddDbParameter(loValue, lsParameterName, loCommand);
							}
						}

                        lnR = Convert.ToInt32(MaxDbCommandLibrary.ExecuteScaler(this.DbCommandProviderName, this.DbCommandLibraryProviderType, loCommand));
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

			return lnR;
		}

        public static MaxIndex AddConfig(string lsName, string lsConnectionString, string lsProviderName, string lsClass, string lsAssemblyPath, Type loSqlProviderType, Type loRepositoryProviderType)
        {
            MaxIndex loR = new MaxIndex();
            loR.Add("MaxProviderName", lsName);
            loR.Add(
                lsName + ":" + typeof(MaxFactry.Provider.CoreProvider.DataLayer.Provider.MaxDbProviderFactoriesDefaultLibraryProvider).ToString() + "-ConnectionString",
                lsConnectionString);
            loR.Add(
                lsName + ":" + typeof(MaxFactry.Provider.CoreProvider.DataLayer.Provider.MaxDbProviderFactoriesDefaultLibraryProvider).ToString() + "-ProviderName",
                lsProviderName);
            loR.Add(
                lsName + ":" + typeof(MaxFactry.Provider.CoreProvider.DataLayer.Provider.MaxDbProviderFactoriesDefaultLibraryProvider).ToString() + "-Class",
                lsClass);
            loR.Add(
                lsName + ":" + typeof(MaxFactry.Provider.CoreProvider.DataLayer.Provider.MaxDbProviderFactoriesDefaultLibraryProvider).ToString() + "-AssemblyPath",
                lsAssemblyPath);
            MaxFactry.Core.MaxFactryLibrary.SetValue(lsName + ":" + typeof(MaxFactry.Provider.CoreProvider.DataLayer.Provider.MaxDbProviderFactoriesDefaultLibraryProvider) + "-Config", loR);

            loR.Add(
                lsName + ":" + typeof(MaxFactry.Base.DataLayer.Provider.MaxDataContextADODbProvider) + "-SqlProviderName", 
                lsName);
            loR.Add(
                lsName + ":" + typeof(MaxFactry.Base.DataLayer.Provider.MaxDataContextADODbProvider) + "-SqlProviderType",
                loSqlProviderType);
            loR.Add(
                lsName + ":" + typeof(MaxFactry.Base.DataLayer.Provider.MaxDataContextADODbProvider) + "-DbCommandLibraryProviderName", 
                lsName);
            loR.Add(
                lsName + ":" + typeof(MaxFactry.Base.DataLayer.Provider.MaxDataContextADODbProvider) + "-DbProviderFactoryProviderName",
                lsName);
            loR.Add(typeof(MaxFactry.Core.MaxProvider) + "-" + MaxDataContextDefaultProvider.DefaultContextProviderConfigName, "DefaultContextProvider-" + lsName);
            loR.Add("DefaultContextProvider-" + lsName, typeof(MaxFactry.Base.DataLayer.Provider.MaxDataContextADODbProvider));
            loR.Add(typeof(MaxFactry.Core.MaxProvider) + "-" + MaxDataContextDefaultProvider.ContextProviderConfigName, lsName);

            MaxFactry.Core.MaxFactryLibrary.SetValue(lsName + ":" + typeof(MaxFactry.Base.DataLayer.Provider.MaxDataContextADODbProvider) + "-Config", loR);
            MaxFactry.Core.MaxFactryLibrary.RemoveSingletonProvider(typeof(MaxFactry.Provider.CoreProvider.DataLayer.Provider.MaxDbProviderFactoriesDefaultLibraryProvider), lsName);
            MaxFactry.Core.MaxFactryLibrary.RemoveSingletonProvider(typeof(MaxFactry.Base.DataLayer.Provider.MaxDataContextADODbProvider), lsName);

            if (null != loRepositoryProviderType)
            {
                MaxFactry.Core.MaxFactryLibrary.SetValue(lsName + ":" + loRepositoryProviderType + "-Config", loR);
                if (null == MaxFactry.Core.MaxFactryLibrary.GetValue(loRepositoryProviderType + "-Config"))
                {
                    MaxFactry.Core.MaxFactryLibrary.SetValue(loRepositoryProviderType + "-Config", loR);
                }

                MaxFactry.Core.MaxFactryLibrary.RemoveSingletonProvider(loRepositoryProviderType);
            }

            return loR;
        }

        public static MaxIndex AddConfigMSAccess(string lsFile, Type loRepositoryProviderType, string lsPassword)
        {
            string lsProviderName = "System.Data.OleDb";
            string lsAssembly = string.Empty;
            string lsAssemblyPath = string.Empty;
            string lsConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + lsFile + ";";
            if (null != lsPassword && lsPassword.Length > 0)
            {
                lsConnectionString += "Jet OLEDB:Database Password=" + lsPassword + ";";
            }

            return AddConfig(
                lsFile,
                lsConnectionString,
                lsProviderName,
                lsAssembly,
                lsAssemblyPath,
                typeof(MaxFactry.Base.DataLayer.Library.Provider.MaxSqlGenerationLibraryMSAccessProvider),
                loRepositoryProviderType);
        }

		/// <summary>
		/// Creates and adds a parameter to a command.
		/// </summary>
		/// <param name="loValue">The value of the parameter.</param>
		/// <param name="lsParameterName">The name of the parameter.</param>
		/// <param name="loCommand">The command to add the parameter.</param>
        protected virtual void AddDbParameter(object loValue, string lsParameterName, DbCommand loCommand)
		{
            DbParameter loParameter = MaxDbProviderFactoryLibrary.GetParameter(this.DbProviderFactoryProviderName, this.DbProviderFactoryProviderType);
			loParameter.Direction = ParameterDirection.Input;
			loParameter.ParameterName = string.Format("@{0}", lsParameterName);
			
			if (null != loValue && loValue.GetType() == typeof(DateTime))
			{
				if (loCommand.Connection is System.Data.OleDb.OleDbConnection)
				{
					loValue = Convert.ToDateTime(loValue).ToString("MMM dd yyyy") + " " + Convert.ToDateTime(loValue).ToLongTimeString();
				}
				else if (loCommand.Connection is System.Data.SqlClient.SqlConnection)
				{
					loParameter.DbType = DbType.DateTime2;
				}
			}

			loParameter.Value = loValue;
			loCommand.Parameters.Add(loParameter);
		}

		/// <summary>
		/// Parses Sql and gives list of parameters found.
		/// </summary>
		/// <param name="lsSql">Sql Filter.</param>
		/// <returns>List of parameters.</returns>
        protected virtual List<string> GetParameterNames(string lsSql)
		{
			List<string> loList = new List<string>();
			if (lsSql.Length > 0 && lsSql.Contains("@"))
			{
				////Inside a quoted value
				bool lbInValue = false;
				////Inside a parameter name (starts with @ and ends when it's not a letter or digit anymore)
				bool lbInParam = false;
				string lsParam = string.Empty;
				for (int lnF = 0; lnF <= lsSql.Length; lnF++)
				{
					char lsChar = ' ';
					if (lnF < lsSql.Length)
					{
						lsChar = lsSql[lnF];
					}

					if (lbInParam)
					{
						if (char.IsLetterOrDigit(lsChar) || '$' == lsChar || '_' == lsChar)
						{
							lsParam += lsChar.ToString();
						}
						else
						{
							loList.Add(lsParam);
							lsParam = string.Empty;
							lbInParam = false;
						}
					}
					else
					{
						if (lsChar == '\'')
						{
							lbInValue = !lbInValue;
						}
						else if (!lbInValue && lsChar == '@')
						{
							lbInParam = true;
							lsParam = string.Empty;
						}
					}
				}
			}

			return loList;
		}

		/// <summary>
		/// Checks to see if a table exists in the database.
		/// </summary>
		/// <param name="lsTableName">name of the table.</param>
		/// <returns>true if exists now, or was found to exist on a previous call.</returns>
        protected virtual bool HasTable(MaxDataModel loDataModel, DbConnection loConnection)
        {
            this.Initialize();
            bool lbR = this.IsTableFound(loDataModel, loConnection);
            if (!lbR)
            {
                this.CreateTable(loDataModel, loConnection);
                lbR = true;
            }

            return lbR;
        }

        /// <summary>
        /// Checks to see if a table exists in the database.
        /// </summary>
        /// <param name="lsTableName">name of the table.</param>
        /// <returns>true if exists now, or was found to exist on a previous call.</returns>
        protected virtual bool IsTableFound(MaxDataModel loDataModel, DbConnection loConnection)
        {
            this.Initialize();
            bool lbR = true;
            if (!loDataModel.DataStorageName.Contains("_View"))
            {
                string lsTableKey = this.GetHasTableKey(loDataModel, loConnection);
                string lsHasTable = MaxCacheRepository.Get(typeof(object), "_IsTableFound" + lsTableKey, typeof(string)) as string;
                if (string.IsNullOrEmpty(lsHasTable))
                {
                    lock (_oLock)
                    {
                        lsHasTable = MaxCacheRepository.Get(typeof(object), "_IsTableFound" + lsTableKey, typeof(string)) as string;
                        if (string.IsNullOrEmpty(lsHasTable))
                        {
                            lbR = false;
                            string lsSql = MaxSqlGenerationLibrary.GetTableExists(this.SqlProviderName, this.SqlProviderType);
                            DbCommand loCommand = MaxDbProviderFactoryLibrary.GetCommand(this.DbProviderFactoryProviderName, this.DbProviderFactoryProviderType);
                            loCommand.CommandText = MaxSqlGenerationLibrary.GetCommandText(this.SqlProviderName, this.SqlProviderType, lsSql);
                            loCommand.Connection = loConnection;
                            try
                            {
                                DbParameter loParameter = loCommand.CreateParameter();
                                loParameter.ParameterName = "@TableName";
                                loParameter.DbType = DbType.String;
                                loParameter.Value = loDataModel.DataStorageName;
                                loCommand.Parameters.Add(loParameter);
                                int lnCount = Convert.ToInt16(MaxDbCommandLibrary.ExecuteScaler(this.DbCommandProviderName, this.DbCommandLibraryProviderType, loCommand));
                                if (lnCount > 0)
                                {
                                    lbR = true;
                                    MaxCacheRepository.Set(typeof(object), "_IsTableFound" + lsTableKey, "Table Found");
                                    //// Check to make sure table columns matches DataModel columns.  Add any that don't exist.
                                    string lsSqlColumnList = MaxSqlGenerationLibrary.GetColumnList(this.SqlProviderName, this.SqlProviderType, loDataModel.DataStorageName);
                                    if (!string.IsNullOrEmpty(lsSqlColumnList))
                                    {
                                        DbCommand loCommandColumnList = MaxDbProviderFactoryLibrary.GetCommand(this.DbProviderFactoryProviderName, this.DbProviderFactoryProviderType);
                                        loCommandColumnList.CommandText = MaxSqlGenerationLibrary.GetCommandText(this.SqlProviderName, this.SqlProviderType, lsSqlColumnList);
                                        loCommandColumnList.Connection = loConnection;
                                        MaxDataList loDataList = new MaxDataList(new MaxDataModel());
                                        int lnColumnCount = MaxDbCommandLibrary.Fill(this.DbCommandProviderName, this.DbCommandLibraryProviderType, loCommandColumnList, loDataList, 0, 0);
                                        if (lnColumnCount > 0)
                                        {
                                            string lsSqlAlterTable = MaxSqlGenerationLibrary.GetTableAlter(this.SqlProviderName, this.SqlProviderType, loDataModel, loDataList);
                                            if (!string.IsNullOrEmpty(lsSqlAlterTable))
                                            {
                                                DbCommand loCommandAlterTable = MaxDbProviderFactoryLibrary.GetCommand(this.DbProviderFactoryProviderName, this.DbProviderFactoryProviderType);
                                                loCommandAlterTable.CommandText = MaxSqlGenerationLibrary.GetCommandText(this.SqlProviderName, this.SqlProviderType, lsSqlAlterTable);
                                                loCommandAlterTable.Connection = loConnection;
                                                MaxDbCommandLibrary.ExecuteNonQueryTransaction(this.DbCommandProviderName, this.DbCommandLibraryProviderType, loCommandAlterTable);
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception loE)
                            {
                                MaxLogLibrary.Log(new MaxLogEntryStructure(this.GetType(), "IsTableFound", MaxEnumGroup.LogError, "Exception checking table exists for {DataStorageName} using ConnectionString {ConnectionString}", loE, loDataModel.DataStorageName, loConnection.ConnectionString));
                                if (this.CreateTable(loDataModel, loConnection))
                                {
                                    lbR = true;
                                }
                            }
                            finally
                            {
                                loCommand.Connection = null;
                                loCommand.Dispose();
                                loCommand = null;
                            }
                        }
                    }
                }
            }

            return lbR;
        }

        /// <summary>
        /// Runs database initialization based on the provider.
        /// </summary>
        protected virtual void Initialize()
        {
            string lsKey = this.DbProviderFactoryProviderType.ToString() + "\t" + this.DbProviderFactoryProviderName;
            if (!_oIsInitializedIndex.Contains(lsKey))
            {
                string lsSql = MaxSqlGenerationLibrary.GetDbInitialization(this.SqlProviderName, this.SqlProviderType);
                if (!string.IsNullOrEmpty(lsSql))
                {
                    DbConnection loConnection = MaxDbProviderFactoryLibrary.GetConnection(this.DbProviderFactoryProviderName, this.DbProviderFactoryProviderType);
                    try
                    {
                        DbCommand loCommand = MaxDbProviderFactoryLibrary.GetCommand(this.DbProviderFactoryProviderName, this.DbProviderFactoryProviderType);
                        loCommand.CommandText = MaxSqlGenerationLibrary.GetCommandText(this.SqlProviderName, this.SqlProviderType, lsSql);
                        loCommand.Connection = loConnection;
                        try
                        {
                            int lnCount = MaxDbCommandLibrary.ExecuteNonQueryWithoutTransaction(this.DbCommandProviderName, this.DbCommandLibraryProviderType, loCommand);
                        }
                        finally
                        {
                            loCommand.Connection = null;
                            loCommand.Dispose();
                            loCommand = null;
                        }
                    }
                    finally
                    {
                        if (null != loConnection)
                        {
                            loConnection.Dispose();
                            loConnection = null;
                        }
                    }
                }

                _oIsInitializedIndex.Add(lsKey);
            }
        }

		/// <summary>
		/// Creates a table in the database (checks to make sure it does not already exist).
		/// </summary>
		/// <param name="loDataModel">Definition of table to be created.</param>
        protected virtual bool CreateTable(MaxDataModel loDataModel, DbConnection loConnection)
		{
            bool lbR = false;
            string lsTableKey = this.GetHasTableKey(loDataModel, loConnection);
            DbCommand loCommand = MaxDbProviderFactoryLibrary.GetCommand(this.DbProviderFactoryProviderName, this.DbProviderFactoryProviderType);
            string lsHasTable = MaxCacheRepository.Get(typeof(object), "_IsTableFound" + lsTableKey, typeof(string)) as string;
            if (string.IsNullOrEmpty(lsHasTable))
            {
                lock (_oLock)
                {
                    //// Assume the table will exist after this, or that we won't be able to tell so need to assume it exists
                    lbR = true;
                    lsHasTable = MaxCacheRepository.Get(typeof(object), "_IsTableFound" + lsTableKey, typeof(string)) as string;
                    if (string.IsNullOrEmpty(lsHasTable))
                    {
                        try
                        {
                            string lsSql = MaxSqlGenerationLibrary.GetTableCreate(this.SqlProviderName, this.SqlProviderType, loDataModel);
                            loCommand.CommandText = MaxSqlGenerationLibrary.GetCommandText(this.SqlProviderName, this.SqlProviderType, lsSql);
                            loCommand.Connection = loConnection;
                            MaxDbCommandLibrary.ExecuteNonQueryTransaction(this.DbCommandProviderName, this.DbCommandLibraryProviderType, loCommand);
                            MaxCacheRepository.Set(typeof(object), "_IsTableFound" + lsTableKey, "Table Created");
                        }
                        catch (Exception loE)
                        {
                            //// Assume it exists, but we cannot tell.
                            //// Errors will occur when the table is accessed.
                            MaxCacheRepository.Set(typeof(object), "_IsTableFound" + lsTableKey, "Table creation failed");
                            MaxLogLibrary.Log(new MaxLogEntryStructure(this.GetType(), "CreateTable", MaxEnumGroup.LogError, "Exception creating table for {DataStorageName} using ConnectionString {ConnectionString}", loE, loDataModel.DataStorageName, loConnection.ConnectionString));
                            //MaxExceptionLibrary.LogException("Error creating table [" + loDataModel.DataStorageName + "] on connection [" + loConnection.ConnectionString + "]", loE);
                        }
                        finally
                        {
                            loCommand.Dispose();
                            loCommand = null;
                        }
                    }
                }
            }

            return lbR;
		}

        protected string GetHasTableKey(MaxDataModel loDataModel, DbConnection loConnection)
        {
            return loConnection.ConnectionString + "[" + loDataModel.DataStorageName + "]";
        }
	}
}
