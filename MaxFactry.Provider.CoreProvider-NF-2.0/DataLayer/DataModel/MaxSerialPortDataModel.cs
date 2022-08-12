// <copyright file="MaxSerialPortDataModel.cs" company="Lakstins Family, LLC">
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
#endregion License

#region Change Log
// <changelog>
// <change date="10/20/2016" author="Brian A. Lakstins" description="Initial creation">
// <change date="5/1/2017" author="Brian A. Lakstins" description="Add some properties to give more information when queries are made.">
// </changelog>
#endregion Change Log

namespace MaxFactry.Provider.CoreProvider.DataLayer
{
    using System;
    using MaxFactry.Base.DataLayer;

    /// <summary>
    /// Defines base data model for information stored in an AzureTable
    /// </summary>
    public class MaxSerialPortDataModel : MaxDataModel
    {

        public readonly string RequestTime = "RequestTime";

        public readonly string Request = "Request";

        public readonly string ResponseTime = "ResponseTime";

        public readonly string Response = "Response";

        public readonly string ResponseRaw = "ResponseRaw";

        public readonly string Log = "Log";

        public readonly string Exception = "Exception";

        /// <summary>
        /// Initializes a new instance of the MaxSerialPortDataModel class
        /// </summary>
        public MaxSerialPortDataModel()
        {
            this.AddNullable(this.RequestTime, typeof(DateTime));
            this.AddNullable(this.Request, typeof(MaxLongString));
            this.AddNullable(this.ResponseTime, typeof(DateTime));
            this.AddNullable(this.Response, typeof(MaxLongString));
            this.AddNullable(this.ResponseRaw, typeof(MaxLongString));
            this.AddNullable(this.Log, typeof(MaxLongString));
            this.AddNullable(this.Exception, typeof(MaxLongString));
        }
    }
}