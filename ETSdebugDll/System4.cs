/*
    Trade Like a Hedge Fund
    20 Successful Uncorrelated Strategies & Techniques to Winning Profits
    JAMES ALTUCHER
    ISBN 0-471-48485-7

    SYSTEM #4 - THE 5 PERCENT GAP WITH MARKET GAP:
    
    • Buy a stock if the stock was down the day before,
      if the stock is opening 5 percent lower than the close the day before, 
      and if QQQ is also gapping down at least one-half percent.
    • Sell if the gap is filled or at the end of the day.

    Test: 
    All Nasdaq 100 stocks (including deletions), from January 1, 1999, to June 30, 2003.

    Results:
        Starting Capital $1,000,000.00
        Ending Capital $2,593,543.00
        Net Profit $1,593,543.00
        Net Profit % 159.35%
        Exposure % 5.22%
        Risk-Adjusted Return 3053.37%
        All Trades 525
        Average Profit/Loss $3,035.32
        Average Profit/Loss % 2.07%
        Average Bars Held 1
        Winning Trades 321 (61.14%)
        Gross Profit $2,875,406.00
        Average Profit $8,957.65
        Average Profit % 5.89%
        Average Bars Held 1
        Maximum Consecutive Winning Trades 13
        Losing Trades 204 (38.86%)
        Gross Loss ($1,281,862.38)
        Average Loss ($6,283.64)
        Average Loss % –4.07%
        Average Bars Held 0.97
        Maximum Consecutive Losing Trades 14
        Maximum Drawdown –8.26%
        Maximum Drawdown $ ($168,763.75)
        Maximum Drawdown Date 9/6/2001
        Recovery Factor 9.44
        Profit Factor 2.24
        Payoff Ratio 1.44
        Risk Reward Ratio 3.37
        Sharpe Ratio of Trades 6.59
*/

using ScriptSolution;
using System;
using ScriptSolution.Model;
using ScriptSolution.Model.Interfaces;

namespace ETSdebugDll
{
    public class System4: Script
    {
        public ParamOptimization Market = new ParamOptimization(EnumTypeGetIEnumerable.GetSeccodeList, "Market", "The market to check for down day.");
        public ParamOptimization GapPercent = new ParamOptimization(5, 1, 10, 1, "Gap Percent", "The size of the Gap in percent of the stock price.");
        
        #pragma warning disable CS8618
        private IPosition _longPos;
        #pragma warning restore CS8618
        
        public override void Execute()
        {
            // warm up
            if (IndexBar == 0) return;
            
            if (LongPos.Count == 0) 
                EntryOrders();
            else 
                ExitOrders();
        }
        
        private void EntryOrders()
        {
            // nothing to do if this (just closed) bar is not a down bar
            if (Candles.CloseSeries[IndexBar-1] < Candles.OpenSeries[IndexBar-1]) return;
            
            // nothing to do if this (just closed) Market bar is not a down bar
            if (Market.FinInfo.Candles.CloseSeries[IndexBar-1] < Market.FinInfo.Candles.OpenSeries[IndexBar-1]) return;
            
            // calculate gap according to user parameter
            var gap = Candles.CloseSeries[IndexBar] * (GapPercent.Value / 100.0);

            // calculate entry price and bar
            var entryPrice = Candles.CloseSeries[IndexBar-1] - gap;
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
            DesParamStratetgy.Description = "Gap Fill Strategy form `Trading like a Hedge Fund` - System #4";
            DesParamStratetgy.Change = "";
            DesParamStratetgy.NameStrategy = "TLHF System4";
        } 
    }
}
