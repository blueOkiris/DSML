using ScottPlot;
using System.Collections.Generic;
using System;
using System.IO;
using Gtk;

namespace DSML {
    class PlotData {
        public string Name;
        public List<double> Times;
        public List<double> Values;

        public PlotData(string name, List<double> times, List<double> values) {
            Name = name;
            Times = times;
            Values = values;
        }
    }
    
    class PlotWindow {
        public static void ShowPlot(string title, PlotData[] data) {
            // Show in a mini window
            Application.Init();

            Window window = new Window("Plot Window");
            window.DeleteEvent += appDeletEvent;

            Notebook container = new Notebook();

            //Plot plot = new Plot(640, 480);
            //plot.Title(title);
            //plot.Legend(location: legendLocation.middleRight);
            //plot.Style(ScottPlot.Style.Gray1);

            /*LineStyle[] shapes = new LineStyle[5] {
                LineStyle.Solid,
                LineStyle.Dash,
                LineStyle.Dot,
                LineStyle.DashDot,
                LineStyle.DashDotDot
            };*/

            int iconIndex = 0;

            foreach(PlotData datum in data) {
                Plot plot = new Plot(1280, 720);
                plot.Title(title);
                //plot.Legend(location: legendLocation.middleRight);
                plot.Style(ScottPlot.Style.Gray1);

                plot.PlotScatter(
                    datum.Times.ToArray(),
                    datum.Values.ToArray(),
                    lineWidth: 3,
                    label: datum.Name,
                    //lineStyle: shapes[iconIndex++],
                    markerSize: 10);
                plot.XLabel("Time");
                plot.YLabel("Data");

                if(iconIndex >= 5)
                    iconIndex = 0;
                
                string imageName = title + DateTime.Now.ToString().Replace('/', '-') + datum.Name + ".png";
                plot.SaveFig(imageName);

                Image plotImage = new Image(imageName);
                container.AppendPage(plotImage, new Label(datum.Name));

                File.Delete(imageName);
            }

            //string imageName = title + DateTime.Now.ToString().Replace('/', '-') + ".png";
            //plot.SaveFig(imageName);

            //Image plotImage = new Image(imageName);
            //window.Add(plotImage);
            window.Add(container);

            window.ShowAll();
            Application.Run();
        }

        private static void appDeletEvent (object obj, DeleteEventArgs args) {
            Application.Quit();
        }
    }
}