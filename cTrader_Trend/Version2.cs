using cAlgo.API;
using cAlgo.API.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestBot;

namespace cTrader_Trend {
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Version2 : Robot {

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

        private const string ORDER_ID = "TrendBot";

        private MovingAverage _shortMA;
        private MovingAverage _longMA;
        private Position _currentPosition;

        // MA cross
        private double previousShort;
        private double previousLong;
        private double currentShort;
        private double currentLong;
        private bool wasShortAbove;
        private bool isShortAbove;

        private bool waitingForMACrossValidation;

        private double _slPips;
        private double _tpPips;

        private bool wasShortAboveBefore;

        #endregion

        #region events

        protected override void OnStart() {
            // Initialisierung der Indikatoren
            _shortMA = Indicators.MovingAverage(Bars.ClosePrices, _shortPeriod, MovingAverageType.Exponential);
            _longMA = Indicators.MovingAverage(Bars.ClosePrices, _longPeriod, MovingAverageType.Exponential);

            // Initialisierung der letzten MA-Ergebnisse
            previousLong = _longMA.Result.LastValue;
            previousShort = _shortMA.Result.LastValue;
            waitingForMACrossValidation = false; // Initialisierung mit false

            // Calculate TP / SL EUR input to useful data
            _takeProfit *= 10000;
            _takeProfit /= _volume;
            _stopLoss *= 10000;
            _stopLoss /= _volume;

            // StopLoss / TakeProfit Berechnung
            _slPips = Tools.CalculateRelativePriceInPips(this, _stopLoss) / 1000000000;
            _tpPips = Tools.CalculateRelativePriceInPips(this, _takeProfit) / 1000000000;
        }


        protected override void OnBar() {
            _currentPosition = Positions.Find(ORDER_ID);
            // Checke alle 2 Kerzen
            if(Bars.Count % 1 != 0) {
                return;
            }
            
            // Letzte Werte der MAS
            double currentShort = _shortMA.Result.LastValue;
            double currentLong = _longMA.Result.LastValue;

            // Schnittpunkt überprüfen
            if((previousShort < previousLong && currentShort > currentLong) || (previousShort > previousLong && currentShort < currentLong)) {
                string trend = currentLong < currentShort ? "LONG" : "SHORT";
                Print("CUT!");

                if (_currentPosition == null) {
                    switch (trend) {
                        case "LONG":
                            Tools.Long(this, _volume, ORDER_ID, _slPips, _tpPips);
                            break;
                        case "SHORT":
                            Tools.Short(this, _volume, ORDER_ID, _slPips, _tpPips);
                            break;
                    }
                }
            }

            previousShort = currentShort;
            previousLong = currentLong;

        }

        protected override void OnStop() {
            foreach(var position in Positions) {
                ClosePosition(position);
            }
        }

        #endregion


    }
}
