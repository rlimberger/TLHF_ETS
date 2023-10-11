/*
    Trade Like a Hedge Fund
    20 Successful Uncorrelated Strategies & Techniques to Winning Profits
    JAMES ALTUCHER
    ISBN 0-471-48485-7

    SYSTEM #3 - THE 5 PERCENT GAP:
    
    • Buy a stock if the stock was down the day before and if the stock is
    opening 5 percent lower than the close the day before.
    • Sell either if the stock hits the close the day before or the stock closes
    without hitting the profit target.

    Test: 
    All Nasdaq 100 stocks (including deletions), from January 1, 1999, to June 30, 2003.

    Results:
        All Trades 993
        Average Profit/Loss % 1.97%
        Average Bars Held 1
        Winning Trades 605 (60.93%)
        Average Profit % 6.02%
        Average Bars Held 1
        Maximum Consecutive Winning Trades 18
        Losing Trades 388 (39.07%)
        Average Loss % –4.47%
        Average Bars Held 0.97
        Maximum Consecutive Losing Trades 10
*/

using ScriptSolution;
using System;
using ScriptSolution.Model;
using ScriptSolution.Model.Interfaces;

namespace ETSdebugDll
{
    public class System3: Script
    {
        public ParamOptimization GapPercent = new ParamOptimization(5, 1, 10, 1, "Gap Percent", "The size of the Gap in percent of the stock price.");
        
        #pragma warning disable CS8618
        private IPosition _longPos;
        #pragma warning restore CS8618
        
        public override void Execute()
        {
            if (LongPos.Count == 0) 
                EntryOrders();
            else 
                ExitOrders();
        }
        
        private void EntryOrders()
        {
            // nothing to do if this (just closed) bar is not a down bar
            if (Candles.CloseSeries[IndexBar] < Candles.OpenSeries[IndexBar]) return;
            
            // calculate gap according to user parameter
            var gap = Candles.OpenSeries[IndexBar] * (GapPercent.Value / 100.0);

            // calculate entry price and bar
            var entryPrice = Candles.CloseSeries[IndexBar] - gap;
            var entryBar = IndexBar + 1;
            var entrySignalName = "Gap Down Long Entry";
            
            // Go long at the open of the next bar, if we open lower than this bar's close (Gap Down)
            _longPos = BuyAtLimit(entryBar, entryPrice, 1, entrySignalName);
        }
        
        private void ExitOrders()
        {
            // Exit long at price we originally gaped down from to take profit
            // aka: the gap down was filled by going up to the close of the bar prior to entry bar
            var profitTarget = Candles.CloseSeries[_longPos.EntryBar - 1];
            
            // FIXME: for some reason, SellAtProfit seems to be off by one bar
            //        to place this order on the next bar we need to use this bar's index :( 
            SellAtProfit(IndexBar, _longPos, profitTarget,"Gap Down Long Profit Target Exit");
                
            // stop out at the close of today if signal's closing price is never hit
            SellAtClose(IndexBar+1, _longPos, "Gap Down Long Stop Exit");
                
            // TODO: add additional risk parameter driven stop loss?
        }

        public override void GetAttributesStratetgy()
        {
            DesParamStratetgy.Version = "1.0";
            DesParamStratetgy.DateRelease = DateTime.Now;
            DesParamStratetgy.DateChange = DateTime.Now;
            DesParamStratetgy.Author = "Rene Limberger";
            DesParamStratetgy.Description = "Gap Fill Strategy form `Trading like a Hedge Fund` - System #3";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.NameStrategy = "TLHF System3";
        } 
    }
}
