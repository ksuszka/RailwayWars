using System;
using System.Configuration;

namespace RailwayWars.WinFormsViewer
{
    public static class Settings
    {
        public static string ContestServerHost => ConfigurationManager.AppSettings["ContestServerHost"] ?? "localhost";
        public static int ContestServerPort => Convert.ToInt32(ConfigurationManager.AppSettings["ContestServerPort"] ?? "9933");
        public static int VisualisationCellSize => Convert.ToInt32(ConfigurationManager.AppSettings["VisualisationCellSize"] ?? "10");
        public static bool ShowTracksForeground => Convert.ToBoolean(ConfigurationManager.AppSettings["ShowTracksForeground"] ?? "true");
        public static string InfoFont => ConfigurationManager.AppSettings["InfoFont"] ?? "Lucida Console, 14pt";
    }
}
