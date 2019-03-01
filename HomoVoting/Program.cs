using SharedKeyManager;
using SharedKeyManager.Db;
using SharedKeyManager.Hsm;
using SharedKeyManager.Keys;
using SharedKeyManager.ShareHolder;
using SharedKeyManager.VoteAnalysis;
using SharedKeyManager.VotingPlace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HomoVoting
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
