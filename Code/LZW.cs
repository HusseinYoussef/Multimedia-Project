using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication3
{
    class LZW
    {
        private Dictionary<string, int> dict;
        private Dictionary<int, string> Inverse;
        private int idx;

        public LZW()
        {       
            idx = 0;
        }

        //Function to build the needed Dictionary for LZW
        private void buildDictionary()
        {
            //each string will have an integer to represent it  (string --> int)
            //so we can later use the integer instead of the string
            dict = new Dictionary<string, int>();

            //Reading the characters to be used in the Dicionary
            StreamReader input = new StreamReader("Dictionary.txt", Encoding.UTF8, false);
            string all_letters = input.ReadToEnd();
            //Removing Duplicates
            string tmp = new String(all_letters.Distinct().ToArray());

            //looping over the characters and assign each char a number 
            for (int i = 0; i < tmp.Length; ++i)
            {
                string x = "";
                x += tmp[i];
                dict[x] = idx++;
            }
            input.Close();
        }

        //Function to Compress the Data
        public void compress(string pathtoRead)
        {
            //Read Data to be Compressed
            StreamReader sr = new StreamReader(pathtoRead, Encoding.UTF8, false);
            string data = sr.ReadToEnd();
            sr.Close();
            
            //Call Function Build to build The Dictionary ^
            buildDictionary();

            //const number of bits to represent any value in the dictionary
            // this is the very required bits to represent largest value we don't need more
            int bits = 17;

            //To Limit the Dictionary -> this will represent the largest value the Dictionary can have
            int dictLimit = (1 << bits)-1;

            
            //List to have the encoded chars as Integers
            List<int> seq = new List<int>();

            //Encoding Sequence : Chars --> integers --> binary --> output as bytes
            
            //Algorithm Logic
            string block = "";
            for (int i = 0; i < data.Length; ++i)
            {
                char letter = data[i];
                //append the char to what we had before
                block += letter;
                
                //if we have this string already in the dictionary
                //we don't store it  so we simply continue 
                if (dict.ContainsKey(block)) continue;
                else
                {   //Otherwise
                    // we store it and give it a number -- (if we didn't reach the Dicionary Limit)
                    if (dict.Count < dictLimit)
                        dict[block] = dict.Count;

                    //Encode the String we had as a number
                    string tmp = block.Remove(block.Length - 1, 1);
                    seq.Add(dict[tmp]);

                    //Begin a new string(block) with 1 letter and so on..
                    block = letter.ToString();
                }
            }
            if (dict.ContainsKey(block)) seq.Add(dict[block]);
            else seq.Add(dict.Count);

            //List to Encode as binary
            List<char> binary = new List<char>();

            //loop over the Encoded integers
            //and convert each ONE of them to binary of ($bits -Look above-) fixed number of bits
            for (int i = 0; i < seq.Count; ++i)
            {
                //Get the Number
                int ID = seq[i];

                //Convert it to Binary
                string bin = Convert.ToString(ID, 2);
                string tmp = "";

                //Adding any missing 0's
                for (int j = 0; j < bits - bin.Length; ++j)
                {
                    tmp += '0';
                }
                tmp += bin;

                //Save it to the list
                for (int j = 0; j < tmp.Length; ++j)
                {
                    binary.Add(tmp[j]);
                }
            }

            output(binary , bits);
        }

        //Function to Output the Encoded chars
        private void output(List <char> binary , int bits)
        {
            //Create binary file and open it
            FileStream fs = new FileStream("EncodedText.bin", FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);

            byte sum = 0;
            byte pow = 8;

            //list to have the converted binary as bytes
            List<byte> mylist = new List<byte>();

            //Add number of bits used for each char in the file as it will be needed for Decoding
            mylist.Add((byte)bits);

            //loop over the bits and construct a byte from each 8 bits
            for (int i = 0; i < binary.Count; ++i)
            {
                char bit = binary[i];
                --pow;
                if (bit == '1')
                    sum += (byte)(1 << pow);

                //When pow = 0 : means that we managed to construct a full byte (8 bits)
                if (pow == 0)
                {
                    //Add the byte to the list
                    mylist.Add(sum);
                    //Reset to get another byte
                    pow = 8;
                    sum = 0;
                }
            }
            // in case we missed the last byte (means that the last byte was't full byte - less than 8 bits -)
            //will consider the left bits as 0's
            if (pow != 8)
                mylist.Add(sum);

            //make array of byte so we can output it 
            Byte[] arr = new byte[mylist.Count];

            for (int i = 0; i < mylist.Count; ++i)
                arr[i] = mylist[i];

            //output
            bw.Write(arr);

            //close the files
            bw.Close();
            fs.Close();
        }
//---------------------------------------------------------------------------------------------- Decoding

        //Function to build the Reversed Dictionary that is used for Decoding
        private void BuildInverseDict()
        {
            //Now each number will represent a string (the opposite of Encoding)
            Inverse = new Dictionary<int, string>();

            //Read the Dictionary
            StreamReader input = new StreamReader("Dictionary.txt", Encoding.UTF8, false);
            string all_letters = input.ReadToEnd();
            string tmp = new String(all_letters.Distinct().ToArray());

            //Same logic used in original Dictionary but in the opposite way
            for (int i = 0; i < tmp.Length; ++i)
            {
                string x = "";
                x += tmp[i];
                Inverse[i] = x;
            }
            input.Close();
        }

        public void Decode(string path)
        {
            //build the Reveresed Dictionary
            BuildInverseDict();

            FileStream fs = new FileStream(path, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);

            //Read the First byte (this byte represents the number of bits used for each char)
            int bits = br.ReadByte();


            //Decoding Sequence : Bytes --> Binary --> Integers --> Chars
            //Reading the Bytes from the Encoded bytes
            List<byte> mylist = new List<byte>(); 
            while (true)
            {
                try
                {
                    //try to read bytes one by one
                    byte x = br.ReadByte();
                    // add it to the list of bytes
                    mylist.Add(x);
                }
                catch (EndOfStreamException ex)
                {
                    //get out when there are no more bytes to read
                    br.Close();
                    fs.Close();
                    break;
                }
            }

            //list of binary 
            List<char> binary = new List<char>();

            //loop over te bytes we just read and convert each one to 8 bits binary
            for (int i = 0; i < mylist.Count; ++i)
            {
                //convert the byte to binry
                string tmp = Convert.ToString(mylist[i], 2);
                string bin = "";

                //adding any missing 0's
                for (int j = 0; j < 8 - tmp.Length; ++j)
                    bin += '0';
                bin += tmp;

                for (int j = 0; j < bin.Length; ++j)
                    binary.Add(bin[j]);
            }

            //list of integers to have the converted binary
            List<int> compressed = new List<int>();
            string b = "";
            //loop over the bits and get each ($bits) bit and construct an integer
            for (int i = 0; i < binary.Count; ++i)
            {
                b += binary[i];
                if (b.Length == bits)
                {
                    int num = Convert.ToInt32(b, 2);
                    compressed.Add(num);
                    b = "";
                }
            }

            //open file to write the decoded text
            StreamWriter SW = new StreamWriter("DecodedText.txt", false, Encoding.UTF8);
            string  Out = "", Last = "";
            // loop over the numbers and convert them to chars using the Reversed Dictionary
            for (int i = 0; i < compressed.Count; ++i)
            {
                int X = compressed[i];
                Out = "";
                if (Inverse.ContainsKey(X))
                {
                    Out = Inverse[X];
                    if (Last != "") Inverse[Inverse.Count] = Last + Out[0];
                }
                else
                {
                    Out = Last + Last[0];
                    Inverse[Inverse.Count] = Out;
                }
                SW.Write(Out);
                Last = Out;
            }
            SW.Close();
        }
    }
}
