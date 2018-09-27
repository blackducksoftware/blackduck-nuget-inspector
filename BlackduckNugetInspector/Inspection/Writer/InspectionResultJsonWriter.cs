using Com.Synopsys.Integration.Nuget.Inspection.Model;
using Com.Synopsys.Integration.Nuget.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Com.Synopsys.Integration.Nuget.Inspection.Writer
{
    class InspectionResultJsonWriter
    {
        private InspectionResult Result;
        private InspectionOutput InspectionOutput;

        public InspectionResultJsonWriter(InspectionResult result)
        {
            Result = result;
            InspectionOutput = new InspectionOutput();
            InspectionOutput.Containers = result.Containers;
        }

        public string FilePath()
        {
            // TODO: fix file name
            return $"{Result.OutputDirectory}{Path.DirectorySeparatorChar}{Result.ResultName}_inspection.json";
        }

        public void Write()
        {
            Write(Result.OutputDirectory);
        }

        public void Write(string outputDirectory)
        {
            Write(outputDirectory, FilePath());
        }

        public void Write(string outputDirectory, string outputFilePath)
        {

            if (outputDirectory == null)
            {
                Console.WriteLine("Could not create output directory: " + outputDirectory);
            }
            else
            {
                Console.WriteLine("Creating output directory: " + outputDirectory);
                Directory.CreateDirectory(outputDirectory);
            }

            Console.WriteLine("Creating output file path: " + outputFilePath);
            using (var fs = new FileStream(outputFilePath, FileMode.Create))
            {
                using (var sw = new StreamWriter(fs))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.NullValueHandling = NullValueHandling.Ignore;
                    JsonTextWriter writer = new JsonTextWriter(sw);
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(writer, InspectionOutput);
                }
            }
        }

    }
}
