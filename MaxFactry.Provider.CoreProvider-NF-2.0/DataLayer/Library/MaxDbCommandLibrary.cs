// <copyright file="MaxDbCommandLibrary.cs" company="Lakstins Family, LLC">
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
// <change date="2/4/2014" author="Brian A. Lakstins" description="Initial Release">
// <change date="2/5/2014" author="Brian A. Lakstins" description="Reviewed documentation.">
// </changelog>
#endregion

namespace MaxFactry.Provider.CoreProvider.DataLayer
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Configuration;
	using System.Data;
	using System.Data.Common;
	using MaxFactry.Core;
    using MaxFactry.Base.DataLayer;

	/// <summary>
	/// Used to run database commands.  Providers for this class are created singleton, so need to be thread safe.
	/// </summary>
    public class MaxDbCommandLibrary : MaxByMethodFactory
	{
        /// <summary>
        /// Internal storage of single object
        /// </summary>
        private static MaxDbCommandLibrary _oInstance = null;

        /// <summary>
        /// Lock object for multi-threaded access.
        /// </summary>
        private static object _oLock = new object();

        /// <summary>
        /// Gets the single instance of this class.
        /// </summary>
        public static MaxDbCommandLibrary Instance
        {
            get
            {
                if (null == _oInstance)
                {
                    lock (_oLock)
                    {
                        if (null == _oInstance)
                        {
                            _oInstance = new MaxDbCommandLibrary();
                        }
                    }
                }

                return _oInstance;
            }
        }

		/// <summary>
		/// Runs a command and returns an object.
		/// </summary>
        /// <param name="lsName">The Name used to determine the provider.</param>
        /// <param name="loType">Type of the provider to use.</param>
		/// <param name="loCommand">DbCommand to run.</param>
		/// <returns>object from database.</returns>
        public static object ExecuteScaler(string lsName, Type loType, DbCommand loCommand)
		{
            IMaxProvider loProvider = Instance.GetProviderByName(lsName, loType);
			if (loProvider is IMaxDbCommandLibraryProvider)
			{
				return ((IMaxDbCommandLibraryProvider)loProvider).ExecuteScaler(loCommand);
			}

            MaxByMethodFactory.HandleInterfaceNotImplemented(loType, loProvider, "MaxDbCommandLibrary", "IMaxDbCommandLibraryProvider");
            return null;
        }

		/// <summary>
		/// Runs a command and returns numbers of records affected.
		/// </summary>
        /// <param name="lsName">The Name used to determine the provider.</param>
        /// <param name="loType">Type of the provider to use.</param>
		/// <param name="loCommand">DbCommand to run.</param>
		/// <returns>number of records affected.</returns>
        public static int ExecuteNonQueryTransaction(string lsName, Type loType, DbCommand loCommand)
		{
            IMaxProvider loProvider = Instance.GetProviderByName(lsName, loType);
			if (loProvider is IMaxDbCommandLibraryProvider)
			{
				return ((IMaxDbCommandLibraryProvider)loProvider).ExecuteNonQueryTransaction(loCommand);
			}

            MaxByMethodFactory.HandleInterfaceNotImplemented(loType, loProvider, "MaxDbCommandLibrary", "IMaxDbCommandLibraryProvider");
            return 0;
        }

		/// <summary>
		/// Runs a command and returns numbers of records affected without using a transaction.
		/// </summary>
        /// <param name="lsName">The Name used to determine the provider.</param>
        /// <param name="loType">Type of the provider to use.</param>
		/// <param name="loCommand">DbCommand to run.</param>
		/// <returns>number of records affected.</returns>
        public static int ExecuteNonQueryWithoutTransaction(string lsName, Type loType, DbCommand loCommand)
		{
            IMaxProvider loProvider = Instance.GetProviderByName(lsName, loType);
			if (loProvider is IMaxDbCommandLibraryProvider)
			{
				return ((IMaxDbCommandLibraryProvider)loProvider).ExecuteNonQuery(loCommand);
			}

            MaxByMethodFactory.HandleInterfaceNotImplemented(loType, loProvider, "MaxDbCommandLibrary", "IMaxDbCommandLibraryProvider");
            return 0;
        }

		/// <summary>
		/// Runs a command and fills the database with the data
		/// </summary>
        /// <param name="lsName">The Name used to determine the provider.</param>
        /// <param name="loType">Type of the provider to use.</param>
		/// <param name="loCommand">DbCommand to run.</param>
		/// <param name="loDataList">List of Data to fill.</param>
		/// <param name="lnPageIndex">Page to return.  Starts at zero for the first page.</param>
		/// <param name="lnPageSize">Items per page.</param>
		/// <returns>Total number of records found.</returns>
		public static int Fill(string lsName, Type loType, DbCommand loCommand, MaxDataList loDataList, int lnPageIndex, int lnPageSize)
		{
            IMaxProvider loProvider = Instance.GetProviderByName(lsName, loType);
			if (loProvider is IMaxDbCommandLibraryProvider)
			{
				return ((IMaxDbCommandLibraryProvider)loProvider).Fill(loCommand, loDataList, lnPageIndex, lnPageSize);
			}

            MaxByMethodFactory.HandleInterfaceNotImplemented(loType, loProvider, "MaxDbCommandLibrary", "IMaxDbCommandLibraryProvider");
            return 0;
		}
	}
}
