// <copyright file="MaxDataContextSerialPortProvider.cs" company="Lakstins Family, LLC">
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
// <change date="10/19/2016" author="Brian A. Lakstins" description="Initial creation">
// <change date="5/11/2017" author="Brian A. Lakstins" description="Update to be more asynchcronous.">
// <change date="4/3/2018" author="Brian A. Lakstins" description="Fix stack overflow in gettting continuous data.">
// <change date="10/18/2019" author="Brian A. Lakstins" description="Add opening and clearing of port when connecting so sending a request is not needed.">
// <change date="10/18/2019" author="Brian A. Lakstins" description="Interact with port to send a request only if there is a request.">
// <change date="10/18/2019" author="Brian A. Lakstins" description="Add raw output data to queue instead of converting to ascii to add to queue.">
// <change date="1/27/2021" author="Brian A. Lakstins" description="Add error handling and logging.">
// <change date="7/7/2021" author="Brian A. Lakstins" description="Review and update.  Add static logging.">
// <change date="7/7/2021" author="Brian A. Lakstins" description="Add more logging.">
// <change date="7/26/2021" author="Brian A. Lakstins" description="Provide connection errors">
// </changelog>
#endregion

namespace MaxFactry.Base.DataLayer.Library
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Ports;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using MaxFactry.Core;

    public class MaxDataSerialPortLibrary
    {
        /// <summary>
        /// Index of names and ports currently managed by this library
        /// </summary>
        private static MaxIndex _oSerialPortIndex = new MaxIndex();

        private static MaxIndex _oSerialPortLockIndex = new MaxIndex();

        private static MaxIndex _oSerialPortDataQueueIndex = new MaxIndex();

        private static MaxIndex _oSerialPortErrorQueueIndex = new MaxIndex();

        private static MaxIndex _oBufferIndex = new MaxIndex();

        private static object _oLock = new object();

        public static string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// Gets a serial port this library is managing
        /// </summary>
        /// <param name="lsName"></param>
        /// <returns></returns>
        protected static SerialPort GetPort(string lsName)
        {
            return _oSerialPortIndex[lsName] as SerialPort;
        }

        /// <summary>
        /// Gets a serial port this library is managing
        /// </summary>
        /// <param name="lsName"></param>
        /// <returns></returns>
        protected static MaxIndex GetPortConfig(string lsName)
        {
            MaxIndex loR = new MaxIndex();
            if (_oSerialPortIndex.Contains(lsName + "-Config") && _oSerialPortIndex[lsName + "-Config"] is MaxIndex)
            {
                loR = (MaxIndex)_oSerialPortIndex[lsName + "-Config"];
            }

            return loR;
        }

        /// <summary>
        /// Adds a lock for a serial port for sending data or received data from the port
        /// Waits up to 2 seconds for any existing locks to free
        /// </summary>
        /// <param name="lsName"></param>
        /// <returns></returns>
        protected static bool AddPortLock(string lsName)
        {
            bool lbR = false;            
            lock (_oLock)
            {
                bool lbAddLock = true;
                if (_oSerialPortIndex.Contains(lsName) && null != _oSerialPortIndex[lsName])
                {
                    lbAddLock = false;
                    double lnWaitTime = 10;
                    DateTime ldWaitUntil = DateTime.UtcNow.AddSeconds(lnWaitTime);
                    //// Wait up some seconds for the lock to free
                    while (null != _oSerialPortIndex[lsName] && ldWaitUntil < DateTime.UtcNow)
                    {
                        System.Threading.Thread.Sleep(500);
                    }

                    if (null == _oSerialPortIndex[lsName])
                    {
                        lbAddLock = true;
                    }
                    else
                    {
                        MaxLogLibrary.Log(new MaxLogEntryStructure(typeof(MaxDataSerialPortLibrary), "AddPortLock[" + lsName + "]", MaxEnumGroup.LogError, "Waited for lock to clear for {WaitTime} seconds, but it was set at {LockTime} and has not been removed yet.", lnWaitTime, _oSerialPortIndex[lsName]));
                    }
                }

                //// Add a lock only if there is not one already
                if (lbAddLock)
                {
                    //// Lock the port and allow any process to use it that gets the lock
                    _oSerialPortLockIndex.Add(lsName, DateTime.UtcNow);
                    lbR = true;
                    MaxLogLibrary.Log(new MaxLogEntryStructure(typeof(MaxDataSerialPortLibrary), "AddPortLock[" + lsName + "]", MaxEnumGroup.LogInfo, "Added lock to port {Name}", lsName));
                }
            }

            return lbR;
        }

        /// <summary>
        /// Removes a lock on a port so it can be used by another thread
        /// </summary>
        /// <param name="lsName"></param>
        protected static void RemovePortLock(string lsName)
        {
            MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.PortLock." + lsName, MaxEnumGroup.LogStatic, "Removing lock to port {Name} during RemovePortLock", lsName));
            _oSerialPortLockIndex.Remove(lsName);
        }

        /// <summary>
        /// Checks to see if a port exists and is in an open state
        /// </summary>
        /// <param name="lsPortName"></param>
        /// <returns></returns>
        public static bool IsConnected(string lsPortName)
        {
            if (lsPortName.Contains("Test"))
            {
                return true;
            }

            SerialPort loPort = GetPort(lsPortName);
            if (null != loPort)
            {
                if (loPort.IsOpen)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a SerialPort object for the port, configures it, opens it, and starts reading from it
        /// </summary>
        /// <param name="lsPortName"></param>
        /// <param name="loPortConfigIndex"></param>
        public static void Connect(string lsPortName, MaxIndex loPortConfigIndex)
        {
            if (!IsConnected(lsPortName))
            {
                MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.Connect." + lsPortName, MaxEnumGroup.LogStatic, "Connecting to serial port {Name} during Connect", lsPortName));
                MaxException loException = null;
                SerialPort loPort = GetPort(lsPortName);
                if (null == loPort)
                {
                    lock (_oLock)
                    {
                        loPort = GetPort(lsPortName);
                        if (null == loPort)
                        {
                            _oSerialPortIndex.Add(lsPortName, null);
                            _oSerialPortIndex.Add(lsPortName + "-Config", loPortConfigIndex);
                            if (!string.IsNullOrEmpty(lsPortName) && lsPortName.ToLower().StartsWith("com"))
                            {
                                SerialPort loPortNew = new SerialPort(lsPortName);
                                _oSerialPortDataQueueIndex.Add(loPortNew.PortName, new Queue<byte[]>());
                                _oSerialPortErrorQueueIndex.Add(loPortNew.PortName, new Queue<IOException>());

                                int lnBaudRate = MaxConvertLibrary.ConvertToInt(typeof(object), loPortConfigIndex["BaudRate"]);
                                if (lnBaudRate <= 0)
                                {
                                    lnBaudRate = 9600;
                                }

                                loPortNew.BaudRate = lnBaudRate;

                                if (loPortConfigIndex["Parity"] is Parity)
                                {
                                    loPortNew.Parity = (Parity)loPortConfigIndex["Parity"];
                                }
                                else
                                {
                                    loPortNew.Parity = Parity.None;
                                }

                                int lnDataBits = MaxConvertLibrary.ConvertToInt(typeof(object), loPortConfigIndex["DataBits"]);
                                if (lnDataBits <= 0)
                                {
                                    lnDataBits = 8;
                                }

                                loPortNew.DataBits = lnDataBits;

                                if (loPortConfigIndex["StopBits"] is StopBits)
                                {
                                    loPortNew.StopBits = (StopBits)loPortConfigIndex["StopBits"];
                                }
                                else
                                {
                                    loPortNew.StopBits = StopBits.One;
                                }

                                if (loPortConfigIndex["Handshake"] is Handshake)
                                {
                                    loPortNew.Handshake = (Handshake)loPortConfigIndex["Handshake"];
                                }
                                else
                                {
                                    loPortNew.Handshake = Handshake.None;
                                }

                                int lnReadTimeout = MaxConvertLibrary.ConvertToInt(typeof(object), loPortConfigIndex["ReadTimeout"]);
                                if (lnReadTimeout <= 0)
                                {
                                    lnReadTimeout = 5000;
                                }

                                loPortNew.ReadTimeout = lnReadTimeout;
                                string lsOpenError = OpenPort(loPortNew);
                                if (string.IsNullOrEmpty(lsOpenError))
                                {
                                    //// Only add the configured port if it can be opened without an error
                                    _oSerialPortIndex.Add(lsPortName, loPortNew);
                                    //// Start reading anything from the port that is being sent
                                    StartReadAsync(loPortNew);
                                }
                                else
                                {
                                    loException = new MaxException("Exception opening serial port [" + lsPortName + "] during Connect [" + lsOpenError + "]");
                                }
                            }
                            else
                            {
                                loException = new MaxException("Serial port [" + lsPortName + "] is not a valid port name during Connect");
                            }
                        }
                        else
                        {
                            loException = new MaxException("Serial port [" + lsPortName + "] is null after lock during Connect");
                        }
                    }
                }
                else
                {
                    
                    loException = new MaxException("Serial port [" + lsPortName + "] is null during Connect");
                }

                if (null != loException)
                {
                    MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.Connect." + lsPortName, MaxEnumGroup.LogStatic, "Exception connecting to serial port {Name} during Connect", loException, lsPortName));
                    throw loException;
                }
            }
        }

        /// <summary>
        /// Opens and initializes a port that is closed
        /// </summary>
        /// <param name="loPort"></param>
        /// <returns></returns>
        public static string OpenPort(SerialPort loPort)
        {
            string lsR = string.Empty;
            if (!loPort.IsOpen)
            {
                try
                {
                    MaxIndex loPortConfiguration = GetPortConfig(loPort.PortName);
                    MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.Connect." + loPort.PortName, MaxEnumGroup.LogStatic, "Opening serial port {Name} with configuration {Config}", loPort.PortName, loPortConfiguration));
                    loPort.Open();
                    loPort.DiscardInBuffer();
                    loPort.DiscardOutBuffer();
                    byte[] loClear = loPortConfiguration["Clear"] as byte[];
                    if (null != loClear)
                    {
                        //' Send Acknowledge to clear port
                        loPort.Write(loClear, 0, loClear.Length);
                    }
                }
                catch (Exception loE)
                {
                    MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.Connect." + loPort.PortName, MaxEnumGroup.LogStatic, "Error opening serial port {Name}", loE, loPort.PortName));
                    lsR = loE.Message;
                }
            }

            return lsR;
        }

        /// <summary>
        /// Sends a request to a port
        /// </summary>
        /// <param name="lsPortName"></param>
        /// <param name="lsRequest"></param>
        public static void SendRequest(string lsPortName, string lsRequest)
        {
            MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.RequestResponse." + lsPortName, MaxEnumGroup.LogStatic, "Start sending request {lsRequest} to serial port {Name} during SendRequest", lsRequest, lsPortName));
            MaxException loException = null;
            if (!string.IsNullOrEmpty(lsPortName) && !string.IsNullOrEmpty(lsRequest))
            {
                if (lsPortName.ToLower().StartsWith("com"))
                {
                    if (IsConnected(lsPortName))
                    {
                        if (AddPortLock(lsPortName))
                        {
                            SerialPort loPort = GetPort(lsPortName);
                            if (null != loPort)
                            {
                                byte[] laRequest = loPort.Encoding.GetBytes(lsRequest);
                                loPort.BaseStream.BeginWrite(laRequest, 0, laRequest.Length, HandleEndWriteEvent, loPort);
                                MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.RequestResponse." + lsPortName, MaxEnumGroup.LogStatic, "End sending request {lsRequest} to serial port {Name} during SendRequest", lsRequest, lsPortName));
                            }
                            else
                            {
                                loException = new MaxException("Serial port [" + lsPortName + "] is null during SendRequest");
                            }
                        }
                        else
                        {
                            string lsSerialPortLockIndex = MaxConvertLibrary.SerializeObjectToString(typeof(object), _oSerialPortLockIndex);
                            loException = new MaxException("Timed out getting lock on port [" + lsPortName + "] during SendRequest\r\n" + lsSerialPortLockIndex);
                        }
                    }
                    else
                    {
                        loException = new MaxException("Port has not been connected [" + lsPortName + "] during SendRequest");
                    }
                }
                else if (!lsPortName.Contains("Test"))
                {
                    loException = new MaxException("Connection name does not start with COM [" + lsPortName + "] during SendRequest");
                }
            }
            else
            {
                loException = new MaxException("Port Name or Request is empty or null during SendRequest");
            }

            if (null != loException)
            {
                MaxLogLibrary.Log(new MaxLogEntryStructure(typeof(MaxDataSerialPortLibrary), "SendRequest", MaxEnumGroup.LogError, "Exception sending request {Request} to serial port {PortName} during SendRequest", loException, lsRequest, lsPortName));
                throw loException;
            }
        }

        /// <summary>
        /// Gets data from a port that has been added to a queue
        /// </summary>
        /// <param name="lsPortName"></param>
        /// <returns></returns>
        public static byte[] GetResponse(string lsPortName)
        {
            byte[] laR = null;
            try
            {
                if (_oSerialPortDataQueueIndex.Contains(lsPortName))
                {
                    Queue<byte[]> loQueue = _oSerialPortDataQueueIndex[lsPortName] as Queue<byte[]>;
                    if (null != loQueue)
                    {
                        byte[] laOut = null;
                        int lnLimit = 0;
                        if (loQueue.Count > 0)
                        {
                            MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.RequestResponse." + lsPortName, MaxEnumGroup.LogStatic, "Checking {Count} items in queue for response from serial port {Name} during GetResponse", loQueue.Count, lsPortName));
                            while (loQueue.Count > 0 && lnLimit < 100)
                            {
                                laOut = loQueue.Dequeue();
                                if (null != laOut)
                                {
                                    if (null == laR)
                                    {
                                        laR = laOut;
                                    }
                                    else
                                    {
                                        byte[] laBuffer = new byte[laR.Length + laOut.Length];
                                        laR.CopyTo(laBuffer, 0);
                                        laOut.CopyTo(laBuffer, laR.Length);
                                        laR = laBuffer;
                                    }
                                }

                                lnLimit++;
                            }
                        }
                    }
                }
            }
            catch (Exception loE)
            {
                MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.RequestResponse." + lsPortName, MaxEnumGroup.LogError, "Error getting response from serial port {Name}", loE, lsPortName));
            }
            
            return laR;
        }

        /// <summary>
        /// Adds received data to a queue related to the port
        /// </summary>
        /// <param name="loPort"></param>
        /// <param name="laData"></param>
        protected static void DataReceived(SerialPort loPort, byte[] laData)
        {
            string lsHex = BitConverter.ToString(laData);
            string lsData = loPort.Encoding.GetString(laData);
            if (null != laData && laData.Length > 0)
            {
                MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary", MaxEnumGroup.LogInfo, "Adding data to queue from serial port {Name}: {lsData} during DataReceived", loPort.PortName, lsData));
                MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.DataReceived." + loPort.PortName, MaxEnumGroup.LogStatic, "Data {lsData}", lsData));
                ((Queue<byte[]>)_oSerialPortDataQueueIndex[loPort.PortName]).Enqueue(laData);
            }
        }

        /// <summary>
        /// Adds errors to a queue and logs the error
        /// </summary>
        /// <param name="loPort"></param>
        /// <param name="loException"></param>
        protected static void ErrorReceived(SerialPort loPort, IOException loException)
        {
            ((Queue<IOException>)_oSerialPortErrorQueueIndex[loPort.PortName]).Enqueue(loException);
            MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.ErrorReceived", MaxEnumGroup.LogCritical, "Error Receiving on Port {Port}", loException, loPort));
            MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.ErrorReceived." + loPort.PortName, MaxEnumGroup.LogStatic, "Error Received", loException));
        }

        /// <summary>
        /// Starts reading from a port on a background thread
        /// </summary>
        /// <param name="loPort"></param>
        protected static void StartReadAsync(SerialPort loPort)
        {
            MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.RequestResponse." + loPort.PortName, MaxEnumGroup.LogStatic, "Start reading from serial port {Name} during StartReadAsync", loPort.PortName));
            //// Create a shared buffer for the port
            _oBufferIndex.Add(loPort.PortName, new byte[1000]);
            //// Start reading from the port in another thread
            Thread thread = new Thread(new ThreadStart(() => { ReadContinuous(loPort.PortName); }));
            thread.IsBackground = true;
            thread.Start();
            _oSerialPortIndex.Add(loPort.PortName + "-Thread", thread);
            //// Add a cleanup event to close all ports
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.RequestResponse." + loPort.PortName, MaxEnumGroup.LogStatic, "Reading process started on serial port {Name} during StartReadAsync", loPort.PortName));
    }

        /// <summary>
        /// Frees all ports when the process exits
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnProcessExit(object sender, EventArgs e)
        {
            foreach (string lsPortName in GetPortNames())
            {
                MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.Connect." + lsPortName, MaxEnumGroup.LogStatic, "Closing serial port {Name} during OnProcessExit", lsPortName));
                Disconnect(lsPortName);
            }
        }

        /// <summary>
        /// Closes an open port and disposes of the port
        /// </summary>
        /// <param name="lsPortName"></param>
        public static void Disconnect(string lsPortName)
        {
            lock (_oLock)
            {
                SerialPort loPort = GetPort(lsPortName);
                if (null != loPort)
                {
                    MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.Connect." + lsPortName, MaxEnumGroup.LogStatic, "Removing serial port {Name} during Disconnect", lsPortName));
                    _oSerialPortIndex.Remove(lsPortName);
                    try
                    {
                        if (loPort.IsOpen)
                        {
                            MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.Connect." + lsPortName, MaxEnumGroup.LogStatic, "Closing serial port {Name} during Disconnect", lsPortName));
                            loPort.Close();
                        }

                        loPort.Dispose();
                    }
                    catch (Exception loE)
                    {
                        MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.Connect." + lsPortName, MaxEnumGroup.LogError, "Error closing Port {Port} during Disconnect", loE, loPort));
                    }
                }
            }
        }

        /// <summary>
        /// Continuously reads the data from a port.  Adds a lock to the port when reading starts and removes it when reading ends
        /// </summary>
        /// <param name="loPort"></param>
        protected static void ReadContinuous(string lsPortName)
        {
            MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.RequestResponse." + lsPortName, MaxEnumGroup.LogStatic, "Getting serial port {Name} during ReadContinuous.  Checking for data on port every 1 second.", lsPortName));
            SerialPort loPort = GetPort(lsPortName);           
            while (null != loPort)
            {
                MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.Read." + lsPortName, MaxEnumGroup.LogStatic, "Adding lock for serial port {Name} during ReadContinuous", loPort.PortName));
                if (AddPortLock(loPort.PortName))
                {
                    MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.Read." + lsPortName, MaxEnumGroup.LogStatic, "Attempting to start read from serial port {Name} during ReadContinuous", loPort.PortName));
                    if (!StartBeginRead(loPort))
                    {
                        //// Remove the lock because there are no bytes or the port is closed
                        RemovePortLock(loPort.PortName);
                        MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.Read." + lsPortName, MaxEnumGroup.LogStatic, "Removed lock from port {Name} because no data or port closed during ReadContinuous", lsPortName));
                    }
                }
                else
                {
                    MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.Read.Error." + lsPortName, MaxEnumGroup.LogStatic, "Failed to add lock for serial port {Name} during ReadContinuous", loPort.PortName));
                }
                
                //// Pause 1 second between attempted reads
                System.Threading.Thread.Sleep(1000);
                //// Get the port again, so if it's been removed that the reading will stop
                loPort = GetPort(lsPortName);
            }
        }

        /// <summary>
        /// Started the BaseStream "BeginRead" process which was indicated to be the only stable way to access serial ports based on this: https://www.sparxeng.com/blog/software/must-use-net-system-io-ports-serialport
        /// </summary>
        /// <param name="loPort"></param>
        /// <returns></returns>
        protected static bool StartBeginRead(SerialPort loPort)
        {
            if (loPort.IsOpen)
            {
                if (loPort.BytesToRead > 0)
                {
                    MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.Read." + loPort.PortName, MaxEnumGroup.LogStatic, "Started read {Count} bytes from serial port {Name} during StartBeginRead", loPort.BytesToRead, loPort.PortName));
                    byte[] loBuffer = _oBufferIndex[loPort.PortName] as byte[];
                    loPort.BaseStream.BeginRead(loBuffer, 0, loBuffer.Length, HandleEndReadEvent, loPort);
                    return true;
                }
                else
                {
                    MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.Read." + loPort.PortName, MaxEnumGroup.LogStatic, "No bytes to read at this time on port {Name} during StartBeginRead", loPort.PortName));
                }
            }
            else
            {
                MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.Read." + loPort.PortName, MaxEnumGroup.LogError, "Cannot read from port because it's not open {Port}", loPort));
            }

            return false;
        }

        /// <summary>
        /// Handle the end of any read process. Make sure the lock is removed
        /// </summary>
        /// <param name="loResult"></param>
        protected static void HandleEndReadEvent(IAsyncResult loResult)
        {
            SerialPort loPort = loResult.AsyncState as SerialPort;
            try
            {
                MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.Read." + loPort.PortName, MaxEnumGroup.LogStatic, "Ending read from serial port {Name} during HandleEndReadEvent", loPort.PortName));
                int lnLength = loPort.BaseStream.EndRead(loResult);
                byte[] laReceived = new byte[lnLength];
                byte[] laBufferReceived = _oBufferIndex[loPort.PortName] as byte[];
                Buffer.BlockCopy(laBufferReceived, 0, laReceived, 0, lnLength);
                DataReceived(loPort, laReceived);
            }
            catch (IOException loException)
            {
                ErrorReceived(loPort, loException);
            }
            finally
            {
                RemovePortLock(loPort.PortName);
                MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.Read." + loPort.PortName, MaxEnumGroup.LogStatic, "Removed lock from port {Name} because read ended during HandleEndReadEvent", loPort.PortName));
            }
        }

        protected static void HandleEndWriteEvent(IAsyncResult loResult)
        {
            SerialPort loPort = loResult.AsyncState as SerialPort;
            try
            {
                RemovePortLock(loPort.PortName);
                MaxLogLibrary.Log(new MaxLogEntryStructure("MaxDataSerialPortLibrary.Write." + loPort.PortName, MaxEnumGroup.LogStatic, "Removed lock from port {Name} because write ended during HandleEndWriteEvent", loPort.PortName));
            }
            catch (IOException loException)
            {
                ErrorReceived(loPort, loException);
            }
        }
    }
}
