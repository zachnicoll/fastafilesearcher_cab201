﻿using System;
using System.IO;
using System.Collections.Generic;

namespace Search16s
{
    class Searchable
    {
        // Declare parrallel arrays to hold the byte offsets and lengths for each line.
        private List<int> offsets = new List<int>();
        private List<int> lengths = new List<int>();
        private FileStream stream;
        public Searchable(string fileName)
        {
            // Create a FileStream that can be parsed into functions, pointing to the .fasta file.
            stream = new FileStream(fileName, FileMode.Open);

            // Find the offsets and lengths of each line in the supplied .fasta file.
            FindOffsets(ref offsets, ref lengths);
        }

        // FindOffsets()
        // Function used for finding the byte offsets for each line of the .fasta file, and
        // each line's respective length. Takes a reference to an integer array to hold the offsets,
        // reference to an integer array to hold the line lengths, and a FileStream to read.
        void FindOffsets(ref List<int> offsetArr, ref List<int> lengthArr)
        {
            // Seek to the beginning of the stream (start of file, with byte offset 0).
            stream.Seek(0, SeekOrigin.Begin);

            // Begin reading the file.
            StreamReader reader = new StreamReader(stream);

            string line; // Holds the current line as a string.
            int i = 0; // Stores the current index of the arrays.
            int position = 0; // Holds the current byte offset in the file.

            // While end of file hasn't been reached.
            while ((line = reader.ReadLine()) != null)
            {
                // Store the length of the current line.
                lengthArr.Add(line.Length + 1);

                // Store the line's byte offset;
                offsetArr.Add(position);

                // Move the offset to the next line.
                position += line.Length + 1;

                // Increment index.
                i++;
            }
        }

        // GetLine()
        // Returns the line from the parsed FileStream with a desired offset.
        string GetLine(int offset)
        {
            string str;

            // Move 'cursor' by the offset relative to the start of the file.
            stream.Seek(offset, SeekOrigin.Begin);

            // Begin reading the file.
            StreamReader reader = new StreamReader(stream);

            // Read and store the current line to be returned.
            str = reader.ReadLine();

            return str;
        }

        // Level1Search()
        // Performs a Level1 sequential search on the parsed stream. Takes a starting line,
        // number of desired output sequences, offset array, and FileStream.
        public void Level1Search(int line, int nSeq)
        {
            // Line number must be odd and greater than 0.
            if (line % 2 == 0 || line < 1)
            {
                Console.WriteLine("\n\t<ERROR>\n\n\tPlease enter an odd line number > 0");
            }
            else
            {
                // Each 'sequence' outputs two lines, therefore the number of sequences must be doubled.
                for (int i = 0; i < 2 * nSeq; i += 2)
                {
                    // Parse the offset at index (line-1) + i, as line numbers start from 1, and indices of arrays
                    // start from 0. This prints out the ID and description.
                    Console.WriteLine(GetLine(offsets[(line - 1) + i]));

                    // Parse the offset at index (line-1) + i + 1 to also print out the sequence string.
                    Console.WriteLine(GetLine(offsets[(line - 1) + i + 1]));
                }
            }

            stream.Close();
        }

        // Level2Search()
        // Performs a Level2 sequential search on the supplied FileStream and prints the sequence
        // with the matching id. Takes an array of byte offsets, an id to compare, and a FileStream to search.
        public void Level2Search(string id)
        {
            int found = 0;

            // Loop through every line int the stream until the id string is mathced.
            for (int i = 0; i < offsets.Count; i++)
            {
                if (GetLine(offsets[i]).Contains(id))
                {
                    found = 1;

                    // Print the matching sequence id and sequence string.
                    Console.WriteLine(GetLine(offsets[i]));
                    Console.WriteLine(GetLine(offsets[i + 1]));
                    break;
                }
            }

            // If the id is not matched, alert the user.
            if (found == 0)
            {
                Console.WriteLine("Error, sequence {0} not found.", id);
            }

            stream.Close();
        }

        // Level3Search()
        // Perform a Level3 sequential search on the parsed FileStream. Similiar to a Level2 search,
        // however a file containing the desired match ids must be parsed, as well as a file to store
        // the resultant matches. Takes an array of byte offsets, a file names of the query file and
        // output file, and a FileStream to search.
        public void Level3Search(string queryFile, string outFile)
        {
            if (File.Exists(queryFile))
            {
                // Create (or overwrite) a new results file every time a Level3 search is run.
                File.Create(outFile).Dispose();

                // Open the query file for reading, and the results file for writing.
                FileStream inStream = new FileStream(queryFile, FileMode.Open);
                StreamReader inReader = new StreamReader(inStream);

                FileStream outStream = new FileStream(outFile, FileMode.Open);
                StreamWriter outWriter = new StreamWriter(outStream);

                string query;

                // While the reader hasn't reached the end of the QUERY file, store the query(id)
                // and perform the equivalent of a Level2 search with it.
                while ((query = inReader.ReadLine()) != null)
                {
                    int found = 0;

                    // Loop through every line of the supplied .fasta FileStream, similar to Level2Search().
                    for (int i = 0; i < offsets.Count; i++)
                    {
                        // Only write the results to the file if the query id is found.
                        if (GetLine(offsets[i]).Contains(query))
                        {
                            found = 1;
                            outWriter.WriteLine(GetLine(offsets[i]));
                            outWriter.WriteLine(GetLine(offsets[i + 1]));
                            break;
                        }
                    }

                    // Display an error message in the console for each query that could not be found.
                    if (found == 0)
                    {
                        Console.WriteLine("Error, sequence {0} not found.", query);
                    }
                }

                inStream.Close();
                outStream.Close();
            }
            // If the supplied query file name does not exist, display an error message to the user.
            else
            {
                Console.WriteLine("\n\t<ERROR>\n\n\tInvalid query file name. Check that the name is correct and that the file exists, and try again.");
            }
        }

        public void Level4Search(string indexFile, string queryFile, string outFile)
        {
            if (File.Exists(indexFile) && File.Exists(queryFile))
            {
                // Create (or overwrite) a new results file every time a Level3 search is run.
                File.Create(outFile).Dispose();

                // Open the query file for reading, and the results file for writing.
                FileStream inStream = new FileStream(queryFile, FileMode.Open);
                StreamReader inReader = new StreamReader(inStream);

                FileStream indexStream = new FileStream(queryFile, FileMode.Open);
                StreamReader indexReader = new StreamReader(indexStream);

                FileStream outStream = new FileStream(outFile, FileMode.Open);
                StreamWriter outWriter = new StreamWriter(outStream);

                List<string> indexList = new List<string>();
                string line;
                while((line = indexReader.ReadLine()) != null)
                {
                    indexList.Add(line);
                }

                // While the reader hasn't reached the end of the QUERY file, store the query(id)
                // and perform the equivalent of a Level2 search with it.
                int searchIndex = -1;
                while ((line = inReader.ReadLine()) != null)
                {
                    int found = 0;
                    for(int i = 0; i < indexList.Count; i++)
                    {
                        if (indexList[i].Contains(line))
                        {
                            searchIndex = Convert.ToInt32(indexList[i].Split(' ')[1]);
                            found = 1;
                            break;
                        }
                    }

                    if (found == 1)
                    {
                        Console.WriteLine(GetLine(searchIndex));
                        //outWriter.WriteLine(GetLine(searchIndex));
                    }
                    else
                    {
                        Console.WriteLine("Error, sequence {0} not found.", line);
                    }
                }

                inStream.Close();
                outStream.Close();
            }
            // If the supplied query file name does not exist, display an error message to the user.
            else
            {
                Console.WriteLine("\n\t<ERROR>\n\n\tInvalid query file name. Check that the name is correct and that the file exists, and try again.");
            }
        }
    }
}
