using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace TerrariaTweaker.Core
{
    /// <summary>
    /// Manages memory operations on external processes using Windows API
    /// Provides methods for reading and writing various data types to game memory
    /// </summary>
    public class MemoryManager : IDisposable
    {
        // Windows API constants for memory access rights
        private const int PROCESS_WM_READ = 0x0010;
        private const int PROCESS_WM_WRITE = 0x0020;
        private const int PROCESS_WM_OPERATION = 0x0008;
        private const int PROCESS_VM_READ = 0x0010;
        private const int PROCESS_VM_WRITE = 0x0020;
        private const int PROCESS_VM_OPERATION = 0x0008;
        
        // Combined access rights for full memory manipulation
        private const int PROCESS_ALL_ACCESS = PROCESS_WM_READ | PROCESS_WM_WRITE | PROCESS_WM_OPERATION | 
                                              PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION;

        // Windows API function declarations
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, 
            int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, 
            int nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        private IntPtr processHandle;
        private Process? targetProcess;
        private bool disposed = false;

        /// <summary>
        /// Gets whether the MemoryManager is currently attached to a valid process
        /// </summary>
        public bool IsAttached => processHandle != IntPtr.Zero && ProcessHelper.IsProcessValid(targetProcess);

        /// <summary>
        /// Gets the currently attached process, or null if not attached
        /// </summary>
        public Process? AttachedProcess => targetProcess;

        /// <summary>
        /// Attaches to a target process for memory operations
        /// Must be called before any read/write operations
        /// </summary>
        /// <param name="process">Process to attach to</param>
        /// <returns>True if successfully attached</returns>
        public bool AttachToProcess(Process process)
        {
            try
            {
                if (!ProcessHelper.IsProcessValid(process))
                {
                    Console.WriteLine("Cannot attach: Invalid process");
                    return false;
                }

                // Close existing handle if attached to another process
                DetachFromProcess();

                // Open process handle with necessary access rights
                processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);
                
                if (processHandle == IntPtr.Zero)
                {
                    Console.WriteLine("Failed to open process handle. Make sure to run as Administrator.");
                    return false;
                }

                targetProcess = process;
                Console.WriteLine($"Successfully attached to process: {process.ProcessName} (PID: {process.Id})");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error attaching to process: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Detaches from the current process and closes the process handle
        /// </summary>
        public void DetachFromProcess()
        {
            if (processHandle != IntPtr.Zero)
            {
                CloseHandle(processHandle);
                processHandle = IntPtr.Zero;
                targetProcess = null;
                Console.WriteLine("Detached from process");
            }
        }

        /// <summary>
        /// Reads raw bytes from the target process memory
        /// This is the foundation method that other read methods build upon
        /// </summary>
        /// <param name="address">Memory address to read from</param>
        /// <param name="size">Number of bytes to read</param>
        /// <returns>Byte array containing the read data, or null if failed</returns>
        public byte[]? ReadBytes(IntPtr address, int size)
        {
            if (!IsAttached || size <= 0)
                return null;

            try
            {
                byte[] buffer = new byte[size];
                bool success = ReadProcessMemory(processHandle, address, buffer, size, out _);
                
                return success ? buffer : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading memory at 0x{address:X}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Writes raw bytes to the target process memory
        /// This is the foundation method for all write operations
        /// </summary>
        /// <param name="address">Memory address to write to</param>
        /// <param name="data">Byte array to write</param>
        /// <returns>True if write was successful</returns>
        public bool WriteBytes(IntPtr address, byte[] data)
        {
            if (!IsAttached || data == null || data.Length == 0)
                return false;

            try
            {
                bool success = WriteProcessMemory(processHandle, address, data, data.Length, out _);
                
                if (!success)
                {
                    Console.WriteLine($"Failed to write {data.Length} bytes to 0x{address:X}");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing memory at 0x{address:X}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Reads a 32-bit integer from memory
        /// Commonly used for health, ammo, money values in games
        /// </summary>
        /// <param name="address">Memory address to read from</param>
        /// <returns>Integer value or 0 if failed</returns>
        public int ReadInt32(IntPtr address)
        {
            byte[]? data = ReadBytes(address, sizeof(int));
            return data != null ? BitConverter.ToInt32(data, 0) : 0;
        }

        /// <summary>
        /// Writes a 32-bit integer to memory
        /// </summary>
        /// <param name="address">Memory address to write to</param>
        /// <param name="value">Integer value to write</param>
        /// <returns>True if write was successful</returns>
        public bool WriteInt32(IntPtr address, int value)
        {
            byte[] data = BitConverter.GetBytes(value);
            return WriteBytes(address, data);
        }

        /// <summary>
        /// Reads a 32-bit floating point number from memory
        /// Often used for player coordinates, speeds, or percentage values
        /// </summary>
        /// <param name="address">Memory address to read from</param>
        /// <returns>Float value or 0 if failed</returns>
        public float ReadFloat(IntPtr address)
        {
            byte[]? data = ReadBytes(address, sizeof(float));
            return data != null ? BitConverter.ToSingle(data, 0) : 0f;
        }

        /// <summary>
        /// Writes a 32-bit floating point number to memory
        /// </summary>
        /// <param name="address">Memory address to write to</param>
        /// <param name="value">Float value to write</param>
        /// <returns>True if write was successful</returns>
        public bool WriteFloat(IntPtr address, float value)
        {
            byte[] data = BitConverter.GetBytes(value);
            return WriteBytes(address, data);
        }

        /// <summary>
        /// Reads a double-precision floating point number from memory
        /// </summary>
        /// <param name="address">Memory address to read from</param>
        /// <returns>Double value or 0 if failed</returns>
        public double ReadDouble(IntPtr address)
        {
            byte[]? data = ReadBytes(address, sizeof(double));
            return data != null ? BitConverter.ToDouble(data, 0) : 0.0;
        }

        /// <summary>
        /// Writes a double-precision floating point number to memory
        /// </summary>
        /// <param name="address">Memory address to write to</param>
        /// <param name="value">Double value to write</param>
        /// <returns>True if write was successful</returns>
        public bool WriteDouble(IntPtr address, double value)
        {
            byte[] data = BitConverter.GetBytes(value);
            return WriteBytes(address, data);
        }

        /// <summary>
        /// Reads a string from memory (null-terminated)
        /// Useful for reading player names or other text data
        /// </summary>
        /// <param name="address">Memory address to read from</param>
        /// <param name="maxLength">Maximum length to read (safety limit)</param>
        /// <returns>String value or empty string if failed</returns>
        public string ReadString(IntPtr address, int maxLength = 256)
        {
            byte[]? data = ReadBytes(address, maxLength);
            if (data == null) return string.Empty;

            // Find the null terminator
            int nullIndex = Array.IndexOf(data, (byte)0);
            int length = nullIndex == -1 ? data.Length : nullIndex;

            return Encoding.UTF8.GetString(data, 0, length);
        }

        /// <summary>
        /// Calculates a memory address using a base address and offset
        /// Essential for following pointer chains in game memory
        /// </summary>
        /// <param name="baseAddress">Base memory address</param>
        /// <param name="offset">Offset to add to base address</param>
        /// <returns>Calculated memory address</returns>
        public IntPtr CalculateAddress(IntPtr baseAddress, long offset)
        {
            return new IntPtr(baseAddress.ToInt64() + offset);
        }

        /// <summary>
        /// Follows a pointer chain to resolve the final address
        /// Common in games where values are stored through multiple pointer levels
        /// </summary>
        /// <param name="baseAddress">Starting address</param>
        /// <param name="offsets">Array of offsets to follow</param>
        /// <returns>Final resolved address or IntPtr.Zero if failed</returns>
        public IntPtr FollowPointerChain(IntPtr baseAddress, params long[] offsets)
        {
            IntPtr currentAddress = baseAddress;

            for (int i = 0; i < offsets.Length; i++)
            {
                if (i < offsets.Length - 1)
                {
                    // Read the pointer at the current address
                    byte[]? pointerData = ReadBytes(currentAddress, IntPtr.Size);
                    if (pointerData == null) return IntPtr.Zero;

                    currentAddress = IntPtr.Size == 8 
                        ? new IntPtr(BitConverter.ToInt64(pointerData, 0))
                        : new IntPtr(BitConverter.ToInt32(pointerData, 0));
                }

                // Add the offset
                currentAddress = CalculateAddress(currentAddress, offsets[i]);
            }

            return currentAddress;
        }

        #region IDisposable Implementation
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    DetachFromProcess();
                }
                disposed = true;
            }
        }

        ~MemoryManager()
        {
            Dispose(false);
        }

        #endregion
    }
}