/*
    Trade Like a Hedge Fund
    20 Successful Uncorrelated Strategies & Techniques to Winning Profits
    JAMES ALTUCHER
    ISBN 0-471-48485-7

    SYSTEM #1 - FILLING THE GAP:
    The following is a test of the basic gap-fill approach:
        • Buy a stock when it opens more than 2 percent lower than the prior
        close.
        • Sell at yesterday’s closing price or at the close if yesterday’s closing
        price is never hit.

    Test: 
    All Nasdaq 100 stocks (including deletions), from January 1, 1999, to June 30, 2003.

    Results:
        All Trades 9,821
        Average Profit/Loss % 0.58%
        Average Bars Held 1
        Winning Trades 6,174 (62.87%)
        Average Profit % 3.21%
        Maximum Consecutive Winning Trades 58
        Losing Trades 3,647 (37.13%)
        Average Loss % –3.97%
        Maximum Consecutive Losing Trades 20
*/

using ScriptSolution;
using System;
using ScriptSolution.Model;
using ScriptSolution.Model.Interfaces;

namespace ETSdebugDll
{
    public class System1: Script
    {
        public ParamOptimization GapPercent = new ParamOptimization(2, 1, 10, 1, "Gap Percent", "The size of the Gap in percent of the stock price.");
        
        #pragma warning disable CS8618
        private IPosition _longPos;
        //private IPosition _shortPos;
        #pragma warning restore CS8618
        
        public override void Execute()
        {
            // calculate gap
            var gap = Candles.OpenSeries[IndexBar] * (GapPercent.Value / 100.0); // TODO: user parameter
            
            // long entry
            if (LongPos.Count == 0)
            {
                // Go long if we open the next bar lower than this bar's close (Gap Down)
                _longPos = BuyAtLimit(IndexBar+1, Candles.CloseSeries[IndexBar]-gap, 1, "Gao Down Long Entry");
            }
            
            // long stop & exit
            else
            {
                // Exit long at price we originally gaped down from to take profit
                // aka: the gap down was filled by going up to the close of the bar prior to entry bar
                SellAtProfit(IndexBar, _longPos, Candles.CloseSeries[_longPos.EntryBar-1],"Gap Down Long Profit Exit");
                
                // stop out at the close of today if signal's closing price is never hit
                SellAtClose(IndexBar, _longPos, "Gap Down Long Stop Exit");
                
                // TODO: add stop loss?
            }
            
            // short entry
            // if (ShortPos.Count == 0)
            // {
            //     // Go short if we open the next bar higher than this bar's close (Gap Up)
            //     _shortPos = ShortAtLimit(IndexBar+1, Candles.CloseSeries[IndexBar]+gap, 1, "Gap Up Long Entry");
            // }
            //
            // // short stop & exit
            // else
            // {
            //     // Cover short at price we originally gaped up from to take profit
            //     // aka: the gap up was filled by going down to the close of the bar prior to entry bar
            //     CoverAtProfit(IndexBar, _shortPos, Candles.CloseSeries[_shortPos.EntryBar-1],"Gap Up Short Profit Exit");
            //     
            //     // stop out at the close of today if the signal's closing price is never hit
            //     CoverAtClose(IndexBar, _shortPos, "Gap Up Stop Exit");
            //     
            //     // TODO: add stop loss?
            // }
        }
        
        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1.0";
            DesParamStratetgy.DateRelease = DateTime.Now;
            DesParamStratetgy.DateChange = DateTime.Now;
            DesParamStratetgy.Author = "Rene Limberger";
            DesParamStratetgy.Description = "Gap Fill Strategy form `Trading like a Hedge Fund` - System #1";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.NameStrategy = "TLHF System1";
        } 
    }
}
