using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Renci.SshNet;
using Renci.SshNet.Sftp;


namespace Essai_SFTP
{
    class Program
    {
        /* === CONSTANTES === */

        /* ~~ Connection parameters for the SFTP test service ~~ */
        private const string SFTP_HOST = "192.168.56.101";   /* VirtualBox */
        private const string SFTP_USER = "setup";
        private const string SFTP_PASS = "setup";

        /* Timeout delay for a SFTP opration */
        private const int TIMEOUT_IN_SECONDS = 10;

        /* Name of the file to down-/up-load using SFTP */
        private const string TEST_FILE_NAME = "test.txt";


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

        private static string GetLineForSftpFile(SftpFile sf) {
            StringBuilder sb = new StringBuilder();
            if (sf.IsBlockDevice) sb.Append('b');
            else if (sf.IsCharacterDevice) sb.Append('c');
            else if (sf.IsDirectory) sb.Append('d');
            else if (sf.IsNamedPipe) sb.Append('p');
            else if (sf.IsSocket) sb.Append('s');
            else if (sf.IsSymbolicLink) sb.Append('l');
            else /* if (fich.IsRegularFile)*/ sb.Append('-');
            if (sf.OwnerCanRead)     sb.Append('r'); else sb.Append('-');
            if (sf.OwnerCanWrite)    sb.Append('w'); else sb.Append('-');
            if (sf.OwnerCanExecute)  sb.Append('x'); else sb.Append('-');
            if (sf.GroupCanRead)     sb.Append('r'); else sb.Append('-');
            if (sf.GroupCanWrite)    sb.Append('w'); else sb.Append('-');
            if (sf.GroupCanExecute)  sb.Append('x'); else sb.Append('-');
            if (sf.OthersCanRead)    sb.Append('r'); else sb.Append('-');
            if (sf.OthersCanWrite)   sb.Append('w'); else sb.Append('-');
            if (sf.OthersCanExecute) sb.Append('x'); else sb.Append('-');
            sb.Append(' ');
            sb.Append(String.Format("{0,5:d}", sf.UserId));
            sb.Append(' ');
            sb.Append(String.Format("{0,5:d}", sf.GroupId));
            sb.Append(' ');
            sb.Append(String.Format("{0,9:d}", sf.Length));
            sb.Append(' ');
            sb.Append(String.Format("{0:yyyy-MM-dd HH:mm:ss}",
                                    sf.LastWriteTime));
            sb.Append(' ');
            sb.Append(sf.Name);
            return sb.ToString();
        }

        private static void PrintDefaultDirectory(SftpClient sftpc) {
            IEnumerable<SftpFile> lstFichs = sftpc.ListDirectory(".");
            foreach (SftpFile fich in lstFichs) {
                Console.Out.WriteLine(GetLineForSftpFile(fich));
            }
            Console.Out.WriteLine();
            Console.Out.Flush();
        }


        /* === PROGRAM'S ENTRY POINT === */

        public static void Main(string[] args) {
            Console.Out.WriteLine(" === C# SFTP CLIENT EXAMPLE === ");
            Console.Out.WriteLine();
            Console.Out.Flush();
            try {
                /* try to connect to SFTP server */
                Console.Out.Write("Connecting to SFTP service... ");
                Console.Out.Flush();
                SftpClient sftpc = new SftpClient(SFTP_HOST,
                                                  SFTP_USER,
                                                  SFTP_PASS);
                sftpc.OperationTimeout = new TimeSpan(0, 0, TIMEOUT_IN_SECONDS);
                sftpc.Connect();
                Console.Out.WriteLine("OK.");
                Console.Out.WriteLine();
                Console.Out.Flush();

                /* list the defaut remote directory contents */
                Console.Out.WriteLine("Default directory contents:");
                PrintDefaultDirectory(sftpc);

                /* try to download a file */
                Console.Out.Write("Downloading file \"{0}\"... ",
                                  TEST_FILE_NAME);
                using (FileStream fsw = new FileStream(TEST_FILE_NAME,
                                                       FileMode.Create,
                                                       FileAccess.Write))
                {
                    sftpc.DownloadFile(TEST_FILE_NAME, fsw);
                }
                Console.Out.WriteLine("Done.");
                Console.Out.WriteLine();
                Console.Out.Flush();

                /* try to upload a copy of the file we just got */
                string nomCopie = String.Format("{0}.bak", TEST_FILE_NAME);
                Console.Out.Write("Uploading file \"{0}\"... ",
                                  nomCopie);
                using (FileStream fsr = new FileStream(TEST_FILE_NAME,
                                                       FileMode.Open,
                                                       FileAccess.Read))
                {
                    sftpc.UploadFile(fsr, nomCopie, true);
                }
                Console.Out.WriteLine("Done.");
                Console.Out.WriteLine();
                Console.Out.Flush();
                Console.Out.WriteLine("New directory contents:");
                PrintDefaultDirectory(sftpc);

                /* delete the local file we downloaded */
                File.Delete(TEST_FILE_NAME);

                /* try to delete the remote uploaded file */
                Console.Out.Write("Deleting remote file \"{0}\"... ",
                                  nomCopie);
                sftpc.DeleteFile(nomCopie);
                Console.Out.WriteLine("Done.");
                Console.Out.WriteLine();
                Console.Out.Flush();
                Console.Out.WriteLine("Last directory contents:");
                PrintDefaultDirectory(sftpc);

                /* disconnect from server and quit */
                Console.Out.Write("Disconnecting from SFTP service... ");
                sftpc.Disconnect();
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

