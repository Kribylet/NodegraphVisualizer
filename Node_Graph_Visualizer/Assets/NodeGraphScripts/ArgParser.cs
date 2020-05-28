using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Nodegraph_Generator
{
    /*
     * Class for parse arguments from the terminal.
     */
    public static class ArgParser
    {
        private static List<int> validArgsCount = new List<int>{ 2, 3 };
        private static Dictionary<string, string> flagsDictionary = new Dictionary<string, string>
            {{"MeshSolution", "m"},
            {"VoxelSolution", "v"}
            };
        private const int flagIndex = 2;

        /*
         * Parse arguments into an input and output path, and decides which graph generator solution to choose.
         */
        public static FilePaths Parse(string[] args)
        {
            CheckValidArgs(args);
            FilePaths filePath = new FilePaths(
                GetInputPath(args[0]),
                GetOutputPath(args[1]));
            SetFlags(args, filePath);
            return filePath;
        }

        /*
         * Check so that the args is the correct size and returns the bool to decide which solution to run, 
         * throws error if arguments are invalid. 
         */
        public static void CheckValidArgs(string[] args)
        {
            if (!validArgsCount.Contains(args.Length))
            {
                throw new ArgumentException("Tried to run program with " + args.Length + " arguments.");
            }

            if (args.Length > flagIndex) {
                for (int i = flagIndex; i < args.Length; i++)
                {
                    if (args[i][0] != '-')
                    {
                        throw new ArgumentException("Invalid flag argument. Missig '-' char.");
                    }
                    if (!flagsDictionary.ContainsValue(args[i].Substring(1)))
                    {
                        throw new ArgumentException("Invalid flag argument. Possible flas are " + flagsDictionary.Values.ToString());
                    }
                }
            }
        }

        /*
         * Fetches input path, using the GetPath method.
         * Also has handling for when a input file is not found.
         */
        public static string GetInputPath(string relativeInputPath)
        {
            string inputPath = ArgParser.GetPath(relativeInputPath);
            if (File.Exists(inputPath))
            {
                return inputPath;
            }
            else
            {
                throw new ArgumentException("file " + inputPath + " does not exist. System exit");
            }
            
        }

        /*
        * Fetches output path, using the GetPath method.
        * Also has handling for when a output file of that name already exists.
        */
        public static string GetOutputPath(string relativeOutputPath)
        {
            string outputFilePath = ArgParser.GetPath(relativeOutputPath);
            if(File.Exists(outputFilePath))
            {
                throw new ArgumentException("Output file " + outputFilePath + " already exists. System exit.");
            }
            return outputFilePath;
        }

        /*
         * Fetches path to given relative path
         */
        public static string GetPath(String relativePath)
        {
            return Path.GetFullPath(relativePath);
        }

        /*
         * Sets all flags based on arguments
         */
        private static void SetFlags(string[] args, FilePaths filePaths)
        {
            SetDefaultFlags(filePaths);
            for (int i = flagIndex; i < args.Length; i++)
            {
                string flag = args[i].Substring(1);
                if (flagsDictionary["MeshSolution"] == flag)
                {
                    filePaths.voxelSolution = false;
                }
                else if (flagsDictionary["VoxelSolution"] == flag)
                {
                    filePaths.voxelSolution = true;
                }
            }
        }

        /*
        * Sets default flags
        */
        private static void SetDefaultFlags(FilePaths filePaths)
        {
            filePaths.voxelSolution = true;
        }
    }
}
