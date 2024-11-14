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

        // Parameter für StopLoss-Preis
        [Parameter("Stop Loss", DefaultValue = -2)]
        public double SL { get; set; }

        // Parameter für TakeProfit-Preis
        [Parameter("Take Profit", DefaultValue = 2)]
        public double TP { get; set; }

        private MovingAverage _shortMa;
        private MovingAverage _longMa;
        private Position _position;
        private double? stopLoss;
        private double? takeProfit;

        protected override void OnStart()
        {
            // Initialisierung der Indikatoren
            _shortMa = Indicators.MovingAverage(Bars.ClosePrices, ShortPeriod, MovingAverageType.Exponential);
            _longMa = Indicators.MovingAverage(Bars.ClosePrices, LongPeriod, MovingAverageType.Exponential);

            // StopLoss Berechnung
            stopLoss = CalculateStopLossInPips(SL);
            takeProfit = CalculateTakeProfitInPips(TP);
        }

        protected override void OnStop()
        {
            // Alle Positionen schließen, wenn der Bot angehalten wird
            foreach (var position in Positions.FindAll("MovingAverageCrossoverBot", SymbolName))
            {
                ClosePosition(position);
            }
        }



        protected override void OnBar()
        {
            // Überprüfen, ob eine offene Position besteht
            _position = Positions.Find("MovingAverageCrossoverBot", SymbolName);

            // Bedingungen für Kaufsignal
            if (_shortMa.Result.LastValue > _longMa.Result.LastValue)
            {
                Print("LONG SIGNAL!");
                // Schließe Short-Position (falls vorhanden)
                if (_position != null)
                {
                    if (_position.TradeType == TradeType.Sell)
                    {
                        // offende Position ist Short
                        ClosePosition(_position);
                        Long();
                    }
                }
                else
                {
                    Long();
                }
            }
            // Bedingungen für Verkaufssignal
            else if (_shortMa.Result.LastValue < _longMa.Result.LastValue)
            {
                Print("SHORT SIGNAL!");
                // Schließe Long-Position (falls vorhanden)
                if (_position != null)
                {
                    if (_position.TradeType == TradeType.Buy && _position.NetProfit > 0)
                    {
                        // offene Position ist Long
                        ClosePosition(_position);
                        Short();
                    }
                }
                else
                {
                    Short();
                }
            }
        }





        // own methods

        private void Short()
        {
            ExecuteMarketOrder(TradeType.Sell, SymbolName, Volume, "MovingAverageCrossoverBot", stopLoss, takeProfit);
        }

        private void Long()
        {
            ExecuteMarketOrder(TradeType.Buy, SymbolName, Volume, "MovingAverageCrossoverBot", stopLoss, takeProfit);
        }

        private double? CalculateStopLossInPips(double targetLossInEuro)
        {
            var pipValue = Symbol.PipValue * Symbol.TickSize;
            var stopLossPips = Abs(targetLossInEuro / pipValue);

            return stopLossPips;
        }
        
        private double? CalculateTakeProfitInPips(double targetProfitInEuro)
        {
            var pipValue = Symbol.PipValue * Symbol.TickSize;
            var takeProfitPips = Abs(targetProfitInEuro / pipValue);

            return takeProfitPips;
        }
        
        

        private double Abs(double d)
        {
            if (d < 0) return d * -1;
            return d;
        }
    }
}
