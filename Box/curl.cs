using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Box
{
    class Curl
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exeLoc">Path and file name of curl executable; e.g., C:\curl.exe</param>
        public Curl(string exeLoc)
        {
            m_exe = exeLoc;
        }

        private string m_exe = "";
        private string m_errMsg = "";

        public string ErrMsg
        {
            get
            {
                return m_errMsg;
            }

            set
            {
                m_errMsg = value;
            }
        }



        public string Execute(string cmdLineArgs)
        {
            try
            {
                string result = "";
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = m_exe;
                psi.Arguments = cmdLineArgs;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                using (Process p = Process.Start(psi))
                {
                    using (StreamReader sR = p.StandardOutput)
                    {
                        result = sR.ReadToEnd();
                    }
                    //Also get the error, if any
                    using (StreamReader sR = p.StandardError)
                    {
                        m_errMsg = sR.ReadToEnd();
                    }
                }
                return result;

            } catch (Exception ex)
            {
                m_errMsg = ex.Message + Environment.NewLine + ex.StackTrace;
                return m_errMsg;
            }
        }

    }
}
