using System;
using System.IO;

namespace xv2savdec_switch
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length > 0 && File.Exists(args[0]))
            {
                Xv2SavFile save = new Xv2SavFile(args[0]);

                if (save.Get_FileType() != "Unknown") {

                    Console.WriteLine("Save appears to be a v"+ save.Get_Version() +" "+ save.Get_FileType() +" file.");

                    if (save.Is_Encrypted()) { save.Decrypt(); }
                    else { save.Encrypt(); }

                }
                else
                {
                    Console.WriteLine("File [" + save.Get_Path() +"] with size "+ save.Get_FileSize().ToString("X") +" does not appear to be a known file size.");
                }
                
            }
            else
            {
                Console.WriteLine("File [" + args[0] + "] does not exist.");
            }
            
            ExitApp();
            
        }

        private static void ExitApp(bool showFinished = true)
        {
            if (showFinished)
            {
                Console.WriteLine("\r\nPress any key to exit...");
                Console.ReadKey();
            }

            Environment.Exit(0);
        }

    }
}
