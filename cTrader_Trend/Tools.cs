using cAlgo.API;

namespace TestBot {
    internal class Tools : Robot {

        public static void Clean(Robot instance) {
            for (int i = 0; i < 10; i++) {
                instance.Print();
            }
        }

        public static double CalculateRelativePriceInPips(Robot instance, double targetPrice) {
            var pipValue = instance.Symbol.PipValue * instance.Symbol.TickSize;
            var pips = Math.Abs(targetPrice / pipValue);

            return pips;
        }

        // Market Orders

        public static bool Long(Robot instance, double volume, string orderID, double slPips, double tpPips) {
            var result = instance.ExecuteMarketOrder(TradeType.Buy, instance.SymbolName, volume, orderID, slPips, tpPips);

            if (!result.IsSuccessful) {
                instance.Print("ERROR: calling from Long() | Order placement failed: " + result.Error);
            }

            return result.IsSuccessful;
        }

        public static bool Short(Robot instance, double volume, string orderID, double slPips, double tpPips) {
            var result = instance.ExecuteMarketOrder(TradeType.Sell, instance.SymbolName, volume, orderID, slPips, tpPips);

            if (!result.IsSuccessful) {
                instance.Print("ERROR: calling from Short() | Order placement failed: " + result.Error);
            }

            return result.IsSuccessful;
        }
    }
}
