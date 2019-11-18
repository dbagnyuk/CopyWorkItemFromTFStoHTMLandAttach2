using System;
using System.Threading;
using System.Windows.Forms;

namespace CopyWorkItemFromTFStoHTMLandAttach2
{
    public static class Input
    {
        /// <summary>
        /// Input the TFS id or 'config' command.
        /// </summary>

        // get the input tfs id or 'config' word for go to edit the config
        public static int processingInput()
        {
            int result; // for return
            char enteredSymbol; // char for the value of the pressed key
            int stringSize = 7; // size for char array in which we will store the entered digits
            var sString = new char[stringSize]; // char array in which we will store the entered digits
            int digitsCount = 0; // how many digits we already store

            // loop in which we will analyze pressed keys
            while (true)
            {
                // read the ASCII code from pressed button and save it in char. ReadKey(true) - is for decline write it in console
                enteredSymbol = Console.ReadKey(true).KeyChar;

                // if pressed Ctrl+V we pasted from Clipboard
                if (enteredSymbol == 22 && digitsCount == 0)
                {
                    // cheking if it's the number if else do nothing
                    if (Int32.TryParse(Clipboard.GetText(), out result))
                    {
                        Console.Write(result);
                        Thread.Sleep(500);
                        break;
                    }
                    else
                        continue;
                }

                // condition: if the entered symbol is a later between (a and z) or (A and Z)
                // and the count of entered digits less than the size of the char array
                if ((enteredSymbol >= 48 && enteredSymbol <= 57) && digitsCount < stringSize)
                {
                    Console.Write((char)enteredSymbol);
                    sString[digitsCount++] = (char)enteredSymbol;
                }

                // condition: if the entered symbol is a digit between 0 and 9
                // and the count of entered digits less than the size of the char array
                if (((enteredSymbol >= 65 && enteredSymbol <= 90) || (enteredSymbol >= 97 && enteredSymbol <= 122)) && digitsCount < stringSize - 1)
                {
                    Console.Write((char)enteredSymbol);
                    sString[digitsCount++] = (char)enteredSymbol;
                }
                // condition: if pressed Enter and we nothing entered before, we do nothing
                else if (enteredSymbol == 13 && digitsCount == 0)
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
                    string savedNumber = new string(sString);
                    if (Int32.TryParse(savedNumber, out result))
                        break;
                    if (savedNumber.ToLower().CompareTo("config") == 0)
                    {
                        result = -1;
                        break;
                    }
                    if (!Int32.TryParse(savedNumber, out result) || savedNumber.ToLower().CompareTo("config") != 0)
                    {
                        // clear the char array from digits we entered before
                        for (int i = 0; i < digitsCount; i++)
                        {
                            sString[i] = '\0';
                            Console.Write("\b \b"); // return cursor on the previous position
                        }
                        digitsCount = 0; // count of digits we entered should be 0
                        continue;
                    }
                }
            }

            Console.Clear(); // clearing the console
            return result;
        }
        // process the download attach confirmation
        public static bool downloadConfirm()
        {
            char enteredSymbol; // char for the value of the pressed key
            bool confirm = false;

            // loop in which we will analyze pressed keys
            while (true)
            {
                // read the ASCII code from pressed button and save it in char. ReadKey(true) - is for decline write it in console
                enteredSymbol = Console.ReadKey(true).KeyChar;

                // condition: if the entered symbol 'y' or 'Y'
                if ((enteredSymbol == 89 || enteredSymbol == 121))
                {
                    Console.Write((char)enteredSymbol);
                    Thread.Sleep(500);
                    confirm = true;
                    break;
                }

                // condition: if the entered symbol 'n' or 'N'
                if ((enteredSymbol == 78 || enteredSymbol == 110))
                {
                    Console.Write((char)enteredSymbol);
                    Thread.Sleep(500);
                    confirm = false;
                    break;
                }

                continue;
            }

            Console.Clear(); // clearing the console
            return confirm; // return the bool
        }
    }
}
