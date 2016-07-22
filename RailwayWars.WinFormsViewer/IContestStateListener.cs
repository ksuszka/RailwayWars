using System;
using RailwayWars.Contracts;

namespace RailwayWars.WinFormsViewer
{
    internal interface IContestStateListener
    {
        void ExceptionDetected(Exception exception);
        void UpdateState(TournamentStateDTO state);
    }
}
