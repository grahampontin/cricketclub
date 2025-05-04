using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CricketClubDomain
{
    public enum MatchType
    {
        All=-1,NELCL, NELCL_Cup, Tour, Friendly, Declaration, Twenty20, Informal


    };

    public enum HomeOrAway
    {
        Home, Away
    };

    public enum ModesOfDismissal
    {
        NotOut, Bowled, LBW, Caught, RunOut, Stumped, HitWicket, DidNotBat, Retired, RetiredHurt, CaughtAndBowled
    };

    public enum BattingOrBowling
    {
        Batting, Bowling
    };

    public enum ThemOrUs
    {
        Us, Them
    };

    public enum CreditDebit
    {
        Credit, Debit
    };

    public enum PaymentType
    {
        MatchFee, Nets, Social, DigitalBanking, Cheque, Cash, Other
    };

    public enum PaymentStatus
    {
        Confirmed, Unconfirmed, Deleted
    };

    public enum PlayingRole
    {
        Batsman, Bowler, Opening_Batsman, Wicket_Keeper_Batsman, All_Rounder 
    };
}
