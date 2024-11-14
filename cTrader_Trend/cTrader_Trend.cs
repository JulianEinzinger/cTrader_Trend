using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MovingAverageCrossoverBot : Robot
    {
        // Parameter für die beweglichen Durchschnitte
        [Parameter("Short Period", DefaultValue = 9)]
        public int ShortPeriod { get; set; }

        [Parameter("Long Period", DefaultValue = 21)]
        public int LongPeriod { get; set; }

        [Parameter("Volume", DefaultValue = 10000)]
        public int Volume { get; set; }

        private MovingAverage _shortMa;
        private MovingAverage _longMa;
        private Position _position;

        protected override void OnStart()
        {
            // Initialisierung der Indikatoren
            _shortMa = Indicators.MovingAverage(Bars.ClosePrices, ShortPeriod, MovingAverageType.Exponential);
            _longMa = Indicators.MovingAverage(Bars.ClosePrices, LongPeriod, MovingAverageType.Exponential);
        }

        protected override void OnBar()
        {
            // Überprüfen, ob eine offene Position besteht
            _position = Positions.Find("MovingAverageCrossoverBot", SymbolName);

            // Bedingungen für Kaufsignal
            if (_shortMa.Result.LastValue > _longMa.Result.LastValue)
            {
                if (_position == null)
                {
                    ExecuteMarketOrder(TradeType.Buy, SymbolName, Volume, "MovingAverageCrossoverBot");
                }
            }
            // Bedingungen für Verkaufssignal
            else if (_shortMa.Result.LastValue < _longMa.Result.LastValue)
            {
                if (_position != null)
                {
                    ClosePosition(_position);
                }
            }
        }

        protected override void OnStop()
        {
            // Alle Positionen schließen, wenn der Bot angehalten wird
            foreach (var position in Positions.FindAll("MovingAverageCrossoverBot", SymbolName))
            {
                ClosePosition(position);
            }
        }
    }
}
