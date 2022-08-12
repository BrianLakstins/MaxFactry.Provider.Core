// <copyright file="MaxDataStreamFolderStorageLibrary.cs" company="Lakstins Family, LLC">
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
// <change date="5/29/2021" author="Brian A. Lakstins" description="Initial creation">
// </changelog>
#endregion

namespace MaxFactry.Base.DataLayer.Library
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Ports;
    using System.Threading;
    using MaxFactry.Core;

    public class MaxDataStreamFolderStorageLibrary
    {
        /// <summary>
        /// Writes stream data to storage.
        /// </summary>
        /// <param name="loData">The data index for the object</param>
        /// <param name="lsKey">Data element name to write</param>
        /// <returns>Number of bytes written to storage.</returns>
        public static bool StreamSave(MaxData loData, string lsKey, string lsFolder)
        {
            bool lbR = false;
            MaxIdGuidDataModel loDataModel = loData.DataModel as MaxIdGuidDataModel;
            if (null != loDataModel)
            {
                object loValueType = loData.DataModel.GetValueType(lsKey);
                if (loData.GetIsChanged(lsKey))
                {
                    //// Check defined storage type
                    if (typeof(MaxLongString).Equals(loValueType) || typeof(byte[]).Equals(loValueType) || typeof(Stream).Equals(loValueType))
                    {
                        object loValue = loData.Get(lsKey);
                        if (null != loValue && (loValue is Stream || loValue is string || loValue is byte[]))
                        {
                            Stream loStream = null;
                            if (loValue is string && ((string)loValue).Length > 2000)
                            {
                                //// Store as stream if over 2K in length
                                loData.Set(lsKey, MaxDataModel.StreamStringIndicator);
                                loStream = new MemoryStream(System.Text.UTF8Encoding.UTF8.GetBytes(((string)loValue)));
                            }
                            else if (loValue is byte[] && ((byte[])loValue).Length > 2000)
                            {
                                //// Store as stream if over 2K in length
                                loData.Set(lsKey, MaxDataModel.StreamByteIndicator);
                                loStream = new MemoryStream((byte[])loValue);
                            }
                            else if (loValue is Stream)
                            {
                                loStream = (Stream)loValue;
                            }

                            if (null != loStream)
                            {
                                string[] laStreamPath = loData.GetStreamPath();
                                string lsStreamPath = string.Empty;
                                for (int lnP = 0; lnP < laStreamPath.Length; lnP++)
                                {
                                    lsStreamPath = Path.Combine(lsStreamPath, laStreamPath[lnP]);
                                }

                                string lsStorageLocation = Path.Combine(lsFolder, lsStreamPath);
                                if (!Directory.Exists(lsStorageLocation))
                                {
                                    Directory.CreateDirectory(lsStorageLocation);
                                }

                                string lsFullPath = Path.Combine(lsStorageLocation, lsKey);
                                bool lbIsChanged = true;
                                if (File.Exists(lsFullPath))
                                {
                                    lbIsChanged = false;
                                    if (loStream.CanSeek)
                                    {
                                        loStream.Seek(0, SeekOrigin.Begin);
                                    }

                                    using (FileStream loFileStream = File.OpenRead(lsFullPath))
                                    {
                                        //// 50K chunks.
                                        int lnBufferSize = 50 * 1024;
                                        byte[] laBufferCurrent = new byte[lnBufferSize];
                                        byte[] laBufferNew = new byte[lnBufferSize];
                                        int lnReadNew = loStream.Read(laBufferNew, 0, lnBufferSize);
                                        int lnReadCurrent = loFileStream.Read(laBufferCurrent, 0, lnBufferSize);
                                        if (lnReadNew != lnReadCurrent)
                                        {
                                            lbIsChanged = true;
                                        }

                                        while (lnReadNew > 0 && lnReadCurrent > 0 && !lbIsChanged)
                                        {
                                            for (int lnR = 0; lnR < lnReadCurrent; lnR++)
                                            {
                                                if (laBufferCurrent[lnR] != laBufferNew[lnR])
                                                {
                                                    lbIsChanged = true;
                                                }
                                            }

                                            if (!lbIsChanged)
                                            {
                                                lnReadNew = loStream.Read(laBufferNew, 0, lnBufferSize);
                                                lnReadCurrent = loFileStream.Read(laBufferCurrent, 0, lnBufferSize);
                                                if (lnReadNew != lnReadCurrent)
                                                {
                                                    lbIsChanged = true;
                                                }
                                            }
                                        }
                                    }

                                    if (lbIsChanged)
                                    {
                                        File.Move(lsFullPath, lsFullPath + "-" + DateTime.UtcNow.Ticks.ToString());
                                    }
                                }

                                if (lbIsChanged)
                                {
                                    if (loStream.CanSeek)
                                    {
                                        loStream.Seek(0, SeekOrigin.Begin);
                                    }

                                    //// 50K chunks.
                                    int lnBufferSize = 50 * 1024;
                                    byte[] laBuffer = new byte[lnBufferSize];
                                    using (FileStream loFile = File.Create(lsFullPath, lnBufferSize))
                                    {
                                        int lnRead = loStream.Read(laBuffer, 0, lnBufferSize);
                                        while (lnRead > 0)
                                        {
                                            loFile.Write(laBuffer, 0, lnRead);
                                            lnRead = loStream.Read(laBuffer, 0, lnBufferSize);
                                        }

                                        loFile.Close();
                                        return true;
                                    }
                                }
                                else
                                {
                                    loData.ClearChanged(lsKey);
                                }
                            }
                        }
                    }
                }
                else if (typeof(MaxLongString).Equals(loValueType) || typeof(byte[]).Equals(loValueType) || typeof(Stream).Equals(loValueType))
                {
                    object loValue = loData.Get(lsKey);
                    if (null != loValue && (loValue is Stream || loValue is string || loValue is byte[]))
                    {
                        if (loValue is string && ((string)loValue).Length > 2000)
                        {
                            //// Store as stream if over 2K in length
                            loData.Set(lsKey, MaxDataModel.StreamStringIndicator);
                        }
                        else if (loValue is byte[] && ((byte[])loValue).Length > 2000)
                        {
                            //// Store as stream if over 2K in length
                            loData.Set(lsKey, MaxDataModel.StreamByteIndicator);
                        }
                    }
                }
            }

            return lbR;
        }

        /// <summary>
        /// Opens stream data in storage
        /// </summary>
        /// <param name="loData">The data index for the object</param>
        /// <param name="lsKey">Data element name to write</param>
        /// <returns>Stream that was opened.</returns>
        public static Stream StreamOpen(MaxData loData, string lsKey, string lsFolder)
        {
            if (loData.DataModel is MaxIdGuidDataModel)
            {
                string[] laStreamPath = loData.GetStreamPath();
                string lsStreamPath = string.Empty;
                for (int lnP = 0; lnP < laStreamPath.Length; lnP++)
                {
                    lsStreamPath = Path.Combine(lsStreamPath, laStreamPath[lnP]);
                }

                string lsStorageLocation = Path.Combine(lsFolder, lsStreamPath);
                string lsFullPath = Path.Combine(lsStorageLocation, lsKey);
                if (File.Exists(lsFullPath))
                {
                    FileStream loStream = File.Open(lsFullPath, FileMode.Open, FileAccess.Read);
                    return loStream;
                }
            }

            return null;
        }

        /// <summary>
        /// Deletes stream data in storage
        /// </summary>
        /// <param name="loData">The data index for the object</param>
        /// <param name="lsKey">Data element name to write</param>
        /// <returns>Stream that was opened.</returns>
        public static bool StreamDelete(MaxData loData, string lsKey, string lsFolder)
        {
            if (loData.DataModel is MaxIdGuidDataModel)
            {
                string[] laStreamPath = loData.GetStreamPath();
                string lsStreamPath = string.Empty;
                for (int lnP = 0; lnP < laStreamPath.Length; lnP++)
                {
                    lsStreamPath = Path.Combine(lsStreamPath, laStreamPath[lnP]);
                }

                string lsStorageLocation = Path.Combine(lsFolder, lsStreamPath);
                string lsFullPath = Path.Combine(lsStorageLocation, lsKey);
                if (File.Exists(lsFullPath))
                {
                    File.Delete(lsFullPath);
                    return true;
                }
            }

            return false;
        }
    }
}
