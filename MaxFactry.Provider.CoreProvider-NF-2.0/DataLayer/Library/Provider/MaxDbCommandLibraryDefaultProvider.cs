// <copyright file="MaxDbCommandLibraryProvider.cs" company="Lakstins Family, LLC">
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
// <change date="2/7/2014" author="Brian A. Lakstins" description="Reviewed documentation.">
// <change date="2/22/2014" author="Brian A. Lakstins" description="Fixes for paging and DbNull handling.">
// <change date="1/28/2015" author="Brian A. Lakstins" description="Fix using name for getting configuration.">
// <change date="4/13/2016" author="Brian A. Lakstins" description="Updates for handling badly stored information.">
// <change date="5/10/2016" author="Brian A. Lakstins" description="DateTime handling update to always return a UTC date time.">
// <change date="12/26/2019" author="Brian A. Lakstins" description="Use specific get commands only if value type does not match.">
// <change date="12/28/2019" author="Brian A. Lakstins" description="Update reader get commands so only one is run per loop.  Update data conversion so only run if does not match.">
// <change date="1/7/2020" author="Brian A. Lakstins" description="Check for DbNull before reading.  Update data conversion to catch any exception.">
// <change date="2/24/2020" author="Brian A. Lakstins" description="Fix issue with ignoring values that are returned that don't match defined fields.">
// </changelog>
#endregion

namespace MaxFactry.Provider.CoreProvider.DataLayer.Provider
{
	using System;
	using System.Collections.Generic;
	using System.Configuration.Provider;
	using System.Data;
	using System.Data.Common;
    using MaxFactry.Core;
    using MaxFactry.Base.DataLayer;

	/// <summary>
	/// Provider used to execute ADO.NET DbCommand objects.  
	/// </summary>
	public class MaxDbCommandLibraryDefaultProvider : MaxProvider, IMaxDbCommandLibraryProvider
	{
        /// <summary>
        /// Initializes the provider
        /// </summary>
        /// <param name="lsName">Name of the provider</param>
        /// <param name="loConfig">Configuration information</param>
        public override void Initialize(string lsName, MaxIndex loConfig)
        {
            base.Initialize(lsName, loConfig);
        }

		/// <summary>
		/// Runs a command and returns an object.
		/// Wraps running the command in a transaction if there is one statement and rolls back the transaction if it fails.
		/// </summary>
		/// <param name="loCommand">DbCommand to run.</param>
		/// <returns>object from database.</returns>
		public object ExecuteScaler(DbCommand loCommand)
		{
			object loReturn = null;
			bool lbUseTransaction = true;
			if ((loCommand.CommandText.StartsWith("SELECT", StringComparison.InvariantCultureIgnoreCase) &&
				!loCommand.CommandText.Contains(";")) ||
				loCommand.CommandType == CommandType.StoredProcedure)
			{
				lbUseTransaction = false;
			}

			DbTransaction loTransaction = null;
			
			if (this.Connect(loCommand))
			{
				if (lbUseTransaction)
				{
					loTransaction = loCommand.Connection.BeginTransaction();
					loCommand.Transaction = loTransaction;
				}

				try
				{
					loReturn = loCommand.ExecuteScalar();
					if (lbUseTransaction)
					{
						loTransaction.Commit();
					}
				}
				catch (Exception loE)
				{
					if (lbUseTransaction)
					{
						loTransaction.Rollback();
					}

					throw new MaxException("Error in ExecuteScaler", loE);
				}
				finally
				{
					loCommand.Connection.Close();
				}
			}

			return loReturn;
		}

		/// <summary>
		/// Runs a command and returns numbers of records affected.  Wraps the execution in a transaction and rolls it back if it fails.
		/// </summary>
		/// <param name="loCommand">DbCommand to run.</param>
		/// <returns>number of records affected.</returns>
		public int ExecuteNonQueryTransaction(DbCommand loCommand)
		{
			int lnRowCount = 0;

			if (this.Connect(loCommand))
			{
				DbTransaction loTransaction = loCommand.Connection.BeginTransaction();
				loCommand.Transaction = loTransaction;
				try
				{
					lnRowCount = loCommand.ExecuteNonQuery();
					loTransaction.Commit();
				}
				catch (Exception loE)
				{
					loTransaction.Rollback();
					throw new MaxException("Error in ExecuteNonQuery", loE);
				}
				finally
				{
					loCommand.Connection.Close();
				}
			}

			return lnRowCount;
		}

		/// <summary>
		/// Runs a command and returns numbers of records affected.
		/// </summary>
		/// <param name="loCommand">DbCommand to run.</param>
		/// <returns>number of records affected.</returns>
		public int ExecuteNonQuery(DbCommand loCommand)
		{
			int lnRowCount = 0;

			if (this.Connect(loCommand))
			{
				try
				{
					lnRowCount = loCommand.ExecuteNonQuery();
				}
				catch (Exception loE)
				{
					throw new MaxException("Error in ExecuteNonQuery", loE);
				}
				finally
				{
					loCommand.Connection.Close();
				}
			}

			return lnRowCount;
		}

		/// <summary>
		/// Runs a command and fills the data list with the data from the command
		/// </summary>
		/// <param name="loCommand">DbCommand to run</param>
		/// <param name="loDataList">Data list to fill</param>
		/// <param name="lnPageIndex">Page to return.  Starts with page 0.</param>
		/// <param name="lnPageSize">Items per page.</param>
		/// <returns>Total number of records.</returns>
		public int Fill(DbCommand loCommand, MaxDataList loDataList, int lnPageIndex, int lnPageSize)
		{
			int lnRows = 0;
			int lnStart = 0;
			int lnEnd = int.MaxValue;
			if (lnPageSize > 0 && lnPageIndex > 0)
			{
				lnStart = (lnPageIndex - 1) * lnPageSize;
				lnEnd = lnStart + lnPageSize;
			}

			if (this.Connect(loCommand))
			{
				try
				{
					DbDataReader loReader = loCommand.ExecuteReader(CommandBehavior.SequentialAccess);
					try
					{
						Dictionary<int, string> loRowNameIndex = new Dictionary<int, string>();
						while (loReader.Read())
						{
							if (lnRows >= lnStart && lnRows < lnEnd)
							{
								MaxData loData = new MaxData(loDataList.DataModel);
								for (int lnR = 0; lnR < loReader.FieldCount; lnR++)
								{
									if (!loRowNameIndex.ContainsKey(lnR))
									{
										loRowNameIndex.Add(lnR, loReader.GetName(lnR));
									}

                                    object loValue = null;
                                    try
                                    {
                                        Type loTypeCurrent = loDataList.DataModel.GetValueType(loRowNameIndex[lnR]);
                                        if (null != loTypeCurrent)
                                        {
                                            if (loReader.IsDBNull(lnR))
                                            {
                                                loValue = null;
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    if (loTypeCurrent == typeof(DateTime))
                                                    {
                                                        loValue = new DateTime(loReader.GetDateTime(lnR).Ticks, DateTimeKind.Utc);
                                                    }
                                                    else if (loTypeCurrent == typeof(bool))
                                                    {
                                                        loValue = loReader.GetBoolean(lnR);
                                                    }
                                                    else if (loTypeCurrent == typeof(Guid))
                                                    {
                                                        loValue = loReader.GetGuid(lnR);
                                                    }
                                                    else
                                                    {
                                                        loValue = loReader.GetValue(lnR);
                                                    }
                                                }
                                                catch (Exception loEReader)
                                                {
                                                    MaxLogLibrary.Log(new MaxLogEntryStructure(MaxEnumGroup.LogError, "Error reading value of {DataStorageName}.  {FieldName}.", loEReader, loDataList.DataModel.DataStorageName, loRowNameIndex[lnR]));
                                                }

                                                if (null == loValue || (loTypeCurrent != loValue.GetType() && (loValue.GetType() != string.Empty.GetType() && (loTypeCurrent.Equals(typeof(MaxShortString)) || loTypeCurrent.Equals(typeof(MaxLongString))))))
                                                {
                                                    if (loTypeCurrent.Equals(typeof(double)))
                                                    {
                                                        loValue = MaxConvertLibrary.ConvertToDouble(typeof(object), loValue);
                                                    }
                                                    else if (loTypeCurrent.Equals(typeof(int)))
                                                    {
                                                        loValue = MaxConvertLibrary.ConvertToInt(typeof(object), loValue);
                                                    }
                                                    else if (loTypeCurrent.Equals(typeof(long)))
                                                    {
                                                        loValue = MaxConvertLibrary.ConvertToLong(typeof(object), loValue);
                                                    }
                                                    else if (loTypeCurrent.Equals(typeof(string)))
                                                    {
                                                        loValue = MaxConvertLibrary.ConvertToString(typeof(object), loValue);
                                                    }
                                                    else if (loTypeCurrent.Equals(typeof(DateTime)))
                                                    {
                                                        loValue = MaxConvertLibrary.ConvertToDateTimeUtc(typeof(object), loValue);
                                                    }
                                                    else if (loTypeCurrent.Equals(typeof(bool)))
                                                    {
                                                        loValue = MaxConvertLibrary.ConvertToBoolean(typeof(object), loValue);
                                                    }
                                                    else if (loTypeCurrent.Equals(typeof(Guid)))
                                                    {
                                                        loValue = MaxConvertLibrary.ConvertToGuid(typeof(object), loValue);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            loValue = loReader.GetValue(lnR);
                                        }
                                    }
                                    catch (Exception loE)
                                    {
                                        if (loE.Message == "Unable to convert MySQL date/time value to System.DateTime")
                                        {
                                            //// MySQL seems to allow storing dates of all zeros, which cannot be converted.
                                        }
                                        else
                                        {
                                            MaxLogLibrary.Log(new MaxLogEntryStructure(MaxEnumGroup.LogError, "Error setting value of {DataStorageName}.  {FieldName} to {Value}.", loE, loDataList.DataModel.DataStorageName, loRowNameIndex[lnR], loValue));
                                        }
                                    }

                                    if (loValue is DBNull)
                                    {
                                        loValue = null;
                                    }

                                    loData.Set(loRowNameIndex[lnR], loValue);
                                }

								loData.ClearChanged();
								loDataList.Add(loData);
							}

                            lnRows++;
						}
					}
					catch (Exception loE)
					{
						throw new MaxException("Error in Fill", loE);
					}
					finally
					{
						loReader.Close();
						loReader = null;
					}
				}
				finally
				{
					loCommand.Connection.Close();
				}
			}

			return lnRows;
		}

		/// <summary>
		/// Opens a connection to the database.
		/// </summary>
		/// <param name="loCommand">dbCommand to use for the connection.</param>
		/// <returns>true if successful.  Exception is thrown if not successful.</returns>
		protected bool Connect(DbCommand loCommand)
		{
			bool lbR = false;
			for (int lnC = 0; lnC < 3; lnC++)
			{
				lbR = this.ConnectAttempt(loCommand, false);
				if (lbR)
				{
					return true;
				}
				else
				{
					System.Threading.Thread.Sleep(500);
				}
			}

			return this.ConnectAttempt(loCommand, true);
		}

		/// <summary>
		/// Attempts to opens a connection to the database
		/// </summary>
		/// <param name="loCommand">dbCommand to use for the connection</param>
		/// <param name="lbExceptionOnFailure">throw an exception if the connection attempt fails.</param>
		/// <returns>true if successful.</returns>
		protected bool ConnectAttempt(DbCommand loCommand, bool lbExceptionOnFailure)
		{
			if (loCommand.Connection.State == ConnectionState.Open)
			{
				return true;
			}
			else if (loCommand.Connection.State == ConnectionState.Closed)
			{
				try
				{
					loCommand.Connection.Open();
					return true;
				}
				catch (Exception loE)
				{
					if (lbExceptionOnFailure)
					{
						throw new MaxException("Unable to open a connection to database. [" + loCommand.Connection.ConnectionString + "]", loE);
					}

					if (loCommand.Connection.State != ConnectionState.Closed)
					{
						try
						{
							loCommand.Connection.Close();
						}
						catch (Exception loEClose)
						{
							if (lbExceptionOnFailure)
							{
								throw new MaxException("Unable to close a connection to database. [" + loCommand.Connection.ConnectionString + "]", loEClose);
							}
						}
					}
				}
			}

			return false;
		}
	}
}
