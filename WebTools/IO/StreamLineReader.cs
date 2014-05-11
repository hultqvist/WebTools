using System;
using System.IO;
using System.Text;

namespace SilentOrbit.IO
{
    public class StreamLineReader : LineReader
    {
        readonly Stream stream;
        readonly byte[] buffer = new byte[1000];
        /// <summary>
        /// bytes filled in buffer
        /// </summary>
        int offset = 0;

        public StreamLineReader(Stream stream)
        {
            this.stream = stream;
        }

        public override byte[] ReadLineBytes()
        {
            bool gotCR = false;
            int start = 0;

            while (true)
            {
                //Scan for new line
                for (int n = start; n < offset; n++)
                {
                    if (gotCR || buffer [n] == '\n')
                    {
                        //We got a line here
                        
                        //Prepare line returned
                        byte[] ret;
                        if (gotCR)
                        {
                            //for CRNL line endings
                            ret = new byte[n - 1];
                            Buffer.BlockCopy(buffer, 0, ret, 0, n - 1);
                        } else
                        {
                            //for NL only line endings
                            ret = new byte[n];
                            Buffer.BlockCopy(buffer, 0, ret, 0, n);
                        }
                        //Fix remaining bytes
                        int off = (buffer [n] == '\n' ? 1 : 0); //1 if current is newline, 0 if current is next character
                        offset = offset - n - off;
                        if (offset > 0)
                            Buffer.BlockCopy(buffer, n + off, buffer, 0, offset);
#if DEBUG
                        /*
                        if(offset == 0)
                            Debug.WriteLine("Remaining: <none>");
                        else
                            Debug.WriteLine("Remaining: " + Encoding.ASCII.GetString(buffer, 0, offset));
                            */
#endif
                        return ret;
                    }
                    
                    //This check must come after the if(gotCR)
                    if (buffer [n] == '\r')
                    {
                        gotCR = true;
                    }
                }
                start = offset;

                if (offset >= buffer.Length)
                    throw new FormatException("No end of line within " + buffer.Length + " bytes");
                
                //Read more data
                int read = stream.Read(buffer, offset, buffer.Length - offset);
                if (read <= 0)
                    return null; //end of file
                offset += read;
                if (offset > buffer.Length)
                    throw new FormatException("Line longer than " + buffer.Length);
#if DEBUG
                //Debug.WriteLine("So far: " + Encoding.ASCII.GetString(buffer, 0, offset));
#endif
            }
        }

		/// <summary>
		/// return whatever is already buffered
		/// </summary>
		public byte[] GetBuffered()
		{
			byte[] data = new byte[offset];
			if(offset > 0)
				Buffer.BlockCopy(buffer, 0, data, 0, offset);
			offset = 0;
			return data;
		}

        public byte[] ReadBytes(int count)
        {
            byte[] data = new byte[count];
            int read = 0;
			//First from buffer
			if (offset > 0)
			{
				if (offset > count)
				{
					Buffer.BlockCopy(buffer, 0, data, 0, count);
					offset -= count;
					Buffer.BlockCopy(buffer, count, buffer, 0, offset);
					return data;
				}
				if (offset == count)
				{
					Buffer.BlockCopy(buffer, 0, data, 0, count);
					offset = 0;
					return data;
				}
				if (offset < count)
				{
					Buffer.BlockCopy(buffer, 0, data, 0, offset);
					read = offset;
					offset = 0;
				}
			}

            while (read < count)
            {
                int r = stream.Read(data, read, count - read);
                if (r < 0)
                    throw new EndOfStreamException();
                read += r;
            }
#if DEBUG
            if (read != count)
                throw new InvalidProgramException("Read more than expected?");
#endif
            return data;
        }

		/// <summary>
		/// Read all data until the end of the connection into the outStream.
		/// </summary>
		/// <param name="outStream">Out stream.</param>
		/// <param name="aboutMax">A fuzzy limit on how much data may be read, use as a precaution for too large files.</param>
		public void ReadToEnd(Stream outStream, int aboutMax)
		{
			int total = offset;
			if (offset > 0)
			{
				outStream.Write(buffer, 0, offset);
				offset = 0;
			}

			while (true)
			{
				int read = stream.Read(buffer, 0, buffer.Length);
				if (read <= 0)
					return;
				total += read;
				if (total > aboutMax)
					throw new InvalidDataException("Read more than MaxContentLength(" + aboutMax + ")");
				outStream.Write(buffer, 0, read);
			}
		}
    }
}

