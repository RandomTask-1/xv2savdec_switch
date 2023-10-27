using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace xv2savdec_switch
{
    public class Xv2SavFile
    {
        /*
         * Dictionary of known PC file sizes 
         * Switch Sizes will be calculated and added during object initialization
        */
        
        public Dictionary<uint, Dictionary<string, object>> file_sizes = new Dictionary<uint, Dictionary<string, object>>
            {
                {
                    0xB08C0, new Dictionary<string,object>
                    { 
                        { "encrypted", true },
                        { "version", 8 },
                        { "platform", "pc" },
                        { "footer", new byte[] { 0x41, 0xFE, 0x40, 0x91, 0xFC, 0xA0, 0x17, 0x98, 0x3C, 0x48, 0x78, 0xD8, 0xE5, 0x30, 0x8A, 0x61 } }
                    }
                },
                {
                    0xB0818, new Dictionary<string,object>
                    {
                        { "encrypted", false },
                        { "version", 8 },
                        { "platform", "pc" },
                        { "footer", new byte[] { 0x41, 0xFE, 0x40, 0x91, 0xFC, 0xA0, 0x17, 0x98, 0x3C, 0x48, 0x78, 0xD8, 0xE5, 0x30, 0x8A, 0x61 } }
                    }
                },
                {
                    0xDF2A0, new Dictionary<string,object>
                    {
                        { "encrypted", true },
                        { "version", 17 },
                        { "platform", "pc" },
                        { "footer", new byte[] { 0x6F, 0xF4, 0x5A, 0x72, 0x53, 0xFD, 0x9A, 0xA5, 0x6D, 0x7D, 0xAB, 0x47, 0x90, 0x46, 0x29, 0x96 } }
                    }
                },
                {
                    0xDF1F8, new Dictionary<string,object>
                    {
                        { "encrypted", false },
                        { "version", 17 },
                        { "platform", "pc" },
                        { "footer", new byte[] { 0x6F, 0xF4, 0x5A, 0x72, 0x53, 0xFD, 0x9A, 0xA5, 0x6D, 0x7D, 0xAB, 0x47, 0x90, 0x46, 0x29, 0x96 } }
                    }
                },
                {
                    0x12A2A0, new Dictionary<string,object>
                    {
                        { "encrypted", true },
                        { "version", 20 },
                        { "platform", "pc" },
                        { "footer", new byte[] { 0x6F, 0xF4, 0x5A, 0x72, 0x53, 0xFD, 0x9A, 0xA5, 0x6D, 0x7D, 0xAB, 0x47, 0x90, 0x46, 0x29, 0x96 } } // Needs update, couldn't find a sample v20 file
                    }
                },
                {
                    0x12A1F8, new Dictionary<string,object>
                    {
                        { "encrypted", false },
                        { "version", 20 },
                        { "platform", "pc" },
                        { "footer", new byte[] { 0x6F, 0xF4, 0x5A, 0x72, 0x53, 0xFD, 0x9A, 0xA5, 0x6D, 0x7D, 0xAB, 0x47, 0x90, 0x46, 0x29, 0x96 } } // Needs update, couldn't find a sample v20 file
                    }
                }
            };

        // Encryption Keys
        readonly byte[] Section1Key = Encoding.ASCII.GetBytes("PR]-<Q9*WxHsV8rcW!JuH7k_ug:T5ApX");
        readonly byte[] Section1Counter = Encoding.ASCII.GetBytes("_Y7]mD1ziyH#Ar=0");

        // Working variables
        private string file_type = "Unknown";
        private int file_version;
        private bool encrypted_file;
        private uint file_size;
        private byte[] data;
        private string sPath;

        public bool Is_Encrypted() { return encrypted_file; }

        public string Get_FileType() { return file_type; }

        public int Get_Version() { return file_version; }

        public uint Get_FileSize() { return file_size; }

        public string Get_Path() { return sPath; }

        public Xv2SavFile(string filename)
        {
            /*
             * Iterate over the dictionary of PC file sizes and create corresponding Switch file sizes
            */
            List<uint> keys = file_sizes.Keys.ToList();

            foreach (uint key in keys) {

                file_sizes.Add(
                    (Convert_Save_Size(key, "switch", Convert.ToBoolean(file_sizes[key]["encrypted"]))), new Dictionary<string, object>
                    {
                        { "encrypted", file_sizes[key]["encrypted"] },
                        { "version", file_sizes[key]["version"] },
                        { "platform", "switch" },
                    }
                );

            }
            
            // Initialize variables from given filepath
            sPath = filename;
            data = File.ReadAllBytes(sPath);
            file_size = (uint)data.Length;

            if (file_sizes.Keys.Contains(file_size)) {
                file_type = file_sizes[file_size]["platform"].ToString();
                file_version = Convert.ToInt16(file_sizes[file_size]["version"]);
                encrypted_file = Convert.ToBoolean(file_sizes[file_size]["encrypted"]);
            }

        }


        /*
         * Calculate the size of a specific platform and encryption state from a known given size.
         * Theoretically this should work for unknown sizes as well.
        */
        public uint Convert_Save_Size(
            uint from_size, 
            string to_platform,
            bool to_encrypted)
        {


            if (!file_sizes.Keys.Contains(from_size))
            {
                Console.WriteLine("Unknown file size " + file_size.ToString("X") + ", cannot determine platform or encryption");
                return 0;
            }

            string from_platform = file_sizes[from_size]["platform"].ToString();
            bool from_encrypted = Convert.ToBoolean(file_sizes[from_size]["encrypted"]);

            if (from_platform == to_platform && from_encrypted == to_encrypted)
            {
                return from_size;
            }
            else if (from_platform == to_platform && from_platform == "switch")
            {
                if (from_encrypted) { return from_size - 0x80; }
                else { return from_size + 0x80; }
            }
            else if (from_platform == to_platform && from_platform == "pc")
            {
                if (from_encrypted) { return from_size - 0xA8; }
                else { return from_size + 0xA8; }
            }
            else
            {
                if (from_encrypted && from_platform == "switch")
                {
                    if (to_encrypted) { return from_size + 0xC0; }
                    else { return from_size + 0x18; }
                }
                else if (from_encrypted && from_platform == "pc")
                {
                    if (to_encrypted) { return from_size - 0xC0; }
                    else { return from_size - 0x140; }
                }
                else if (!from_encrypted && from_platform == "switch")
                {
                    if (to_encrypted) { return from_size + 0x140; }
                    else { return from_size + 0x98; }
                }
                else
                {
                    if (to_encrypted) { return from_size - 0x18; }
                    else { return from_size - 0x98; }
                }

            }

        } 


        // Decrypt the current file
        public void Decrypt()
        {
            if (!encrypted_file)
            {
                Console.WriteLine("File " + sPath + " does not appear to be encrypted.");
                return;
            }
            if (file_type != "switch")
            {
                Console.WriteLine("File " + sPath + " does not appear to be an encrypted Switch save file.");
                return;
            }

            Console.WriteLine("Decrypting [" + sPath + "] ...");
            
            byte temp;
            byte[] Section2Key, Section2Counter;

            //switch version has no extra md5 header, it starts directly with the encrypted #SAV section

            using (var br = new BinaryReader(File.OpenRead(sPath)))
            {
                byte[] section1 = br.ReadBytes(0x80);
                section1 = Utils.AesCtrDecrypt(Section1Key, Section1Counter, section1);

                byte[] fileSize = File.ReadAllBytes(sPath);
                if (fileSize.Length != file_size)
                {
                    Console.WriteLine("Error!  Encrypted file " + sPath + " size " + fileSize.Length + " doesn't match the initialized size.  Expected size " + file_size);
                    return;
                }

                if (section1[0] != 0x23 || section1[1] != 0x53 || section1[2] != 0x41 || section1[3] != 0x56 || section1[4] != 0x00) //#SAV + 0x00
                {
                    Console.WriteLine("Failed at signature of first section.");
                    return;
                }

                byte Checksum1 = section1[0x14];
                byte Checksum2 = section1[0x1B];
                byte Checksum3 = section1[0x19];
                byte Checksum4 = section1[0x18];
                byte Checksum5 = section1[0x17];
                byte Checksum6 = section1[0x16];
                byte Checksum7 = section1[0x15];
                byte Checksum8 = section1[0x1A];

                int section2size = BitConverter.ToInt32(section1, 0x7C);
                byte[] section2 = br.ReadBytes(section2size);

                //Checksum1
                temp = section1[0x5];
                for (int i = 0; i < 7; i++) temp += section1[0x15 + i];
                if (Checksum1 != temp)
                {
                    Console.WriteLine($"Checksum1 failed ({temp} != {Checksum1}).");
                    return;
                }

                //Checksum2
                temp = 0;
                for (int i = 0; i < section2size / 0x20; i++)
                    temp += section2[i * 0x20];
                if (Checksum2 != temp)
                {
                    Console.WriteLine($"Checksum2 failed ({temp} != {Checksum2}).");
                    return;
                }

                //Checksum3
                temp = (byte)(section1[0x6C] + section1[0x70] + section1[0x74] + section1[0x78]);
                if (Checksum3 != temp)
                {
                    Console.WriteLine($"Checksum3 failed ({temp} != {Checksum3}).");
                    return;
                }

                //Checksum4
                temp = (byte)(section1[0x3C] + section1[0x40] + section1[0x44] + section1[0x48]);
                if (Checksum4 != temp)
                {
                    Console.WriteLine($"Checksum4 failed ({temp} != {Checksum4}).");
                    return;
                }

                //Checksum5
                temp = 0;
                for (int i = 0; i < 8; i++) temp += section1[0x4C + (i * 4)];
                if (Checksum5 != temp)
                {
                    Console.WriteLine($"Checksum5 failed ({temp} != {Checksum5}).");
                    return;
                }

                //Checksum6
                temp = 0;
                for (int i = 0; i < 8; i++) temp += section1[0x1C + (i * 4)];
                if (Checksum6 != temp)
                {
                    Console.WriteLine($"Checksum6 failed ({temp} != {Checksum6}).");
                    return;
                }

                //Checksum7
                temp = 0;
                for (int i = 0; i < 14; i++) temp += section1[0x6 + i];
                if (Checksum7 != temp)
                {
                    Console.WriteLine($"Checksum7 failed ({temp} != {Checksum7}).");
                    return;
                }

                Section2Key = new byte[0x20];
                Section2Counter = new byte[0x10];

                if ((section1[0x5] & 4) > 0)
                {
                    Array.Copy(section1, 0x4C, Section2Key, 0, Section2Key.Length);
                    Array.Copy(section1, 0x6C, Section2Counter, 0, Section2Counter.Length);
                }
                else
                {
                    Array.Copy(section1, 0x1C, Section2Key, 0, Section2Key.Length);
                    Array.Copy(section1, 0x3C, Section2Counter, 0, Section2Counter.Length);
                }

                section2 = Utils.AesCtrDecrypt(Section2Key, Section2Counter, section2);

                if (section2[0] != 0x23 || section2[1] != 0x53 || section2[2] != 0x41 || section2[3] != 0x56 || section2[4] != 0x00) //#SAV + 0x00
                {
                    Console.WriteLine("Failed at signature of second section.");
                    return;
                }

                //Checksum8
                temp = 0;
                for (int i = 0; i < section2size / 0x20; i++) temp += section2[i * 0x20];
                if (Checksum8 != temp)
                {
                    Console.WriteLine($"Checksum8 failed ({temp} != {Checksum8}).");
                    return;
                }
                
                string newPath = sPath.Substring(0, sPath.Length - 4);
                Console.WriteLine("Decryption success");
                File.WriteAllBytes(newPath + ".switch.sav.dec", section2);
                Console.WriteLine("Switch Version: [" + newPath + ".switch.sav.dec]");

                /*
                 * Create a faux pc format decrypted file.  
                 * The file will not actually work on PC, it is simply the proper size
                 * with padding for the MD5 and the PC footer added.
                */
                byte[] pcfile = new byte[Convert_Save_Size(file_size, "pc", false)];

                
                // The first 8 bytes of the Switch and PC files are the same
                for (int i = 0; i < 8; i++)
                {
                    pcfile[i] = section2[i];
                }

                
                //  The save data for the PC file starts after 
                for (int i = 8; i < section2.Length; i++)
                {
                    pcfile[i + 8] = section2[i];
                }

                // Add the PC footer
                byte[] pcfooter = (byte[])file_sizes[Convert_Save_Size(file_size, "pc", encrypted_file)]["footer"];

                for (int i = 0; i < pcfooter.Length; i++)
                {
                    pcfile[i + pcfile.Length - 16] = pcfooter[i];
                }

                // Write the pc file version
                Console.WriteLine("PC Version: [" + newPath + ".pc.sav.dec]");
                File.WriteAllBytes(newPath + ".pc.sav.dec", pcfile);

            }
        }

        // Encrypts the current file
        public void Encrypt()
        {
            Console.WriteLine("Encrypting [" + sPath + "] ...");

            byte[] Section2Key, Section2Counter;
            byte[] section2 = File.ReadAllBytes(sPath);

            // Set the encrypted_size and decrypted_size values to switch
            //uint encrypted_size = Convert_Save_Size(file_size, "switch", true);
            //uint decrypted_size = Convert_Save_Size(file_size, "switch", false);

            //Check for PC file
            if ((uint)section2.Length == Convert_Save_Size(file_size, "pc", false)) {
                // If we have a PC file, convert it to a switch file
                byte[] temp = new byte[Convert_Save_Size(file_size, "switch", false)];
                for (int i = 0; i < 16; i++)
                {
                    temp[i] = section2[i];
                }
                for (int i = 16; i < temp.Length; i++)
                {
                    temp[i] = section2[i + 8];
                }
                section2 = temp;   
            }

            byte[] section1 = new byte[0x80];
            Utils.GetRandomData(section1, 0x80);
            section1[0x5] = 0x34; // How was this obtained? Why was it specifically put in?

            // For checksum 8
            section1[0x1A] = 0;
            for (int i = 0; i < section2.Length / 0x20; i++) section1[0x1A] += section2[i * 0x20];

            Section2Key = new byte[0x20];
            Section2Counter = new byte[0x10];

            Array.Copy(section1, 0x4C, Section2Key, 0, Section2Key.Length);
            Array.Copy(section1, 0x6C, Section2Counter, 0, Section2Counter.Length);

            section2 = Utils.AesCtrDecrypt(Section2Key, Section2Counter, section2);

            // For checksum 7
            section1[0x15] = 0;
            for (int i = 0; i < 14; i++) section1[0x15] += section1[0x6 + i];

            // For checksum 6
            section1[0x16] = 0;
            for (int i = 0; i < 8; i++) section1[0x16] += section1[0x1C + (i * 4)];

            // For checksum 5
            section1[0x17] = 0;
            for (int i = 0; i < 8; i++) section1[0x17] += section1[0x4C + (i * 4)];

            /// For checksum 4
            section1[0x18] = 0;
            for (int i = 0; i < 4; i++) section1[0x18] += section1[0x3C + i * 4];
            
            // For checksum 3
            section1[0x19] = 0;
            for (int i = 0; i < 4; i++) section1[0x19] += section1[0x6C + i * 4];
            
            // For checksum 2
            section1[0x1B] = 0;
            for (int i = 0; i < section2.Length / 0x20; i++) section1[0x1B] += section2[i * 0x20];

            // For checksum 1
            section1[0x14] = section1[0x5];
            for (int i = 0; i < 7; i++) section1[0x14] += section1[0x15 + i];

            // #SAV. (File magic number)
            section1[0] = 0x23;
            section1[1] = 0x53;
            section1[2] = 0x41;
            section1[3] = 0x56;
            section1[4] = 0x00;

            byte[] sizeArray = BitConverter.GetBytes(Convert_Save_Size(file_size, "switch", false));
            Array.Copy(sizeArray, 0, section1, 0x7c, sizeArray.Length);
            section1 = Utils.AesCtrDecrypt(Section1Key, Section1Counter, section1);

            byte[] completeFile = new byte[Convert_Save_Size(file_size, "switch", true)];
            Array.Copy(section1, 0, completeFile, 0, section1.Length);
            Array.Copy(section2, 0, completeFile, 0x80, section2.Length);

            string newPath;
            if (sPath.EndsWith(".pc.sav.dec")) {
                newPath = sPath.Substring(0, sPath.Length - 11);
            }
            else if (sPath.EndsWith(".switch.sav.dec")) { 
                newPath = sPath.Substring(0, sPath.Length - 15);
            }
            else if (sPath.EndsWith(".sav.dec"))
            {
                newPath = sPath.Substring(0, sPath.Length - 8);
            }
            else 
            {
                newPath = sPath.Replace(".dec", "");
            }

            Console.WriteLine("Encryption success");
            File.WriteAllBytes(newPath + ".dat", completeFile);
            Console.WriteLine("[" + newPath + ".dat]");
        }
    

        private void Test_Size_Conversions()
        {
            /*   
            public const uint encrypted_size_switch_v8 = 0xB0800;  //Subtract 192 or 0xC0 from PC size  // - 128 0x80 for decrypted size
            public const uint encrypted_size_switch_v17 = 0xDF1E0;  //Subtract 192 or 0xC0 from PC size  // - 128 0x80 for decrypted size
            public const uint encrypted_size_switch_v20 = 0x12A1E0;  //Subtract 192 or 0xC0 from PC size  // - 128 0x80 for decrypted size
            public const uint decrypted_size_switch_v8 = 0xB0780;  // Subtract 152 or 0x98 from PC size  
            public const uint decrypted_size_switch_v17 = 0xDF160;  // Subtract 152 or 0x98 from PC size
            public const uint decrypted_size_switch_v20 = 0x12A160;  // Subtract 152 or 0x98 from PC size
            public const uint encrypted_size_pc_v8 = 0xB08C0;  // Subtract 168 0xA8 for decrypted size
            public const uint encrypted_size_pc_v17 = 0xDF2A0;
            public const uint encrypted_size_pc_v20 = 0x12A2A0;
            public const uint decrypted_size_pc_v8 = 0xB0818;
            public const uint decrypted_size_pc_v17 = 0xDF1F8;
            public const uint decrypted_size_pc_v20 = 0x12A1F8;
            */


            Console.WriteLine("Encrypted Switch -> Decrypted Switch");
            Console.WriteLine(Convert_Save_Size(0xB0800, "switch", false).ToString("X"));
            Console.WriteLine("B0780");

            Console.WriteLine("Encrypted Switch -> Encrypted Switch");
            Console.WriteLine(Convert_Save_Size(0xB0800, "switch", true).ToString("X"));
            Console.WriteLine("B0800");

            Console.WriteLine("Encrypted Switch -> Decrypted pc");
            Console.WriteLine(Convert_Save_Size(0xB0800, "pc", false).ToString("X"));
            Console.WriteLine("B0818");

            Console.WriteLine("Encrypted Switch -> Encrypted pc");
            Console.WriteLine(Convert_Save_Size(0xB0800, "pc", true).ToString("X"));
            Console.WriteLine("B08C0");

            Console.WriteLine("Encrypted pc -> Decrypted Switch");
            Console.WriteLine(Convert_Save_Size(0x12A2A0, "switch", false).ToString("X"));
            Console.WriteLine("12A160");

            Console.WriteLine("Encrypted pc -> Encrypted Switch");
            Console.WriteLine(Convert_Save_Size(0x12A2A0, "switch", true).ToString("X"));
            Console.WriteLine("12A1E0");

            Console.WriteLine("Encrypted pc -> Decrypted pc");
            Console.WriteLine(Convert_Save_Size(0x12A2A0, "pc", false).ToString("X"));
            Console.WriteLine("12A1F8");

            Console.WriteLine("Encrypted pc -> Encrypted pc");
            Console.WriteLine(Convert_Save_Size(0x12A2A0, "pc", true).ToString("X"));
            Console.WriteLine("12A2A0");

            Console.WriteLine("Decrypted Switch -> Decrypted Switch");
            Console.WriteLine(Convert_Save_Size(0xB0780, "switch", false).ToString("X"));
            Console.WriteLine("B0780");

            Console.WriteLine("Decrypted Switch -> Encrypted Switch");
            Console.WriteLine(Convert_Save_Size(0xB0780, "switch", true).ToString("X"));
            Console.WriteLine("B0800");

            Console.WriteLine("Decrypted Switch -> Decrypted pc");
            Console.WriteLine(Convert_Save_Size(0xB0780, "pc", false).ToString("X"));
            Console.WriteLine("B0818");

            Console.WriteLine("Decrypted Switch -> Encrypted pc");
            Console.WriteLine(Convert_Save_Size(0xB0780, "pc", true).ToString("X"));
            Console.WriteLine("B08C0");

            Console.WriteLine("Decrypted pc -> Decrypted Switch");
            Console.WriteLine(Convert_Save_Size(0x12A1F8, "switch", false).ToString("X"));
            Console.WriteLine("12A160");

            Console.WriteLine("Decrypted pc -> Encrypted Switch");
            Console.WriteLine(Convert_Save_Size(0x12A1F8, "switch", true).ToString("X"));
            Console.WriteLine("12A1E0");

            Console.WriteLine("Decrypted pc -> Decrypted pc");
            Console.WriteLine(Convert_Save_Size(0x12A1F8, "pc", false).ToString("X"));
            Console.WriteLine("12A1F8");

            Console.WriteLine("Decrypted pc -> Encrypted pc");
            Console.WriteLine(Convert_Save_Size(0x12A1F8, "pc", true).ToString("X"));
            Console.WriteLine("12A2A0");
        }


    }

}
