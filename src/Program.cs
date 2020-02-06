using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using ScottPlot;

namespace DSML
{
    class Program
    {
        private static string TokenString(Token[] tokens) { 
            StringBuilder tokenStr = new StringBuilder();

            foreach(Token token in tokens) {
                tokenStr.Append(token.ToString());
                tokenStr.Append("\n");
            }

            return tokenStr.ToString();
        }

        private static DigitalSystem GetSystem(string filename) {
            if(!File.Exists(filename)) {
                Console.WriteLine("File does not exist: " + filename);
                return null;
            }

            Token[] tokenTree = Parser.ParseText(File.ReadAllText(filename));
            //Console.WriteLine(TokenString(tokenTree));

            DigitalSystem system = new DigitalSystem();
            system.Initialize(tokenTree);

            return system;
        }

        static void Main(string[] args)
        {
            // Can do two things: build a diagram or simulate
            if(args.Length != 2)
                Console.WriteLine("Unexpected number of arguments");
            else {
                switch(args[0]) {
                    case "diagram": {
                        DigitalSystem system = GetSystem(args[1]);
                        if(system == null)
                            return;
                        break;
                    }
                    
                    case "simulate": {
                        DigitalSystem system = GetSystem(args[1]);
                        if(system == null)
                            return;

                        foreach(string simulation in system.Simulations.Keys)
                            system.Simulations[simulation].Simulate();
                        break;
                    }
                    
                    case "plot": {
                        DigitalSystem system = GetSystem(args[1]);
                        if(system == null)
                            return;

                        foreach(string simulation in system.Simulations.Keys) {
                            List<PlotData> data = system.Simulations[simulation].CreatePlot();
                            PlotWindow.ShowPlot(args[1], data.ToArray());
                        }

                        break;
                    }

                    default:
                        Console.WriteLine("Unexpected argument: " + args[0]);
                        return;
                }
            }
        }
    }
}
