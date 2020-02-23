using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CricketClubDAL;
using CricketClubDomain;



namespace CricketClubAccounts
{
    public class AccountEntry
    {
        private AccountEntryData _data;

        public int PlayerID
        {
            get { return _data.PlayerID; }
        }

        public PaymentType Type
        {
            get { return (PaymentType)_data.Type; }
            set { _data.Type = (int)value; }
        }
        
        /// <summary>
        /// Always enter as a positive number - user credit debit to determine
        /// type.
        /// </summary>
        public double Amount
        {
            get { return _data.Amount; }
            set { if (value >= 0) { _data.Amount = value; } else { _data.Amount = -1 * value; } }
        }

        public string Description
        {
            get { return _data.Description; }
            set { _data.Description = value; }
        }


        public DateTime Date
        {
            get { return _data.Date; }
            set { _data.Date = value; }
        }

        /// <summary>
        /// Credit - Cash paid to the club
        /// Debit - monies owed to the club - match fees etc
        /// </summary>
        public CreditDebit CreditOrDebit
        {
            get { return (CreditDebit)_data.CreditOrDebit; }
            set { _data.CreditOrDebit = (int)value; }
        }

        public int MatchID
        {
            get { return _data.MatchID; }
            set { _data.MatchID = value; }
        }

        public int ID
        {
            get { return _data.ID; }
        }

        public bool isConfirmed
        {
            get {
                if (this.Status == PaymentStatus.Confirmed)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public PaymentStatus Status
        {
            get { return (PaymentStatus)_data.Status; }
            set { _data.Status = (int)value; }
        }
        
        
        protected AccountEntry(AccountEntryData data) {
            _data = data;
        }

        public AccountEntry(int AccountEntryID)
        {
            AccountEntry ac = (from a in GetAll() where a.ID == AccountEntryID select a).FirstOrDefault();
            if (ac != null)
            {
                _data = ac._data;
            }
            else
            {
                throw new ApplicationException("No account entry with that ID exists - Use Create to create a new entry");
            }
        }

        internal static AccountEntry Create(PlayerAccount account, double amount, string description, DateTime Date, CreditDebit CreditOrDebit, PaymentType Type, int MatchID, PaymentStatus Status)
        {
            Dao myDao = new Dao();
            int newID = myDao.CreateNewAccountEntry(account.PlayerID, description, amount,(int)CreditOrDebit, (int)Type, MatchID, (int)Status, Date);
            ClearCache();
            return new AccountEntry(newID);
        }

        private static List<AccountEntry> _accountCache;

        public static List<AccountEntry> GetAll()
        {
            if (_accountCache == null)
            {
                List<AccountEntry> allAccounts = new List<AccountEntry>();
                Dao myDao = new Dao();
                List<AccountEntryData> data = myDao.GetAllAccountData();
                foreach (AccountEntryData row in data)
                {
                    allAccounts.Add(new AccountEntry(row));
                }
                _accountCache = allAccounts;
            }
            return _accountCache;
        }

        public void Save()
        {
            Dao myDao = new Dao();
            myDao.UpdateAccountEntry(_data);
            ClearCache();
        }

        private static void ClearCache()
        {
            _accountCache = null;
        }
        
    }
}
