using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace RailwayWars.ContestRunner
{
    internal class Program
    {
        static class Settings
        {
            public static TimeSpan GameBreakTime => TimeSpan.FromMilliseconds(Convert.ToInt32(ConfigurationManager.AppSettings["GameBreakTimeMilliseconds"] ?? "1000"));
            public static int ViewersConnectorPort => Convert.ToInt32(ConfigurationManager.AppSettings["ViewersConnectorPort"] ?? "9933");
            public static int PlayersConnectorPort => Convert.ToInt32(ConfigurationManager.AppSettings["PlayersConnectorPort"] ?? "9977");
        }

        private static void Main(string[] args)
        {

            Console.WriteLine("Configuration: {0}", string.Join("|", ConfigurationManager.AppSettings.AllKeys));
            try
            {
                var options = new Options();
                if (Parser.Default.ParseArgumentsStrict(args, options))
                {
                    var generators = new Dictionary<string, Func<Func<int, BoardDefinition>>>
                    {
                        {
                            "square",
                            () => playersCount => BoardFactory.RandomSquareBoard(options.Size, options.WaterPercentage, options.TurnTimeMilliseconds, options.ExpectedMatchDurationSeconds, playersCount)
                        },
                        {
                            "squareloop",
                            () => BoardFactory.CreateRandomSquareBoardLoopGenerator(options.Size, options.WaterPercentage, options.TurnTimeMilliseconds, options.ExpectedMatchDurationSeconds)
                        },
                        {
                            "mountain",
                            () => playersCount => BoardFactory.MountainBoard(options.Size, options.TurnTimeMilliseconds, options.ExpectedMatchDurationSeconds, playersCount)
                        }
                    };

                    Func<Func<int, BoardDefinition>> boardGeneratorFactory;
                    if (generators.TryGetValue(options.Generator.ToLower().Trim(), out boardGeneratorFactory))
                    {
                        RunTournament(boardGeneratorFactory());
                    }
                    else
                    {
                        Console.WriteLine($"Invalid board generator name: {options.Generator}.");
                        Console.WriteLine($"Available generators: {string.Join(", ", generators.Keys)}.");
                    }
                }
                else
                {
                    // Display the default usage information
                    Console.WriteLine(HelpText.AutoBuild(options));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
            }
        }

        private static void RunTournament(Func<int, BoardDefinition> boardGenerator)
        {
            try
            {
                var tournament = new Tournament(boardGenerator)
                {
                    GameBreakTime = Settings.GameBreakTime
                };

                var connectorCts = new CancellationTokenSource();

                var viewersConnector = new ViewersConnector(Settings.ViewersConnectorPort);
                tournament.StateUpdated += state => viewersConnector.UpdateState(state);
                var viewersConnectorTask = viewersConnector.Start(connectorCts.Token);

                var playersConnector = new PlayersConnector(Settings.PlayersConnectorPort, tournament.Players);
                var playersConnectorTask = playersConnector.Start(connectorCts.Token);

                var stopTournamentSignal = new ManualResetEventSlim();
                var gameRunnerTask = Task.Factory.StartNew(() => { tournament.Run(() => stopTournamentSignal.IsSet); });

                Console.WriteLine("Press any key to stop tournament...");
                Task.WaitAny(gameRunnerTask, playersConnectorTask, viewersConnectorTask,
                    Task.Factory.StartNew(() => Console.ReadKey()));

                stopTournamentSignal.Set();
                Task.WaitAll(gameRunnerTask);
                connectorCts.Cancel();
                Task.WaitAll(playersConnectorTask, viewersConnectorTask);
            }
            catch (Exception ex)
                when (
                    (ex is TaskCanceledException) ||
                    (((ex as AggregateException)?.InnerExceptions.All(ie => (ie is TaskCanceledException))) ?? false))
            {
                // Ignore task cancellation exceptions
            }
        }

        private class Options
        {
            [Option('g', "generator", DefaultValue = "square", HelpText = "Board generator for tournament.")]
            public string Generator { get; set; }

            [Option('t', "turn", DefaultValue = 200, HelpText = "Turn time in milliseconds.")]
            public int TurnTimeMilliseconds { get; set; }

            [Option('d', "duration", DefaultValue = 120, HelpText = "Expected single match duration in seconds.")]
            public int ExpectedMatchDurationSeconds { get; set; }

            [Option('s', "size", DefaultValue = 32, HelpText = "Board size (if applicable).")]
            public int Size { get; set; }

            [Option('w', "water", DefaultValue = 0, HelpText = "Water percentage on board (if applicable).")]
            public int WaterPercentage { get; set; }
        }
    }
}
