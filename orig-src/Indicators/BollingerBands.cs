using System;

package com.quantconnect.lean.Indicators
{
    /**
     * This indicator creates a moving average (middle band) with an upper band and lower band
     * fixed at k standard deviations above and below the moving average.
    */
    public class BollingerBands : Indicator
    {
        /**
         * Gets the type of moving average
        */
        public MovingAverageType MovingAverageType { get; private set; }

        /**
         * Gets the standard deviation
        */
        public IndicatorBase<IndicatorDataPoint> StandardDeviation { get; private set; }

        /**
         * Gets the middle bollinger band (moving average)
        */
        public IndicatorBase<IndicatorDataPoint> MiddleBand { get; private set; }

        /**
         * Gets the upper bollinger band (middleBand + k * stdDev)
        */
        public IndicatorBase<IndicatorDataPoint> UpperBand { get; private set; }

        /**
         * Gets the lower bollinger band (middleBand - k * stdDev)
        */
        public IndicatorBase<IndicatorDataPoint> LowerBand { get; private set; }

        /**
         * Initializes a new instance of the BollingerBands class
        */
         * @param period The period of the standard deviation and moving average (middle band)
         * @param k The number of standard deviations specifying the distance between the middle band and upper or lower bands
         * @param movingAverageType The type of moving average to be used
        public BollingerBands(int period, BigDecimal k, MovingAverageType movingAverageType = MovingAverageType.Simple)
            : this( String.format( "BB(%1$s,%2$s)", period, k), period, k, movingAverageType) {
        }

        /**
         * Initializes a new instance of the BollingerBands class
        */
         * @param name The name of this indicator
         * @param period The period of the standard deviation and moving average (middle band)
         * @param k The number of standard deviations specifying the distance between the middle band and upper or lower bands
         * @param movingAverageType The type of moving average to be used
        public BollingerBands(String name, int period, BigDecimal k, MovingAverageType movingAverageType = MovingAverageType.Simple)
            : base(name) {
            MovingAverageType = movingAverageType;
            StandardDeviation = new StandardDeviation(name + "_StandardDeviation", period);
            MiddleBand = movingAverageType.AsIndicator(name + "_MiddleBand", period);
            LowerBand = MiddleBand.Minus(StandardDeviation.Times(k), name + "_LowerBand");
            UpperBand = MiddleBand.Plus(StandardDeviation.Times(k), name + "_UpperBand");
        }
           
        /**
         * Gets a flag indicating when this indicator is ready and fully initialized
        */
        public @Override boolean IsReady
        {
            get { return MiddleBand.IsReady && UpperBand.IsReady && LowerBand.IsReady; }
        }

        /**
         * Computes the next value of the following sub-indicators from the given state:
         * StandardDeviation, MiddleBand, UpperBand, LowerBand
        */
         * @param input The input given to the indicator
        @returns The input is returned unmodified.
        protected @Override BigDecimal ComputeNextValue(IndicatorDataPoint input) {
            StandardDeviation.Update(input);
            MiddleBand.Update(input);
            UpperBand.Update(input);
            LowerBand.Update(input);
            return input;
        }

        /**
         * Resets this indicator and all sub-indicators (StandardDeviation, LowerBand, MiddleBand, UpperBand)
        */
        public @Override void Reset() {
            StandardDeviation.Reset();
            MiddleBand.Reset();
            UpperBand.Reset();
            LowerBand.Reset();
            base.Reset();
        }
    }
}
