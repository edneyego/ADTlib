﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GiacomoFurlan.ADTlib.Utils
{
    public class Exe
    {

        private static ProcessStartInfo RunPrepare(string executable, Device device, IEnumerable<string> parameters)
        {
            var mgr = ResourcesManager.Instance;
            executable = Path.Combine(mgr.GetExecPath(), executable);

            if (!File.Exists(executable)) throw new FileNotFoundException(executable + " was not found");

            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                ErrorDialog = false,
                FileName = executable,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = (device != null && !String.IsNullOrEmpty(device.SerialNumber)
                    ? "-s " + device.SerialNumber
                    : "") + " " + String.Join(" ", parameters.Select(x => x.Replace("\"", "\\\"")))
            };

            return startInfo;
        }

        /// <summary>
        /// Executes adb or fastboot from the executing folder (%AppData%)
        /// </summary>
        /// <param name="executable">ResourcesManager.AdbExe or ResourcesManager.FastbootExe</param>
        /// <param name="device">The device to execute the command on (required attribute: SerialNumber)</param>
        /// <param name="parameters">the list of parameters passed to the executable</param>
        /// <returns>the output, null in case executable is not a file</returns>
        private static void Run(string executable, Device device, IEnumerable<string> parameters)
        {
            try
            {
                var startInfo = RunPrepare(executable.WrapInQuotes(), device, parameters);

                // unescape escaped quotes
                startInfo.FileName = startInfo.FileName.Replace("\\\"", "\"");
                startInfo.Arguments = startInfo.Arguments.Replace("\\\"", "\"");

                var proc = new Process { StartInfo = startInfo };
                if (!proc.Start()) throw new Exception("Unable to start process " + executable + " " + startInfo.Arguments);
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }

        }

        /// <summary>
        /// Executes adb or fastboot from the executing folder (%AppData%)
        /// </summary>
        /// <param name="executable">ResourcesManager.AdbExe or ResourcesManager.FastbootExe</param>
        /// <param name="device">The device to execute the command on (required attribute: SerialNumber)</param>
        /// <param name="parameters">the list of parameters passed to the executable</param>
        /// <returns>the output, null in case executable is not a file</returns>
        private static string RunReturnString(string executable, Device device, IEnumerable<string> parameters)
        {
            try
            {
                var startInfo = RunPrepare(executable, device, parameters);
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;

                // unescape escaped quotes
                startInfo.FileName = startInfo.FileName.Replace("\\\"", "\"");
                startInfo.Arguments = startInfo.Arguments.Replace("\\\"", "\"");

                var proc = new Process { StartInfo = startInfo };
                if (!proc.Start()) throw new Exception("Unable to start process " + executable + " " + startInfo.Arguments);
                proc.WaitForExit();

                var error = proc.StandardError.ReadToEnd();
                Debug.WriteIf(String.IsNullOrEmpty(error), error);

                return proc.StandardOutput.ReadToEnd();
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
                return null;
            }

        }

        public static void Adb(Device device, string[] parameters)
        {
            Run(ResourcesManager.AdbExe, device, parameters);
        }

        public static string AdbReturnString(Device device, string[] parameters)
        {
            return RunReturnString(ResourcesManager.AdbExe, device, parameters);
        }
    }
}