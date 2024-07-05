using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Renci.SshNet;


namespace Test_SSH
{
    class Program
    {

        /* === CONSTANTES === */

        /* ~~ Connection parameters for the SSH test service ~~ */
        private const string SSH_HOST = "192.168.56.101";   /* VirtualBox */
        private const string SSH_USER = "setup";
        private const string SSH_PASS = "setup";

        /* Chars in returned strings we don't want to print */
        private static readonly char[] UNWANTED_CHARS = { ' ', '\n' };


        /* === PRIVATE UTILITY METHODS === */

        private static void PrintException(Exception exc) {
            Console.Error.WriteLine("Type: {0}", exc.GetType().Name);
            Console.Error.WriteLine("Message: \"{0}\"", exc.Message);
            Console.Error.WriteLine("Source: {0}", exc.Source);
            Console.Error.WriteLine("Stack trace:\n{0}", exc.StackTrace);
            if (exc.Data != null && (exc.Data.Count > 0)) {
                Console.Error.WriteLine("Included data:");
                foreach (DictionaryEntry dent in exc.Data) {
                    Console.Error.WriteLine(" - {0} : {1}",
                                            dent.Key,
                                            dent.Value);
                }
            }
            Console.Error.WriteLine();
            Console.Error.Flush();
            if (exc.InnerException != null) {
                Console.Error.WriteLine(" ** INNER EXCEPTION:");
                PrintException(exc.InnerException);
            }
        }

        private static void CheckForCmdError(SshCommand cmd) {
            if (cmd.ExitStatus != 0) {
                Console.Out.Flush();
                Console.Error.WriteLine("** COMMAND FAILED :");
                Console.Error.WriteLine("\"{0}\" returned with code {1}.",
                                        cmd.CommandText,
                                        cmd.ExitStatus);
                Console.Error.WriteLine("Error message : \"{0}\"",
                                        cmd.Error.Trim(UNWANTED_CHARS));
                Console.Error.WriteLine();
                Console.Error.Flush();
                Environment.Exit(cmd.ExitStatus);
            }
        }


        /* === PROGRAM'S ENTRY POINT === */

        public static void Main(string[] args) {
            Console.Out.WriteLine(" === C# SSH CLIENT EXAMPLE === ");
            Console.Out.WriteLine();
            Console.Out.Flush();
            try {
                /* try to connect to SFTP server */
                Console.Out.Write("Connecting to SSH service... ");
                Console.Out.Flush();
                SshClient sshc = new SshClient(SSH_HOST,
                                               SSH_USER,
                                               SSH_PASS);
                sshc.Connect();
                Console.Out.WriteLine("OK.");
                Console.Out.WriteLine();
                Console.Out.Flush();

                /* get info on the SSH server */
                Console.Out.WriteLine("SSH server info:");
                SshCommand cmdUname = sshc.RunCommand("uname -a");
                                      /* we assume an unix-like server... */
                CheckForCmdError(cmdUname);
                Console.Out.WriteLine("\"{0}\"",
                                      cmdUname.Result.Trim(UNWANTED_CHARS));
                Console.Out.WriteLine();
                Console.Out.Flush();

                /* show remote default directory contents */
                Console.Out.WriteLine("Remote default directory contents:");
                SshCommand cmdLs = sshc.RunCommand("ls -la");
                CheckForCmdError(cmdLs);
                Console.Out.WriteLine(cmdLs.Result.Trim(UNWANTED_CHARS));
                Console.Out.WriteLine();
                Console.Out.Flush();

                /* show current identity (login) on SSH server */
                Console.Out.WriteLine("Current identity on server:");
                SshCommand cmdWho = sshc.RunCommand("who am i");
                CheckForCmdError(cmdWho);
                Console.Out.WriteLine("\"{0}\"",
                                      cmdWho.Result.Trim(UNWANTED_CHARS));
                Console.Out.WriteLine();
                Console.Out.Flush();

                /* TODO: add other commands here, if needed */

                /* disconnect from server and quit */
                Console.Out.Write("Disconnecting from SSH service... ");
                sshc.Disconnect();
                Console.Out.WriteLine("OK.");
                Console.Out.WriteLine();
                Console.Out.Flush();
                Environment.Exit(0);

            } catch (Exception exc) {
                /* rapporte une erreur à l'utilisateur, et quitte */
                Console.Error.WriteLine("\n\n*** EXCEPTION THROWN ***");
                PrintException(exc);
                Environment.Exit(-1);
            }
        }

    }
}

