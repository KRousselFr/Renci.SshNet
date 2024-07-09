using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;

using Renci.SshNet;


namespace Test_SSH
{
    class Program
    {
        /* === CONSTANTES === */

        /* ~~ Connection parameters for the SSH test service ~~ */
        private const string SSH_HOST = "192.168.56.104";   /* VirtualBox */
        private const string SSH_USER = "setup";
        private const string SSH_PASS = "setup";

        /* Chars in returned strings we don't want to print */
        private static readonly char[] UNWANTED_CHARS = { ' ', '\n' };

        /* delay in milliseconds to allow command execution */
        private const int DELAY_MS_CMD_EXEC = 100;
        
        /* Buffer size for SSH transmissions */
        private const int SSH_BUFFER_SIZE = 4096;


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

        private static string RunShellCommand(string cmd,
                                              ShellStream shs)
        {
            shs.WriteLine(cmd);
            /* wait a little to let command execute */
            Thread.Sleep(DELAY_MS_CMD_EXEC);
            /* read command response (wait for new prompt) */
            string rep = shs.Expect(new Regex("([$#])"),
                                    new TimeSpan(0, 0, 10));
            /* empty the shell's incoming buffer */
            shs.Read();
            /* return the command's response */
            return rep;
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

                /* opens a SSH shell proper */
                Console.Out.Write("Starting shell... ");
                ShellStream shStream = sshc.CreateShellStream("VT",
                                                              132, 24,
                                                              1024, 768,
                                                              SSH_BUFFER_SIZE);
                string prompt = shStream.Expect(new Regex("[$]"));
                Console.Out.WriteLine("OK.");
                Console.Out.WriteLine();
                Console.Out.Flush();

                /* show current identity (login) on SSH server */
                Console.Out.WriteLine("Current identity on server:");
                string repWho = RunShellCommand("echo $USER", shStream);
                Console.Out.WriteLine("\"{0}\"",
                                      repWho.Trim(UNWANTED_CHARS));
                Console.Out.WriteLine();
                Console.Out.Flush();

                /* show shell environment variables */
                Console.Out.WriteLine("Shell Environment:");
                string repEnv = RunShellCommand("env", shStream);
                Console.Out.WriteLine("\"{0}\"",
                                      repEnv.Trim(UNWANTED_CHARS));
                Console.Out.WriteLine();
                Console.Out.Flush();

                /* switch to super-user ('root') */
                Console.Out.Write("Switching to super-user ('root')... ");
                string repSu = RunShellCommand("su - root", shStream);
                Console.Out.WriteLine("OK.");
                Console.Out.WriteLine();
                Console.Out.Flush();

                /* show NEW current identity (login) on SSH server */
                Console.Out.WriteLine("Current identity on server:");
                repWho = RunShellCommand("echo $USER", shStream);
                Console.Out.WriteLine("\"{0}\"",
                                      repWho.Trim(UNWANTED_CHARS));
                Console.Out.WriteLine();
                Console.Out.Flush();

                /* show NEW shell environment variables */
                Console.Out.WriteLine("Shell Environment:");
                repEnv = RunShellCommand("env", shStream);
                Console.Out.WriteLine("\"{0}\"",
                                      repEnv.Trim(UNWANTED_CHARS));
                Console.Out.WriteLine();
                Console.Out.Flush();

                /* revert to first user account */
                Console.Out.Write("Reverting to '{0}' user... ", SSH_USER);
                string repExit = RunShellCommand("exit", shStream);
                Console.Out.WriteLine("OK.");
                Console.Out.WriteLine();
                Console.Out.Flush();

                /* show LAST current identity (login) on SSH server */
                Console.Out.WriteLine("Current identity on server:");
                repWho = RunShellCommand("echo $USER", shStream);
                Console.Out.WriteLine("\"{0}\"",
                                      repWho.Trim(UNWANTED_CHARS));
                Console.Out.WriteLine();
                Console.Out.Flush();

                /* show LAST shell environment variables */
                Console.Out.WriteLine("Shell Environment:");
                repEnv = RunShellCommand("env", shStream);
                Console.Out.WriteLine("\"{0}\"",
                                      repEnv.Trim(UNWANTED_CHARS));
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

