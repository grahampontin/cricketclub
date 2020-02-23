using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CricketClubMiddle;
using CricketClubDAL;
using CricketClubDomain;

namespace CricketClubAccounts
{
    public class PlayerAccount
    {
        private Player _player;

        public PlayerAccount(Player player)
        {
            _player = player;
        }

        public List<AccountEntry> GetStatement()
        {
            var entries = from a in AccountEntry.GetAll() where a.PlayerID == PlayerID select a; 
            
            return entries.ToList();
        }

        public double GetBalance()
        {
            double balance = 0.0;

            foreach (AccountEntry entry in this.GetStatement())
            {
                if (entry.CreditOrDebit == CreditDebit.Credit)
                {
                    balance = balance + entry.Amount;
                }
                else
                {
                    balance = balance - entry.Amount;
                }
            }

            return Math.Round(balance,2);
        }

        public int PlayerID
        {
            get { return _player.ID; }
        }

        public void AddPayment(double amount, string description, DateTime date, Match Match, PaymentStatus Status, PaymentType Type, CreditDebit CreditOrDebit)
        {
            if (null != Match)
            {
                AccountEntry.Create(this, amount, description, date, CreditOrDebit, Type, Match.ID, Status);
            }
            else
            {
                AccountEntry.Create(this, amount, description, date, CreditOrDebit, Type, 0, Status);
            }
        }

    }
}
