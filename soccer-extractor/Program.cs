using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace soccer_reader
{
    class Program
    {
        public class Team
        {
            public Team(string name, string country)
            {
                Id = Guid.NewGuid();
                Name = name;
                Country = country;
            }

            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Country { get; set; }
        }

        public class Match
        {
            public Match(string season, string phase, Team homeTeam, Team visitorTeam, int homeGoals, int visitorGoals)
            {
                Id = Guid.NewGuid();
                Season = season;
                Phase = phase;
                HomeTeam = homeTeam;
                VisitorTeam = visitorTeam;
                HomeGoals = homeGoals;
                VisitorGoals = visitorGoals;
            }

            public Guid Id { get; set; }
            public string Season { get; set; }
            public string Phase { get; set; }
            public Team HomeTeam { get; set; }
            public Team VisitorTeam { get; set; }
            public int HomeGoals { get; set; }
            public int VisitorGoals { get; set; }
        }

        static Dictionary<string, string> GetCompetitionFileNames()
        {
            string mainDirectory = @"C:\TEMP\master\champs";

            var competitions = new Dictionary<string, string>();
            foreach (var d in Directory.GetDirectories(mainDirectory))
                competitions.Add(d.Split('\\').LastOrDefault(), $@"{d}\champs.csv");

            return competitions;
        }

        static void Main(string[] args)
        {
            var teams = new List<Team>();
            var matches = new List<Match>();

            var competitionFileNames = GetCompetitionFileNames();

            foreach (var competitionFile in competitionFileNames)
            {
                if (int.Parse(competitionFile.Key.Split('-')[0]) < 1996)
                    continue;

                using (var reader = new StreamReader(competitionFile.Value))
                {
                    string line = reader.ReadLine();
                    line = reader.ReadLine();

                    while (!string.IsNullOrEmpty(line))
                    {
                        string[] splittedLine = line.Split(",");

                        Team homeTeam;
                        string[] homeTeamStr = splittedLine[4].Split(" › ");
                        string homeTeamName = homeTeamStr[0];
                        string homeTeamCountry = homeTeamStr[1].Split(" (")[0];
                        if ((homeTeam = teams.FirstOrDefault(x => x.Name == homeTeamName && x.Country == homeTeamCountry)) == null)
                        {
                            homeTeam = new Team(homeTeamName, homeTeamCountry);
                            teams.Add(homeTeam);
                        }

                        Team visitorTeam;
                        string[] visitorTeamStr = splittedLine[7].Split(" › ");
                        string visitorTeamName = visitorTeamStr[0];
                        string visitorTeamCountry = visitorTeamStr[1].Split(" (")[0];
                        if ((visitorTeam = teams.FirstOrDefault(x => x.Name == visitorTeamName && x.Country == visitorTeamCountry)) == null)
                        {
                            visitorTeam = new Team(visitorTeamName, visitorTeamCountry);
                            teams.Add(visitorTeam);
                        }

                        string[] matchStr = (splittedLine[5].Length > 3 ? splittedLine[5].Substring(0, 4).Trim() : splittedLine[5]).Split('-');
                        matches.Add(new Match(competitionFile.Key, splittedLine[0], homeTeam, visitorTeam, int.Parse(matchStr[0]), int.Parse(matchStr[1])));

                        line = reader.ReadLine();
                    }
                }
            }

            var a = teams.Select(x => x.Country).Distinct().ToList();

            foreach (var m in matches)
            {
                Console.WriteLine($"({m.HomeTeam.Country}) {m.HomeTeam.Name} {m.HomeGoals} X {m.VisitorGoals} {m.VisitorTeam.Name} ({m.VisitorTeam.Country})");
            }

            using (var streamWriter = new StreamWriter(@"C:\TEMP\master\champs\nos.csv"))
            {
                streamWriter.WriteLine("Id;Label;Country");
                foreach (var t in teams)
                    streamWriter.WriteLine($"{t.Id};{t.Name};{t.Country}");
            }

            using (var streamWriter = new StreamWriter(@"C:\TEMP\master\champs\arestas.csv"))
            {
                streamWriter.WriteLine("Id;Type;Season;Phase;Source;Home Team Name;Home Team Goals;Target;Visitor Team Name;Visitor Team Goals");
                foreach (var m in matches)
                    streamWriter.WriteLine($"{m.Id};undirected;{m.Season};{m.Phase};{m.HomeTeam.Id};{m.HomeTeam.Name};{m.HomeGoals};{m.VisitorTeam.Id};{m.VisitorTeam.Name};{m.VisitorGoals}");
            }
        }
    }
}
