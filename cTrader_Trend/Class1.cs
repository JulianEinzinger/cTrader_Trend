using cAlgo.API;
using cAlgo.API.Indicators;
using System;

namespace TestBot {
    //[Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Class1 : Robot {
        #region parameter

        [Parameter("[MA] Short Period", DefaultValue = 9, Group = "Moving Averages")]
        public int _shortPeriod { get; set; }

        [Parameter("[MA] Long Period", DefaultValue = 21, Group = "Moving Averages")]
        public int _longPeriod { get; set; }

        // Tradingvolume 100.000 = 1 Lot
        [Parameter("Trading Volume", DefaultValue = 10000, Group = "Trading")]
        public int _volume { get; set; }

        [Parameter("Take Profit EUR", DefaultValue = 0, Group = "Trading")]
        public int _takeProfit { get; set; }

        [Parameter("Stop Loss EUR", DefaultValue = 0, Group = "Trading")]
        public int _stopLoss { get; set; }

        #endregion

        #region fields

        private MovingAverage _shortMA;
        private MovingAverage _longMA;
        private Position _currentPosition;
        private double _slPips;
        private double _tpPips;

        private const string ORDER_ID = "TrendBot";

        #endregion

        #region methods

        private bool CheckLong() {
            // Bedingungen für Kaufsignal
            if (_shortMA.Result.LastValue > _longMA.Result.LastValue) {
                Print("calling from CheckLong event handler: Possible Long signal detected!");

                // Schließe Short-Position (falls vorhanden)
                if (_currentPosition != null) {
                    if (_currentPosition.TradeType == TradeType.Sell && _currentPosition.NetProfit > 0) {
                        // Aktuelle Position ist Short
                        ClosePosition(_currentPosition);
                        return true;
                    }
                } else {
                    return true;
                }
            }
            return false;
        }

        private bool CheckShort() {
            // Bedingungen für Verkaufsignal
            if (_shortMA.Result.LastValue < _longMA.Result.LastValue) {
                Print("calling from CheckShort event handler: Possible Short signal detected!");

                // Schließe Long-Position (falls vorhanden)
                if (_currentPosition != null) {
                    if (_currentPosition.TradeType == TradeType.Buy && _currentPosition.NetProfit > 0) {
                        // Aktuelle Position ist Long
                        ClosePosition(_currentPosition);
                        return true;
                    }
                } else {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region events

        protected override void OnStart() {
            // Initialiserung der Indikatoren
            _shortMA = Indicators.MovingAverage(Bars.ClosePrices, _shortPeriod, MovingAverageType.Exponential);
            _longMA = Indicators.MovingAverage(Bars.ClosePrices, _longPeriod, MovingAverageType.Exponential);

            // Calculate TP / SL EUR input to useful data
            _takeProfit *= 10000;
            _takeProfit /= _volume;
            _stopLoss *= 10000;
            _stopLoss /= _volume;

            // StopLoss / TakeProfit Berechnung
            _slPips = Tools.CalculateRelativePriceInPips(this, _stopLoss) / 1000000000;
            _tpPips = Tools.CalculateRelativePriceInPips(this, _takeProfit) / 1000000000;
        }

        protected override void OnStop() {
            // Alle Positionen schließen
            foreach (var position in Positions.FindAll(ORDER_ID, SymbolName)) {
                ClosePosition(position);
            }
        }

        protected override void OnBar() {
            // Grabben der (evtl.) offenen Position
            _currentPosition = Positions.Find(ORDER_ID, SymbolName);

            // Check for long opportunity
            if (CheckLong()) Tools.Long(this, _volume, ORDER_ID, _slPips, _tpPips);

            // Check for short opportunity
            if (CheckShort()) Tools.Short(this, _volume, ORDER_ID, _slPips, _tpPips);

        }

        #endregion
    }
}
