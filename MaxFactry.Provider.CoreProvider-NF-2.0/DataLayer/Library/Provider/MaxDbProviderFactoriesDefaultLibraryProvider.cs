// <copyright file="MaxDbProviderFactoriesLibraryProvider.cs" company="Lakstins Family, LLC">
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
// <change date="1/22/2014" author="Brian A. Lakstins" description="Initial Release">
// <change date="2/22/2014" author="Brian A. Lakstins" description="Change default connection string.">
// <change date="1/28/2015" author="Brian A. Lakstins" description="Fix using name for getting configuration.">
// <change date="7/4/2016" author="Brian A. Lakstins" description="Updated to access provider configuration using base provider methods.">
// <change date="12/29/2016" author="Brian A. Lakstins" description="Updated to allow creating DbProviderFactory using multiple methods">
// <change date="12/29/2016" author="Brian A. Lakstins" description="Encapsulated methods to create DbProviderFactory.">
// <change date="6/5/2020" author="Brian A. Lakstins" description="Updated for change to base.">
// </changelog>
#endregion

namespace MaxFactry.Provider.CoreProvider.DataLayer.Provider
{
	using System;
	using System.Configuration;
    using System.Data;
	using System.Data.Common;
    using System.Reflection;
    using MaxFactry.Core;
	
	/// <summary>
	/// Factory used to generate Db Objects
	/// </summary>
	public class MaxDbProviderFactoriesDefaultLibraryProvider : MaxProvider, IMaxDbProviderFactoryLibraryProvider
	{
        /// <summary>
        /// Stores a list of already accessed providers for quicker access
        /// </summary>
        private MaxIndex _oDbProviderFactoryIndex = new MaxIndex();

		/// <summary>
		/// Internal provider name
		/// </summary>
        private string _sProviderName = string.Empty;

		/// <summary>
		/// Internal connection string
		/// </summary>
        private string _sConnectionString = string.Empty;

		/// <summary>
		/// Internal name of the database directory
		/// </summary>
        private string _sDataDirectory = string.Empty;

        /// <summary>
        /// Internal name of the assembly
        /// </summary>
        private string _sClass = string.Empty;

        /// <summary>
        /// Internal route to the assembly path
        /// </summary>
        private string _sAssemblyPath = string.Empty;

		/// <summary>
		/// Initializes a new instance of the MaxDbProviderFactoriesLibraryProvider class
		/// </summary>
		public MaxDbProviderFactoriesDefaultLibraryProvider() : base()
		{
		}

		/// <summary>
		/// Gets the provider name for the database
		/// </summary>
		protected string ProviderName
		{
			get
			{
                if (string.IsNullOrEmpty(this._sProviderName))
                {
                    throw new MaxException("MaxDbProviderFactoriesDefaultLibraryProvider ProviderName needs to be set");
                }

				return this._sProviderName;
			}
		}

		/// <summary>
		/// Gets the connection string for the database
		/// </summary>
		protected string ConnectionString
		{
			get
			{
                if (string.IsNullOrEmpty(this._sConnectionString))
                {
                    throw new MaxException("MaxDbProviderFactoriesDefaultLibraryProvider ConnectionString needs to be set");
                }

                return this._sConnectionString;
			}
		}

		/// <summary>
		/// Gets the data directory for the database
		/// </summary>
		protected string DataDirectory
		{
			get
			{
                if (string.IsNullOrEmpty(this._sDataDirectory))
                {
                    throw new MaxException("MaxDbProviderFactoriesDefaultLibraryProvider DataDirectory needs to be set");
                }
                
                return this._sDataDirectory;
			}
		}

        /// <summary>
        /// Gets the class used to access the database
        /// </summary>
        protected string Class
        {
            get
            {
                return this._sClass;
            }
        }

        /// <summary>
        /// Gets the Full path to the assembly to use
        /// </summary>
        protected string AssemblyPath
        {
            get
            {
                return this._sAssemblyPath;
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
            string lsProviderName = this.GetConfigValue(loConfig, "ProviderName") as string;
            if (null != lsProviderName)
            {
                this._sProviderName = lsProviderName;
            }

            string lsConnectionString = this.GetConfigValue(loConfig, "ConnectionString") as string;
            if (null != lsConnectionString)
            {
                this._sConnectionString = lsConnectionString;
            }

            string lsDataDirectory = this.GetConfigValue(loConfig, "DataDirectory") as string;
            if (null != lsDataDirectory)
            {
                this._sDataDirectory = lsDataDirectory;
            }

            string lsClass = this.GetConfigValue(loConfig, "Class") as string;
            if (null != lsClass)
            {
                this._sClass = lsClass;
            }

            string lsAssemblyPath = this.GetConfigValue(loConfig, "AssemblyPath") as string;
            if (null != lsAssemblyPath)
            {
                this._sAssemblyPath = lsAssemblyPath;
            }
        }

		/// <summary>
		/// Gets the default DbProviderFactory
		/// </summary>
		/// <returns>default DbProviderFactory</returns>
		public DbProviderFactory GetDbProvider()
		{
            DbProviderFactory loDbProviderFactory = this.GetDbProvider(this.ProviderName);
			return loDbProviderFactory;
		}

		/// <summary>
		/// Gets the DbProviderFactory specified by the name
		/// </summary>
		/// <param name="lsProviderName">name of the DbProviderFactory</param>
		/// <returns>DbProviderFactory that matches the name</returns>
		public DbProviderFactory GetDbProvider(string lsProviderName)
		{
            if (!this._oDbProviderFactoryIndex.Contains(lsProviderName))
            {
                DbProviderFactory loDbProviderFactoryByAssembly = GetDbProviderFactoryFromAssembly(this.Class, this.AssemblyPath);
                if (null != loDbProviderFactoryByAssembly)
                {
                    this._oDbProviderFactoryIndex.Add(lsProviderName, loDbProviderFactoryByAssembly);
                }

                if (!this._oDbProviderFactoryIndex.Contains(lsProviderName))
                {
                    DbProviderFactory loDbProviderFactoryByRow = GetDbProviderFactoryFromConfigRow(this.Class, lsProviderName);
                    if (null != loDbProviderFactoryByRow)
                    {
                        this._oDbProviderFactoryIndex.Add(lsProviderName, loDbProviderFactoryByRow);
                    }
                }

                if (!this._oDbProviderFactoryIndex.Contains(lsProviderName))
                {
                    DbProviderFactory loDbProviderFactoryByName = DbProviderFactories.GetFactory(lsProviderName);
                    this._oDbProviderFactoryIndex.Add(lsProviderName, loDbProviderFactoryByName);
                }
            }

            DbProviderFactory loR = this._oDbProviderFactoryIndex[lsProviderName] as DbProviderFactory;
            return loR;
		}

		/// <summary>
		/// Gets a DbConnection for the default DbProviderFactory
		/// </summary>
		/// <returns>DbConnection for the default DbProviderFactory</returns>
		public DbConnection GetConnection()
		{
            DbProviderFactory loDbProviderFactory = this.GetDbProvider(this.ProviderName);
			try
			{
				DbConnection loConnection = loDbProviderFactory.CreateConnection();
				loConnection.ConnectionString = this.ConnectionString;
				return loConnection;
			}
			catch (Exception loE)
			{
				throw new MaxException("GetConnection Error: ProviderName=[" + this.ProviderName + "]ConnectionString=[" + this.ConnectionString + "]", loE);
			}
		}

		/// <summary>
		/// Gets a DbCommand for the default DbProviderFactory
		/// </summary>
		/// <returns>DbCommand for the default DbProviderFactory</returns>
		public DbCommand GetCommand()
		{
            DbProviderFactory loDbProviderFactory = this.GetDbProvider(this.ProviderName);
			DbCommand loCommand = loDbProviderFactory.CreateCommand();
			//// Wait up to 2 minutes for a command to time out
			if (loCommand.CommandTimeout > 0)
			{
				loCommand.CommandTimeout = 120;
			}

			return loCommand;
		}

		/// <summary>
		/// Gets a DbParameter using the default provider
		/// </summary>
		/// <returns>DbParameter from the default provider</returns>
		public DbParameter GetParameter()
		{
            DbProviderFactory loDbProviderFactory = this.GetDbProvider(this.ProviderName);
			return loDbProviderFactory.CreateParameter();
		}

		/// <summary>
		/// Gets a DbConnectionStringBuilder for the default provider
		/// </summary>
		/// <returns>a DbConnectionStringBuilder</returns>
		public DbConnectionStringBuilder GetConnectionString()
		{
            DbProviderFactory loDbProviderFactory = this.GetDbProvider(this.ProviderName);
			DbConnectionStringBuilder loBuilder = loDbProviderFactory.CreateConnectionStringBuilder();
			loBuilder.ConnectionString = this.ConnectionString;
			return loBuilder;
		}

		/// <summary>
		/// Gets the name of the provider used in DbProviderFactories
		/// </summary>
		/// <returns>The name of the provider</returns>
		public string GetFactoryProviderName()
		{
			return this.ProviderName;
		}

        /// <summary>
        /// Creates a DbProviderFactory instance without needing configuration file
        /// </summary>
        /// <param name="lsClass">Class and assembly information.  Like "System.Data.SQLite.SQLiteFactory, System.Data.SQLite"</param>
        /// <param name="lsAssemblyFile">Full path to the assembly DLL. Like "c:\references\System.Data.SQLite.dll"</param>
        /// <returns>A specific DbProviderFactory instance, or null if one can't be found</returns>
        protected static DbProviderFactory GetDbProviderFactoryFromAssembly(string lsClass, string lsAssemblyFile)
        {
            if (lsAssemblyFile != string.Empty && lsClass != string.Empty)
            {
                Assembly loAssembly = System.Reflection.Assembly.LoadFrom(lsAssemblyFile);
                if (null != loAssembly)
                {
                    string[] laAssembly = lsClass.Split(new char[] { ',' });
                    Type loType = loAssembly.GetType(laAssembly[0].Trim());
                    FieldInfo loInfo = loType.GetField("Instance");
                    if (null != loInfo)
                    {
                        object loInstance = loInfo.GetValue(null);
                        if (null != loInstance)
                        {
                            if (loInstance is System.Data.Common.DbProviderFactory)
                            {
                                return loInstance as DbProviderFactory;
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a DbProviderFactory instance without needing configuration file
        /// </summary>
        /// <param name="lsClass">Class and assembly information.  Like "System.Data.SQLite.SQLiteFactory, System.Data.SQLite"</param>
        /// <param name="lsProviderName">Name of the provider.  Like "System.Data.SQLite"</param>
        /// <returns>A specific DbProviderFactory instance, or null if one can't be found</returns>
        protected static DbProviderFactory GetDbProviderFactoryFromConfigRow(string lsClass, string lsProviderName)
        {
            if (string.Empty != lsProviderName && string.Empty != lsClass)
            {
                DataRow loConfig = null;
                DataSet loDataSet = ConfigurationManager.GetSection("system.data") as DataSet;
                foreach (DataRow loRow in loDataSet.Tables[0].Rows)
                {
                    if ((loRow["InvariantName"] as string) == lsProviderName)
                    {
                        loConfig = loRow;
                    }
                }

                if (null == loConfig)
                {
                    loConfig = loDataSet.Tables[0].NewRow();
                    loConfig["InvariantName"] = lsProviderName;
                    loConfig["Description"] = "Dynamically added";
                    loConfig["Name"] = lsProviderName + "Name";
                    loConfig["AssemblyQualifiedName"] = lsClass;
                    loDataSet.Tables[0].Rows.Add(loConfig);
                }

                try
                {
                    DbProviderFactory loDbProviderFactoryByRow = DbProviderFactories.GetFactory(loConfig);
                    return loDbProviderFactoryByRow;
                }
                catch (Exception loE)
                {
                    //// Handled exception if needed, otherwise, null is returned and another method can be tried.
                }
            }

            return null;
        }
	}
}