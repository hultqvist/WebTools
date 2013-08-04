using System;
using System.IO;
using System.Text;
using System.Net.Sockets;

namespace SilentOrbit.IO
{
    /// <summary>
    /// Line reading for compatibility with reading directly from stream
    /// </summary>
    public abstract class LineReader
    {
        public abstract byte[] ReadLineBytes();

        public string ReadLine()
        {
            var lineBuffer = ReadLineBytes();
            if (lineBuffer == null)
                return null;
            string line = Encoding.ASCII.GetString(lineBuffer);
            return line;
        }

        //helper for ReadHeader
        string nextLine = null;
        bool firstHeaderLine = true;
        
        /// <summary>
        /// Reads and unfolds a header "line".
        /// This method do read ahead unless it return "".
        /// </summary>
        /// <returns>
        /// The unfolded header line.
        /// </returns>
        public string ReadHeader()
        {
            if (firstHeaderLine)
            {
                nextLine = ReadLine();
                firstHeaderLine = false;
            }
            if (nextLine == null)
                return null; //end of file while reading header
            if (nextLine == "")
                return ""; //end of header, no more pre-reading
            
            //Unfold
            string line = nextLine;
            while (true)
            {
                nextLine = ReadLine();
                if (nextLine == null)
                    return line;
                if (nextLine == "")
                    return line;
                if (nextLine [0] == ' ' || nextLine [0] == '\t') //Folding detected
                {
                    line += nextLine;
                    continue;
                }
                return line;
            }
        }
    }
}

