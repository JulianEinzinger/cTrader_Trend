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


        int _lastCrossIndex = -1;

        protected override void OnTick() {
            // Grab current position
            _currentPosition = Positions.Find(ORDER_ID);

            // Check for time-related closing signals
            #region Time Check

            DateTime currentTime = Server.Time;

            // Check, ob es Freitag 21 Uhr ist (UTC)
            if (currentTime.DayOfWeek == DayOfWeek.Friday && currentTime.Hour >= 20) {
                // Schließe alle Position
                for (int i = 0; i < Positions.Count; i++) {
                    if (Positions[i].Label == ORDER_ID) {
                        ClosePosition(Positions[i]);
                    }
                }
                return;
            }

            #endregion

            // Check for MA crosses
            #region MA cross

            int lastBarIndex = Bars.ClosePrices.Count - 2;

            // Aktuelle MA Ergebnisse
            currentLong = _longMA.Result.LastValue;
            currentShort = _shortMA.Result.LastValue;

            // Valuevergleiche der MAs
            wasShortAbove = previousShort > previousLong;
            isShortAbove = currentShort > currentLong;

            // Setzen der letzten Ergebnisse für nächste Operation
            previousLong = _longMA.Result.LastValue;
            previousShort = _shortMA.Result.LastValue;

            // Vergleichen der Linien für Open
            if (wasShortAbove != isShortAbove) {
                // möglicher Schnittpunkt (aktuell)
                waitingForMACrossValidation = true;
                _lastCrossIndex = lastBarIndex;

                // Checken, ob Long oder Short Signal und ob Position offen
                if (_currentPosition == null) {
                    if (isShortAbove) {
                        // potentielles LONG
                        Tools.Long(this, _volume, ORDER_ID, _slPips, _tpPips);
                    } else {
                        // potentielles SHORT
                        Tools.Short(this, _volume, ORDER_ID, _slPips, _tpPips);
                    }
                }
            }


            if (waitingForMACrossValidation && lastBarIndex == _lastCrossIndex + 2) {
                // Long Bedingungen checken
                if(_currentPosition?.TradeType == TradeType.Buy && _shortMA.Result.LastValue < _longMA.Result.LastValue) {
                    ClosePosition(_currentPosition);
                }
                // Short Bedingungen checken
                if (_currentPosition?.TradeType == TradeType.Sell && _shortMA.Result.LastValue > _longMA.Result.LastValue) {
                    ClosePosition(_currentPosition);
                }

                waitingForMACrossValidation = false;
            }


            #endregion


            
        }

        #endregion

    }
}
