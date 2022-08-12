// <copyright file="MaxDbProviderFactoryLibrary.cs" company="Lakstins Family, LLC">
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
	/// Library used to encapsulate DbProviderFactory.
	/// </summary>
    public class MaxDbProviderFactoryLibrary : MaxByMethodFactory
	{
        /// <summary>
        /// Internal storage of single object
        /// </summary>
        private static MaxDbProviderFactoryLibrary _oInstance = null;

        /// <summary>
        /// Lock object for multi-threaded access.
        /// </summary>
        private static object _oLock = new object();

        /// <summary>
        /// Gets the single instance of this class.
        /// </summary>
        public static MaxDbProviderFactoryLibrary Instance
        {
            get
            {
                if (null == _oInstance)
                {
                    lock (_oLock)
                    {
                        if (null == _oInstance)
                        {
                            _oInstance = new MaxDbProviderFactoryLibrary();
                        }
                    }
                }

                return _oInstance;
            }
        }


		/// <summary>
		/// Gets a DbConnection for the DbProviderFactory specified by the type.
		/// </summary>
        /// <param name="lsName">The Name used to determine the provider.</param>
        /// <param name="loType">The Type used to determine the provider.</param>
		/// <returns>DbConnection for the DbProviderFactory.</returns>
		public static DbConnection GetConnection(string lsName, Type loType)
		{
            IMaxProvider loProvider = Instance.GetProviderByName(lsName, loType);
			if (loProvider is IMaxDbProviderFactoryLibraryProvider)
			{
				return ((IMaxDbProviderFactoryLibraryProvider)loProvider).GetConnection();
			}

            MaxByMethodFactory.HandleInterfaceNotImplemented(loType, loProvider, "MaxDbProviderFactoryLibrary", "IMaxDbProviderFactoryLibraryProvider");
            return null;
        }

		/// <summary>
		/// Gets a DbCommand for the DbProviderFactory specified by the type.
		/// </summary>
        /// <param name="lsName">The Name used to determine the provider.</param>
        /// <param name="loType">The Type used to determine the provider.</param>
		/// <returns>DbCommand for the DbProviderFactory.</returns>
        public static DbCommand GetCommand(string lsName, Type loType)
		{
            IMaxProvider loProvider = Instance.GetProviderByName(lsName, loType);
			if (loProvider is IMaxDbProviderFactoryLibraryProvider)
			{
				return ((IMaxDbProviderFactoryLibraryProvider)loProvider).GetCommand();
			}

            MaxByMethodFactory.HandleInterfaceNotImplemented(loType, loProvider, "MaxDbProviderFactoryLibrary", "IMaxDbProviderFactoryLibraryProvider");
            return null;
        }

		/// <summary>
		/// Gets a DbParameter for the DbProviderFactory specified by the type.
		/// </summary>
        /// <param name="lsName">The Name used to determine the provider.</param>
        /// <param name="loType">The Type used to determine the provider.</param>
		/// <returns>DbParameter for the DbProviderFactory.</returns>
        public static DbParameter GetParameter(string lsName, Type loType)
		{
            IMaxProvider loProvider = Instance.GetProviderByName(lsName, loType);
			if (loProvider is IMaxDbProviderFactoryLibraryProvider)
			{
				return ((IMaxDbProviderFactoryLibraryProvider)loProvider).GetParameter();
			}

            MaxByMethodFactory.HandleInterfaceNotImplemented(loType, loProvider, "MaxDbProviderFactoryLibrary", "IMaxDbProviderFactoryLibraryProvider");
            return null;
        }

		/// <summary>
		/// Gets a DbConnectionStringBuilder for the DbProviderFactory specified by the type.
		/// </summary>
        /// <param name="lsName">The Name used to determine the provider.</param>
        /// <param name="loType">The Type used to determine the provider.</param>
		/// <returns>DbConnectionStringBuilder for the DbProviderFactory.</returns>		
        public static DbConnectionStringBuilder GetConnectionString(string lsName, Type loType)
		{
            IMaxProvider loProvider = Instance.GetProviderByName(lsName, loType);
			if (loProvider is IMaxDbProviderFactoryLibraryProvider)
			{
				return ((IMaxDbProviderFactoryLibraryProvider)loProvider).GetConnectionString();
			}

            MaxByMethodFactory.HandleInterfaceNotImplemented(loType, loProvider, "MaxDbProviderFactoryLibrary", "IMaxDbProviderFactoryLibraryProvider");
            return null;
        }

		/// <summary>
		/// Gets the name of the DbProviderFactory specified by the type.
		/// </summary>
        /// <param name="lsName">The Name used to determine the provider.</param>
        /// <param name="loType">The Type used to determine the provider.</param>
		/// <returns>The name of the provider.</returns>
        public static string GetFactoryProviderName(string lsName, Type loType)
		{
            IMaxProvider loProvider = Instance.GetProviderByName(lsName, loType);
			if (loProvider is IMaxDbProviderFactoryLibraryProvider)
			{
				return ((IMaxDbProviderFactoryLibraryProvider)loProvider).GetFactoryProviderName();
			}

            MaxByMethodFactory.HandleInterfaceNotImplemented(loType, loProvider, "MaxDbProviderFactoryLibrary", "IMaxDbProviderFactoryLibraryProvider");
            return null;
		}
	}
}
