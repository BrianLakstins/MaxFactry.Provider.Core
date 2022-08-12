// <copyright file="IMaxDbProviderFactoryLibraryProvider.cs" company="Lakstins Family, LLC">
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
	using System.Data.Common;
    using MaxFactry.Core;

	/// <summary>
	/// Defines an interface for interacting with DbProviderFactory.
	/// </summary>
	public interface IMaxDbProviderFactoryLibraryProvider : IMaxProvider
	{
		/// <summary>
		/// Gets the connection object.
		/// </summary>
		/// <returns>a connection object.</returns>
		DbConnection GetConnection();

		/// <summary>
		/// Gets the command object.
		/// </summary>
		/// <returns>a command object.</returns>
		DbCommand GetCommand();

		/// <summary>
		/// Gets a parameter object.
		/// </summary>
		/// <returns>a parameter object.</returns>
		DbParameter GetParameter();

		/// <summary>
		/// Gets the connection string.
		/// </summary>
		/// <returns>a connection string.</returns>
		DbConnectionStringBuilder GetConnectionString();

		/// <summary>
		/// Gets the provider name.
		/// </summary>
		/// <returns>The name of the provider.</returns>
		string GetFactoryProviderName();
	}
}
