using cAlgo.API;
using cAlgo.API.Indicators;
using System;

namespace TestBot {
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Class1 : Robot {
        #region parameter

        [Parameter("[BB] Short Period", DefaultValue = 9, Group = "Bollinger Bands")]
        public int _shortPeriod { get; set; }

        [Parameter("[BB] Long Period", DefaultValue = 21, Group = "Bollinger Bands")]
        public int _longPeriod { get; set; }

        // Tradingvolume 100.000 = 1 Lot
        [Parameter("Trading Volume", DefaultValue = 10000, Group = "Trading")]
        public int _volume { get; set; }

        [Parameter("Take Profit", DefaultValue = 2, Group = "Trading")]
        public int _takeProfit { get; set; }

        [Parameter("Stop Loss", DefaultValue = -2, Group = "Trading")]
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

        private void Short() {
            ExecuteMarketOrder(TradeType.Sell, SymbolName, _volume, ORDER_ID, _slPips, _tpPips);
        }

        private void Long() {
            ExecuteMarketOrder(TradeType.Buy, SymbolName, _volume, ORDER_ID, _slPips, _tpPips);
        }

        private void CheckLong() {
            throw new NotImplementedException();
        }

        private void CheckShort() {
            throw new NotImplementedException();
        }

        #endregion

        #region events

        protected override void OnStart() {
            // Initialiserung der Indikatoren
            _shortMA = Indicators.MovingAverage(Bars.ClosePrices, _shortPeriod, MovingAverageType.Exponential);
            _longMA = Indicators.MovingAverage(Bars.ClosePrices, _longPeriod, MovingAverageType.Exponential);

            // StopLoss / TakeProfit Berechnung
            _slPips = Tools.CalculateRelativePriceInPips(this, _stopLoss);
            _tpPips = Tools.CalculateRelativePriceInPips(this, _takeProfit);
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
            CheckLong();

            // Check for short opportunity
            CheckShort();

        }

        #endregion
    }
}
