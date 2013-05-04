using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

using Eac3ToLib.Models;
using System.Text.RegularExpressions;

namespace Eac3ToLib
{
    public class Eac3ToLib
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Eac3ToLib));


        public static void GetAllEnglishSubTitles(string folderPath, string subTitleDestinationFolderPath)
        {
            var dirInfo = new System.IO.DirectoryInfo(folderPath);

            var titles = ReadTitlesFromFolder(string.Format("\"{0}\"", folderPath));

            log.InfoFormat("{0} titles found in {1}", titles.Count, folderPath);

            // run through the titles
            foreach (var title in titles)
            {
                log.InfoFormat("Title {0} with Length {1} and MPLS Number {2}", title.Number, title.Length, title.MPLS);
                var tracks = GetTracksFromTitle(dirInfo.FullName, title);

                log.InfoFormat("{0} tracks found in {1}", tracks.Count, folderPath);

                foreach (var subTitleTrack in tracks.Where(t => t.Type.StartsWith("Subtitle (PGS)", StringComparison.OrdinalIgnoreCase) &&
                                                                string.Equals(t.Details, "english", StringComparison.OrdinalIgnoreCase)
                                                            )
                    )
                {
                    string subTitleFileName = string.Format("{0} {1} {2}-{3}.sup", dirInfo.Name,
                                                                            title.MPLS,
                                                                            subTitleTrack.Details,
                                                                            subTitleTrack.Number);

                    string subTitleFullPath = System.IO.Path.Combine(subTitleDestinationFolderPath, subTitleFileName);

                    string outputArgs = string.Format("\"{0}\" {1}) {2}: \"{3}\"", dirInfo.FullName,
                                                                                        title.Number,
                                                                                        subTitleTrack.Number,
                                                                                        subTitleFullPath);

                    // finally output the subtitles
                    var results = RunEac3To(outputArgs);
                    log.InfoFormat("Subtitle written to {0}", subTitleFullPath);

                }
            }

        }

        private static List<Title> ReadTitlesFromFolder(string folderPath)
        {
            List<Title> titles = new List<Title>();

            var titleLines = RunEac3To(folderPath);

            foreach (string line in titleLines)
            {
                string clean = line.Substring(80);

                string pattern = @"^(?<Number>\d+)\)\s(?<MPLS_Number>\d+)\.mpls.*(?<Length>\d+:\d+:\d+).*";

                Match m = Regex.Match(clean, pattern);

                if (m.Success)
                {
                    titles.Add(new Title
                    {
                        Number = Convert.ToInt32(m.Groups["Number"].Value),
                        MPLS = m.Groups["MPLS_Number"].Value,
                        Length = m.Groups["Length"].Value
                    });
                }
            }


            return titles;
        }



        private static List<Track> GetTracksFromTitle(string folderPath, Title title)
        {
            List<Track> tracks = new List<Track>();

            var trackLines = RunEac3To(string.Format("\"{0}\" {1})", folderPath, title.Number));

            foreach (string line in trackLines)
            {
                string clean = line.Substring(80);

                string pattern = @"^(?<Number>\d+):\s(?<Type>[^,]+),\s(?<Details>.+)";

                Match m = Regex.Match(clean, pattern);

                if (m.Success)
                {
                    tracks.Add(new Track
                    {
                        Number = Convert.ToInt32(m.Groups["Number"].Value),
                        Type = m.Groups["Type"].Value,
                        Details = m.Groups["Details"].Value.Trim()
                    });
                }
            }

            return tracks;
        }





        private static List<string> RunEac3To(string args)
        {
            List<string> results = new List<string>();

            //args.Dump("Arguments");

            log.InfoFormat("Running Eac3To with args {0}", args);

            Process p = new Process();
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;

            p.StartInfo.StandardOutputEncoding = Encoding.ASCII;

            p.StartInfo.Arguments = args;
            p.StartInfo.FileName = "eac3to.exe";


            p.Start();

            Thread collectOutputThread = new Thread((ThreadStart)delegate
            {

                string line = null;
                while ((line = p.StandardOutput.ReadLine()) != null)
                {
                    //line.Dump();
                    results.Add(line);
                }

            });

            collectOutputThread.Start();

            Thread collectStdErrorThread = new Thread((ThreadStart)delegate
            {

                string line = null;

                while ((line = p.StandardError.ReadLine()) != null)
                {
                    log.Error(line);
                }

            });

            collectStdErrorThread.Start();

            p.WaitForExit();

            log.Info("Eac3to Done");

            return results;
        }


    }
}
