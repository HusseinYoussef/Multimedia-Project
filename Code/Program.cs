using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCG = System.Collections.Generic;
using C5;
using System.Collections;
using System;
using System.Text;

namespace ConsoleApplication3
{
    class Program
    {
        public static void Main(string[] args)
        {

            LZW obj = new LZW();


            obj.compress("DataSet.txt");

            obj.Decode("EncodedText.bin");

            long original = new FileInfo("C:\\Users\\hussein\\Documents\\visual studio 2015\\Projects\\ConsoleApplication3\\ConsoleApplication3\\bin\\Debug\\Dataset.txt").Length;
            long encoded = new FileInfo("C:\\Users\\hussein\\Documents\\visual studio 2015\\Projects\\ConsoleApplication3\\ConsoleApplication3\\bin\\Debug\\EncodedText.bin").Length;

            Console.WriteLine("Length of the Binary file: " + encoded + " Bytes");
           
            Console.WriteLine("Compression Ratio =  " + Math.Round(original * 1.0 / encoded, 2));
        
        }
    }
}
