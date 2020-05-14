using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HeaderUpdater
{
    class Program
    {
        private static string[] OldHeaderDump;
        private static string[] NewHeaderDump;

        private static string[] ComposerClass;
        private static string[] EventClass;

        private static Dictionary<string, string> OldIncomingHeaderList = new Dictionary<string, string>();
        private static Dictionary<string, string> OldOutgoingHeaderList = new Dictionary<string, string>();

        private static Dictionary<string, string> NewIncomingHeaderList = new Dictionary<string, string>();
        private static Dictionary<string, string> NewOutgoingHeaderList = new Dictionary<string, string>();

        private static List<string> UpdatedComposerClass = new List<string>();
        private static List<string> UpdatedEventClass = new List<string>();

        static void Main(string[] args)
        {
            EventClass = File.ReadAllLines("incoming.txt");
            ComposerClass = File.ReadAllLines("outgoing.txt");

            OldHeaderDump = File.ReadAllLines("old_dump.txt");
            NewHeaderDump = File.ReadAllLines("new_dump.txt");

            ParseHeaderDump(OldHeaderDump, false);
            ParseHeaderDump(NewHeaderDump, true);

            Console.WriteLine("(" + OldHeaderDump.Length + ") Successfully parsed " + OldIncomingHeaderList.Count + " old incoming headers and " + OldOutgoingHeaderList.Count + " old outgoing headers");
            Console.WriteLine("(" + NewHeaderDump.Length + ") Successfully parsed " + NewIncomingHeaderList.Count + " new incoming headers and " + NewOutgoingHeaderList.Count + " new outgoing headers");

            int LineNumber = 0;
            int IncomingHeaderSuccess = 0;

            foreach (string Line in EventClass)
            {
                LineNumber++;

                if (!(Line.Contains(" = ") && Line.Contains(";")))
                {
                    UpdatedEventClass.Add(Line);
                }
                else
                {

                    string NewLine = Line.Substring(0, Line.IndexOf(";"));
                    string[] SplitData = NewLine.Split(' ');

                    string header = SplitData[SplitData.Length - 1];
                    string errorLine = NewLine.Replace(header, (header.Contains("-") ? header + ";" : "-" + header + ";") + "//" + header);

                    string Hash = FindHash(header, OldIncomingHeaderList);

                    if (Hash != null)
                    {
                        string Newheader = FindHeader(Hash, NewIncomingHeaderList);

                        if (Newheader != null)
                        {
                            UpdatedEventClass.Add(NewLine.Replace(header, Newheader) + ";//" + header);
                            IncomingHeaderSuccess++;
                        }
                        else
                        {
                            UpdatedEventClass.Add(errorLine);
                        }
                    }
                    else
                    {
                        UpdatedEventClass.Add(errorLine);
                    }

                }
            }

            LineNumber = 0;
            int OutgoingHeaderSuccess = 0;

            foreach (string Line in ComposerClass)
            {
                LineNumber++;

                if (!(Line.Contains(" = ") && Line.Contains(";")))
                {
                    UpdatedComposerClass.Add(Line);
                }
                else
                {
                    string NewLine = Line.Substring(0, Line.IndexOf(";"));
                    string[] SplitData = NewLine.Split(' ');

                    string header = SplitData[SplitData.Length - 1];
                    string errorLine = NewLine.Replace(header, (header.Contains("-") ? header + ";" : "-" + header + ";") + "//" + header);

                    string Hash = FindHash(header, OldOutgoingHeaderList);

                    if (Hash != null)
                    {
                        string Newheader = FindHeader(Hash, NewOutgoingHeaderList);

                        if (Newheader != null)
                        {
                            UpdatedComposerClass.Add(NewLine.Replace(header, Newheader) + ";//" + header);
                            OutgoingHeaderSuccess++;
                        }
                        else
                        {
                            UpdatedComposerClass.Add(errorLine);
                        }
                    }
                    else
                    {
                        UpdatedComposerClass.Add(errorLine);
                    }
                }
            }

            File.WriteAllLines("new_incoming.txt", UpdatedEventClass.ToArray());
            Console.WriteLine("Incoming header success: " + IncomingHeaderSuccess + " (" + ComposerClass.Length + " / " + UpdatedComposerClass.Count + ")");

            File.WriteAllLines("new_outgoing.txt", UpdatedComposerClass.ToArray());
            Console.WriteLine("Outgoing header success: " + OutgoingHeaderSuccess + " (" + EventClass.Length + " / " + UpdatedEventClass.Count + ")");

            Console.Read();
        }

        private static string FindHash(string header, Dictionary<string, string> HeaderList)
        {
            foreach (var kvp in HeaderList)
            {
                if (kvp.Value.Equals(header))
                {
                    return kvp.Key;
                }
            }

            return null;
        }

        private static string FindHeader(string hash, Dictionary<string, string> HeaderList)
        {
            foreach (var kvp in HeaderList)
            {
                if (kvp.Key.Equals(hash))
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        private static void ParseHeaderDump(string[] HeaderDump, bool NewRevision)
        {
            foreach (String Line in HeaderDump)
            {
                if (!Line.Contains("] = "))
                {
                    continue;
                }

                if (Line.ToLower().Contains("collisions"))
                {
                    //continue;
                }

                String NewLine = Line;
                NewLine = NewLine.Replace("[Dead]", "");
                NewLine = NewLine.Replace("[Collisions: 2]", "");
                NewLine = NewLine.Split('=')[0].TrimEnd(new char[] { ' ' }).Replace(" ", "").Replace("[", ",").Replace("]", "");

                bool Outgoing = NewLine.Split(',')[0].Equals("Incoming"); // remember it's reversed for the client
                string Header = NewLine.Split(',')[1];
                string Hash = NewLine.Split(',')[2];

                if (NewRevision)
                {
                    if (Outgoing)
                    {
                        if (!NewOutgoingHeaderList.ContainsKey(Hash))
                        {
                            NewOutgoingHeaderList.Add(Hash, Header);
                        }
                    }
                    else
                    {
                        if (!NewIncomingHeaderList.ContainsKey(Hash))
                        {
                            NewIncomingHeaderList.Add(Hash, Header);
                        }
                    }
                }
                else
                {
                    if (Outgoing)
                    {
                        if (!OldOutgoingHeaderList.ContainsKey(Hash))
                        {
                            OldOutgoingHeaderList.Add(Hash, Header);
                        }
                    }
                    else
                    {
                        if (!OldIncomingHeaderList.ContainsKey(Hash))
                        {
                            OldIncomingHeaderList.Add(Hash, Header);
                        }
                    }
                }
            }
        }


    }
}
