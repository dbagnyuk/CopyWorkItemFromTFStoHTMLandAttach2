using System;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace CopyWorkItemFromTFStoHTMLandAttach2
{
    public static class Config
    {
        /// <summary>
        /// Create/Edit/Read the Config File.
        /// </summary>

        // call the creating or editing the config file
        public static void callEditConfig(string conf)
        {
            Console.Clear();
            Console.WriteLine("Creating/Editing the config file...");
            Thread.Sleep(1500);
            Console.Clear();
            Config.editConfigFile(conf);
            Console.Clear();
        }
        // call the reading the config file
        public static bool callReadConfig(string configFile, ref string[] config)
        {
            return readConfigFile(configFile, ref config);
        }
        // read the config file to memory
        private static bool readConfigFile(string configFile, ref string[] config)
        {
            bool configReaded = false;

            // check if the config file exist and if it more than 0 bytes
            if (!File.Exists(configFile) || new FileInfo(configFile).Length == 0)
            {
                callEditConfig(configFile);
                return configReaded;
            }
            // read the config file
            try
            {
                // read config file into string array
                config = System.IO.File.ReadAllLines(configFile);
            }
            catch (Exception ex)
            {
                Program.exExit(ex);
                callEditConfig(configFile);
                return configReaded;
            }
            // chek config if it has less or more than 3 string
            if (config.Length != 3)
            {
                callEditConfig(configFile);
                return configReaded;
            }
            // check the string from string array if it's empty or not
            foreach (var s in config)
                if (s.Length == 0)
                {
                    callEditConfig(configFile);
                    return configReaded;
                }
            return true;
        }
        // function for create and editing the config file
        private static void editConfigFile(string ConfFile)
        {
            string[] config = null;
            string tempInput;

            FileStream fileStream = null;
            StreamWriter streamWriter = null;

            // check if config exists or biger than 0 byes
            if (File.Exists(ConfFile) && new FileInfo(ConfFile).Length != 0)
            {
                config = System.IO.File.ReadAllLines(ConfFile);
                fileStream = new FileStream(ConfFile, FileMode.Truncate);
            }
            else
            {
                File.Delete(ConfFile);
                fileStream = new FileStream(ConfFile, FileMode.CreateNew);
            }
            streamWriter = new StreamWriter(fileStream);

            Console.WriteLine("Enter a new login, password, and path to attachments.\nUse Enter to leave previous login and path.\n");

            Console.Write("Enter new login [domain\\login]: ");
            tempInput = pathAndLoginInput();
            if (tempInput.CompareTo("#") == 0)
            {
                Console.Write(config[0]);
                streamWriter.WriteLine(config[0]);
                Console.Write("\n");
            }
            else
            {
                streamWriter.WriteLine(tempInput.ToLower());
                Console.Write("\n");
            }

            Console.Write("Enter new password: ");
            //streamWriter.WriteLine(passwordInput());
            streamWriter.WriteLine(Cipher.Encrypt(passwordInput(), Program.Key));
            Console.Write("\n");

            //while (true)
            //{
            //    Console.Clear();
            //    Console.Write("Enter old password: ");
            //    string old_pass = Console.ReadLine();
            //    if (config[1].CompareTo(old_pass) == 0)
            //    {
            //        Console.Clear();
            //        Console.Write("Enter new password: ");
            //        config[1] = Console.ReadLine();
            //        break;
            //    }
            //}

            Console.Write("Enter new save destination: ");
            tempInput = pathAndLoginInput();
            if (tempInput.CompareTo("#") == 0)
            {
                Console.Write(config[2]);
                streamWriter.WriteLine(config[2]);
                Console.Write("\n");
            }
            else
            {
                streamWriter.WriteLine(tempInput.ToLower());
                Console.Write("\n");
            }

            streamWriter.Close();
            fileStream.Close();

            Thread.Sleep(500);
            Console.Clear();
            Console.Write("Config was updated!");
            Thread.Sleep(1000);
        }
        // process password input
        private static string passwordInput()
        {
            char enteredSymbol; // char for the value of the pressed key
            int stringSize = 64; // size for char array in which we will store the entered digits
            var sString = new char[stringSize]; // char array in which we will store the entered digits
            int digitsCount = 0; // how many digits we already store
            string password = null; // returned string

            while (true)
            {
                enteredSymbol = Console.ReadKey(true).KeyChar;

                // if pressed Ctrl+V we pasted from Clipboard
                if (enteredSymbol == 22 && digitsCount == 0)
                {
                    password = Clipboard.GetText();
                    Console.Write(password);
                    Thread.Sleep(400);
                    for (int i = 0; i < password.Length; i++)
                        Console.Write("\b \b");
                    for (int i = 0; i < password.Length; i++)
                        Console.Write((char)42);
                    Thread.Sleep(500);
                    break;
                }
                // control inputted symbol letters, digits, special
                if ((enteredSymbol >= 33 && enteredSymbol <= 126) && digitsCount < stringSize)
                {
                    Console.Write((char)enteredSymbol);
                    Thread.Sleep(400);
                    Console.Write("\b \b");
                    Console.Write((char)42);
                    sString[digitsCount++] = (char)enteredSymbol;
                }

                // condition: if pressed Enter and we nothing entered before, we do nothing
                if (enteredSymbol == 13 && digitsCount == 0)
                {
                    continue;
                }
                // condition: if pressed Backspace we delete digit from char array which is we wrote in before
                else if (enteredSymbol == 8 && digitsCount > 0)
                {
                    digitsCount--;
                    Console.Write("\b \b"); // return cursor on the previous position
                    sString[digitsCount] = '\0';
                }
                // condition: if pressed Esc we clear the char array and digits in console
                else if (enteredSymbol == 27)
                {
                    // clear the char array from digits we entered before
                    for (int i = 0; i < digitsCount; i++)
                    {
                        sString[i] = '\0';
                        Console.Write("\b \b"); // return cursor on the previous position
                    }
                    digitsCount = 0; // count of digits we entered should be 0
                }
                // condition: if the Enter button pressed we finish read the entering
                else if (enteredSymbol == 13)
                {
                    password = new string(sString, 0, digitsCount);
                    break;
                }
            }
            return password;
        }
        // process login and path input
        private static string pathAndLoginInput()
        {
            char enteredSymbol; // char for the value of the pressed key
            int stringSize = 64; // size for char array in which we will store the entered digits
            var sString = new char[stringSize]; // char array in which we will store the entered digits
            int digitsCount = 0; // how many digits we already store
            string output = null;

            while (true)
            {
                enteredSymbol = Console.ReadKey(true).KeyChar;

                // if pressed Ctrl+V we pasted from Clipboard
                if (enteredSymbol == 22 && digitsCount == 0)
                {
                    output = Clipboard.GetText();
                    Console.Write(output);
                    Thread.Sleep(500);
                    break;
                }
                // control inputted symbols. only letters, digits, colon, and slash
                if (((enteredSymbol >= 48 && enteredSymbol <= 58) || (enteredSymbol >= 65 && enteredSymbol <= 90) || (enteredSymbol >= 97 && enteredSymbol <= 122) || enteredSymbol == 92) && digitsCount < stringSize - 1)
                {
                    Console.Write((char)enteredSymbol);
                    sString[digitsCount++] = (char)enteredSymbol;
                }
                // condition: if pressed Enter and we nothing entered before, we do nothing
                if (enteredSymbol == 13 && digitsCount == 0)
                {
                    sString[digitsCount++] = (char)35;
                    output = new string(sString);
                    break;
                }
                // condition: if pressed Backspace we delete digit from char array which is we wrote in before
                else if (enteredSymbol == 8 && digitsCount > 0)
                {
                    digitsCount--;
                    Console.Write("\b \b"); // return cursor on the previous position
                    sString[digitsCount] = '\0';
                }
                // condition: if pressed Esc we clear the char array and digits in console
                else if (enteredSymbol == 27)
                {
                    // clear the char array from digits we entered before
                    for (int i = 0; i < digitsCount; i++)
                    {
                        sString[i] = '\0';
                        Console.Write("\b \b"); // return cursor on the previous position
                    }
                    digitsCount = 0; // count of digits we entered should be 0
                }
                // condition: if the Enter button pressed we finish read the entering
                else if (enteredSymbol == 13)
                {
                    output = new string(sString, 0, digitsCount);
                    break;
                }
            }
            return output;
        }

    }
}
