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
    }
}
