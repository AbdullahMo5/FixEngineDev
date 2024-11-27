using FixEngine.Models;
using System.Collections.Concurrent;

namespace FixEngine.Simulation
{
    public class CustomDictionary               //Enhance UserMargin make them two seperate models. one contains List, othe contains margin
    {
        private readonly ConcurrentDictionary<int, UserMargin> marginBook;    //Key is User Id
        private readonly ConcurrentDictionary<int?, List<UserMargin>> userBook;     //Key is Symbol Id

        public CustomDictionary()
        {
            marginBook = new ConcurrentDictionary<int, UserMargin>();
            userBook = new ConcurrentDictionary<int?, List<UserMargin>>();
        }

        public void AddOrUpdate(int key1, int? key2, UserMargin value)
        {
            if(marginBook.ContainsKey(key1))
            {
                marginBook[key1].PoseSize += value.PoseSize;
                marginBook[key1].RiskUserId = value.RiskUserId;
                marginBook[key1].Balance = value.Balance;
                marginBook[key1].Leverage = value.Leverage;
                marginBook[key1].UnFilledPositions.AddRange(value.UnFilledPositions);
            }
            else
            {
                var margn = new UserMargin
                {
                    Balance = value.Balance,
                    Leverage = value.Leverage,
                    RiskUserId = value.RiskUserId,
                    PoseSize = value.PoseSize
                };
                margn.UnFilledPositions.AddRange(value.UnFilledPositions);

                marginBook.TryAdd(key1, margn);
            }

            //SymbolId Exist
            if (userBook.ContainsKey(key2))
            {
                //UserMargin Exist
                var userMargin = userBook[key2].FirstOrDefault(u => u.RiskUserId == key1);
                if (userMargin != null)
                {
                    userMargin.PoseSize += value.PoseSize;  //Increse PosSize

                    userMargin.Balance = value.Balance;     //Update Balance
                    userMargin.Leverage = value.Leverage;   //Update Leverage
                    userMargin.UnFilledPositions.AddRange(value.UnFilledPositions);    //Update List
                }
                else
                {
                    var margn = new UserMargin { 
                        Balance = value.Balance, 
                        Leverage = value.Leverage,
                        RiskUserId = value.RiskUserId, 
                        PoseSize = value.PoseSize };
                    margn.UnFilledPositions.AddRange(value.UnFilledPositions);

                    userBook[key2].Add(margn);
                }
            }
            else
            {
                var usersList = userBook.GetOrAdd(key2, _ => new List<UserMargin>());
                usersList.Add(value);
            }
        }

        public bool ContainsKey(int key1)
        {
            if (marginBook.ContainsKey(key1)) return true;

            return false;
        }

        public bool ContainsKey(int? key2)
        {
            if (userBook.ContainsKey(key2)) return true;

            return false;
        }

        public bool ContainsKey(int key1, int? key2)
        {
            if (marginBook.ContainsKey(key1) && userBook.ContainsKey(key2)) return true;

            return false;
        }

        public UserMargin Get(int key)
        {
            if (marginBook.ContainsKey(key)) return marginBook[key];

            return null;
        }

        public List<UserMargin> GetList(int key)
        {
            if (userBook.ContainsKey(key)) return userBook[key];

            return null;
        }
    }
}
