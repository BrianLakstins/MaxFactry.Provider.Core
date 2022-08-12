// <copyright file="IMaxDbCommandLibraryProvider.cs" company="Lakstins Family, LLC">
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
	/// Interface to encapsulate communication with a database through the DbCommand object.
	/// </summary>
	public interface IMaxDbCommandLibraryProvider : IMaxProvider
	{
		/// <summary>
		/// Runs a command and returns an object.
		/// </summary>
		/// <param name="loCommand">DbCommand to run.</param>
		/// <returns>object from a database.</returns>
		object ExecuteScaler(DbCommand loCommand);

		/// <summary>
		/// Runs a command and returns numbers of records affected.
		/// Wraps the command in a transaction.  Rolls back the transaction if it fails.
		/// </summary>
		/// <param name="loCommand">DbCommand to run.</param>
		/// <returns>number of records affected by the command.</returns>
		int ExecuteNonQueryTransaction(DbCommand loCommand);

		/// <summary>
		/// Runs a command and returns numbers of records affected without using a transaction.
		/// </summary>
		/// <param name="loCommand">DbCommand to run.</param>
		/// <returns>number of records affected.</returns>
		int ExecuteNonQuery(DbCommand loCommand);

		/// <summary>
		/// Runs a command and fills the database with the data
		/// </summary>
		/// <param name="loCommand">DbCommand to run.</param>
		/// <param name="loDataList">List of Data to fill.</param>
		/// <param name="lnPageIndex">Page to return.  Starts at zero for the first page.</param>
		/// <param name="lnPageSize">Items per page.</param>
		/// <returns>Total number of records found.</returns>
		int Fill(DbCommand loCommand, MaxDataList loDataList, int lnPageIndex, int lnPageSize);
	}
}
