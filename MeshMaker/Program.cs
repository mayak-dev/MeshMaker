using ObjLoader.Loader.Loaders;
using System;
using System.IO;

namespace MeshMaker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MeshVersion meshVersion = MeshVersion.VERSION_UNKNOWN;
            string fileName = string.Empty;
            string outFileName = string.Empty;

            bool fileNameArgFlag = false;
            bool outFileNameArgFlag = false;

            foreach (string arg in args)
            {
                // -f <file>
                if (fileName == string.Empty)
                {
                    if (fileNameArgFlag)
                    {
                        fileName = arg;
                        continue;
                    }
                    else if (arg == "-f")
                    {
                        fileNameArgFlag = true;
                        continue;
                    }
                }

                // -o <file>
                if (outFileName == string.Empty)
                {
                    if (outFileNameArgFlag)
                    {
                        outFileName = arg;
                        continue;
                    }
                    else if (arg == "-o")
                    {
                        outFileNameArgFlag = true;
                        continue;
                    }
                }

                // -v(1/2)
                if (meshVersion == MeshVersion.VERSION_UNKNOWN)
                {
                    if (arg == "-v1")
                    {
                        Console.WriteLine("Using mesh version 1.01");
                        meshVersion = MeshVersion.VERSION_101;
                        continue;
                    }
                    else if (arg == "-v2")
                    {
                        Console.WriteLine("Using mesh version 2.00");
                        meshVersion = MeshVersion.VERSION_200;
                        continue;
                    }
                }
            }

            if (fileName == string.Empty)
            {
                Console.WriteLine("An OBJ file name was not provided. Use -f <path>.");
                return;
            }

            if (outFileName == string.Empty)
            {
                outFileName = fileName;

                int pos = outFileName.LastIndexOf('.');
                if (pos != -1)
                    outFileName = outFileName.Remove(pos);

                outFileName += ".mesh";

                Console.WriteLine("An output file name was not provided. Using \"{0}\"...", outFileName);
                Console.WriteLine("Note: You can provide your own output file name by using -o <file>.");
            }

            if (meshVersion == MeshVersion.VERSION_UNKNOWN)
            {
                meshVersion = MeshVersion.VERSION_200;

                Console.WriteLine("A mesh version was not specified. Defaulting to version 2.00...");
                Console.WriteLine("Note: The desired mesh version can be set using -v1 or -v2.");
            }

            Console.WriteLine("Opening file \"{0}\"...", fileName);

            LoadResult loadedObj;
            using (FileStream inFile = File.OpenRead(fileName))
            {
                IObjLoader objLoader = new ObjLoaderFactory().Create();
                loadedObj = objLoader.Load(inFile);
            }

            Console.WriteLine("Writing file \"{0}\"...", outFileName);

            FileStream outFile = File.Open(outFileName, FileMode.Create);
            using (MeshWriter meshWriter = MeshWriter.Create(meshVersion, outFile, loadedObj))
            {
                meshWriter.Write();
            }
        }
    }
}
