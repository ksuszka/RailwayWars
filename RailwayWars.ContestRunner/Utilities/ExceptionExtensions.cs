using System;
using System.Linq;

namespace RailwayWars.ContestRunner
{
    static class ExceptionExtensions
    {
        public static string GetFlatMessage(this Exception ex)
        {
            var ae = ex as AggregateException;
            if (ae == null) return ex.Message;
            if (ae.InnerExceptions == null) return ae.Message;
            return $"{ae.Message}: {string.Join("; ", ae.InnerExceptions.Select(s => s.Message))}";
        }
    }
}
